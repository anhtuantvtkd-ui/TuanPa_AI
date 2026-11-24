@echo off
setlocal
echo ==============================================================
echo [TVXD] Dang chuan bi cai dat TVXD.BG V21...
tasklist /FI "IMAGENAME eq acad.exe" 2>NUL | find /I /N "acad.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo [THONG BAO] Phat hien tien trinh AutoCAD dang mo hoac chay ngam.
    echo Dang tu dong tat tien trinh AutoCAD de cap nhat file he thong...
    taskkill /F /IM acad.exe >NUL 2>&1
    timeout /t 1 /nobreak >NUL
)
tasklist /FI "IMAGENAME eq accoreconsole.exe" 2>NUL | find /I /N "accoreconsole.exe">NUL
if "%ERRORLEVEL%"=="0" (
    taskkill /F /IM accoreconsole.exe >NUL 2>&1
    timeout /t 1 /nobreak >NUL
)
"%~dp0CAI_DAT.exe" %*
exit /b %ERRORLEVEL%
