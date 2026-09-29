# Point de reprise

**À lire en premier par tout agent (Claude Code ou Codex) qui reprend le chantier, et à mettre
à jour à la fin de chaque session** : date, agent, ce qui a été fait, prochaine action.

## Dernière session

- **Date et agent** : 29/09/2026, Claude Code.
- **Fait** :
  - bilan et correction des erreurs de commandes, garde-fou `shell-guard.py`, doc
    `SHELL_PITFALLS.md`, scripts `.ps1` passés en UTF-8 et lancés avec `pwsh` ;
  - découverte : les scènes sont des images fixes (`CALL_BG_IMG`), proposition de l'étape 1c ;
  - feuille de route `ROADMAP.md`, ce fichier, `AGENTS.md` pour Codex.
- **Rien de déployé ni testé en jeu** pendant cette session : la dernière version testée est
  celle du test 4 (28/09/2026, réussi).
- **Enregistré dans git** et publié sur GitHub (dépôt public https://github.com/MattrimCotton/Persona-3-Portable-Access).
- **Décisions du joueur** : tous les menus d'abord, puis l'étape 1c (images) ; copie publique GitHub.

## Session suivante (29/09/2026, Claude Code) : système de traduction

- Messages du mod sortis de `Loc.cs` vers `lang/*.json` ; réglage de langue étendu aux 9 langues
  du jeu ; `Loc.F` protège des marqueurs cassés ; vérificateur et contrôle GitHub.
- **Compilé et déjà déployé** (la compilation Debug écrit directement dans le dossier du mod),
  **pas encore testé en jeu** : au prochain lancement, vérifier que « P3P Access chargé. » et les
  menus de l'écran titre parlent toujours en français (sinon : `Lang\` absent ou illisible, voir
  les lignes `[Language]` du journal).

## Session suivante (29/09/2026, Claude Code) : début de « tous les menus »

- Carte des 273 modules de l'exe (`MENUS.md`) : modules de chaque menu de l'étape 1b repérés.
- **Saisie du nom** écrite (`NameEntry.cs`, doc `NAME_ENTRY.md`) : touche sous le curseur (lettres
  accentuées, majuscules, symboles en mots), nom tapé. **Compilé et déployé, pas encore testé.**
- Nouveau piège : l'outil Bash réduit une barre oblique inverse doublée ; bloqué par
  `shell-guard.py` (`SHELL_PITFALLS.md`, n° 11).
- **Non enregistré dans git** : traduction déjà poussée, mais pas la saisie du nom ni les docs de
  cette session.

- Test 5 : traduction et clavier réussis, mais valider est difficile. Ajouté : **signature
  automatique** (héros Yuki Makoto ou héroïne Shiomi Kotone selon IsFemc, puis Start simulé).
  **Déployé, pas encore testé.**

## Session suivante (29/09/2026, Claude Code) : menu système et manette

- Test 6 : signature réussie (nom réécrit à la main, Start pressé par le joueur), correction du
  délai de Start déployée.
- **Menu système du début de partie** (`SystemMenu.cs`, `MENUS.md`) et **manette**
  (`ControllerInput.cs`, `CONTROLLER.md`) : déployés, **pas encore testés**.
- Le joueur doute du déplacement : à examiner à l'étape 3 (ville).

## Récapitulatif (29/09/2026, Codex)

- Lecture des documents de suivi, de l'état Git et du dernier journal pour le bilan demandé.
- Aucun changement de code, compilation, déploiement ou nouveau test en jeu.
- `CLAUDE.md` mentionne aussi le menu camp, la capture de texte et les emplacements de
  sauvegarde/chargement comme compilés, mais non validés en jeu.
- Le test 6 reste **partiel** selon `TEST_LOG.md` : remplissage des noms constaté, mais
  validation Start automatique corrigée encore à retester pour les deux protagonistes.
- Prochaine action : les tests en jeu ci-dessous ; les nouveaux lecteurs compilés restent
  également à valider. La priorité demeure de terminer tous les menus.

## Session suivante (29/09/2026, Codex) : images de menus préparées en lot

- Demande du joueur : se familiariser avec les deux projets, puis préparer les images de
  menus à l'avance pour accélérer la suite. Repères écrits dans `docs/WORKSPACE_GUIDE.md`.
- Catalogue créé dans `extracted/menu_catalog/` : **1 112 planches PNG, 4 542 sprites découpés,
  247 planches-contact**. Index Markdown, galerie par système, origines et métadonnées JSON.
  Archives communes et françaises ; voir `docs/MENU_IMAGES.md` pour le périmètre et les limites.
