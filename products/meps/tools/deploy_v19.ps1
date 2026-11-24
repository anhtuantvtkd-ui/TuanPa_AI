$src = "D:\TuanPa_AI\products\meps\release\TVXD.BG_V4"
$dst1 = "D:\TuanPa_AI\OUTPUT\19. TVXD.BG V19"
$dst2 = "D:\OneDrive\TuanPa_AI\OUTPUT\19. TVXD.BG V19"
$appdata = "$env:LOCALAPPDATA\TVXD\MEPS060\TVXD.Meps.dll"

Write-Host "Copying to OUTPUT 19..."
if (Test-Path $dst1) { Remove-Item -Path $dst1 -Recurse -Force }
Copy-Item -Path $src -Destination $dst1 -Recurse -Force

Write-Host "Copying to OneDrive 19..."
if (Test-Path $dst2) { Remove-Item -Path $dst2 -Recurse -Force }
Copy-Item -Path $src -Destination $dst2 -Recurse -Force

Write-Host "Updating LOCALAPPDATA..."
Copy-Item -Path "$src\MEPS060\TVXD.Meps.dll" -Destination $appdata -Force

Write-Host "Verifying hashes:"
Get-FileHash -Algorithm SHA256 "$dst1\MEPS060\TVXD.Meps.dll"
Get-FileHash -Algorithm SHA256 "$dst2\MEPS060\TVXD.Meps.dll"
Get-FileHash -Algorithm SHA256 $appdata
