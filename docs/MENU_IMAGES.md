# Catalogue local des images de menus

Créé le 29/09/2026 par Codex, à la demande du joueur : préparer les images en une fois pour
accélérer la suite. Il s'agit des **ressources graphiques extraites**, pas de captures des
écrans composés par le jeu. Aucun lecteur ni hook du mod n'a été modifié pour cette opération.

## Résultat

- [Index texte local](../extracted/menu_catalog/INDEX.md), recherchable avec `rg`.
- [Galerie par système](../extracted/menu_catalog/index.html).
- [Inventaire complet](../extracted/menu_catalog/manifest.json), avec provenance et rectangles.
- [Couverture et limites](../extracted/menu_catalog/coverage.json).
- [Vérification des fichiers](../extracted/menu_catalog/verification.json).

Ces liens locaux ne fonctionneront pas sur GitHub : toutes les images et les métadonnées
extraites restent sous `extracted/`, déjà exclu de Git. Seuls les outils et ce guide sont
destinés au dépôt public.

Passage du 29/09/2026 : 46 865 entrées inventoriées dans quatre archives, 1 145 fichiers sources
sélectionnés, 1 609 occurrences de ressources graphiques, **851 ressources distinctes** après
déduplication SHA-256. Export de **1 112 planches PNG**, **4 542 éléments découpés** et
**247 planches-contact**. Les 5 901 PNG et les 3 593 liens HTML ont passé le contrôle hors jeu.
Aucune erreur d'extraction ou de décodage de texture dans le périmètre sélectionné.

Le catalogue couvre les familles titre, menus pause et système, saisie du nom, sauvegarde,
liens sociaux, boutiques, Velvet Room, combat et résultats, cartes, interface commune et
crédits. Il conserve aussi les portraits et cartes inclus dans ces familles. Cela prépare
les étapes futures sans prétendre que leurs lecteurs sont déjà implémentés ou testés.

## Retrouver un écran sans refaire l'extraction

1. Chercher le nom du fichier ou du système dans `extracted/menu_catalog/INDEX.md`.
2. Privilégier l'origine `data_FR/umd0.cpk` pour les textes français. `data/umd0.cpk` contient
   les éléments communs et parfois des variantes japonaises. Les deux sont conservés séparément
   si leurs octets diffèrent ; toutes les origines sont listées si elles sont identiques.
3. Ouvrir la planche PNG ou la planche-contact indiquée dans la galerie.
4. Dans `assets/<identifiant>/metadata.json`, retrouver le numéro de sprite, le nom interne,
   le numéro de texture et les rectangles `rect_psp` / `rect_pc`.
5. Relier ce numéro à la table utilisée par la fonction de dessin P3P. **Le numéro de sprite
   n'est pas nécessairement le numéro d'entrée du menu.** Les noms internes, souvent japonais
   ou anciens, ne remplacent pas la lecture de l'image française.

Repères vérifiés visuellement :

- Menu pause : `init_free.bin!init/camp.bin!c_main_01.spr`, ressource française
  `ebf9a7993afd2261f9bd` ; 22 textures, 696 sprites déclarés. La première planche-contact
  montre les libellés de compétences, objets, Persona, équipement, état et système.
  Les liens sociaux portent un autre numéro : ne pas déduire l'ordre du menu de cette planche.
- Autres éléments camp : `c_main01x2.spr`, `c_msw.spr`, autres membres de `camp.bin`.
- Clavier : `dict/name.bin!p3p_nameent01.spr`, français `00bb11f448451007c74f` ;
  6 textures, 248 sprites déclarés. Vérification visuelle d'une planche de lettres et chiffres.
- Sauvegarde : `memcard/save.bin!savespr01.spr`, français `3ba841ed5e82308ef8d7` ;
  6 textures, 11 sprites, avec variantes bleu/rose et prompts de sauvegarde/chargement/suppression.
- Titre : `title/title.bin!menu.spr`, français `fa8b03d6971d5b3868a7`.
- Difficulté personnalisée : `camp/mode_custom/mode_custom.bin!difficulty01.spr`.
- Boutiques/fusion : chercher `facility`, `k_shop`, `i_shop`, `velvet`, `combine`.

