############################################################################################
#      NSIS Installation Script created by NSIS Quick Setup Script Generator v1.09.18
#               Entirely Edited with NullSoft Scriptable Installation System                
#              by Vlasis K. Barkas aka Red Wine red_wine@freemail.gr Sep 2006               
############################################################################################

!define APP_NAME "OS Game-Launcher"
!define COMP_NAME "Matix Media, Inc."
!define WEB_SITE "https://os-game-launcher.matix-media.net"
!define VERSION "0.1.1.16"
!define COPYRIGHT "Matix Media © 2020"
!define DESCRIPTION "Application"
!define LICENSE_TXT "C:\Users\user\Documents\Licenses\Matix Media.txt"
!define INSTALLER_NAME "C:\Users\user\source\repos\OS Game-Launcher\bin\Setups\os-gamelauncher_v01116-alpha.exe"
!define MAIN_APP_EXE "OS Game-Launcher.exe"
!define INSTALL_TYPE "SetShellVarContext current"
!define REG_ROOT "HKCU"
!define REG_APP_PATH "Software\Microsoft\Windows\CurrentVersion\App Paths\${MAIN_APP_EXE}"
!define UNINSTALL_PATH "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"

######################################################################

VIProductVersion  "${VERSION}"
VIAddVersionKey "ProductName"  "${APP_NAME}"
VIAddVersionKey "CompanyName"  "${COMP_NAME}"
VIAddVersionKey "LegalCopyright"  "${COPYRIGHT}"
VIAddVersionKey "FileDescription"  "${DESCRIPTION}"
VIAddVersionKey "FileVersion"  "${VERSION}"

######################################################################

SetCompressor ZLIB
Name "${APP_NAME}"
Caption "${APP_NAME}"
OutFile "${INSTALLER_NAME}"
BrandingText "${APP_NAME}"
XPStyle on
InstallDirRegKey "${REG_ROOT}" "${REG_APP_PATH}" ""
InstallDir "$PROGRAMFILES\OS Game-Launcher"

######################################################################

!include "MUI.nsh"

!define MUI_ABORTWARNING
!define MUI_UNABORTWARNING

!insertmacro MUI_PAGE_WELCOME

!ifdef LICENSE_TXT
!insertmacro MUI_PAGE_LICENSE "${LICENSE_TXT}"
!endif

!ifdef REG_START_MENU
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "OS Game-Launcher"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT "${REG_ROOT}"
!define MUI_STARTMENUPAGE_REGISTRY_KEY "${UNINSTALL_PATH}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "${REG_START_MENU}"
!insertmacro MUI_PAGE_STARTMENU Application $SM_Folder
!endif

!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_RUN "$INSTDIR\${MAIN_APP_EXE}"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM

!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

######################################################################

Section -MainProgram
${INSTALL_TYPE}
SetOverwrite ifnewer
SetOutPath "$INSTDIR"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\CommandLine.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\CommandLine.xml"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\ControlzEx.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\ControlzEx.pdb"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\DotNetZip.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\DotNetZip.pdb"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\DotNetZip.xml"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\MahApps.Metro.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\MahApps.Metro.pdb"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\MahApps.Metro.xml"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\Microsoft.WindowsAPICodePack.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\Microsoft.WindowsAPICodePack.Shell.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\Microsoft.WindowsAPICodePack.Shell.xml"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\Microsoft.WindowsAPICodePack.xml"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\Newtonsoft.Json.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\Newtonsoft.Json.xml"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\OS Game-Launcher.application"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\OS Game-Launcher.exe"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\OS Game-Launcher.exe.config"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\OS Game-Launcher.exe.manifest"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\OS Game-Launcher.pdb"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\RegisterRegistry.exe"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\RegisterRegistry.exe.config"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\RegisterRegistry.pdb"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\RestSharp.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\RestSharp.xml"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\System.Windows.Interactivity.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\ToggleSwitch.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\XamlAnimatedGif.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\Xceed.Wpf.AvalonDock.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\Xceed.Wpf.AvalonDock.Themes.Aero.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\Xceed.Wpf.AvalonDock.Themes.Metro.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\Xceed.Wpf.AvalonDock.Themes.VS2010.dll"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\Xceed.Wpf.Toolkit.dll"
SetOutPath "$INSTDIR\zh-Hans"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\zh-Hans\Xceed.Wpf.AvalonDock.resources.dll"
SetOutPath "$INSTDIR\sv"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\sv\Xceed.Wpf.AvalonDock.resources.dll"
SetOutPath "$INSTDIR\ru"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\ru\Xceed.Wpf.AvalonDock.resources.dll"
SetOutPath "$INSTDIR\ro"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\ro\Xceed.Wpf.AvalonDock.resources.dll"
SetOutPath "$INSTDIR\pt-BR"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\pt-BR\Xceed.Wpf.AvalonDock.resources.dll"
SetOutPath "$INSTDIR\it"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\it\Xceed.Wpf.AvalonDock.resources.dll"
SetOutPath "$INSTDIR\hu"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\hu\Xceed.Wpf.AvalonDock.resources.dll"
SetOutPath "$INSTDIR\fr"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\fr\Xceed.Wpf.AvalonDock.resources.dll"
SetOutPath "$INSTDIR\es"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\es\Xceed.Wpf.AvalonDock.resources.dll"
SetOutPath "$INSTDIR\de"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\de\Xceed.Wpf.AvalonDock.resources.dll"
SetOutPath "$INSTDIR\cs-CZ"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\cs-CZ\Xceed.Wpf.AvalonDock.resources.dll"
SetOutPath "$INSTDIR\app.publish"
File "C:\Users\user\source\repos\OS Game-Launcher\bin\Release\app.publish\OS Game-Launcher.exe"
SectionEnd

