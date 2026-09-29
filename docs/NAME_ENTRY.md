# Saisie du nom (contrat)

Écran où le héros signe le contrat, juste après la scène `event\e100\e101_001` (« Au bas, un
espace attend votre signature... »). Lecteur : `Components/NameEntry.cs`. Trouvé le 29/09/2026,
**pas encore testé en jeu**.

## Comment il a été trouvé

1. Modules dans l'exe : `name_entry\nentry.c` (commun) et une variante par famille de langues :
   `nentry_figs.c` (français, italien, allemand, espagnol), `nentry_en.c`, `nentry_ch.c`,
   `nentry_ck.c`, `nentry_kr.c`. `tools/re/xref.py` sur les chaînes de chemin donne leurs
   fonctions.
2. `0x140290460` crée la tâche européenne : mise à jour `0x14028CC80`, destruction `0x14028CC10`.
   La mise à jour est une machine à 6 étapes (`travail +0x00`) : 0 prépare les deux noms, 1-2
   chargent `dict/name.bin`, 3 crée la **sous-tâche clavier** (travail de 0x2218 octets, mise à
   jour `0x14028F210`), 4 attend sa fin, 5 termine.
3. Le traitement des touches (`0x14028F310`) déplace le curseur : `travail +0x20` colonne
   (0-19, boucle), `+0x24` ligne (0-5), `+0x28` index linéaire, `+0x2C/+0x30` position
   précédente. Chaque déplacement saute les cases vides via `GetCell`.
4. `GetCell(ligne, colonne)` `0x1402901C0` : lit une grille 6 × 20 de valeurs 16 bits choisie selon
   un identifiant de langue global (5 = français `0x14062D440`, 6 `0x14062D710`, 7 `0x14062D530`,
   8 = espagnol `0x14062D620`) ; valeur négative = case vide.
5. **Les touches sont des images** : le dessin (`0x14028F0A0`) affiche le sprite `valeur + 0x20`
   de `p3p_nameent01.spr`, deuxième fichier de `dict/name.bin` (le premier, `namedic_dat.txt`, est
   un dictionnaire de noms japonais). Rectangles des sprites à l'entrée `+0x54` (x1, y1, x2, y2),
   texture à `+0x14`, **en coordonnées PSP : multiplier par 4** pour la texture PC 1024 × 1024.
   Les noms internes des sprites sont restés en japonais : seul le dessin fait foi. Table valeur →
   caractère relevée en regardant chaque sprite (dans `NameEntry.cs`).
6. Les deux noms en cours de saisie : chaînes du jeu en `0x1409F37A8` et `0x1409F37C0`
   (8 caractères, `80 80` = case vide), initialisées à l'étape 0 de la mise à jour.

## Grille française

Lignes 0-5 : majuscules en colonnes 0-4, minuscules en 5-9, chiffres en 10-14 (lignes 0-1),
lettres accentuées (Â À É È Ê Î Œ Ç â à ä é è ê ë ï î ü û ù ö ç) en colonnes 10-14 des lignes
2-5, symboles en 15-19. Les boutons Suppr, OK et Fin sont dessinés à part (pas des cases).

## Fonctionnement du mod

- Accroche la mise à jour du clavier ; à la première image d'un nouvel écran : annonce
  « Saisie du nom… ».
- Quand la colonne ou la ligne change : `GetCell` (appel de la fonction du jeu), puis le nom de
  la touche : majuscule annoncée « majuscule A », symboles en mots (`lang/*.json`, clés `sym_*`).
- Quand un des deux noms change (lettre ajoutée ou effacée) : « Nom : … » ou « Nom vide ».

## Test du 29/09/2026 (test 5) et signature automatique

- Clavier : **réussi** (touches, accents, symboles, « Nom : SIM… »). Le premier tampon
  (`0x1409F37A8`) reçoit le premier nom tapé. Mais valider sur ce clavier est pénible (comme dans
  P4G) : le joueur a demandé une signature automatique avec un nom défini.
- Boutons : le masque des boutons pressés (`0x143624544`, signature `F6 05 ?? ?? ?? ?? 08 74 28
  45 85 F6 74 23 C7 47 18 03 00 00 00`, 5 variantes qui visent la même variable) reprend les codes
  de la PSP : 0x10 haut, 0x40 bas, 0x20 droite, 0x80 gauche, 0x4000 croix, **0x8 Start = Fin**.
