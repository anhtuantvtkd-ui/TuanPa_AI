# TuanPa AI - Autonomous Git Watcher & Compiler Daemon
$ErrorActionPreference = "Continue"

$git = "C:\Users\ADMIN\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\git\cmd\git.exe"
$repoDir = "D:\TuanPa_AI"
$branch = "task/TASK-0016-fix-toado-vertical-horizontal"
$logFile = "$repoDir\products\meps\tools\watcher.log"

Write-Host "Starting TuanPa Git Watcher on branch '$branch'..."
"Watcher started at $(Get-Date)" | Out-File -FilePath $logFile -Encoding utf8

while ($true) {
    try {
        & $git -C $repoDir fetch origin $branch 2>$null
        $localSha = (& $git -C $repoDir rev-parse HEAD).Trim()
        $remoteSha = (& $git -C $repoDir rev-parse "origin/$branch").Trim()

        if ($localSha -ne $remoteSha) {
            $msg = "Detected new commit from Codex on remote: $remoteSha (Local: $localSha)"
            Write-Host $msg
            "$msg at $(Get-Date)" | Out-File -FilePath $logFile -Append -Encoding utf8

            # Pull changes
            & $git -C $repoDir pull origin $branch

            # Check workflow
            Set-Location $repoDir
            node workflow-check.mjs

            # If C# source changed, rebuild and retest
            $changedFiles = (& $git -C $repoDir diff --name-only "$localSha" "$remoteSha")
            if ($changedFiles -match '\.cs') {
                Write-Host "C# source changed! Rebuilding TVXD.Meps.dll..."
                Stop-Process -Name accoreconsole -Force -ErrorAction SilentlyContinue
                cmd.exe /c "cd /d $repoDir\products\meps\tools && build_v4.cmd"

                # Run accoreconsole automated tests
                & 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /target:library /out:"$repoDir\products\meps\test\TestGeometryToado.dll" /r:'C:\Program Files\Autodesk\AutoCAD 2021\acdbmgd.dll' /r:'C:\Program Files\Autodesk\AutoCAD 2021\acmgd.dll' /r:'C:\Program Files\Autodesk\AutoCAD 2021\accoremgd.dll' /r:"$repoDir\products\meps\release\TVXD.BG_V4\MEPS060\TVXD.Meps.dll" "$repoDir\products\meps\test\TestGeometryToado.cs"
                & 'C:\Program Files\Autodesk\AutoCAD 2021\accoreconsole.exe' /s "$repoDir\products\meps\test\RunTestGeometry.scr" | Out-File -FilePath "$repoDir\products\meps\test\accoreconsole_test_report.txt" -Encoding utf8
                Remove-Item -Path "$repoDir\products\meps\test\TestGeometryToado.dll" -Force -ErrorAction SilentlyContinue

                # Copy to localappdata live path
                Copy-Item -Path "$repoDir\products\meps\release\TVXD.BG_V4\MEPS060\TVXD.Meps.dll" -Destination "$env:LOCALAPPDATA\TVXD\MEPS060\TVXD.Meps.dll" -Force

                # Commit and push results back
                & $git -C $repoDir add products/meps/test/accoreconsole_test_report.txt products/meps/release/TVXD.BG_V4/
                & $git -C $repoDir commit -m "ci(autobuild): rebuild and test accoreconsole for commit $remoteSha"
                & $git -C $repoDir push origin $branch
                Write-Host "Auto-build and verification pushed to GitHub!"
            }
        }
    } catch {
        Write-Host "Watcher iteration error: $_"
    }

    Start-Sleep -Seconds 20
}
