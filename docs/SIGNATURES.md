# Signatures et adresses de P3P.exe

Toutes les signatures SigScan, fonctions et adresses trouvées dans Persona 3 Portable (Steam).
Une ligne par trouvaille. Statuts : `non testée`, `unique dans l'exe`, `validée en jeu`, `cassée`.

## L'exécutable

| Propriété | Valeur | Vérifié le |
|---|---|---|
| Fichier | `D:\SteamLibrary\steamapps\common\P3P\P3P.exe`, 443 164 608 octets | 2026-09-28 |
| Architecture | PE32+ (x64), base d'image `0x140000000` | 2026-09-28 |
| ASLR | **désactivé** (DllCharacteristics `0x8120`, pas de DYNAMIC_BASE) : adresses statiques constantes d'un lancement à l'autre, comme P4G | 2026-09-28 |

Même sans ASLR, préférer les signatures : une mise à jour Steam déplace les adresses.

## Fonctions et structures

| Système | Fonction / donnée | Signature | Cible / offset | Source | Date | Statut |
|---|---|---|---|---|---|---|
| Menu de statut | stats sociales | `E8 ?? ?? ?? ?? 48 63 0D ?? ?? ?? ?? 33 FF` | cible du `call` | p3ppc.socialStatTracker | 2026-09-28 | non testée |
| Dialogues | `MsgWindow::DrawAll` : dessine toutes les fenêtres de message ouvertes (corps de `DrawDialog` de P4G inliné dans une boucle) | `40 55 48 83 EC 30 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 8B E8 48 85 C0 0F 84` | début de fonction, `0x14023B510` | trouvée en cherchant dans P3P le fragment `83 23 E7 83 0B 20` du DrawDialog de P4G | 2026-09-28 | unique dans l'exe |
| Dialogues | liste des fenêtres de message ouvertes (pointeur global vers le premier nœud) | même signature, `mov rax,[rip+disp]` à +0x12 (déplacement à +0x15) | `0x1409F11D0` | idem | 2026-09-28 | unique dans l'exe |
| Fenêtre | mise à jour du titre de la fenêtre (images par seconde), saut `jge` qui l'évite | `0F 8D ?? ?? ?? ?? 33 D2 0F 29 B4 24 ?? ?? ?? ?? 41 B8 00 01 00 00` | `0x140353E2A` (le `jge`) | code identique à `UpdateTitleBar` de P4G, trouvé par le fragment `B8 40 42 0F 00 99 F7 F9` | 2026-09-28 | unique dans l'exe |
