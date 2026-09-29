# Repères dans les deux projets

Étude du 29/09/2026, Codex, complétée après la clarification du joueur : P3P Access rend
Persona 3 Portable jouable avec NVDA ; P4G Access est le mod de référence pour comprendre
les systèmes similaires et conserver des commandes cohérentes. Les images extraites sont
un outil de recherche, pas le cœur du projet.

Lecture des documents de suivi, de tous les composants actuellement initialisés par
`Mod.cs`, du socle texte/mémoire/localisation et de la gestion des commandes de P4G.
Cette étude du code ne valide pas les fonctions qui attendent un test du joueur.

## Fonctionnement concret de P3P Access

Le joueur commande le jeu original. Le mod C# est chargé par Reloaded-II dans P3P.exe.
Il retrouve les fonctions par signatures, observe les états et curseurs du jeu, décode ses
textes Atlus et transmet les annonces à NVDA par Tolk. Chaque lecteur doit reconnaître
son écran actif et ne parler que lorsque l'information change.

`Mod.cs` initialise la langue du jeu, la langue du mod, le décodage, Tolk et `GameStrings`,
puis conserve les instances des lecteurs et des hooks. `Utils.cs` centralise les accès
mémoire gardés, les signatures, le journal et le contrôle du premier plan. Un échec de
signature doit désactiver le lecteur concerné. Les données variables sont lues avec des
bornes ; une adresse plausible seule ne suffit pas.

- `Dialogue` suit les fenêtres, le locuteur, les pages et les choix. `MsgText` parcourt les
  listes de lignes et de glyphes avec des limites. Les tampons sont réutilisés par le jeu :
  comparer seulement leur adresse ne permet pas de détecter tous les changements.
- `Speech` normalise le texte et conserve vingt annonces. La déduplication de l'historique
  n'empêche pas à elle seule de répéter une annonce : les lecteurs gèrent leurs transitions.
- `TitleMenu` lit le titre ; `SexSelectMenu` lit le protagoniste et la difficulté.
- `NameEntry` lit le clavier européen et les noms saisis. La signature automatique remplit
  les noms configurés puis tente Start dans le bon état ; la confirmation reste au joueur.
- `SystemMenu` lit le menu simplifié du début de partie. `CampMenu` lit les sept entrées
  du vrai menu pause et leur aide. Ce sont deux systèmes distincts ; leurs sous-menus
  ne deviennent pas accessibles automatiquement parce que la racine est lue.
- `TextCapture` collecte le texte effectivement dessiné. `SaveSlots` encadre le dessin
  de l'emplacement sélectionné pour en annoncer les informations. Il ne charge ni
  n'enregistre une sauvegarde à la place du joueur.
- `TitleBar` stabilise le titre de fenêtre pour éviter les interruptions NVDA dues aux FPS.

Les textes existants du jeu passent par `GameStrings` ou les structures du système.
Les messages propres au mod passent par `Loc.T` / `Loc.F` et `lang/*.json`. La langue du
jeu détectée dans Steam et celle choisie pour le mod sont deux réglages distincts.

## Commandes actuellement codées dans P3P

Les raccourcis du mod sont lus seulement lorsque le jeu a le premier plan.

- Maj+P : répéter la dernière annonce.
- Maj+[ et Maj+] : parcourir l'historique. Le code utilise `VK_OEM_4` et `VK_OEM_6` ;
  ces libellés américains ne garantissent pas les mêmes caractères sur un clavier AZERTY.
- Maj+M : basculer la lecture automatique des dialogues pendant la session. Les choix
  restent lus et les dialogues restent dans l'historique. Le réglage de configuration
  `AnnounceDialogue` désactive plus largement le lecteur : ce n'est pas le même mécanisme.
- LT+RT avec croix haut : répéter ; gauche/droite : historique ; bas : basculer les dialogues.
  `ControllerInput` appelle ces fonctions directement, sans simuler les raccourcis clavier.

La manette est interrogée par XInput toutes les 33 ms, avec seuils distincts d'appui et
de relâchement des gâchettes. Pendant LT+RT, le hook de `Pad::Update` masque les boutons
du jeu après leur assemblage. Les sticks sont conservés. L'absence de mouvement parasite
du curseur reste à confirmer en jeu. Les codes XInput et les codes PSP internes diffèrent.

Les commandes ordinaires du jeu viennent de `P3P.ini`, section `[ActionConfig_P3P]`.
Lecture locale effectuée sans modification : validation sur Espace/Entrée, annulation sur
C/Échap ; deux actions Start distinctes utilisent les positions 44 et 45, soit W et X
sur AZERTY. Les numéros d'action et le contexte comptent : ne pas déduire une commande
universelle du seul nom d'une touche. Voir `NAME_ENTRY.md` pour le cas du contrat.

F9 dans `TextCapture` est un diagnostic de texte rendu, pas un menu d'aide pour le joueur.
P3P ne possède actuellement ni le menu d'accessibilité F1 de P4G, ni ses commandes
d'informations, de navigation et de combat.

