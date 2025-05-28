[Setup]
AppName=Wustatus Beacon
AppVersion={#MyAppVersion}
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
Source: "..\ZentrixLabs.WustatusBeacon\bin\x64\Release\net48\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "..\WustatusBeaconInstaller\bin\Release\ZentrixLabs.WustatusBeaconInstaller.exe"; DestDir: "{app}"; Flags: ignoreversion uninsneveruninstall
Source: "..\beacon.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Wustatus Beacon"; Filename: "{app}\ZentrixLabs.WustatusBeacon.exe"; IconFilename: "{app}\beacon.ico"; WorkingDir: "{app}"
Name: "{autodesktop}\Wustatus Beacon"; Filename: "{app}\ZentrixLabs.WustatusBeacon.exe"; IconFilename: "{app}\beacon.ico"; WorkingDir: "{app}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "netsh"; \
  Parameters: "http add urlacl url=http://localhost:7341/wustatus/ user=""NT AUTHORITY\SYSTEM"""; \
  Flags: runhidden waituntilterminated

Filename: "netsh"; \
  Parameters: "http add urlacl url=http://localhost:7341/health/ user=""NT AUTHORITY\SYSTEM"""; \
  Flags: runhidden waituntilterminated
  
Filename: "{app}\ZentrixLabs.WustatusBeaconInstaller.exe"; \
  Parameters: "--install ""{app}\ZentrixLabs.WustatusBeacon.exe"""; \
  Flags: runhidden waituntilterminated; \
  Description: "Install Wustatus Beacon Service"


[UninstallRun]
; Run C++ uninstaller (logs and stops the service)
Filename: "{app}\ZentrixLabs.WustatusBeaconInstaller.exe"; \
  Parameters: "uninstall"; \
  Flags: runhidden waituntilterminated

; Failsafe: forcibly delete the service if needed
Filename: "sc.exe"; Parameters: "delete Wustatus.Beacon"; Flags: runhidden waituntilterminated

; Cleanup the helper EXE — use cmd to avoid Inno race conditions
Filename: "cmd.exe"; Parameters: "/c del /f /q ""{app}\ZentrixLabs.WustatusBeaconInstaller.exe"""; Flags: runhidden

