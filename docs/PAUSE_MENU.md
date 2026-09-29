# Menu pause (« camp ») et ses sous-menus

Doc de système du menu pause, commencée le 29/09/2026 (Claude Code). Méthode générale des menus :
`MENUS.md` ; outils : `GHIDRA.md`. **Rien de ce fichier n'est encore testé en jeu** : le joueur
est au début du jeu, le menu pause complet n'y est pas encore ouvert.

## Constat d'ensemble

- Modules `src\camp\cmp*.c` (liste des fonctions par module : `gh.sh xrefs` sur les chemins).
- Les tâches des sous-menus sont créées par du code de `.arch` (chiffré), mais leurs fonctions de
  dessin, lisibles, reçoivent la structure de travail : on les accroche.
- Les lignes de liste passent par des fonctions **partagées** qui reçoivent une petite structure
  « ligne » avec un état « sous le curseur » : un lecteur par fonction partagée couvre tous les
  écrans qui l'utilisent.
- Beaucoup de nombres sont dessinés en caractères-images (`c_main_01.spr`), et en français les
  noms longs passent par le texte « resserré » de `.arch` : lire les numéros (objet, compétence)
  et demander le nom au jeu plutôt que capturer le texte.

## Racine (`CampMenu.cs`)

Voir `MENUS.md` et `SIGNATURES.md` : `CampRoot::Draw` `0x14014B350`, curseur 0-6 à +0x20, aide
du jeu `GetHardcodedText(44 + curseur)`.

## Objets (`ItemRows.cs`)

- Écran Objets (`cmpitem.c`) : dessin `0x140133D90(?, travail)`. Travail : +0x34 première ligne
  visible, +0x30 curseur dans les 5 lignes visibles, +0x50 + i × 8 liste (numéro d'objet sur 2
  octets, quantité sur 2 octets à +4), +0x3CD0 nombre d'objets. Autres listes (autres catégories)
  à +0x1050 (+0x3CD4) et +0x3850 (+0x3CD8).
- Chaque ligne visible est dessinée par `DrawItemRow(position, int, alpha, ligne)` `0x1402AD4F0`
  (`shared\item.c`) ; ligne : +0x00 numéro, +0x02 quantité (-1 : aucune), +0x16 état (1 = sous le
  curseur). Aussi appelée par un visualiseur de débogage des développeurs (`0x140179D60`).
- Nom : `GetItemName(numéro)` = cible de l'appel trouvé par la signature d'AnimatedSwine37
  (`0x14025A090`, saut vers `.arch`, appelable). Fiche d'objet : `0x14025A0D0` (numéro × 0x38 +
  table globale).
- Annonce : « Nom, 3 en stock » à chaque changement d'objet ou de quantité.
- Pas encore : description de l'objet (dessinée par `.arch`, `0x1401327B0`), choix du
  personnage qui reçoit l'objet (`0x140133900`, panneau du groupe).

## Compétences (`SkillRows.cs`)

- `DrawSkillRow(position, float, alpha, ligne, …)` `0x1402C3400` (`shared\shdskill.c`), appelée par
  l'écran Compétences (`0x140152150`) et les panneaux de compétences des Persona.
  Ligne : +0x00 sorte (0 nom seul, 1 compétence avec coût, 2 autre), +0x02 état (3 = sous le
  curseur dans l'écran Compétences, 2 normal, 4 inutilisable), +0x0A numéro de compétence.
- Écran Compétences : travail +0x3E curseur, +0x40 première ligne visible, +0x58 + i × 0xC liste,
  +0x4D8 nombre.
- Nom : `GetSkillName(numéro)` `0x1400A7F50` (`battle\data\datcalc.c`) : fiche de largeur fixe
  (0x1D en français, 0x13 dans d'autres langues).
- Pas encore : coût en PS/PV, description, lignes de sorte 0 et 2 (panneaux de Persona).

## Équipement (`EquipMenu.cs`)

- `Equip::Draw(travail)` `0x1401292F0` (`cmpequip.c`), chaque image. Travail : +0x3C membre
  choisi (indice), +0x52 + i × 2 numéros de personnage, +0x28 drapeaux (0x20 = choix de
  l'emplacement), +0x3E emplacement 0-3.
- Objet équipé : `GetEquipped(personnage, emplacement)` (`0x140259760`, saut vers `.arch`),
  appelée par ce même dessin ; nom par `GetItemName`, personnage par `GetCharacterName`.
- Annonce : « Équipement. Yuki Makoto » puis, sur un emplacement, « Arme : Épée courte, 1 sur 4 ».
- **Ordre des emplacements à vérifier** : le code dessine des icônes (0 = selon l'arme, puis
  icônes 0x23, 0x24, 0x25). Ordre supposé : arme, armure, chaussures, accessoire (celui de
  P3 FES). Le nom de l'objet annoncé permet de repérer une erreur.
- Pas encore : liste des équipements proposés (`0x14012DC20`, `0x14012E910`, `0x14012EA20`) et
  comparaison des caractéristiques.

## Statut : caractéristiques sociales (`SocialStats.cs`)

- `DrawSocialStats(position, ?, alpha, panneau)` `0x1402C8860` (`shared\shdstatus.c`), appelée
  chaque image depuis `.arch` quand le panneau est affiché. Panneau +0x0A + i × 2 = niveau 1-6
  de la caractéristique i (0 Savoir, 1 Charme, 2 Courage).
- Nom du rang : `GetRankName(i, niveau)` `0x14016A3F0` (`community\cmmmisc.c`) ; noms des
  caractéristiques : textes du jeu 97-99 (« Savoir », « Charme », « Courage »).
- Annonce à l'apparition du panneau : « Savoir : Moyen, Charme : …, Courage : … ».
- Pas encore : le reste de l'écran Statut (niveau, PV/PS, Persona, caractéristiques de combat ;
  écran lui-même dessiné depuis `.arch`, initialisation `0x140158680`).

## Liens sociaux (`SocialLinkMenu.cs`)

- `SocialLink::Draw(travail)` `0x14011AE80` (`cmpcommu.c`), chaque image. Travail : +0x18
  drapeaux (2 = liste affichée), +0x2C curseur dans les 5 lignes, +0x2E première ligne visible,
  +0x194 nombre ; entrée i à +0x44 + i × 0xC : +0x00 arcane + 1 (octet), +0x02 numéro du lien,
  +0x04 rang. Lignes dessinées par `0x14011DF20`.
- Arcane : texte du jeu 9 + arcane ; personne : `GetSLinkName(numéro)` (`0x14BC8ABE0`, dans
  `.arch`, signature d'AnimatedSwine37).
- Annonce : « Liens sociaux. Le Magicien, Kenji Tomochika, rang 2, 1 sur 5 ».
- Pas encore : fiche détaillée du lien (texte d'aide, progression +0x08).

## À faire

Persona (`cmppersona.c`, `shared\shdpersona.c`), calendrier (`cmpcalendar.c`), système (`cmpsystem.c` : état, quêtes, glossaire, config, effacer,
charger, titre ; textes `GetHardcodedText(59…65)`), noms des personnages `0x14025AD20`.
