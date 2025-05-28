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
AppPublisherURL=https://zentrixlabs.net
AppSupportURL=https://github.com/ZentrixLabs/ZentrixLabs.WustatusBeacon/issues
AppUpdatesURL=https://github.com/ZentrixLabs/ZentrixLabs.WustatusBeacon
SetupLogging=yes
ArchitecturesInstallIn64BitMode=x64
DefaultDialogFontName=Segoe UI

[Files]
Source: "..\ZentrixLabs.WustatusBeacon\bin\Release\net48\ZentrixLabs.WustatusBeacon.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\WustatusBeaconInstaller\bin\Release\ZentrixLabs.WustatusBeaconInstaller.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\beacon.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Wustatus Beacon"; Filename: "{app}\ZentrixLabs.WustatusBeacon.exe"; IconFilename: "{app}\beacon.ico"; WorkingDir: "{app}"
Name: "{autodesktop}\Wustatus Beacon"; Filename: "{app}\ZentrixLabs.WustatusBeacon.exe"; IconFilename: "{app}\beacon.ico"; WorkingDir: "{app}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "{app}\ZentrixLabs.WustatusBeaconInstaller.exe"; \
  Parameters: "--install ""{app}\ZentrixLabs.WustatusBeacon.exe"""; \
  Flags: runhidden waituntilterminated; \
  Description: "Install Wustatus Beacon Service"


[UninstallRun]
Filename: "{app}\ZentrixLabs.WustatusBeaconInstaller.exe"; Parameters: "uninstall"; Flags: runhidden waituntilterminated