- Outil reproductible `tools/re/menu_catalog.py`, contrôleur `check_menu_catalog.py`,
  extension de `spr2png.py` aux textures 4 bits. Deux formats d'archives gérés ; celui des noms
  de 32 octets est aussi documenté par P4G `AreaArcUnpack.py`.
- Contrôle hors jeu : 5 901 PNG et 3 593 liens HTML vérifiés ; aucune erreur de texture.
  Rectangles non découpables signalés, planches intégrales conservées. Exemples de menus
  pause, titre, nom et sauvegarde inspectés visuellement.
- Aucun changement au code des lecteurs, aucun déploiement, aucun test joueur nouveau,
  aucun commit. Modifications antérieures préservées ; projet P4G consulté sans modification.
- Prochaine action : utiliser cet index au lieu de réextraire les ressources, puis reprendre
  la validation des lecteurs déjà présents et les sous-menus de l'étape 1b.

## Session suivante (29/09/2026, Codex) : fonctionnement et commandes des deux mods

- Clarification du joueur : comprendre P3P Access en profondeur et étudier P4G Access
  comme modèle d'accessibilité, particulièrement ses commandes. L'extraction d'images
  est une aide de recherche, pas l'objectif principal du développement.
- Lecture de tous les composants actifs de P3P, du socle texte/mémoire/localisation,
  et comparaison de la parole, du clavier, de la manette et du menu de réglages P4G.
- `WORKSPACE_GUIDE.md` enrichi : architecture, commandes réellement disponibles,
  commandes P4G encore absentes, masquage des entrées, distinction jeu/mod/diagnostic.
  Configuration des touches du jeu consultée en lecture seule.
- Aucun changement de code du mod, compilation, déploiement ou nouveau test joueur.
  Les lecteurs non testés gardent ce statut ; aucun commit effectué.

## Session suivante (29/09/2026, Codex) : lecture de tous les Markdown du projet

- Lecture détaillée des 35 fichiers Markdown de documentation, procédures et rôles,
  hors ressources extraites et fichiers générés. Synthèse dans `docs/DOC_REVIEW.md`.
- Relevé des contradictions : traduction/clavier déjà testés, auto-signature partielle,
  racine camp et sauvegarde présentes mais non validées, docs techniques manquantes,
  exemples de commandes périmés et hypothèses à conserver comme telles.
- Ordre des étapes et portée de « tous les menus » explicités dans la revue. Le travail
  par lots demandé par le joueur prime sur les anciennes attentes après chaque lecteur.
- Aucun changement de code, aucune compilation, aucun déploiement ni nouveau test joueur.

## Session suivante (29/09/2026, soir, Claude Code) : lot « tous les menus », partie 1

- Lecture de l'historique Codex (`~/.codex/sessions`) : sa dernière demande (audiodescription
  des images sans texte du début du jeu) a échoué sur la limite d'utilisation, **rien n'a été
  écrit**. Le joueur l'a mise **en attente** : la faire après tous les menus.
- Travail en attente de Codex et de Claude enregistré et poussé (`9f28953`).
- **Ghidra** mis en place (`GHIDRA.md`, `tools/re/ghidra/`) : décompilation en ~15 s.
- **Menu Config** écrit (`ConfigMenu.cs`, `CONFIG.md`) : onglet, réglage, valeur, aide, valeur
  modifiée. **Compilé et déployé, pas testé.**
- Docs manquantes relevées par Codex écrites : `TEXT_CAPTURE.md`, `SAVE_LOAD.md` ; signatures
  de `CampMenu`, `GameStrings`, `TextCapture`, `SaveSlots`, `ConfigMenu` ajoutées à
  `SIGNATURES.md` (unicité contrôlée).
- Constat : les sous-menus du menu pause créent leurs tâches et fenêtres depuis la section
  chiffrée `.arch` ; leurs fonctions de dessin restent lisibles (liste des fonctions par module
  obtenue avec `gh.sh xrefs` sur les chemins `src\camp\*.c`).
- Le joueur est au début du jeu : le vrai menu pause n'y est pas encore disponible. Le lot
  testable tout de suite = écran titre → Config / Charger, menu des commandes → Config /
  Charger / Sauvegarde rapide / Écran titre.
- Suite de la même session : sous-menus du menu pause écrits (doc `PAUSE_MENU.md`) :
  Objets (`ItemRows`), Compétences (`SkillRows`), Équipement (`EquipMenu`), caractéristiques
  sociales de l'écran Statut (`SocialStats`), Liens sociaux (`SocialLinkMenu`), Persona
  (`PersonaMenu`), Système (`CampSystemMenu`). Noms tirés du jeu (`Native/Text/GameNames.cs` :
  objets, compétences, Persona, personnages, liens sociaux, objet équipé). **Compilés et
  déployés, aucun testé.**
