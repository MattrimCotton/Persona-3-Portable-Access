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

## Prochaine action

1. Test en jeu : nouvelle partie jusqu'au contrat, avec le héros puis avec l'héroïne : la
   signature doit se faire seule, puis la confirmation du jeu doit être lue (elle devrait donner
   l'ordre nom / prénom). Voir `NAME_ENTRY.md`.
2. En parallèle : **menu pause** (`camp\*.c`, voir `MENUS.md`), puis sauvegarde (`memcard\*.c`),
   date et heure (`blbrd\date\dtdraw.c`). Lire d'abord le code des mods `p3ppc.socialStatTracker`
   et `p3ppc.unhardcodedNames` sur GitHub. Les images de scène (1c) ne viennent qu'après la fin
   de tous les menus.

## Boucle de travail (identique pour tous les agents)

1. Lire la doc du système (`docs/<SYSTÈME>.md`) et l'équivalent P4G
   (`C:\Users\asdes.ASUS\Documents\SourceCode\Persona-4-Golden-Access`).
2. Rétro-ingénierie : fichiers du jeu décompilés (`decompiled/`), outils `tools/re/`, mods
   `p3ppc.*`. Noter toute signature dans `SIGNATURES.md`.
3. Écrire le lecteur (`src/p3ppc.accessibility/Components/`), messages dans `Loc.cs`.
4. Compiler, puis déployer **jeu fermé** (procédure : `.claude/skills/build-deploy/SKILL.md`).
5. Donner au joueur des consignes de test courtes (`.claude/skills/test-session/SKILL.md`).
6. Après son retour : lire le journal (`pwsh -NoProfile -File tools/checklog.ps1`), consigner le
   test dans `TEST_LOG.md`, mettre à jour `ROADMAP.md`, ce fichier et l'état dans `CLAUDE.md`.
