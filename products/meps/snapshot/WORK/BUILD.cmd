@echo off
setlocal
rem Builds the 0.6.0 configuration of the current WORK sources (MEPS060 compact palette, MEPS060ADV, LIBRARY_0_6). TVXD.Meps.0.1/0.2/0.3/0.3.1/0.4/0.4.1/0.4.2/0.5/0.5.1/0.5.2.dll are preserved
rem (older loaded builds must not be overwritten);
rem 0.1/0.2 sources are Git commits 973bad5 and aa4db11, the 0.3 source is commit 4f8533e, the accepted 0.3.1 source is commit 41bd093, the 0.4.2 source is commit 67329c2,
rem the 0.5.1 source is commit 7ce30d0, the 0.5.2 source is commit 76dcb13.
rem TVXD-MEPS-LAYERS-012: Plugin.cs needs LegendPlace011.cs since task 011 (added below). The accepted TVXD.Meps.0.6.0.dll is never overwritten: give a NEW
rem output file name as the first argument (default OUTPUT\TVXD.Meps.0.6.0.dev012.dll); an existing output is refused. The current candidate with layer
rem standards and wires is built by TESTS\BUILD012.ps1 (defines WIRES010;STANDARDS012), not here.
rem A DLL already NETLOADed in AutoCAD is locked; build before any live load.
set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set "CAD=C:\Program Files\Autodesk\AutoCAD 2021"
set "OUT=%~1"
if "%OUT%"=="" set "OUT=%~dp0..\OUTPUT\TVXD.Meps.0.6.0.dev012.dll"
if exist "%OUT%" (
 echo REFUSED: %OUT% already exists; compiled outputs are never overwritten. Pass a new file name.
 exit /b 2
)
"%CSC%" /nologo /codepage:65001 /target:library /r:System.Web.Extensions.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /r:"%CAD%\AcMgd.dll" /r:"%CAD%\AcDbMgd.dll" /r:"%CAD%\AcCoreMgd.dll" /r:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll" /r:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll" /out:"%OUT%" "%~dp0Core.cs" "%~dp0Catalog.cs" "%~dp0Library.cs" "%~dp0Legend.cs" "%~dp0Meps06.cs" "%~dp0LibraryView.cs" "%~dp0CompactLibraryView.cs" "%~dp0CompactView05.cs" "%~dp0CompactView06.cs" "%~dp0PreflightView06.cs" "%~dp0LibraryCad.cs" "%~dp0LibraryCad06.cs" "%~dp0PlaceJig06.cs" "%~dp0LibraryStyle.cs" "%~dp0LegendCad.cs" "%~dp0LegendPlace011.cs" "%~dp0Panel.cs" "%~dp0Plugin.cs"
exit /b %errorlevel%
