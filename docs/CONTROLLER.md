# Manette

Demande du joueur (29/09/2026) : utiliser les fonctions du mod à la manette. Lecteur :
`Components/ControllerInput.cs`, sur le modèle de `ControllerInput.cs` de P4G Access. **Déployé
le 29/09/2026, pas encore testé.**

## Raccourcis

Modificateur : **les deux gâchettes tenues ensemble (LT + RT, L2 + R2 sur PlayStation)**, comme
dans P4G Access.

- LT + RT + croix haut : répéter la dernière phrase (clavier : Maj+P).
- LT + RT + croix gauche / droite : historique de la parole (Maj+[ / Maj+]).
- LT + RT + croix bas : lecture des dialogues activée / désactivée (Maj+M).

À ajouter au fil des étapes (menu F1, informations de combat, navigation), en gardant les
raccourcis de P4G quand ils ont un équivalent.

## Fonctionnement

- Lecture de la manette par XInput (`xinput1_4.dll`), toutes les 33 ms, seulement quand la
  fenêtre du jeu est au premier plan. Seul l'emplacement connu est interrogé ; nouvelle recherche
  toutes les 2 s si aucune manette (interroger un emplacement vide ralentit le jeu : leçon P4G).
- Seuils des gâchettes avec hystérésis : appui au-delà de 45, relâché en dessous de 20.
- **Le jeu ne doit pas réagir aux mêmes boutons** : accroche de la mise à jour de la manette du
  jeu `Pad::Update` (`0x1403A68B0`) ; juste après, tant que LT + RT sont tenues, mise à zéro des
  masques de boutons du bloc manette (`0x143624540`).

## Bloc manette du jeu (`0x143624540`, trouvé le 29/09/2026)

- Rempli à chaque image par `0x1403A68B0` (copie de 0x140 octets par manette).
- Masques de boutons (codes PSP : 0x8 Start, 0x10 haut, 0x20 droite, 0x40 bas, 0x80 gauche,
  0x100 L, 0x200 R, 0x1000 triangle, 0x2000 rond, 0x4000 croix, 0x8000 carré) : `+0x00`, `+0x04`
  (pressés à cette image, le plus lu), `+0x08`, `+0x0C`, et une seconde série `+0xE0` à `+0xFC`
  (`+0xF8` : répétition automatique, utilisée par les curseurs). Rôle exact de chacun à confirmer.
- `+0x10` à `+0x15` : sticks analogiques (0x80 = centre), jamais modifiés par le mod.
- Le clavier passe par les mêmes codes : `P3P.ini`, section `[ActionConfig_P3P]` (voir
  `NAME_ENTRY.md`). Aucune action du jeu n'utilise Select (0x1).

## À vérifier au premier test

- La manette est vue par XInput (manette Xbox, ou autre manette à travers Steam Input).
- LT + RT seules ne déclenchent rien dans le jeu, et la croix ne déplace pas le curseur du jeu
  pendant un raccourci.
