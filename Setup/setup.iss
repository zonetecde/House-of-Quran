; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "House of Quran"
#define MyAppVersion "1.1"
#define MyAppPublisher "zonetecde"
#define MyAppURL "github.com/zonetecde"
#define MyAppExeName "House of Quran.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{BE8149C4-4ABD-47BB-BEBB-CA0657402ED5}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
OutputDir=E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\Setup
OutputBaseFilename=setup
SetupIconFile=E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\hoq_icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Files]
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\House of Quran.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\House of Quran.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\House of Quran.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\House of Quran.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\House of Quran.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\House of Quran.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\NAudio.Asio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\NAudio.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\NAudio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\NAudio.Midi.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\NAudio.Vorbis.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\NAudio.Wasapi.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\NAudio.WinForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\NAudio.WinMM.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\NVorbis.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\Xceed.Wpf.AvalonDock.NET5.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\Xceed.Wpf.AvalonDock.Themes.Aero.NET5.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\Xceed.Wpf.AvalonDock.Themes.Metro.NET5.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\Xceed.Wpf.AvalonDock.Themes.VS2010.NET5.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\Xceed.Wpf.Toolkit.NET5.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\data\*"; DestDir: "{app}\data"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\fr\*"; DestDir: "{app}\fr"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "E:\Cloud\OneDrive - Conseil r�gional Grand Est - Num�rique Educatif\Programmation\c#\House of Quran\House of Quran\bin\Release\net6.0-windows\fonts\*"; DestDir: "{app}\fonts"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

