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

## À faire

Équipement (`cmpequip.c`, dessin `0x1401292F0` : emplacements arme / armure / accessoire, noms
par `GetItemName`), Persona (`cmppersona.c`, `shared\shdpersona.c`), statut (`cmpstatus.c`),
liens sociaux (`cmpcommu.c`, noms : `GetSLinkName` de p3ppc.unhardcodedNames, dans `.arch`),
calendrier (`cmpcalendar.c`), système (`cmpsystem.c` : état, quêtes, glossaire, config, effacer,
charger, titre ; textes `GetHardcodedText(59…65)`), noms des personnages `0x14025AD20`.