## Périmètre et limites précises

Inventaires de `data/umd0.cpk`, `data/umd1.cpk`, `data_FR/umd0.cpk` et `sysdat/umd2.cpk`.
Sélection des dossiers d'interface listés dans `UI_ROOTS` de l'outil, de tous les `init*`,
des `.spr` en dehors de ces dossiers, et de tout `sysdat`. Déballage récursif des archives.
`umd1` a été inventorié mais ne contient aucune entrée correspondant à cette sélection.

Les archives des autres langues, les décors des scènes et les modèles 3D en dehors de ces
familles ne sont pas convertis. Le filtre est documenté ; il ne prouve pas qu'aucune ressource
d'interface ne se cache dans un autre conteneur. La composition finale des écrans, les textes
dynamiques, les couleurs de sélection et la disponibilité des commandes restent à examiner
dans le code et à vérifier en jeu.

Sur les 5 343 sprites déclarés : 4 542 découpes exportées ; 681 rectangles vides ;
100 rectangles hors des limites avec l'échelle documentée ×4 ; 20 références sans texture
associée. Ces entrées sont conservées avec leur statut. Les planches complètes correspondantes
sont disponibles. Certains éléments sont des aplats/polygones, d'autres d'anciens éléments :
ne pas inventer de découpe ou de libellé pour les résoudre.

## Outils et formats

Commande à exécuter depuis la racine du dépôt (Python UTF-8, Git Bash sous Windows) :

```sh
PYTHONUTF8=1 PYTHONIOENCODING=utf-8 python tools/re/menu_catalog.py --game D:/SteamLibrary/steamapps/common/P3P
PYTHONUTF8=1 PYTHONIOENCODING=utf-8 python tools/re/check_menu_catalog.py
```

`--refresh-sheets` régénère uniquement les planches-contact en plus du parcours normal ;
les conversions déjà présentes sont réutilisées. `--output` doit rester sous `extracted/`.
Après une mise à jour du jeu, ajouter `--reextract` : le cache des sources utilise sinon
les noms et tailles des fichiers, sans détecter un changement de contenu de même taille.
Les sources décompressées représentent environ 3,17 Go, auxquels s'ajoutent les PNG et index.
Les anciens fichiers de cache non référencés ne font pas partie du manifeste courant.

- CPK : réutilisation de `tools/re/CpkExtract` et CriFsV2Lib. Seul cet utilitaire est compilé,
  pas le projet du mod (dont la compilation déploie directement dans Reloaded-II).
- PAK : réutilisation du parseur `pakunpack.py`, noms de 252 octets, alignement de 64 octets.
- Conteneur à compteur : `u32 nombre`, puis `nom[32]`, `u32 taille`, données sans alignement.
  Même format que celui documenté dans P4G `database/tools/AreaArcUnpack.py`.
  Nécessaire notamment pour `camp.bin` et `name.bin` : l'ancien parseur seul les ratait.
- SPR : table des textures et table des sprites à l'en-tête ; nom de sprite sur 16 octets,
  texture à +0x14, rectangle de quatre entiers à +0x54. ×4 issu de `NAME_ENTRY.md`.
- TMX : RGBA32 et palettes 256/16 couleurs ; alpha conservé dans les PNG.
  Support ajouté aux textures 4 bits et variantes, petit quartet en premier ; référence :
  [Amicitia, lecture TMX](https://github.com/tge-was-taken/Amicitia/blob/master/Source/AmicitiaLibrary/Graphics/TMX/TMXFile.cs)
  et [PS2PixelFormatHelper](https://github.com/tge-was-taken/Amicitia/blob/master/Source/AmicitiaLibrary/PS2/Graphics/PS2PixelFormatHelper.cs).

`check_menu_catalog.py` vérifie les PNG, leurs dimensions, les références des galeries,
la cohérence des métadonnées et des comptages. Il teste également le décodage 4 bits
(ordre des couleurs, alpha) et le rejet de données tronquées. Ces contrôles hors jeu ne
valident aucun comportement vocal : aucun nouveau test joueur n'a été enregistré.
