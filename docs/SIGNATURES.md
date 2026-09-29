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
| Écran titre | `Title::Update` (titledraw.c), rcx = tâche, tâche +0x48 = structure : +0x00 étape (7 « appuyez sur une touche », 9 menu), +0x28 curseur 0-4 | `40 53 41 54 41 56 41 57 48 81 EC 88 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 78 4C 8B F1 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 49 8B CE E8 ?? ?? ?? ?? 48 8B D8 48 63 00 83 F8 1C` | `0x14024BCA0` | chaîne `...\src\game\titledraw.c`, appels à la bibliothèque de listes | 2026-09-28 | unique dans l'exe |
| Choix du sexe / difficulté | `SexSelect::Update` (sexselect.c), tâche +0x48 = structure : +0x20 étape (5 sexe, 9 difficulté), +0x24 curseur sexe (0 masc., 1 fém.), +0x26 curseur difficulté (0-4, départ 2) | `40 53 56 41 56 48 81 EC 90 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 88 00 00 00 48 8B D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 5B 48 33 F6 F6 03 01` | `0x1402AA210` | chaîne `...\src\sex_select\sexselect.c` | 2026-09-28 | unique dans l'exe |
| Saisie du nom | clavier européen, mise à jour de la sous-tâche (nentry_figs.c), tâche +0x48 = travail : +0x08 étape (3 = fermeture), +0x20 colonne 0-19, +0x24 ligne 0-5 | `40 53 48 83 EC 20 48 8B D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 43 48 83 78 08 03 74 2B E8 ?? ?? ?? ?? 48 8D 0D 95 0E 00 00 48 8B D0 48 89 48 10 48 8D 0D D7 74 0C 03` | `0x14028F210` | chaîne `...\src\name_entry\nentry_figs.c`, création de tâche `0x140290460` | 2026-09-29 | unique ; 5 variantes identiques (autres langues) sans la fin |
| Saisie du nom | `GetCell(ligne, colonne)` : valeur de la touche selon la langue du jeu (négatif = vide) | `48 89 5C 24 08 57 48 83 EC 20 48 63 F9 48 8D 0D ?? ?? ?? ?? 48 63 DA E8 ?? ?? ?? ?? 44 8B 05 ?? ?? ?? ?? B9 FF FF FF FF 41 83 E8 05` | `0x1402901C0` | appels depuis le dessin du clavier | 2026-09-29 | unique |
| Saisie du nom | deux noms en cours de saisie (chaînes, `80 80` = vide), déplacements à +0xB et +0x8C de la signature | `44 0F B6 0D ?? ?? ?? ?? 4C 8D 1D C7 6A 76 00 44 0F B7 15` | `0x1409F37A8`, `0x1409F37C0` | étape 0 de la mise à jour `0x14028CC80` | 2026-09-29 | unique (sans le déplacement : 2, l'anglais écrit en `0x1409F3748`) |
| Boutons | masque des boutons pressés (codes PSP : 0x8 Start, 0x10 haut, 0x20 droite, 0x40 bas, 0x80 gauche, 0x4000 croix) ; global = `test byte [rip+disp], 8` : adresse + 7 + disp | `F6 05 ?? ?? ?? ?? 08 74 28 45 85 F6 74 23 C7 47 18 03 00 00 00` | `0x143624544` | boutons de la saisie du nom (`0x14028F5D0`) | 2026-09-29 | 5 résultats (variantes de langue), même global |
| Protagoniste | IsFemc (héroïne choisie, dword ≠ 0) : opérande de l'instruction qui précède la signature (`GetGlobalAddress(adresse - 4)`) | `48 8D 35 ?? ?? ?? ?? 0F 28 05 ?? ?? ?? ??` | `0x143387510` | p3ppc.visibleRankupReady (AnimatedSwine37) | 2026-09-29 | unique |
| Listes (générique) | bibliothèque `sdkliststate.c` : init `0x1403DDED0` (liste, nombre, visibles, curseur, …), curseur courant `0x1403DD950` (= liste +0x24), nombre = liste +0x14. 121 appels d'initialisation dans le jeu | — | — | lecture du code du choix de la difficulté | 2026-09-28 | non testée |
