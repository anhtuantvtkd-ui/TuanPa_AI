$src = "D:\TuanPa_AI\products\meps\release\TVXD.BG_V4"
$out25 = "D:\TuanPa_AI\OUTPUT\25. TVXD.BG V25"
$outOneDrive25 = "D:\OneDrive\TuanPa_AI\OUTPUT\25. TVXD.BG V25"
$appDataDir = "$env:LOCALAPPDATA\TVXD\MEPS060"

# Deploy base payload to OUTPUT\25. TVXD.BG V25
if (!(Test-Path $out25)) { New-Item -ItemType Directory -Force -Path $out25 | Out-Null }
Copy-Item -Path "$src\*" -Destination $out25 -Recurse -Force -ErrorAction Stop

# Remove backup/log artifacts from $out25 so package is clean
Get-ChildItem -Path $out25 -Recurse -File -Force -Filter "*.bak" | Remove-Item -Force -ErrorAction SilentlyContinue
if (Test-Path (Join-Path $out25 "MEPS060\LOGS_0_6")) {
    Remove-Item -Path (Join-Path $out25 "MEPS060\LOGS_0_6") -Recurse -Force -ErrorAction SilentlyContinue
}

# Copy extra LISP/Excel tools into $out25 before generating SHA256SUMS.json
if (Test-Path "D:\TuanPa_AI\products\meps\src\bkl_cap.lsp") {
    Copy-Item "D:\TuanPa_AI\products\meps\src\bkl_cap.lsp" "$out25\bkl_cap.lsp" -Force
}
if (Test-Path "D:\TuanPa_AI\OUTPUT\24. TVXD.BG V24\bkl_ttll.lsp") {
    Copy-Item "D:\TuanPa_AI\OUTPUT\24. TVXD.BG V24\bkl_ttll.lsp" "$out25\bkl_ttll.lsp" -Force
}
if (Test-Path "D:\TuanPa_AI\INPUT\BKL CAP DIEN\BKL_CAP_DIEN.xlsx") {
    Copy-Item "D:\TuanPa_AI\INPUT\BKL CAP DIEN\BKL_CAP_DIEN.xlsx" "$out25\BKL_CAP_DIEN.xlsx" -Force
}

# Generate SHA256SUMS.json from the final $out25 package
$entries = New-Object System.Collections.Generic.List[object]
Get-ChildItem -Path $out25 -Recurse -File -Force | ForEach-Object {
    $rel = $_.FullName.Substring($out25.Length + 1).Replace('\', '/')
    if ($rel -eq 'SHA256SUMS.json') { return }
    $h = (Get-FileHash -Algorithm SHA256 -LiteralPath $_.FullName).Hash.ToLowerInvariant()
    $entries.Add([ordered]@{ path = $rel; sha256 = $h })
}
$entries = $entries | Sort-Object { $_.path }
$json = ConvertTo-Json -InputObject @($entries) -Compress
[System.IO.File]::WriteAllText((Join-Path $out25 "SHA256SUMS.json"), $json, (New-Object System.Text.UTF8Encoding($false)))
[System.IO.File]::WriteAllText((Join-Path $src "SHA256SUMS.json"), $json, (New-Object System.Text.UTF8Encoding($false)))

# Sync to OneDrive & LocalAppData
if (Test-Path "D:\OneDrive\TuanPa_AI\OUTPUT") {
    if (!(Test-Path $outOneDrive25)) { New-Item -ItemType Directory -Force -Path $outOneDrive25 | Out-Null }
    Copy-Item -Path "$out25\*" -Destination $outOneDrive25 -Recurse -Force -ErrorAction SilentlyContinue
}
if (Test-Path $appDataDir) {
    Copy-Item -Path "$src\MEPS060\*" -Destination $appDataDir -Recurse -Force
    Write-Host "Synced MEPS060 to $appDataDir"
}

# Verify SHA256SUMS.json in $out25 (both hash match and no unlisted files)
$manifest = Get-Content (Join-Path $out25 "SHA256SUMS.json") -Raw -Encoding UTF8 | ConvertFrom-Json
$listed = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
$mismatch = 0
foreach ($item in $manifest) {
    [void]$listed.Add($item.path)
    $fp = Join-Path $out25 ($item.path.Replace('/', '\'))
    if (!(Test-Path $fp)) { Write-Host "MISSING: $($item.path)"; $mismatch++; continue }
    $actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $fp).Hash.ToLowerInvariant()
    if ($actual -ne $item.sha256) { Write-Host "MISMATCH: $($item.path)"; $mismatch++ }
}
$extra = 0
Get-ChildItem -Path $out25 -Recurse -File -Force | ForEach-Object {
    $rel = $_.FullName.Substring($out25.Length + 1).Replace('\', '/')
    if ($rel -ne 'SHA256SUMS.json' -and -not $listed.Contains($rel)) {
        Write-Host "UNLISTED: $rel"
        $extra++
    }
}
if ($mismatch -gt 0 -or $extra -gt 0) {
    Write-Error "FAILED: $mismatch hash mismatches, $extra unlisted files in $out25"
    exit 1
}
$pb = Get-Content (Join-Path $out25 "PHIEN_BAN.json") -Raw -Encoding UTF8 | ConvertFrom-Json
$dllOut = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $out25 "MEPS060\TVXD.Meps.dll")).Hash.ToLowerInvariant()
if ($dllOut -ne $pb.dll_sha256) {
    Write-Error "FAILED: TVXD.Meps.dll ($dllOut) != PHIEN_BAN.json dll_sha256 ($($pb.dll_sha256))"
    exit 1
}
Write-Host "SUCCESS: Verified $($manifest.Count)/$($manifest.Count) hashes (0 unlisted, DLL=$dllOut) in $out25"