######################################################################

Section -Icons_Reg
SetOutPath "$INSTDIR"
WriteUninstaller "$INSTDIR\uninstall.exe"

!ifdef REG_START_MENU
!insertmacro MUI_STARTMENU_WRITE_BEGIN Application
CreateDirectory "$SMPROGRAMS\$SM_Folder"
CreateShortCut "$SMPROGRAMS\$SM_Folder\${APP_NAME}.lnk" "$INSTDIR\${MAIN_APP_EXE}"
CreateShortCut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${MAIN_APP_EXE}"
!ifdef WEB_SITE
WriteIniStr "$INSTDIR\${APP_NAME} website.url" "InternetShortcut" "URL" "${WEB_SITE}"
CreateShortCut "$SMPROGRAMS\$SM_Folder\${APP_NAME} Website.lnk" "$INSTDIR\${APP_NAME} website.url"
!endif
!insertmacro MUI_STARTMENU_WRITE_END
!endif

!ifndef REG_START_MENU
CreateDirectory "$SMPROGRAMS\OS Game-Launcher"
CreateShortCut "$SMPROGRAMS\OS Game-Launcher\${APP_NAME}.lnk" "$INSTDIR\${MAIN_APP_EXE}"
CreateShortCut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${MAIN_APP_EXE}"
!ifdef WEB_SITE
WriteIniStr "$INSTDIR\${APP_NAME} website.url" "InternetShortcut" "URL" "${WEB_SITE}"
CreateShortCut "$SMPROGRAMS\OS Game-Launcher\${APP_NAME} Website.lnk" "$INSTDIR\${APP_NAME} website.url"
!endif
!endif

WriteRegStr ${REG_ROOT} "${REG_APP_PATH}" "" "$INSTDIR\${MAIN_APP_EXE}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "DisplayName" "${APP_NAME}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "UninstallString" "$INSTDIR\uninstall.exe"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "DisplayIcon" "$INSTDIR\${MAIN_APP_EXE}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "DisplayVersion" "${VERSION}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "Publisher" "${COMP_NAME}"

!ifdef WEB_SITE
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "URLInfoAbout" "${WEB_SITE}"
!endif
SectionEnd

######################################################################

