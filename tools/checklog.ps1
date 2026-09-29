<#
.SYNOPSIS
  Show the tail of the most recent Reloaded II log for P3P and surface any exceptions.
.PARAMETER Tail
  Number of trailing lines to print (default 80).
.PARAMETER Full
  If set, prints the whole file instead of just the tail.
.EXAMPLE
  .\tools\checklog.ps1              # last 80 lines of latest P3P Reloaded log
  .\tools\checklog.ps1 -Tail 200    # last 200 lines
  .\tools\checklog.ps1 -Full        # entire latest log
#>
param(
    [int]$Tail = 80,
    [switch]$Full
)

# Windows PowerShell 5 reads BOM-less files as ANSI and prints in the OEM code page: force UTF-8 both ways.
[Console]::OutputEncoding = [Text.Encoding]::UTF8

$logDir = Join-Path $env:APPDATA 'Reloaded-Mod-Loader-II\Logs'
if (-not (Test-Path $logDir)) { Write-Host "No Reloaded log directory at $logDir"; exit 1 }

$latest = Get-ChildItem $logDir -Filter '*.txt' | Where-Object { $_.Name -match 'P3P' } |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $latest) { Write-Host "No P3P logs found in $logDir"; exit 1 }

Write-Host "=== $($latest.Name) ($([int]((Get-Date) - $latest.LastWriteTime).TotalMinutes) min ago, $([int]($latest.Length/1kb)) KB) ==="

if ($Full) { Get-Content $latest.FullName -Encoding UTF8 } else { Get-Content $latest.FullName -Encoding UTF8 -Tail $Tail }

# Surface error-ish lines anywhere in the file.
$errLines = Select-String -Path $latest.FullName -Encoding UTF8 -Pattern 'Exception|Error|Crash|Unhandled|fatal'
if ($errLines) {
    Write-Host "`n=== Error-like lines ($($errLines.Count)) ==="
    $errLines | ForEach-Object { "{0,5}: {1}" -f $_.LineNumber, $_.Line } | Select-Object -Last 40
}
