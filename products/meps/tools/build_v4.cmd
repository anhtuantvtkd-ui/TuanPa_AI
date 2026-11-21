@echo off
setlocal enabledelayedexpansion

echo ========================================================
echo       BUILD TVXD.MEPS V4 - AUTOMATIC COMPILER
echo ========================================================

set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set "ILASM=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ilasm.exe"
set "ILDASM=C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\ildasm.exe"
set "ACAD_DIR=C:\Program Files\Autodesk\AutoCAD 2021"

set "TOOLS_DIR=%~dp0"
set "SRC_TOADO=%TOOLS_DIR%..\src\Toado\ToadoPart.cs"
set "BASE_IL=%TOOLS_DIR%base\meps_orig.il"
set "BASE_RES=%TOOLS_DIR%base\meps_orig.res"
set "RELEASE_DLL=%TOOLS_DIR%..\release\TVXD.BG_V4\MEPS060\TVXD.Meps.dll"

echo [1/5] Compiling ToadoPart.cs...
"%CSC%" /target:library /out:"%TOOLS_DIR%ToadoPart.dll" /r:"%ACAD_DIR%\acdbmgd.dll" /r:"%ACAD_DIR%\acmgd.dll" /r:"%ACAD_DIR%\accoremgd.dll" /r:System.Windows.Forms.dll /r:System.Drawing.dll "%SRC_TOADO%"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to compile ToadoPart.cs
    exit /b 1
)

echo [2/5] Disassembling ToadoPart.dll...
"%ILDASM%" /out:"%TOOLS_DIR%ToadoPart.il" "%TOOLS_DIR%ToadoPart.dll"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to disassemble ToadoPart.dll
    exit /b 1
)

echo [3/5] Merging Toado into Base IL...
"%CSC%" /out:"%TOOLS_DIR%merge_toado.exe" "%TOOLS_DIR%merge_toado.cs"
"%TOOLS_DIR%merge_toado.exe"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to merge IL
    exit /b 1
)

echo [4/5] Assembling final TVXD.Meps.dll...
"%ILASM%" /dll /quiet /res:"%BASE_RES%" /out:"%TOOLS_DIR%TVXD.Meps.Merged.dll" "%TOOLS_DIR%meps_final.il"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to assemble TVXD.Meps.Merged.dll
    exit /b 1
)

copy /y "%TOOLS_DIR%TVXD.Meps.Merged.dll" "%RELEASE_DLL%" >nul

echo [5/5] Updating release SHA256 checksums...
"%CSC%" /out:"%TOOLS_DIR%update_v4.exe" "%TOOLS_DIR%update_v4.cs"
"%TOOLS_DIR%update_v4.exe"

REM Cleanup temporary files
del /q "%TOOLS_DIR%ToadoPart.dll" "%TOOLS_DIR%ToadoPart.il" "%TOOLS_DIR%ToadoPart.res" 2>nul
del /q "%TOOLS_DIR%merge_toado.exe" "%TOOLS_DIR%update_v4.exe" 2>nul
del /q "%TOOLS_DIR%TVXD.Meps.Merged.dll" "%TOOLS_DIR%meps_final.il" 2>nul

echo ========================================================
echo       BUILD V4 COMPLETED SUCCESSFULLY!
echo  Release payload: %RELEASE_DLL%
echo ========================================================
