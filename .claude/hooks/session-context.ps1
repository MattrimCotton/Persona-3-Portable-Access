# SessionStart hook: prints a short project status that is added to Claude's context.
$ErrorActionPreference = 'SilentlyContinue'
$root = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path }
Set-Location $root

$lines = @('## P3P Access - session context')

$branch = git rev-parse --abbrev-ref HEAD 2>$null
$changes = @(git status --porcelain 2>$null).Count
$lastCommit = git log -1 --format='%h %s (%cr)' 2>$null
$lines += "- git: branch $branch, $changes changed file(s), last commit: $(if ($lastCommit) { $lastCommit } else { 'none' })"

$game = 'D:\SteamLibrary\steamapps\common\P3P\P3P.exe'
$lines += "- game installed: $(if (Test-Path $game) { 'yes' } else { 'NO (P3P.exe missing)' })"

$app = 'D:\SteamLibrary\Reloaded-II\Apps\p3p.exe'
$lines += "- P3P registered in Reloaded-II: $(if (Test-Path $app) { 'yes' } else { 'no' })"

$mod = 'D:\SteamLibrary\Reloaded-II\Mods\p3ppc.accessibility'
$lines += "- mod deployed: $(if (Test-Path $mod) { 'yes' } else { 'no' })"

$logDir = Join-Path $env:APPDATA 'Reloaded-Mod-Loader-II\Logs'
$log = Get-ChildItem $logDir -Filter '*.txt' | Where-Object { $_.Name -match 'P3P' } |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($log) {
    $errs = @(Select-String -Path $log.FullName -Pattern 'Exception|Error|Crash|Unhandled|fatal')
    $lines += "- latest P3P log: $($log.Name), $($errs.Count) error-like line(s)"
    $errs | Select-Object -Last 3 | ForEach-Object { $lines += "    $($_.LineNumber): $($_.Line.Trim())" }
} else {
    $lines += '- latest P3P log: none yet'
}

$lines -join "`n"
exit 0
