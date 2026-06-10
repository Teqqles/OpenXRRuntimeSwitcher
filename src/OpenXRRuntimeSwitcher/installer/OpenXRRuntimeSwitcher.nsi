!define APP_NAME "OpenXR Runtime Switcher"
!define APP_EXE "OpenXRRuntimeSwitcher.exe"
!define APP_DIR "OpenXRRuntimeSwitcher"
!define TASK_NAME "OpenXRRuntimeSwitcher_AutoStart"
!define REG_UNINSTALL "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\${APP_NAME}"

; Version can be passed from command line via /DVERSION=x.y.z
!ifndef VERSION
  !define VERSION "0.0.0"
!endif

OutFile "..\..\..\publish\OpenXRRuntimeSwitcher-Setup-${VERSION}.exe"
Name "${APP_NAME} ${VERSION}"
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
  File /r "..\..\..\publish\*.*"

  WriteRegStr HKLM "${REG_UNINSTALL}" "DisplayName" "${APP_NAME}"
  WriteRegStr HKLM "${REG_UNINSTALL}" "DisplayVersion" "${VERSION}"
  WriteRegStr HKLM "${REG_UNINSTALL}" "UninstallString" '"$INSTDIR\\uninstall.exe"'
  WriteRegStr HKLM "${REG_UNINSTALL}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKLM "${REG_UNINSTALL}" "DisplayIcon" '"$INSTDIR\\${APP_EXE}"'
  WriteRegStr HKLM "${REG_UNINSTALL}" "Publisher" "OpenXR Runtime Switcher Project"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "NoModify" 1
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "NoRepair" 1

  WriteUninstaller "$INSTDIR\\uninstall.exe"

  CreateDirectory "$SMPROGRAMS\\${APP_NAME}"
  CreateShortCut "$SMPROGRAMS\\${APP_NAME}\\${APP_NAME}.lnk" "$INSTDIR\\${APP_EXE}"
  CreateShortCut "$DESKTOP\\${APP_NAME}.lnk" "$INSTDIR\\${APP_EXE}"

  ${If} $RunAtStartup == "1"
    DetailPrint "Creating startup task via schtasks..."
    nsExec::ExecToLog 'schtasks /Create /TN "${TASK_NAME}" /TR "\"$INSTDIR\\${APP_EXE}\"" /SC ONLOGON /RL HIGHEST /F'
    Pop $0
    ${If} $0 != 0
      DetailPrint "Warning: Failed to create startup task (error $0). You can enable this later from the application."
    ${EndIf}
  ${EndIf}
SectionEnd

Section "Uninstall"
  ; Remove startup task if it exists
  DetailPrint "Removing startup task..."
  nsExec::ExecToLog 'schtasks /Delete /TN "${TASK_NAME}" /F'

  ; Remove shortcuts
  Delete "$DESKTOP\\${APP_NAME}.lnk"
  RMDir /r "$SMPROGRAMS\\${APP_NAME}"

  ; Remove registry entries
  DeleteRegKey HKLM "${REG_UNINSTALL}"

  ; Remove installation directory
  RMDir /r "$INSTDIR"
SectionEnd
