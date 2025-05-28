[Setup]
AppName=Wustatus Beacon
AppVersion=0.1.0
DefaultDirName={pf64}\ZentrixLabs\WustatusBeacon
DefaultGroupName=Wustatus Beacon
OutputDir=..\artifacts
OutputBaseFilename=WustatusBeaconInstaller
SetupIconFile=..\beacon.ico
UninstallDisplayIcon={app}\beacon.ico
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
AppPublisher=ZentrixLabs
AppPublisherURL=https://zentrixlabs.com
AppSupportURL=https://zentrixlabs.com/support
AppUpdatesURL=https://zentrixlabs.com/updates
SetupLogging=yes
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "..\WustatusBeacon\bin\x64\Release\WustatusBeacon.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\WustatusBeaconInstaller\bin\x64\Release\ZentrixLabs.WustatusBeaconInstaller.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\beacon.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Wustatus Beacon"; Filename: "{app}\WustatusBeacon.exe"; IconFilename: "{app}\beacon.ico"; WorkingDir: "{app}"
Name: "{autodesktop}\Wustatus Beacon"; Filename: "{app}\WustatusBeacon.exe"; IconFilename: "{app}\beacon.ico"; WorkingDir: "{app}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "{app}\ZentrixLabs.WustatusBeaconInstaller.exe"; Parameters: "install"; Flags: runhidden waituntilterminated; Description: "Install Wustatus Beacon Service"

[UninstallRun]
Filename: "{app}\ZentrixLabs.WustatusBeaconInstaller.exe"; Parameters: "uninstall"; Flags: runhidden waituntilterminated
