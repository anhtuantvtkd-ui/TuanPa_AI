# Agent handoff loop

This runner turns Git commits and `COMMUNICATION.md` into a deterministic polling loop.

## Run on Windows

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\agent-loop.ps1 -Repo D:\TuanPa_AI -Branch task/TASK-0016-fix-toado-vertical-horizontal
```

It pulls only fast-forward changes, runs `npm run workflow:check` when available, and writes `agent-loop.log`.
The runner never edits application code, merges, deploys, or writes `APPROVED`.

## Required handoff marker

Each handoff should include one of:

- `NEXT_ACTOR: Codex`
- `NEXT_ACTOR: Gemini`
- `NEXT_ACTOR: Owner`

This is an automation loop, not an AI wake-up API. Gemini must still be running in Antigravity and must itself poll/push, or a separate Gemini-side watcher must be installed.
