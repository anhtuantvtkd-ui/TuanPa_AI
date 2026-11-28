$src = "D:\TuanPa_AI\products\meps\release\TVXD.BG_V4"
$out25 = "D:\TuanPa_AI\OUTPUT\25. TVXD.BG V25"
$outOneDrive25 = "D:\OneDrive\TuanPa_AI\OUTPUT\25. TVXD.BG V25"
$appDataDir = "$env:LOCALAPPDATA\TVXD\MEPS060"

if (!(Test-Path $out25)) { New-Item -ItemType Directory -Force -Path $out25 | Out-Null }
Copy-Item -Path "$src\*" -Destination $out25 -Recurse -Force

if (Test-Path "D:\TuanPa_AI\products\meps\src\bkl_cap.lsp") {
    Copy-Item "D:\TuanPa_AI\products\meps\src\bkl_cap.lsp" "$out25\bkl_cap.lsp" -Force
}
if (Test-Path "D:\TuanPa_AI\products\meps\src\bkl_ttll.lsp") {
    Copy-Item "D:\TuanPa_AI\products\meps\src\bkl_ttll.lsp" "$out25\bkl_ttll.lsp" -Force
}
if (Test-Path "D:\TuanPa_AI\INPUT\BKL CAP DIEN\BKL_CAP_DIEN.xlsx") {
    Copy-Item "D:\TuanPa_AI\INPUT\BKL CAP DIEN\BKL_CAP_DIEN.xlsx" "$out25\BKL_CAP_DIEN.xlsx" -Force
}

if (Test-Path "D:\OneDrive\TuanPa_AI\OUTPUT") {
    if (!(Test-Path $outOneDrive25)) { New-Item -ItemType Directory -Force -Path $outOneDrive25 | Out-Null }
    Copy-Item -Path "$out25\*" -Destination $outOneDrive25 -Recurse -Force
}

if (Test-Path $appDataDir) {
    Copy-Item -Path "$src\MEPS060\*" -Destination $appDataDir -Recurse -Force
    Write-Host "Synced MEPS060 (including LIBRARY_0_6, linetypes076, wire-menu.json, WirePattern076.lsp) to $appDataDir"
}

Write-Host "SUCCESS: Deployed V25 to $out25"
