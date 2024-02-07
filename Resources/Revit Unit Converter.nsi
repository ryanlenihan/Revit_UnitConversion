;NSIS Modern User Interface
;Welcome/Finish Page Example Script
;Written by Joost Verburg

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Define name of the product
  !define PRODUCT "Revit Unit Conversion Tool"
  
  ;Name and file
  
  ;Define the main name of the installer
  Name "${PRODUCT}"
  
  OutFile "${PRODUCT} Setup.exe"
  Unicode True

  ;Default installation folder
  InstallDir "$APPDATA\Autodesk\Revit\Addins\"

  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\${PRODUCT}" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING
  
  ;Use optional a custom picture for the 'Welcome' and 'Finish' page:
  !define MUI_HEADERIMAGE_RIGHT
  !define MUI_WELCOMEFINISHPAGE_BITMAP "${NSISDIR}\Docs\Modern UI\images\rau.bmp"  # for the Installer
  !define MUI_UNWELCOMEFINISHPAGE_BITMAP "${NSISDIR}\Docs\Modern UI\images\rau.bmp"  # for the later created UnInstaller

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE ".\License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  ;!insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH

  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Revit 2024" Revit2024

  SectionIn RO # Just means if in component mode this is locked

  ;Set output path to the installation directory.
  SetOutPath $INSTDIR\2024

  ;Put the following file in the SetOutPath
  File ".\2024\UnitsConverter.dll"
  File ".\2024\UnitsConverter.addin"

  ;Store installation folder in registry
  WriteRegStr HKLM "Software\${PRODUCT}" "" $INSTDIR\2024

  ;Registry information for add/remove programs
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT}" "DisplayName" "${PRODUCT}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT}" "UninstallString" '"$INSTDIR\${PRODUCT}_uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT}" "NoRepair" 1

  ;Create uninstaller and Main component
  
  ;Create uninstaller
  WriteUninstaller "${PRODUCT}_uninstaller.exe"

SectionEnd

Section "Revit 2023" Revit2023

  ; Save something else optional to the installation directory.
  SetOutPath $INSTDIR\2023

  ;Put the following file in the SetOutPath
  File ".\2023\UnitsConverter.dll"
  File ".\2023\UnitsConverter.addin"


SectionEnd

Section "Revit 2022" Revit2022

  ; Save something else optional to the installation directory.
  SetOutPath $INSTDIR\2022

  ;Put the following file in the SetOutPath
  File ".\2022\UnitsConverter.dll"
  File ".\2022\UnitsConverter.addin"


SectionEnd



;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_Revit2024 ${LANG_ENGLISH} "Revit 2024 metric and imperial conversion component. Includes both current file and batch processor."
  LangString DESC_Revit2023 ${LANG_ENGLISH} "Revit 2023 metric and imperial conversion component. Includes both current file and batch processor."
  LangString DESC_Revit2022 ${LANG_ENGLISH} "Revit 2022 metric and imperial conversion component. Includes both current file and batch processor."
  

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${Revit2024} $(DESC_Revit2024)
	!insertmacro MUI_DESCRIPTION_TEXT ${Revit2023} $(DESC_Revit2023)
	!insertmacro MUI_DESCRIPTION_TEXT ${Revit2022} $(DESC_Revit2022)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ;Remove all registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT}"
  DeleteRegKey HKLM "Software\${PRODUCT}"

  ;Delete the installation directory + all files in it
  ;Add 'RMDir /r "$INSTDIR\folder\*.*"' for every folder you have added additionaly
  Delete "$INSTDIR\2024\UnitsConverter.dll"
  Delete "$INSTDIR\2024\UnitsConverter.addin"
  Delete "$INSTDIR\2023\UnitsConverter.dll"
  Delete "$INSTDIR\2023\UnitsConverter.addin"
  Delete "$INSTDIR\2022\UnitsConverter.dll"
  Delete "$INSTDIR\2022\UnitsConverter.addin"
  
  ;Delete Start Menu Shortcuts
  Delete "$SMPROGRAMS\${PRODUCT}\*.*"
  RmDir  "$SMPROGRAMS\${PRODUCT}"

SectionEnd