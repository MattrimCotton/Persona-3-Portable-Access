# Ghidra : décompiler P3P.exe

Mis en place le 29/09/2026 (Claude Code). Donne le pseudo-C d'une fonction en quelques
secondes, au lieu de lire l'assembleur avec `disasm.py`.

## La section `.arch` n'est pas chiffrée (corrigé le 29/09/2026)

`P3P.exe` contient une section `.arch` de 400 Mo (`0x1436CA000` à `0x15D410000`). Elle a d'abord
été crue chiffrée : c'est faux. Son code est en clair (vérifié avec `disasm.py` sur
`0x1511E71F0` GetItemName, `0x151026790` GetEquipped, `0x14BC8ABE0` GetSLinkName, `0x14FD516F0`
texte resserré) ; les fonctions `thunk_FUN_14b…` à `thunk_FUN_15…` du code principal sont de
simples sauts vers elle. Elle est seulement énorme, ce qui rend l'analyse automatique complète
interminable. D'où **deux projets** :

| Projet | Contenu | Pour quoi |
|---|---|---|
| `proj` (P3P_noarch.exe) | exe sans `.arch`, **analysé** (~25 min, une fois) | noms, types, références (`xrefs`), code principal |
| `proj_full` (P3P.exe) | exe complet, **non analysé** (~2 min) | décompiler à la demande n'importe quelle fonction, `.arch` comprise |

Dans `proj_full`, le pseudo-C est un peu plus brut (globales vues comme `iRam0000…` : lire
l'adresse), et les références croisées ne sont pas calculées.

## Préparation (une fois ; à refaire après une mise à jour Steam)

```bash
python tools/re/ghidra/prepare.py
"$GHIDRA_HOME/support/analyzeHeadless.bat" "C:/Users/asdes.ASUS/Tools/ghidra_work/proj" P3P -import "C:/Users/asdes.ASUS/Tools/ghidra_work/P3P_noarch.exe" -overwrite -max-cpu 6
mkdir -p /c/Users/asdes.ASUS/Tools/ghidra_work/proj_full
"$GHIDRA_HOME/support/analyzeHeadless.bat" "C:/Users/asdes.ASUS/Tools/ghidra_work/proj_full" P3PFull -import "D:/SteamLibrary/steamapps/common/P3P/P3P.exe" -noanalysis -max-cpu 6
```

Les projets sont dans `C:\Users\asdes.ASUS\Tools\ghidra_work` (hors dépôt). Le dossier du projet
doit exister avant l'import. Ajouter `< /dev/null` aux commandes lancées depuis Bash : en cas
d'erreur, le `.bat` attend sinon une touche.

## Utilisation

```bash
sh tools/re/ghidra/gh.sh decomp C:/Users/asdes.ASUS/Tools/ghidra_work/out.txt 0x140122070 0x140127460
sh tools/re/ghidra/gh.sh xrefs C:/Users/asdes.ASUS/Tools/ghidra_work/out.txt 0x1407824C0
sh tools/re/ghidra/gh.sh decomp-full C:/Users/asdes.ASUS/Tools/ghidra_work/out.txt 0x151026790
```

- `decomp` : pseudo-C de la fonction qui contient chaque adresse (crée la fonction si besoin).
- `xrefs` : toutes les références (code et données) vers chaque adresse, avec la fonction
  (code principal seulement).
- `decomp-full` : comme `decomp`, dans l'exe complet ; à utiliser pour les cibles des `thunk_…`.
- 8 à 15 s par appel (chargement du projet) : regrouper les adresses dans un même appel.
- Scripts Java : `tools/re/ghidra/Decomp.java`, `XRefs.java`.

## Méthode qui a marché (menu Config)

1. Trouver un texte de l'écran dans l'exe (`strfind.py`), puis la table de pointeurs qui le
   contient (recherche de l'adresse sur 8 octets), puis `xrefs` sur le début de la table.
2. `decomp` des fonctions trouvées : la fonction de dessin reçoit la structure de travail de
   l'écran ; les champs lus pour choisir la ligne en surbrillance donnent curseur et défilement.
3. Les appels à `0x1402323B0` (et aux autres variantes de `DrawText`) montrent quels textes
   passent par la capture de texte du mod (`TEXT_CAPTURE.md`).
4. Pour un `thunk_FUN_…` : `decomp-full` sur sa cible (l'adresse après `thunk_FUN_`).

## Limites

- Les références depuis `.arch` vers le code principal ne sont pas connues (projet complet non
  analysé) : une fonction « sans appelant » dans `proj` peut être appelée depuis `.arch`.
- `__CheckForDebuggerJustMyCode` / `FUN_140422348` en tête de fonction est du bruit de
  compilation, à ignorer.
- Des octets de protection suivent certains sauts (`0x140232DC0`) : ne pas y poser d'accroche.
