#!/bin/sh
# Décompile ou liste les références dans le projet Ghidra déjà analysé (lecture seule).
# Usage : sh tools/re/ghidra/gh.sh decomp|xrefs FICHIER_SORTIE 0x140122070 [0x...]
# FICHIER_SORTIE : chemin Windows avec des / (ex. C:/Users/asdes.ASUS/Tools/ghidra_work/out.txt).
# Environ 15 s par appel ; regrouper les adresses. Méthode : docs/GHIDRA.md.
W=/c/Users/asdes.ASUS/Tools/ghidra_work
SCRIPTS="$(cd "$(dirname "$0")" && pwd -W 2>/dev/null || pwd)"
case "$1" in decomp) S=Decomp.java;; xrefs) S=XRefs.java;; *) echo "usage: gh.sh decomp|xrefs OUT ADDR..."; exit 1;; esac
shift
OUT="$1"; shift
rm -f "$OUT"
"$GHIDRA_HOME/support/analyzeHeadless.bat" "C:/Users/asdes.ASUS/Tools/ghidra_work/proj" P3P \
  -process P3P_noarch.exe -noanalysis -readOnly \
  -scriptPath "$SCRIPTS" \
  -postScript $S "$OUT" "$@" > "$W/last_run.log" 2>&1
grep -E "ERROR|Exception" "$W/last_run.log" | head -5
cat "$OUT"
