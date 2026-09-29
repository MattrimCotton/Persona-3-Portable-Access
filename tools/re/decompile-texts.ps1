# Décompile tous les scripts (.bf) et messages (.bmd) extraits d'une archive de P3P en texte lisible
# (.flow / .msg), avec Atlus Script Tools et la table de caractères de la langue.
# Préalable : extraire l'archive avec tools/re/CpkExtract (voir docs/REFERENCES.md).
# Nécessite PowerShell 7 (pwsh) : les fichiers sont traités en parallèle (le compilateur met ~1 s
# à démarrer, 3 452 fichiers en série prendraient plus d'une heure). Les fichiers déjà décompilés
# sont sautés, on peut donc relancer après une interruption.
# Usage : pwsh -File tools/re/decompile-texts.ps1 [-Src extracted\data_FR] [-Dst decompiled\data_FR] [-Encoding P3P_EFIGS]
param(
    [string]$Src = "extracted\data_FR",
    [string]$Dst = "decompiled\data_FR",
    [string]$Encoding = "P3P_EFIGS",
    [string]$Compiler = "$env:USERPROFILE\Tools\AtlusScriptTools\AtlusScriptCompiler.exe",
    [int]$Threads = [Environment]::ProcessorCount
)

$srcRoot = (Resolve-Path $Src).Path
New-Item -ItemType Directory -Force $Dst | Out-Null
$dstRoot = (Resolve-Path $Dst).Path
$files = Get-ChildItem $srcRoot -Recurse -File | Where-Object { $_.Extension -in '.bf', '.bmd' }

$fail = $files | ForEach-Object -ThrottleLimit $Threads -Parallel {
    $f = $_
    $rel = $f.FullName.Substring($using:srcRoot.Length).TrimStart('\')
    $out = Join-Path $using:dstRoot ($rel + $(if ($f.Extension -eq '.bf') { '.flow' } else { '.msg' }))
    if (Test-Path $out) { return }
    New-Item -ItemType Directory -Force (Split-Path $out) | Out-Null
    $fmt = if ($f.Extension -eq '.bf') { 'FlowScriptBinary' } else { 'MessageScriptBinary' }
    # Les .bf de conver_temp\bf\ sont en fait des messages (en-tête "MSG1" à +8), pas des scripts.
    $head = [byte[]]::new(12)
    $fs = [IO.File]::OpenRead($f.FullName); [void]$fs.Read($head, 0, 12); $fs.Close()
    if ([Text.Encoding]::ASCII.GetString($head, 8, 4) -eq 'MSG1') {
        $fmt = 'MessageScriptBinary'
        $out = [IO.Path]::ChangeExtension($out, '.msg')
        if (Test-Path $out) { return }
    }
    # Start-Process + délai : sur certains fichiers le compilateur attend une touche et ne rend
    # jamais la main (constaté le 28/09/2026) ; on le tue au bout de 60 s.
    $argsLine = "-In `"$($f.FullName)`" -InFormat $fmt -Decompile -Library P3P -Encoding $using:Encoding -Out `"$out`""
    # Chaque appel a son propre dossier de travail : le compilateur écrit AtlusScriptCompiler.log
    # dans le dossier courant, et deux instances sur le même fichier plantent.
    $work = Join-Path ([IO.Path]::GetTempPath()) ("asc_" + [Guid]::NewGuid().ToString("N"))
    New-Item -ItemType Directory $work | Out-Null
    $stdin = Join-Path $work "stdin.txt"; New-Item $stdin | Out-Null
    $p = Start-Process $using:Compiler -ArgumentList $argsLine -NoNewWindow -PassThru -WorkingDirectory $work `
        -RedirectStandardOutput (Join-Path $work "out.txt") -RedirectStandardInput $stdin
    if (-not $p.WaitForExit(60000)) { $p.Kill(); Remove-Item $out -ErrorAction SilentlyContinue }
    Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue
    if (-not (Test-Path $out)) { $rel }
}

$done = (Get-ChildItem $dstRoot -Recurse -File -Include *.flow, *.msg | Where-Object { $_.Name -notlike '*.flow.msg' }).Count
"$done / $($files.Count) fichiers décompilés dans $Dst"
if ($fail) { "Échecs ($(@($fail).Count)) :"; $fail | Select-Object -First 20 }
