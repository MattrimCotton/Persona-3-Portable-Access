# Menus (étape 1b)

Doc de système des menus de P3P. Un lecteur par écran, dans `src/p3ppc.accessibility/Components/`.
Liste des menus à couvrir : `PLAN.md`, étape 1b.

## Méthode pour trouver un menu (28/09/2026)

1. **Chemins des fichiers source restés dans l'exe.** Les assertions du jeu gardent le chemin du
   fichier C (`e:\projects\p3p-pc\p3p_steam\p3p\skeleton\src\...\xxx.c`). `tools/re/strfind.py
   "xxx.c"` donne l'adresse de la chaîne, `tools/re/xref.py <adresse>` toutes les fonctions du
   module qui l'utilisent.
2. **Structure de travail.** Les écrans sont des tâches : la fonction de mise à jour reçoit la tâche
   dans `rcx`, et `tâche +0x48` pointe vers la structure de travail de l'écran (étape, curseurs).
   On accroche la mise à jour, on laisse le jeu la faire, puis on lit la structure (lectures
   gardées) et on parle quand l'étape ou le curseur change.
3. **Bibliothèque de listes** (`sdkliststate.c`) : beaucoup d'écrans gèrent leur curseur avec
   elle (121 appels). La structure de liste est souvent recopiée sur la pile à chaque image, et
   le menu garde son propre curseur : c'est ce champ qu'il faut lire.
4. **Libellés dessinés en images.** Quand les entrées sont des images (`.spr`), les noms viennent de
   `Loc.cs`. Pour vérifier l'ordre : extraire le `.spr` (`tools/re/pakunpack.py`), lire les noms
   des sprites dans le fichier, et la table des sprites dessinés dans le code.
5. **Textes du jeu** : les messages (`.bmd`) décompilés dans `decompiled/` (voir `REFERENCES.md`).

## Écran titre (`TitleMenu.cs`)

- `Title::Update` `0x14024BCA0` (titledraw.c). Structure : `+0x00` étape, `+0x28` curseur.
- Étape 7 : « Appuyez sur une touche » (annoncé en entrant). Étape 9 : menu.
- Entrées, de haut en bas : Nouvelle partie, Charger, Continuer, Config, Quitter (images
  `NEW GAME`, `LOAD GAME`, `RESUME`, `config`, `EXIT` de `title/menu.spr`, table des sprites à
  `0x1407D6F60`). Curseur de départ : Continuer s'il y a une sauvegarde rapide, sinon Charger s'il y
  a des sauvegardes, sinon Nouvelle partie.
- Pas encore traité : Licence et Localisation (ouverts par d'autres touches, étapes 0x12 et 0x13),
  entrée « Continuer » grisée quand elle n'est pas disponible.

## Choix du sexe et de la difficulté (`SexSelectMenu.cs`)

- `SexSelect::Update` `0x1402AA210` (sexselect.c). Structure : `+0x20` étape, `+0x24` curseur du
  sexe, `+0x26` curseur de la difficulté, `+0x18` numéro d'exécution des messages.
- Étape 5 : choix du sexe (0 masculin à gauche, 1 féminin à droite ; la souris choisit selon
  x ≥ 480). Étape 9 : difficulté, 0 Débutant à 4 Maniaque, départ sur Normale.
- Les explications et les Oui / Non sont des fenêtres de message, lues par `Dialogue.cs`
  (`title/sex_select.bin` → `sex_select.bmd`, message affiché = curseur + 2 pour le sexe,
  curseur + 5 pour la difficulté).
- Pas encore lu : les descriptions des difficultés (`sex_select_help.bmd`, affichées par un autre
  chemin que les fenêtres de message).

## À faire

Menu pause (compétences, objets, équipement, Persona, statut, liens sociaux, calendrier,
réglages), sauvegarde / chargement, messages système, saisie du nom, écrans de chargement, argent,
date et heure.

## Écran muet après « Maintenant, amusez-vous bien en jouant. »

Juste après le choix de la difficulté, environ 50 s avant « Terminus, dans la soirée... », pas de
musique, Entrée une ou deux fois (tests 3 et 4). Recherches du 28/09/2026, sans résultat :
- aucun message de `sex_select.bmd` ni de script d'événement ne correspond ; la scène de la gare
  (`event\e100\e100_001`) commence par `BGM(109)` (donc avec musique) ; `e000` est une scène de
  test et `e080` la fin du jeu (texte « La Mort ne cesse jamais de rôder... » affiché par
  `EVT_FUNCTION_0051`, fonction utilisée nulle part ailleurs) ;
- en ligne, un joueur décrit un « texte d'ouverture » affiché après le choix du personnage et de la
  difficulté, puis un fondu au noir (forum wololo, PSP) : le texte exact n'est pas cité ;
- aucune image candidate trouvée dans `data_FR` (`limit\TIMELIMIT01.spr` = minuteur « TIME LIMIT »).

**Piste la plus probable (Game UI Database, lue avec Claude in Chrome)** : la seule image
« Cutscenes & Story » de P3P est une horloge bleue aux chiffres romains sur fond noir, aiguilles
juste avant minuit, sans texte (l'Heure Sombre). Elle est classée juste avant « > Terminal
station, evening... ». À confirmer par une capture ; si c'est bien elle, il suffira d'annoncer
« Horloge : minuit approche » (texte du mod) pendant cet écran.

Suite : capture d'écran en jeu. Outil pour regarder une image du jeu : `tools/re/spr2png.py`.

**Images des scènes (29/09/2026).** Les scripts d'événements (`decompiled/data_FR/event/**/*.flow`)
affichent les scènes par `CALL_BG_IMG(a, b, c, d)` : 1 774 appels, 355 combinaisons différentes.
La scène de la gare (`e100_001`) enchaîne `CALL_BG_IMG(38, 1, 0, 2)`, `(38, 1, 0, 3)`… avec
`MSG_WND_DSP` entre deux, `FADE`, `BGM` et `FUNCTION_000D` (attente probable). Les images avec
texte sont dans `data_FR` (`field2d/bg/bNN_*.abin`, préfixe = premier argument ?) ; les autres
sont sans doute dans `data\umd0.cpk` / `umd1.cpk` (communs à toutes les langues, pas encore
ouverts). À faire si le joueur valide : trouver la fonction de P3P.exe appelée par
`CALL_BG_IMG`, vérifier la correspondance numéros → fichiers, puis annoncer une description
(`data/`, FR/EN) au changement d'image, avant le texte.

Note pour l'étape 3 (ville) : les textes des personnages de la ville sont dans
`field2d/bg/*.abin` et `field2d/*.bin` (encodage Atlus), pas dans des `.bmd`.
