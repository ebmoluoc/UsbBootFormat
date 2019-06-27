#include "pch.h"
#include "ContextMenuHandler.h"

///////////////////////////////////////////////////////////////////////////////////////////////////

ContextMenuHandler::ContextMenuHandler(Dll& dll) : dll_{ dll }, refCount_{ 1 }
{
	dll_.AddRef();

	hBitmapMenu_ = hlp::BitmapFromIconResource(dll_.Handle(), IDI_ICON_MENU, 0, 0);
}

///////////////////////////////////////////////////////////////////////////////////////////////////

ContextMenuHandler::~ContextMenuHandler()
{
	if (hBitmapMenu_ != nullptr)
	{
		DeleteObject(hBitmapMenu_);
	}

	dll_.Release();
}

///////////////////////////////////////////////////////////////////////////////////////////////////

IFACEMETHODIMP ContextMenuHandler::QueryInterface(REFIID riid, LPVOID* ppvObject)
{
	if (ppvObject == nullptr)
	{
		return E_POINTER;
	}
	else if (IsEqualIID(riid, IID_IContextMenu))
	{
		*ppvObject = static_cast<LPCONTEXTMENU>(this);
	}
	else if (IsEqualIID(riid, IID_IShellExtInit))
	{
		*ppvObject = static_cast<LPSHELLEXTINIT>(this);
	}
	else
	{
		*ppvObject = nullptr;
		return E_NOINTERFACE;
	}

	this->AddRef();

	return S_OK;
}

///////////////////////////////////////////////////////////////////////////////////////////////////

IFACEMETHODIMP_(ULONG) ContextMenuHandler::AddRef()
{
	return ++refCount_;
}

///////////////////////////////////////////////////////////////////////////////////////////////////

IFACEMETHODIMP_(ULONG) ContextMenuHandler::Release()
{
	auto rc{ --refCount_ };
	if (rc == 0)
	{
		delete this;
	}
	return rc;
}

///////////////////////////////////////////////////////////////////////////////////////////////////

IFACEMETHODIMP ContextMenuHandler::Initialize(PCIDLIST_ABSOLUTE pidlFolder, LPDATAOBJECT pdtobj, HKEY hkeyProgID)
{
	UNREFERENCED_PARAMETER(pidlFolder);
	UNREFERENCED_PARAMETER(hkeyProgID);

	if (pdtobj == nullptr)
	{
		return E_INVALIDARG;
	}

	if (pathList_.Load(pdtobj) && !pathList_.IsMultiItems())
	{
		WCHAR drive[]{ L"\\\\.\\A:" };
		drive[4] = pathList_.GetFirstItem().front();

		hlp::StorageDeviceDescriptor sdd;
		if (sdd.Load(drive) && sdd.RemovableMedia() == TRUE && sdd.BusType() == BusTypeUsb)
		{
			return S_OK;
		}
	}

	return E_FAIL;
}

///////////////////////////////////////////////////////////////////////////////////////////////////

IFACEMETHODIMP ContextMenuHandler::QueryContextMenu(HMENU hmenu, UINT indexMenu, UINT idCmdFirst, UINT idCmdLast, UINT uFlags)
{
	UNREFERENCED_PARAMETER(idCmdLast);

	if ((uFlags & CMF_DEFAULTONLY) == 0)
	{
		if (hlp::AddMenuItem(hmenu, indexMenu, PRODUCT_NAME_W, idCmdFirst, nullptr, hBitmapMenu_))
		{
			if (hlp::MutexExists(PRODUCT_MUTEX_W))
			{
				EnableMenuItem(hmenu, indexMenu, MF_BYPOSITION | MF_DISABLED);
			}
			return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, 1);
		}
	}

	return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, 0);
}

///////////////////////////////////////////////////////////////////////////////////////////////////

IFACEMETHODIMP ContextMenuHandler::InvokeCommand(LPCMINVOKECOMMANDINFO pici)
{
	if (HIWORD(pici->lpVerb) != 0 || LOWORD(pici->lpVerb) != 0)
	{
		return E_FAIL;
	}

	auto exePath{ hlp::RenamePath(dll_.Path(), L"UsbBootFormat.exe") };
	auto usbDrive{ pathList_.GetFirstItem() };
	auto argDrive{ hlp::EscapeArgument(usbDrive) };

	auto arguments{ L"-d:" + argDrive };

	ShellExecuteW(nullptr, nullptr, exePath.c_str(), arguments.c_str(), nullptr, SW_SHOW);

	return S_OK;
}

///////////////////////////////////////////////////////////////////////////////////////////////////

IFACEMETHODIMP ContextMenuHandler::GetCommandString(UINT_PTR idCmd, UINT uType, UINT* pReserved, LPSTR pszName, UINT cchMax)
{
	UNREFERENCED_PARAMETER(idCmd);
	UNREFERENCED_PARAMETER(uType);
	UNREFERENCED_PARAMETER(pReserved);
	UNREFERENCED_PARAMETER(pszName);
	UNREFERENCED_PARAMETER(cchMax);

	return E_NOTIMPL;
}