- Boucle de l'écran `0x14028FBD0` (travail : +0x08 étape, +0x18 mode : 1 frappe, 3 confirmation).
  En mode 1, la fonction des boutons `0x14028F5D0` compte les cases vides (`80 80`) des deux
  noms ; si les deux sont remplis et que Start est pressé : mode 3, puis `0x14028DD40` ouvre la
  confirmation.
- Format d'un nom : 8 cases de 2 octets (« S » = `80 B3`, « è » = `9C A7`, vide = `80 80`), puis 0.
  Même les lettres ASCII sont sur 2 octets : `AtlusEncoding.TryGetWideBytes`. Compteurs globaux
  vus à 6 après 6 lettres : `0x1409F3788` et `0x1409F37D4` (rôle exact inconnu, non écrits).
- **Signature automatique** (réglage `AutoSignContract`, activé par défaut). Noms selon le
  personnage choisi (demande du joueur : héros **et** héroïne) : `HeroLastName` / `HeroFirstName`
  (Yuki / Makoto, nom officiel du héros) ou `HeroineLastName` / `HeroineFirstName` (Shiomi /
  Kotone, nom officiel de l'héroïne, par défaut dans la version de 2023). Le choix suit
  l'indicateur **IsFemc** du jeu (`0x143387510`, signature de `p3ppc.visibleRankupReady`
  d'AnimatedSwine37 : `48 8D 35 ?? ?? ?? ?? 0F 28 05 ?? ?? ?? ??`, global = opérande de
  l'instruction précédente). La fonction du jeu qui enregistre le sexe (`0x14025DF80`, appelée à
  la confirmation du choix) saute dans la section `.arch`, crue chiffrée à l'époque ; elle est
  en fait lisible avec `gh.sh decomp-full` (`GHIDRA.md`, correction du 29/09/2026). Déroulement : à l'ouverture, le mod écrit les deux noms, puis met
  le bit Start pendant au plus 60 images jusqu'au mode 3. Sinon : « Noms remplis. Appuyez sur
  Start pour signer. » La confirmation du jeu reste à valider par le joueur.

## Test 6 (29/09/2026) et touches du clavier

- La signature automatique a écrit « Yuki Makoto », mais Start a été simulé pendant l'animation
  d'ouverture (mode 0), ignoré, et le mod a abandonné au bout d'une seconde. Les frappes du joueur
  ont ensuite remplacé le nom depuis la première case (« AAAAAA »). Corrigé : Start n'est
  simulé qu'en mode 1 (au plus une seconde), abandon après 10 s au total. Nom réécrit à la main
  dans la partie ouverte (mémoire du processus, `tools/re/p3pmem.py`) : `Yuki` et
  compteurs `0x1409F3788` / `0x1409F37D4` ramenés à 4 (position de la prochaine lettre).
- **Touches du clavier** : `%LOCALAPPDATA%\SEGA\P3P\steam\<id Steam>\P3P.ini`, section
  `[ActionConfig_P3P]`. Chaque action : `3-<bouton PSP>` (manette), `1-<code de position
  DirectInput>` (clavier), deux fois. Par défaut : haut `W`/flèche (codes 17, 200), bas `S`/flèche,
  gauche `A`/flèche, droite `D`/flèche, croix (valider) Espace ou Entrée (57, 28), rond (annuler)
  `C` ou Échap (46, 1), **Start : codes 44 et 45** (Z et X en QWERTY, **W et X en AZERTY**).
  Les codes sont des positions : sur AZERTY, « W » QWERTY (17) est la touche Z, etc.
- À faire : lire `P3P.ini` pour nommer les vraies touches du joueur dans les annonces.

## Inconnues à vérifier au premier test

- Ordre des deux noms (nom de famille puis prénom ?) et lequel des deux tampons est lequel.
- Si les tampons changent bien à chaque lettre (sinon : trouver le vrai tampon d'affichage).
- Touches du clavier et de la manette pour Suppr, OK et Fin ; confirmation finale.
- Autres langues : la table valeur → caractère vient des sprites français ; les variantes
  anglaise et asiatiques (`nentry_en.c`…) ont leurs propres fonctions, non accrochées.
