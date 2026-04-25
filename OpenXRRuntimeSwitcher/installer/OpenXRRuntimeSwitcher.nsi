!define APP_NAME "OpenXR Runtime Switcher"
!define APP_EXE "OpenXRRuntimeSwitcher.exe"
!define APP_DIR "OpenXRRuntimeSwitcher"
!define REG_UNINSTALL "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\${APP_NAME}"

OutFile "OpenXRRuntimeSwitcher-Setup.exe"
InstallDir "$PROGRAMFILES64\\${APP_DIR}"
RequestExecutionLevel admin
ShowInstDetails show
ShowUninstDetails show

!include "MUI2.nsh"
!include "nsDialogs.nsh"
!include "LogicLib.nsh"

!define MUI_COMPONENTSPAGE_NODESC
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
Page custom StartupPageCreate StartupPageLeave
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

Var HWND_RUNATSTARTUP
Var RunAtStartup

Function StartupPageCreate
  nsDialogs::Create 1018
  Pop $0
  ${NSD_CreateCheckbox} 0 0 100% 12u "Run ${APP_NAME} at Windows startup"
  Pop $HWND_RUNATSTARTUP
  ${NSD_Check} $HWND_RUNATSTARTUP
  nsDialogs::Show
FunctionEnd

Function StartupPageLeave
  ${NSD_GetState} $HWND_RUNATSTARTUP $RunAtStartup
FunctionEnd

Section "MainSection" SEC_MAIN
  SetOutPath "$INSTDIR"
  File /r "publish\\*.*"

  WriteRegStr HKLM "${REG_UNINSTALL}" "DisplayName" "${APP_NAME}"
  WriteRegStr HKLM "${REG_UNINSTALL}" "UninstallString" '"$INSTDIR\\uninstall.exe"'
  WriteRegStr HKLM "${REG_UNINSTALL}" "InstallLocation" "$INSTDIR"

  WriteUninstaller "$INSTDIR\\uninstall.exe"

  CreateDirectory "$SMPROGRAMS\\${APP_NAME}"
  CreateShortCut "$SMPROGRAMS\\${APP_NAME}\\${APP_NAME}.lnk" "$INSTDIR\\${APP_EXE}"
  CreateShortCut "$DESKTOP\\${APP_NAME}.lnk" "$INSTDIR\\${APP_EXE}"

  ${If} $RunAtStartup == "1"
    WriteRegStr HKLM "Software\\Microsoft\\Windows\\CurrentVersion\\Run" "${APP_NAME}" '"$INSTDIR\\${APP_EXE}"'
  ${EndIf}
SectionEnd

Section "Uninstall"
  Delete "$DESKTOP\\${APP_NAME}.lnk"
  RMDir /r "$SMPROGRAMS\\${APP_NAME}"
  DeleteRegKey HKLM "${REG_UNINSTALL}"
  DeleteRegValue HKLM "Software\\Microsoft\\Windows\\CurrentVersion\\Run" "${APP_NAME}"
  RMDir /r "$INSTDIR"
SectionEnd
