$src = "D:\TuanPa_AI\products\meps\release\TVXD.BG_V4"
$out23 = "D:\TuanPa_AI\OUTPUT\23. TVXD.BG V23"
$out22 = "D:\TuanPa_AI\OUTPUT\22. TVXD.BG V22"
$outOneDrive23 = "D:\OneDrive\TuanPa_AI\OUTPUT\23. TVXD.BG V23"
$outOneDrive22 = "D:\OneDrive\TuanPa_AI\OUTPUT\22. TVXD.BG V22"
$appDataDll = "$env:LOCALAPPDATA\TVXD\MEPS060\TVXD.Meps.dll"

# Create V23 directory
if (!(Test-Path $out23)) { New-Item -ItemType Directory -Force -Path $out23 | Out-Null }
Copy-Item -Path "$src\*" -Destination $out23 -Recurse -Force

# Copy extra tools
if (Test-Path "D:\TuanPa_AI\products\meps\src\bkl_cap.lsp") {
    Copy-Item "D:\TuanPa_AI\products\meps\src\bkl_cap.lsp" "$out23\bkl_cap.lsp" -Force
}
if (Test-Path "D:\TuanPa_AI\INPUT\BKL CAP DIEN\BKL_CAP_DIEN.xlsx") {
    Copy-Item "D:\TuanPa_AI\INPUT\BKL CAP DIEN\BKL_CAP_DIEN.xlsx" "$out23\BKL_CAP_DIEN.xlsx" -Force
}

# Update V22 directory as well
if (Test-Path $out22) {
    Copy-Item -Path "$src\*" -Destination $out22 -Recurse -Force
}

# OneDrive
if (Test-Path "D:\OneDrive\TuanPa_AI\OUTPUT") {
    if (!(Test-Path $outOneDrive23)) { New-Item -ItemType Directory -Force -Path $outOneDrive23 | Out-Null }
    Copy-Item -Path "$out23\*" -Destination $outOneDrive23 -Recurse -Force

    if (Test-Path $outOneDrive22) {
        Copy-Item -Path "$out22\*" -Destination $outOneDrive22 -Recurse -Force
    }
}

# Directly update AutoCAD LocalAppData
if (Test-Path (Split-Path $appDataDll)) {
    Copy-Item -Path "$src\MEPS060\TVXD.Meps.dll" -Destination $appDataDll -Force
    Write-Host "Directly updated $appDataDll"
}

Write-Host "Verifying hashes:"
$hSrc = (Get-FileHash -Algorithm SHA256 "$src\MEPS060\TVXD.Meps.dll").Hash
$hOut23 = (Get-FileHash -Algorithm SHA256 "$out23\MEPS060\TVXD.Meps.dll").Hash
$hApp = (Get-FileHash -Algorithm SHA256 $appDataDll).Hash

Write-Host "SRC:     $hSrc"
Write-Host "OUT23:   $hOut23"
Write-Host "APPDATA: $hApp"

if ($hSrc -eq $hOut23 -and $hSrc -eq $hApp) {
    Write-Host "SUCCESS: V23 Deployment hashes match identically!"
} else {
    Write-Host "WARNING: Hash mismatch detected!"
}