Section Uninstall
${INSTALL_TYPE}
Delete "$INSTDIR\CommandLine.dll"
Delete "$INSTDIR\CommandLine.xml"
Delete "$INSTDIR\ControlzEx.dll"
Delete "$INSTDIR\ControlzEx.pdb"
Delete "$INSTDIR\DotNetZip.dll"
Delete "$INSTDIR\DotNetZip.pdb"
Delete "$INSTDIR\DotNetZip.xml"
Delete "$INSTDIR\MahApps.Metro.dll"
Delete "$INSTDIR\MahApps.Metro.pdb"
Delete "$INSTDIR\MahApps.Metro.xml"
Delete "$INSTDIR\Microsoft.WindowsAPICodePack.dll"
Delete "$INSTDIR\Microsoft.WindowsAPICodePack.Shell.dll"
Delete "$INSTDIR\Microsoft.WindowsAPICodePack.Shell.xml"
Delete "$INSTDIR\Microsoft.WindowsAPICodePack.xml"
Delete "$INSTDIR\Newtonsoft.Json.dll"
Delete "$INSTDIR\Newtonsoft.Json.xml"
Delete "$INSTDIR\OS Game-Launcher.application"
Delete "$INSTDIR\OS Game-Launcher.exe"
Delete "$INSTDIR\OS Game-Launcher.exe.config"
Delete "$INSTDIR\OS Game-Launcher.exe.manifest"
Delete "$INSTDIR\OS Game-Launcher.pdb"
Delete "$INSTDIR\RegisterRegistry.exe"
Delete "$INSTDIR\RegisterRegistry.exe.config"
Delete "$INSTDIR\RegisterRegistry.pdb"
Delete "$INSTDIR\RestSharp.dll"
Delete "$INSTDIR\RestSharp.xml"
Delete "$INSTDIR\System.Windows.Interactivity.dll"
Delete "$INSTDIR\ToggleSwitch.dll"
Delete "$INSTDIR\XamlAnimatedGif.dll"
Delete "$INSTDIR\Xceed.Wpf.AvalonDock.dll"
Delete "$INSTDIR\Xceed.Wpf.AvalonDock.Themes.Aero.dll"
Delete "$INSTDIR\Xceed.Wpf.AvalonDock.Themes.Metro.dll"
Delete "$INSTDIR\Xceed.Wpf.AvalonDock.Themes.VS2010.dll"
Delete "$INSTDIR\Xceed.Wpf.Toolkit.dll"
Delete "$INSTDIR\zh-Hans\Xceed.Wpf.AvalonDock.resources.dll"
Delete "$INSTDIR\sv\Xceed.Wpf.AvalonDock.resources.dll"
Delete "$INSTDIR\ru\Xceed.Wpf.AvalonDock.resources.dll"
Delete "$INSTDIR\ro\Xceed.Wpf.AvalonDock.resources.dll"
Delete "$INSTDIR\pt-BR\Xceed.Wpf.AvalonDock.resources.dll"
Delete "$INSTDIR\it\Xceed.Wpf.AvalonDock.resources.dll"
Delete "$INSTDIR\hu\Xceed.Wpf.AvalonDock.resources.dll"
Delete "$INSTDIR\fr\Xceed.Wpf.AvalonDock.resources.dll"
Delete "$INSTDIR\es\Xceed.Wpf.AvalonDock.resources.dll"
Delete "$INSTDIR\de\Xceed.Wpf.AvalonDock.resources.dll"
Delete "$INSTDIR\cs-CZ\Xceed.Wpf.AvalonDock.resources.dll"
Delete "$INSTDIR\app.publish\OS Game-Launcher.exe"
 
RmDir "$INSTDIR\app.publish"
RmDir "$INSTDIR\cs-CZ"
RmDir "$INSTDIR\de"
RmDir "$INSTDIR\es"
RmDir "$INSTDIR\fr"
RmDir "$INSTDIR\hu"
RmDir "$INSTDIR\it"
RmDir "$INSTDIR\pt-BR"
RmDir "$INSTDIR\ro"
RmDir "$INSTDIR\ru"
RmDir "$INSTDIR\sv"
RmDir "$INSTDIR\zh-Hans"
 
Delete "$INSTDIR\uninstall.exe"
!ifdef WEB_SITE
Delete "$INSTDIR\${APP_NAME} website.url"
!endif

RmDir "$INSTDIR"

!ifdef REG_START_MENU
!insertmacro MUI_STARTMENU_GETFOLDER "Application" $SM_Folder
Delete "$SMPROGRAMS\$SM_Folder\${APP_NAME}.lnk"
!ifdef WEB_SITE
Delete "$SMPROGRAMS\$SM_Folder\${APP_NAME} Website.lnk"
!endif
Delete "$DESKTOP\${APP_NAME}.lnk"

RmDir "$SMPROGRAMS\$SM_Folder"
!endif

!ifndef REG_START_MENU
Delete "$SMPROGRAMS\OS Game-Launcher\${APP_NAME}.lnk"
!ifdef WEB_SITE
Delete "$SMPROGRAMS\OS Game-Launcher\${APP_NAME} Website.lnk"
!endif
Delete "$DESKTOP\${APP_NAME}.lnk"

RmDir "$SMPROGRAMS\OS Game-Launcher"
!endif

DeleteRegKey ${REG_ROOT} "${REG_APP_PATH}"
DeleteRegKey ${REG_ROOT} "${UNINSTALL_PATH}"
SectionEnd

######################################################################

