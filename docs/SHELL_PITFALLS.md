# Pièges des commandes (shell) sous Windows

Bilan des 22 commandes en échec des sessions du 28/09/2026, relevé le 29/09/2026 dans les
transcriptions de Claude Code. Chaque piège a une parade ; ceux marqués **[hook]** sont refusés
automatiquement par `.claude/hooks/shell-guard.py` (PreToolUse sur PowerShell et Bash), qui
indique la bonne méthode.

## Choix de l'outil

| Besoin | Outil |
|---|---|
| Lire, chercher, modifier un fichier | Read, Grep, Glob, Edit, Write |
| git, python, dotnet, scripts, traitement de texte | Bash (heredoc `python - <<'EOF'` pour un script Python) |
| Script `.ps1` du projet | `pwsh -NoProfile -File script.ps1` (depuis Bash ou PowerShell) |
| Registre, variables d'environnement Windows, cmdlets sans équivalent | PowerShell, commandes courtes |

## Pièges et parades

1. **Faux échecs PowerShell** (7 fois). Le résultat était bon mais la dernière commande a mis le
   code de sortie à 1 : `Get-Process` quand le jeu est fermé **[hook]**, `Select-String` sans
   résultat, winget, `analyzeHeadless -help`. Le résultat d'un « échec » s'affiche aussi avec les
   accents cassés. Parade : Bash ; si PowerShell est nécessaire, `try { … -ErrorAction Stop } catch {}`.
   Jeu lancé ? `tasklist //NH //FO CSV | grep -q '"P3P.exe"' && echo lance || echo ferme`.
2. **Accent grave de PowerShell** (2 fois) : dans une chaîne entre guillemets doubles, `` `f `` est
   devenu un saut de page (`field2d/bg` écrit `field2dg` dans `MENUS.md`). Écriture de fichier par
   `Set-Content` / `Out-File` / `Add-Content` **[hook]** et `python -c` dans PowerShell **[hook]**
   interdits.
3. **Accents illisibles** : Windows PowerShell 5 (`powershell.exe`) lit un fichier UTF-8 sans BOM
   comme de l'ANSI et écrit dans la page OEM (« chargǸ »). Parade : `checklog.ps1` et
   `session-context.ps1` forcent `[Console]::OutputEncoding` et `-Encoding UTF8` ; tout nouveau
   `.ps1` fait de même. Python : `PYTHONUTF8=1` et `PYTHONIOENCODING=utf-8` dans
   `.claude/settings.json`.
4. **Chemin court du dossier temporaire** : `$env:TEMP` = `C:\Users\ASDES~1.ASU\...`, que
   `Push-Location` / `Resolve-Path` ne trouvent pas **[hook]**. Parade : chemin complet du
   scratchpad de la session.
5. **git dans PowerShell** **[hook]** : `git rm` pris pour une suppression dangereuse, commit bloqué.
   Parade : git toujours dans Bash.
6. **Attente** : `Start-Sleep` est refusé **[hook]**. Parade : outil Monitor (outil différé : le
   charger avec ToolSearch avant l'appel, sinon ses paramètres sont refusés) ou `run_in_background`.
7. **`Select-String -Recurse`** n'existe pas **[hook]**. Parade : Grep.
8. **Edit refusé, « fichier modifié depuis la lecture »** (3 fois, `CLAUDE.md` et `MENUS.md`) :
   relire le fichier juste avant de le modifier, surtout `CLAUDE.md` (hook Stop).
9. **Module Python manquant** : `capstone` absent a fait échouer une commande. Parade :
   `python -m pip install -r tools/re/requirements.txt` (installé le 29/09/2026).
10. **Chrome** : capture d'écran de Game UI Database expirée (30 s). Parade : lire la page avec
    `get_page_text` / `read_page` avant de tenter une capture.
