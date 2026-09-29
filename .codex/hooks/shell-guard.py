# PreToolUse hook (PowerShell | Bash): refuse the command patterns that failed in past sessions
# and tell Claude what to use instead. Rules and history: docs/SHELL_PITFALLS.md.
import json
import re
import sys

try:
    payload = json.load(sys.stdin)
except Exception:
    sys.exit(0)

tool = payload.get("tool_name", "")
cmd = (payload.get("tool_input") or {}).get("command", "") or ""

PS_RULES = [
    (r"\bStart-Sleep\b",
     "Start-Sleep est refusé par Claude Code. Pour attendre une condition : outil Monitor "
     "(le charger d'abord avec ToolSearch \"select:Monitor\"), ou run_in_background."),
    (r"\bSelect-String\b[^|;]*-Recurse\b",
     "Select-String n'a pas de -Recurse. Chercher dans des fichiers : outil Grep."),
    (r"\b(Set-Content|Out-File|Add-Content)\b",
     "Ne pas écrire de fichier texte depuis PowerShell (l'accent grave abîme les chemins, "
     "encodage incertain). Utiliser Write / Edit, ou un script Python lancé depuis Bash."),
    (r"\bpython3?\s+-c\b",
     "Pas de python -c dans PowerShell (guillemets, accent grave et * mal interprétés). "
     "Écrire le script dans le dossier scratchpad puis le lancer, ou Bash avec un heredoc "
     "(python - <<'EOF')."),
    (r"\$env:(TEMP|TMP)\b|~1\.",
     "$env:TEMP donne un chemin court (ASDES~1.ASU) que PowerShell ne résout pas. "
     "Utiliser le chemin complet du scratchpad de la session."),
    (r"(^|[;&|(]\s*)git\s",
     "Lancer git depuis Bash, pas PowerShell (git rm y est pris pour une suppression "
     "dangereuse et le commit est bloqué)."),
    (r"\bGet-Process\b",
     "Get-Process renvoie une erreur quand le jeu n'est pas lancé. Utiliser Bash : "
     "tasklist //NH //FO CSV | grep -q '\"P3P.exe\"' && echo lance || echo ferme"),
]

BASH_RULES = [
    (r"\\\\",
     "L'outil Bash réduit une barre oblique inverse doublée à une seule, même dans un heredoc "
     "entre apostrophes (chemins et Markdown abîmés, scripts Python cassés). Utiliser chr(92) "
     "en Python, des barres obliques / dans les chemins, et Edit / Write pour les textes."),
    (r"\b(powershell|pwsh)(\.exe)?\b[^|;&]*\s-(Command|c)\b",
     "Pas de powershell -Command depuis Bash (double interprétation des guillemets). "
     "Lancer un script .ps1 avec -File, ou utiliser directement l'outil PowerShell."),
]

rules = PS_RULES if tool == "PowerShell" else BASH_RULES if tool == "Bash" else []
for pattern, reason in rules:
    if re.search(pattern, cmd, re.IGNORECASE | re.MULTILINE):
        print(json.dumps({
            "hookSpecificOutput": {
                "hookEventName": "PreToolUse",
                "permissionDecision": "deny",
                "permissionDecisionReason": "[shell-guard] " + reason,
            }
        }))
        sys.exit(0)
sys.exit(0)
