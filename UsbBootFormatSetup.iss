[Setup]
AppId={{5A1E4ADA-7A01-423E-92EC-22ECC8616763}
SetupMutex=Global\5A1E4ADA-7A01-423E-92EC-22ECC8616763
AppMutex=Global\D8431041-A677-4728-857A-08A99B7FB8E8
AppCopyright=Copyright (c) 2019 Philippe Coulombe
AppPublisher=Philippe Coulombe
AppVersion=1.0.0.0
VersionInfoVersion=1.0.0.0
AppVerName=USB Boot Format 1.0
AppName=USB Boot Format
DefaultDirName={commonpf}\USB Boot Format
UninstallDisplayIcon={app}\UsbBootFormat.exe
OutputBaseFilename=UsbBootFormatSetup
OutputDir=.
LicenseFile=LICENSE
DisableProgramGroupPage=yes
DisableDirPage=yes
SolidCompression=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
MinVersion=6.1.7601
WizardSizePercent=120,100

[Files]
Source: "LICENSE"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion
Source: "UsbBootFormat\bin\x64\Release\CsharpHelpers.dll"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion
Source: "UsbBootFormat\bin\x64\Release\UsbBootFormat.exe"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion
Source: "UsbBootFormat\bin\x64\Release\UsbBootFormat.exe.config"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion
Source: "UsbBootFormat\bin\x64\Release\System.Windows.Interactivity.dll"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion
Source: "x64\Release\UsbBootFormatExt.dll"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion regserver

[Icons]
Name: "{commonprograms}\USB Boot Format"; Filename: "{app}\UsbBootFormat.exe"
Name: "{commondesktop}\USB Boot Format"; Filename: "{app}\UsbBootFormat.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; Flags: unchecked

[Run]
Filename: "{app}\UsbBootFormat.exe"; Description: "{cm:LaunchProgram,USB Boot Format}"; Flags: nowait postinstall skipifsilent unchecked

[Code]
procedure InitializeWizard();
begin
    WizardForm.LicenseMemo.Font.Name := 'Consolas';
end;
