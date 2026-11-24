# Agent handoff loop for TuanPa_AI
[CmdletBinding()]
param(
  [string]$Repo = 'D:\TuanPa_AI',
  [string]$Branch = 'task/TASK-0016-fix-toado-vertical-horizontal',
  [int]$PollSeconds = 30,
  [switch]$Once
)
$ErrorActionPreference = 'Stop'

if (!(Get-Command git -ErrorAction SilentlyContinue)) {
  $knownGit = "C:\Users\ADMIN\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\git\cmd"
  if (Test-Path $knownGit) { $env:PATH = "$knownGit;$env:PATH" }
}

$log = Join-Path $Repo 'agent-loop.log'
$lock = Join-Path $Repo '.agent-loop.lock'
function Log([string]$m) { "$(Get-Date -Format o) $m" | Add-Content -LiteralPath $log }
if (!(Test-Path -LiteralPath $Repo)) { throw "Repo not found: $Repo" }
if (Test-Path -LiteralPath $lock) { throw "Loop already running: $lock" }
New-Item -ItemType File -Path $lock -Force | Out-Null
try {
  Set-Location $Repo
  git fetch origin $Branch --quiet
  $last = ''
  do {
    git fetch origin $Branch --quiet
    $head = (git rev-parse "origin/$Branch").Trim()
    if ($head -ne $last) {
      Log "NEW_COMMIT branch=$Branch sha=$head"
      git checkout $Branch --quiet
      git pull --ff-only origin $Branch --quiet
      $check = if (Test-Path 'package.json') { npm run workflow:check 2>&1 } else { 'workflow-check skipped: no package.json' }
      $check | Add-Content -LiteralPath $log
      $state = if (Test-Path 'STATE.md') { Get-Content 'STATE.md' -Raw } else { 'STATE.md missing' }
      $comm = if (Test-Path 'COMMUNICATION.md') { Get-Content 'COMMUNICATION.md' -Raw } else { 'COMMUNICATION.md missing' }
      
      # Match the last occurrence of NEXT_ACTOR in COMMUNICATION.md
      $allMatches = [regex]::Matches($comm, '(?i)(?:\*\*)?NEXT_ACTOR(?:\*\*)?[:=]\s*(\w+)')
      $actor = if ($allMatches.Count -gt 0) { $allMatches[$allMatches.Count - 1].Groups[1].Value } else { '' }
      if ($actor -match '(?i)Gemini') { Log "ACTION=GEMINI_REVIEW_REQUIRED sha=$head" }
      elseif ($actor -match '(?i)Codex') { Log "ACTION=CODEX_IMPLEMENTATION_REQUIRED sha=$head" }
      else { Log "ACTION=OWNER_OR_MANUAL_REVIEW sha=$head" }
      
      $last = $head
    }
    if ($Once) { break }
    Start-Sleep -Seconds $PollSeconds
  } while ($true)
} finally { Remove-Item -LiteralPath $lock -Force -ErrorAction SilentlyContinue }
