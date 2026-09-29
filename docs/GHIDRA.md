# Ghidra : décompiler P3P.exe

Mis en place le 29/09/2026 (Claude Code). Donne le pseudo-C d'une fonction en quelques
secondes, au lieu de lire l'assembleur avec `disasm.py`.

## Préparation (une fois, environ 25 minutes)

`P3P.exe` contient une section `.arch` de 400 Mo, chiffrée : Ghidra ne peut rien en tirer et
l'analyse deviendrait interminable. `tools/re/ghidra/prepare.py` en fait une copie où cette section
est déclarée vide ; les autres sections gardent leurs octets et leurs adresses (`0x140000000`).

```bash
python tools/re/ghidra/prepare.py
"$GHIDRA_HOME/support/analyzeHeadless.bat" "C:/Users/asdes.ASUS/Tools/ghidra_work/proj" P3P -import "C:/Users/asdes.ASUS/Tools/ghidra_work/P3P_noarch.exe" -overwrite -max-cpu 6
```

Le projet est dans `C:\Users\asdes.ASUS\Tools\ghidra_work\proj` (hors dépôt). À refaire après une
mise à jour Steam du jeu.

## Utilisation

```bash
sh tools/re/ghidra/gh.sh decomp C:/Users/asdes.ASUS/Tools/ghidra_work/out.txt 0x140122070 0x140127460
sh tools/re/ghidra/gh.sh xrefs C:/Users/asdes.ASUS/Tools/ghidra_work/out.txt 0x1407824C0
```

- `decomp` : pseudo-C de la fonction qui contient chaque adresse (crée la fonction si besoin).
- `xrefs` : toutes les références (code et données) vers chaque adresse, avec la fonction.
- Environ 15 s par appel (chargement du projet) : regrouper les adresses dans un même appel.
- Scripts Java : `tools/re/ghidra/Decomp.java`, `XRefs.java`.

## Méthode qui a marché (menu Config)

1. Trouver un texte de l'écran dans l'exe (`strfind.py`), puis la table de pointeurs qui le
   contient (recherche de l'adresse sur 8 octets), puis `xrefs` sur le début de la table.
2. `decomp` des fonctions trouvées : la fonction de dessin reçoit la structure de travail de
   l'écran ; les champs lus pour choisir la ligne en surbrillance donnent curseur et défilement.
3. Les appels à `0x1402323B0` (et aux 5 autres variantes de `DrawText`) montrent quels textes
   passent par la capture de texte du mod (`TEXT_CAPTURE.md`).

## Limites

- Le code qui appelle `thunk_FUN_14b…` à `thunk_FUN_15…` part dans `.arch` : illisible ici.
  Une partie des menus (création des tâches du menu pause, fenêtres de dialogue PC) y est.
- `__CheckForDebuggerJustMyCode` en tête de fonction est du bruit de compilation, à ignorer.
