$src = "D:\TuanPa_AI\products\meps\release\TVXD.BG_V4"
$out1 = "D:\TuanPa_AI\OUTPUT\21. TVXD.BG V21"
$out2 = "D:\OneDrive\TuanPa_AI\OUTPUT\21. TVXD.BG V21"
$appDataDll = "$env:LOCALAPPDATA\TVXD\MEPS060\TVXD.Meps.dll"

if (!(Test-Path $out1)) { New-Item -ItemType Directory -Force -Path $out1 | Out-Null }
Copy-Item -Path "$src\*" -Destination $out1 -Recurse -Force
Write-Host "Copied to $out1"

if (Test-Path "D:\OneDrive\TuanPa_AI\OUTPUT") {
    if (!(Test-Path $out2)) { New-Item -ItemType Directory -Force -Path $out2 | Out-Null }
    Copy-Item -Path "$src\*" -Destination $out2 -Recurse -Force
    Write-Host "Copied to $out2"
}

if (Test-Path (Split-Path $appDataDll)) {
    Copy-Item -Path "$src\MEPS060\TVXD.Meps.dll" -Destination $appDataDll -Force
    Write-Host "Directly updated $appDataDll"
}

Write-Host "Verifying hashes:"
$hSrc = (Get-FileHash -Algorithm SHA256 "$src\MEPS060\TVXD.Meps.dll").Hash
$hOut1 = (Get-FileHash -Algorithm SHA256 "$out1\MEPS060\TVXD.Meps.dll").Hash
$hOut2 = (Get-FileHash -Algorithm SHA256 "$out2\MEPS060\TVXD.Meps.dll").Hash
$hApp = (Get-FileHash -Algorithm SHA256 $appDataDll).Hash

Write-Host "SRC:     $hSrc"
Write-Host "OUT1:    $hOut1"
Write-Host "OUT2:    $hOut2"
Write-Host "APPDATA: $hApp"

if ($hSrc -eq $hOut1 -and $hSrc -eq $hOut2 -and $hSrc -eq $hApp) {
    Write-Host "SUCCESS: All hashes match identically!"
} else {
    Write-Host "WARNING: Hash mismatch detected!"
}