## Ce que les commandes de P4G apportent comme modèle

Sources consultées : `Components/ControllerInput.cs`, `HistoryKeys.cs`, `Speech.cs`,
le traitement des touches dans `SettingsMenu.cs` et `database/help_content.json`.
Les fonctions ci-dessous existent dans P4G ; cette liste ne les annonce pas disponibles
dans P3P.

- Deux gâchettes : mêmes commandes de parole à la croix. Start ouvre les réglages
  d'accessibilité. D'autres boutons commandent les aides sonores et descriptions.
- RT seul : informations de groupe, ennemis, argent, lieu/date/heure ; croix pour les
  panneaux d'information ou la grille selon le contexte, commandes de caméra et de groupe.
- LT seul : catégories et cibles de navigation, nom/distance, balise, marche automatique,
  interaction rapide et orientation. Ces fonctions dépendent des systèmes de navigation.
- F1 : menu vocal de réglages et d'aide. Flèches pour parcourir et régler, Entrée pour
  ouvrir/valider, Échap pour revenir. Les autres raccourcis cèdent la priorité au menu.

Principes réutilisables : cohérence des gestes, détection d'un nouvel appui, contrôle du
premier plan, hystérésis des gâchettes, annonces seulement utiles. La structure de P4G
est transposable, mais ses signatures, offsets et dépendances ne le sont pas tels quels.

P4G masque les entrées à l'endroit où le jeu les a réunies, car filtrer seulement XInput
ne couvre pas tous les chemins Steam Input/DirectInput. Son menu conserve le masquage
jusqu'au relâchement des touches après fermeture : sinon Échap ou validation peuvent
agir aussi sur le jeu. Certaines actions appellent directement un composant, d'autres
simulent une touche. Les flèches simulées doivent être marquées étendues pour éviter
qu'elles soient traitées comme le pavé numérique par NVDA.

P3P n'a pour l'instant que la combinaison à deux gâchettes : ne pas copier les raccourcis
RT/LT de P4G sans leurs lecteurs et leur gestion de contexte. La ville de P3P fonctionne
au curseur 2D ; la navigation 3D de P4G demande donc une adaptation de conception.

## État réel et continuité

Les résultats de `TEST_LOG.md` priment sur les anciens résumés : dialogues, premiers
menus et clavier du nom ont des tests réussis. Le test 6 est partiel ; la correction du
Start automatique attend confirmation pour les deux protagonistes. Menu système et
manette attendent un test ; menu camp, capture de texte et emplacements sont présents
mais ne doivent pas être présentés comme validés.

Les documents `SAVE_LOAD.md` et `TEXT_CAPTURE.md` mentionnés par le code sont absents
au moment de cette étude. Les compléter lors de la reprise de ces systèmes. La priorité
du joueur reste tous les menus, puis les descriptions d'images de scène, puis les étapes
suivantes de la feuille de route. Précision ultérieure du joueur : avancer par lots cohérents,
faire les contrôles techniques soi-même et regrouper les tests en jeu lorsqu'il y a de la
matière. Ne pas attendre un retour après chaque sous-menu ou petite modification.

Vérification locale du 29/09/2026 : jeux dans `D:/SteamLibrary/steamapps/common/P3P`
et `D:/SteamLibrary/steamapps/common/Persona 4 Golden`. `P3P.exe`, Ghidra 12.1.4
(avec `support/analyzeHeadless.bat`), AtlusScriptTools et CriFsLib.GUI sont présents.
Les dossiers `extracted/data_FR` et `decompiled/data_FR` et les outils de signatures,
désassemblage, références croisées, extraction et mémoire de `tools/re/` sont disponibles.
Cette vérification constate leur présence, pas une nouvelle exécution de chaque outil.

## P3P Access

Racine : `C:/Users/asdes.ASUS/Documents/SourceCode/Persona-3-Portable-Access`.

- `docs/HANDOFF.md`, `CLAUDE.md`, `docs/ROADMAP.md` : reprise, règles et état.
  Les résultats détaillés de `docs/TEST_LOG.md` priment sur les résumés devenus anciens.
- `src/p3ppc.accessibility/Mod.cs` : initialise la langue, le décodage Atlus, Tolk et les lecteurs.
- `Components/` : un lecteur par système. Dialogues et premiers menus déjà validés ;
  menu camp, capture de texte et emplacements de sauvegarde présents dans le code actuel.
- `Native/Text/` : décodage du texte Atlus, choix de langue et textes courts du jeu.
  `docs/GAME_STRINGS.md` donne les numéros des textes réutilisables.
- `Utils.cs` : lectures mémoire gardées, recherche de signatures, journal et contrôle du focus.
- `lang/` et `Loc.cs` : messages propres au mod, référence anglaise, français complet,
  contexte pour les traducteurs et repli. `tools/lang_check.py` assure la cohérence.
