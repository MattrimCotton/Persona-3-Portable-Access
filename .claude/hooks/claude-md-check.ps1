# Stop hook: when structural project files changed since CLAUDE.md was last written,
# ask Claude once (per change set) to check whether CLAUDE.md needs an update.
$ErrorActionPreference = 'SilentlyContinue'
$payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
if ($payload.stop_hook_active) { exit 0 }

$root = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path }
Set-Location $root
$claudeMd = Join-Path $root 'CLAUDE.md'
if (-not (Test-Path $claudeMd)) { exit 0 }

# Structural = project/build files, docs, tools, Claude config, and NEW source files.
$structural = @()
foreach ($line in @(git status --porcelain --untracked-files=all 2>$null)) {
    if ($line.Length -lt 4) { continue }
    $code = $line.Substring(0, 2)
    $path = $line.Substring(3).Trim('"')
    if ($path -match ' -> ') { $path = ($path -split ' -> ')[-1] }
    if ($path -eq 'CLAUDE.md') { continue }
    $isNew = $code -match '\?\?|A|R|D'
    if ($path -match '\.(csproj|sln|props|targets)$|ModConfig\.json$|^docs/|^tools/|^data/|^\.claude/(agents|skills)/|^\.claude/settings\.json$|^PLAN\.md$|^\.gitignore$' -or
        ($isNew -and $path -match '^src/.+\.cs$')) {
        $structural += $path
    }
}
if ($structural.Count -eq 0) { exit 0 }

# Already updated after the latest structural change?
$claudeTime = (Get-Item $claudeMd).LastWriteTimeUtc
$newest = $structural | Where-Object { Test-Path $_ } | ForEach-Object { (Get-Item $_).LastWriteTimeUtc } |
    Sort-Object -Descending | Select-Object -First 1
if ($newest -and $claudeTime -ge $newest) { exit 0 }

# Only nag once per change set.
$marker = Join-Path $root '.claude\.claude-md-checked'
$key = ($structural | Sort-Object) -join '|'
if ((Test-Path $marker) -and ((Get-Content $marker -Raw).Trim() -eq $key)) { exit 0 }
Set-Content -Path $marker -Value $key -NoNewline

$list = ($structural | Select-Object -First 15) -join ', '
$reason = "Structural files changed since CLAUDE.md was last updated: $list. " +
    "Check whether CLAUDE.md (current stage, repo layout, commands, dependencies, rules, lessons) " +
    "still matches the project; update it briefly if not, otherwise reply that no update is needed."
@{ decision = 'block'; reason = $reason } | ConvertTo-Json -Compress
exit 0
