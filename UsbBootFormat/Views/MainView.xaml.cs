using CsharpHelpers.DialogServices;
using CsharpHelpers.Helpers;
using CsharpHelpers.Interops;
using CsharpHelpers.WindowServices;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace UsbBootFormat.Views
{

    public partial class MainView : Window
    {

        public MainView()
        {
            InitializeComponent();
            SourceInitialized += OnSourceInitialized;

            new WindowSystemMenu(this) { AboutDialog = new AboutDialog1() };
            new WindowShowMain(this);
            new WindowPlacement(this)
            {
                PlacementPath = AppHelper.DataDirectory.GetFilePath(GetType().Name),
                PlacementType = PlacementType.SizeAndPosition
            };
        }


        private void OnSourceInitialized(object sender, EventArgs e)
        {
            if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
                hwndSource.AddHook(WndProc);
        }


        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeConstants.WM_DEVICECHANGE)
            {
                switch (wParam.ToInt32())
                {
                    case NativeConstants.DBT_DEVICEARRIVAL:
                    case NativeConstants.DBT_DEVICEREMOVECOMPLETE:
                        var dbhdr = Marshal.PtrToStructure<DEV_BROADCAST_HDR>(lParam);
                        if (dbhdr.dbch_devicetype == NativeConstants.DBT_DEVTYP_VOLUME)
                        {
                            var dbv = Marshal.PtrToStructure<DEV_BROADCAST_VOLUME>(lParam);
                            if (dbv.dbcv_flags == 0)
                            {
                                ((ViewModels.MainViewModel)DataContext).UpdateDrives();
                            }
                        }
                        break;
                }
            }

            return IntPtr.Zero;
        }

    }

}
