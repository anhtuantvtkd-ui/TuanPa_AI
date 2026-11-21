# Build script for TVXD.Meps.dll
param(
    [string]$OutFile = "C:\Users\ADMIN\.gemini\antigravity\scratch\MEPS_ELECTRICAL\output\TVXD.Meps.dll",
    [string]$CadPath = "C:\Program Files\Autodesk\AutoCAD 2021",
    [string]$CscPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Building TVXD.Meps (.NET 4.8 / AutoCAD 2021)" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$srcRoot = "C:\Users\ADMIN\.gemini\antigravity\scratch\MEPS_ELECTRICAL\src"
$outDir = Split-Path $OutFile -Parent
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }

# Locate all C# files
$csFiles = Get-ChildItem -Path $srcRoot -Filter "*.cs" -Recurse | Select-Object -ExpandProperty FullName
Write-Host "Found $($csFiles.Count) C# source files." -ForegroundColor Green

# Prepare response file for csc
$rspFile = "$outDir\build.rsp"
$defines = @(
    "WIRES010",
    "STANDARDS012",
    "IDENTITY015",
    "CABLES016",
    "WIRECOMMIT018",
    "WIRESCROLL018",
    "MODELLIB019",
    "FLEX022",
    "LIBUPDATE023",
    "LAYOUT030",
    "UI031",
    "CIRCUIT032",
    "METADATA032",
    "TAG033",
    "BDGKL034",
    "ROUTE034",
    "ROOMCAD037"
) -join ";"

$refs = @(
    "/target:library",
    "/codepage:65001",
    "/nologo",
    "/optimize+",
    "/platform:x64",
    "/d:$defines",
    "/out:`"$OutFile`"",
    "/r:System.dll",
    "/r:System.Core.dll",
    "/r:System.Data.dll",
    "/r:System.Drawing.dll",
    "/r:System.Windows.Forms.dll",
    "/r:System.Web.Extensions.dll",
    "/r:System.IO.Compression.dll",
    "/r:System.IO.Compression.FileSystem.dll",
    "/r:System.Xml.dll",
    "/r:System.Xml.Linq.dll",
    "/r:`"$CadPath\AcMgd.dll`"",
    "/r:`"$CadPath\AcDbMgd.dll`"",
    "/r:`"$CadPath\AcCoreMgd.dll`"",
    "/r:`"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll`"",
    "/r:`"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll`""
)

$rspContent = ($refs + ($csFiles | ForEach-Object { "`"$_`"" })) -join "`r`n"
[System.IO.File]::WriteAllText($rspFile, $rspContent, [System.Text.Encoding]::UTF8)

Write-Host "Compiling via csc.exe..." -ForegroundColor Yellow
$proc = Start-Process -FilePath $CscPath -ArgumentList "@`"$rspFile`"" -Wait -NoNewWindow -PassThru

if ($proc.ExitCode -eq 0) {
    Write-Host "Build SUCCESS! Output -> $OutFile" -ForegroundColor Green
    Get-Item $OutFile | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
} else {
    Write-Host "Build FAILED with exit code $($proc.ExitCode)" -ForegroundColor Red
    exit $proc.ExitCode
}
