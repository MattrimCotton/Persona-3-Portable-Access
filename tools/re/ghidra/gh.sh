#!/bin/sh
# Décompile ou liste les références dans les projets Ghidra de P3P.exe (lecture seule).
# Usage : sh tools/re/ghidra/gh.sh decomp|xrefs|decomp-full FICHIER_SORTIE 0x140122070 [0x...]
#   decomp / xrefs : projet analysé (P3P_noarch.exe, sans la section .arch) : types, noms, références.
#   decomp-full    : projet complet non analysé (P3P.exe, section .arch comprise) : décompile à la
#                    demande n'importe quelle fonction, y compris celles de .arch (0x1436CA000 et +).
# FICHIER_SORTIE : chemin Windows avec des / (ex. C:/Users/asdes.ASUS/Tools/ghidra_work/out.txt).
# Environ 8 à 15 s par appel : regrouper les adresses. Méthode : docs/GHIDRA.md.
W=/c/Users/asdes.ASUS/Tools/ghidra_work
SCRIPTS="$(cd "$(dirname "$0")" && pwd -W 2>/dev/null || pwd)"
PROJ="C:/Users/asdes.ASUS/Tools/ghidra_work/proj"; NAME=P3P; PROG=P3P_noarch.exe
case "$1" in
  decomp) S=Decomp.java;;
  xrefs) S=XRefs.java;;
  decomp-full) S=Decomp.java; PROJ="C:/Users/asdes.ASUS/Tools/ghidra_work/proj_full"; NAME=P3PFull; PROG=P3P.exe;;
  *) echo "usage: gh.sh decomp|xrefs|decomp-full OUT ADDR..."; exit 1;;
esac
shift
OUT="$1"; shift
rm -f "$OUT"
"$GHIDRA_HOME/support/analyzeHeadless.bat" "$PROJ" $NAME \
  -process $PROG -noanalysis -readOnly \
  -scriptPath "$SCRIPTS" \
  -postScript $S "$OUT" "$@" > "$W/last_run.log" 2>&1 < /dev/null
grep -E "ERROR|Exception" "$W/last_run.log" | head -5
cat "$OUT"