- `lib/tolk/`, projet Reloaded et `Template/` : infrastructure ; la compilation du mod
  écrit directement dans le dossier Reloaded-II. Ne pas compiler pour une simple exploration.
- `tools/re/` : extraction CPK/PAK, images, signatures, chaînes, références, désassemblage
  et accès mémoire externe. Le nouveau catalogue est expliqué dans `MENU_IMAGES.md`.
- `extracted/` : ressources originales et images converties, locales uniquement.
  `decompiled/` : scripts et messages décompilés, également exclus de Git.
- `.claude/` et `.agents/` : procédures et rôles ; `.github/` : vérification de traduction.
  Les changements déjà présents au début de cette session ne doivent pas être écrasés.

## P4G Access

Racine : `C:/Users/asdes.ASUS/Documents/SourceCode/Persona-4-Golden-Access`.
Code sous `p4g64.accessibility-master/p4g64.accessibility/`, documentation et données sous
`database/`. Projet consulté comme référence, sans modification.

- Socle : `Mod.cs`, `Utils.cs`, `Speech.cs`, `HistoryKeys.cs`, `ControllerInput.cs`,
  `SettingsMenu.cs`, `ModSettings.cs`, `SoundSettings.cs`. Réutiliser la conception,
  en respectant les réglages et traductions propres à P3P.
- Menu pause : `Components/CommandMenus/CommandMenu.cs`, `PlayerMenu.cs`, `PersonaMenu.cs`,
  `SocialLinkDetail.cs`, `QuestMenu.cs`. Les curseurs, structures et signatures sont propres
  à P4G ; ils ne s'appliquent pas directement aux tâches `camp` de P3P.
- Configuration : `ConfigMenu.cs` et `ConfigValueText.cs`. P4G associe curseur, onglet,
  libellé traduit du jeu et valeur dessinée. Piste pour la configuration P3P.
- Sauvegarde : `LoadScreenTracker.cs`. Exemple de suivi du curseur ; la capture du dessin
  des emplacements est l'approche actuellement commencée dans `SaveSlots.cs` de P3P.
- Texte rendu : `UiTextSpy.cs`. Collecte avec couleur et déduplication ; inspire le
  diagnostic F9 de `TextCapture.cs` dans P3P. Ne pas confondre capture de texte et d'image.
- Boutiques : `database/SHOP_SYSTEM.md`, `ShopMenu.cs`, `DaidaraCharSelect.cs`.
  Leçon : identifier les états et le véritable curseur de chaque sous-écran ; un hook de
  menu racine peut cesser d'être appelé quand une liste prend le focus.
- Velvet Room : `database/VELVET_ROOM.md`, `VelvetFusion.cs`. Le vieux `VelvetMenu.cs`
  est signalé comme retiré de l'initialisation. Suivre la documentation actuelle, pas
  seulement un nom de fichier qui semble correspondre.
- Combat : `database/BATTLE_SYSTEM.md`, `Components/Battle/` ; plusieurs lecteurs spécialisés
  partagent le contexte du combat et bornent leurs annonces aux écrans actifs.
- Ville : `database/OVERWORLD.md`, `OVERWORLD_AUTOWALK.md` et `Components/Navigation/`.
  Les données de présence actives du jeu évitent de recalculer tout son calendrier.
  P3P utilise toutefois une navigation 2D au curseur, différente de P4G.
- Donjon : `database/DUNGEON_AUTOWALK.md`, `DUNGEON_DOORS_AND_BEACON.md`, `AutoWalk/`.
  Distinguer les sections courantes de l'historique des systèmes abandonnés.
- Investigation mémoire : `database/SNAPSHOT_METHOD.md`. Instantanés externes en lecture
  seule, actions contrôlées du joueur, plusieurs observations et confirmations croisées.
- Extraction : `database/tools/AreaArcUnpack.py` décrit correctement les conteneurs à
  noms de 32 octets. `ArcExtractor.py` contient une ancienne hypothèse avec huit octets
  supplémentaires : ne pas la reprendre pour les conteneurs identifiés dans P3P.

## Méthode désormais préparée pour les menus P3P

1. Lire la doc du système P3P, son état de test et le lecteur P4G correspondant pour
   comprendre les informations et commandes nécessaires au joueur.
2. Consulter les scripts et textes du jeu, puis les images déjà extraites dans
   `extracted/menu_catalog/INDEX.md` si les libellés sont dessinés sous forme de sprites.
3. Retrouver dans P3P l'état, le curseur et les données du menu ; utiliser les textes du jeu
   quand ils existent, les images pour les libellés dessinés.
4. Documenter les signatures, protéger les lectures et annoncer seulement les changements.
5. Contrôler un lot cohérent hors jeu, déployer selon la procédure, puis proposer un test
   joueur court du lot. Garder un statut explicite pour ce qui n'a pas encore été testé.

Le catalogue réduit la recherche de ressources ; il ne remplace pas l'identification des
structures actives, ni les tests NVDA du joueur.
