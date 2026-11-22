<#
.SYNOPSIS
  Safely watches a task branch and validates each new commit.

.DESCRIPTION
  This runner is intentionally a coordinator, not an AI agent. It never merges,
  deploys, overwrites a dirty worktree, or marks a task APPROVED. It fast-forwards
  only a clean local checkout, runs repository checks, and appends a handoff log.
  Stop it by creating the configured stop file or pressing Ctrl+C.
#>
[CmdletBinding()]
param(
  [string]$RepoPath = (Join-Path $PSScriptRoot 'worktree'),
  [string]$Branch = 'task/TASK-0016-fix-toado-vertical-horizontal',
  [int]$IntervalSeconds = 60,
  [string]$StopFile = (Join-Path $PSScriptRoot 'watch_and_sync.stop'),
  [switch]$Once
)

$ErrorActionPreference = 'Stop'
$remote = 'https://github.com/anhtuantvtkd-ui/TuanPa_AI.git'
$lockPath = Join-Path $RepoPath '.watch_and_sync.lock'
$logPath = Join-Path $RepoPath 'COMMUNICATION.md'

function Write-Log([string]$Message) {
  $line = "[{0}] watch_and_sync: {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz'), $Message
  Write-Host $line
  if (Test-Path $logPath) { Add-Content -LiteralPath $logPath -Value ("`r`n- " + $line) }
}

function Invoke-Git([string[]]$Args) {
  $output = & git @Args 2>&1
  if ($LASTEXITCODE -ne 0) { throw (($output | Out-String).Trim()) }
  return ($output | Out-String).Trim()
}

if (Test-Path $lockPath) { throw "Another runner appears active: $lockPath" }
New-Item -ItemType Directory -Force -Path $RepoPath | Out-Null
New-Item -ItemType File -Force -Path $lockPath | Out-Null
try {
  if (-not (Test-Path (Join-Path $RepoPath '.git'))) { Invoke-Git @('clone','--branch',$Branch,'--single-branch',$remote,$RepoPath) | Out-Null }
  Set-Location -LiteralPath $RepoPath
  if ((Invoke-Git @('status','--porcelain'))) { throw 'Local worktree is dirty; refusing to pull or test.' }
  do {
    if (Test-Path $StopFile) { Write-Log 'Stop file detected; exiting.'; break }
    Invoke-Git @('fetch','origin',$Branch) | Out-Null
    $remoteSha = Invoke-Git @('rev-parse',"origin/$Branch")
    $localSha = Invoke-Git @('rev-parse','HEAD')
    if ($remoteSha -ne $localSha) {
      Invoke-Git @('merge','--ff-only',"origin/$Branch") | Out-Null
      Write-Log "Advanced to $remoteSha."
      try {
        & node workflow-check.mjs; if ($LASTEXITCODE -ne 0) { throw 'workflow-check.mjs failed.' }
        & npm test; if ($LASTEXITCODE -ne 0) { throw 'npm test failed.' }
        & npm run type-check; if ($LASTEXITCODE -ne 0) { throw 'type-check failed.' }
        Write-Log "Checks PASS for $remoteSha. Native CAD build/test was not run by this coordinator."
      } catch { Write-Log "Checks BLOCKED for $remoteSha: $($_.Exception.Message)" }
    } elseif ($Once) { Write-Log "Already current at $localSha."; break }
    if (-not $Once) { Start-Sleep -Seconds $IntervalSeconds }
  } while ($true)
} finally { Remove-Item -LiteralPath $lockPath -Force -ErrorAction SilentlyContinue }