- **Défaut corrigé avant tout test** : l'accroche de `DrawText` (capture de texte) ne transmettait
  que 8 des 9 à 11 arguments ; le 9e est un pointeur de sortie écrit par le jeu → risque de
  plantage. Nouvelle règle dans `CLAUDE.md` / `AGENTS.md` (« Accroche = tous les arguments »).
  Toutes les autres accroches revérifiées avec Ghidra.
- Accroche du saut vers le texte « resserré » (`0x140232DC0`) essayée puis retirée : octets de
  protection juste après, qu'une accroche longue écraserait.
- Pas faits : date et heure (dessinées par `.arch`), calendrier, quêtes, glossaire, écran Statut
  complet, fiches détaillées, liste des équipements proposés, descriptions d'objets.

## Prochaine action

**Test regroupé proposé au joueur (29/09/2026, Claude Code)**, tout accessible dès le début :

1. Écran titre → Config : onglets (touches d'onglet), lignes, valeurs gauche / droite, aide.
   Quitter le Config (la fenêtre « Enregistrer les modifications ? » est-elle lue ?).
2. Écran titre → Charger : emplacements (« Emplacement 1… »), question « Charger ce fichier ? ».
3. En jeu, menu des commandes (`SystemMenu`) : entrées, Config, Sauvegarde rapide.
4. Manette : LT + RT + croix (répéter, historique, dialogues).
5. Si possible, F9 (TextSpy, mode débogage) sur la fenêtre de fin du Config et l'écran de
   sauvegarde, pour les textes non encore lus.

Le menu pause complet (objets, compétences, équipement, statut, liens sociaux, Persona,
système) sera testé quand le jeu l'aura ouvert. À la prochaine session sans test : date et heure,
calendrier, quêtes, glossaire, fiches détaillées ; puis l'audiodescription de l'ouverture
(demande du joueur mise en attente, voir mémoire du projet).

Dernière précision du joueur (29/09/2026, consignée par Codex) : il ne peut pas tester
chaque petite avancée. Développer des lots substantiels et faire les contrôles techniques
avant de lui proposer un parcours de test regroupé. Cette consigne remplace les anciennes
formulations imposant son retour à chaque sous-étape.

1. Reprendre les lecteurs du menu pause et de sauvegarde, puis les sous-menus manquants
   de l'étape 1b, en constituant un lot cohérent. Exploiter directement P3P.exe, ses archives,
   les textes décompilés et les outils locaux ; P4G sert de modèle de comportement.
2. Vérifier les signatures, accès mémoire, transitions et traductions ; compléter la doc.
   Proposer ensuite un test court du lot, incluant les lecteurs antérieurs non validés
   lorsque le parcours s'y prête. La signature du contrat pourra être retestée à une
   prochaine nouvelle partie, sans imposer de recommencer maintenant.

Présence vérifiée des deux jeux dans la bibliothèque Steam sur D:, de Ghidra (mode sans
interface), AtlusScriptTools, CriFsLib.GUI et des extractions françaises. Consignes alignées
dans AGENTS.md, CLAUDE.md, ROADMAP.md et WORKSPACE_GUIDE.md. Aucun nouveau code, déploiement
ou test en jeu pendant cette mise au point.

## Boucle de travail (identique pour tous les agents)

1. Lire la doc du système (`docs/<SYSTÈME>.md`) et l'équivalent P4G
   (`C:\Users\asdes.ASUS\Documents\SourceCode\Persona-4-Golden-Access`).
2. Rétro-ingénierie : fichiers du jeu décompilés (`decompiled/`), outils `tools/re/`, mods
   `p3ppc.*`. Noter toute signature dans `SIGNATURES.md`.
3. Écrire le lecteur (`src/p3ppc.accessibility/Components/`), messages dans `lang/*.json`
   via `Loc.T` / `Loc.F`.
4. Compiler, puis déployer **jeu fermé** (procédure : `.claude/skills/build-deploy/SKILL.md`).
5. Donner au joueur des consignes de test courtes (`.claude/skills/test-session/SKILL.md`).
6. Après son retour : lire le journal (`pwsh -NoProfile -File tools/checklog.ps1`), consigner le
   test dans `TEST_LOG.md`, mettre à jour `ROADMAP.md`, ce fichier et l'état dans `CLAUDE.md`.
