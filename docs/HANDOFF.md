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
- **Enregistré dans git** : commit `0f38cfe` (29/09/2026). Pas de dépôt distant (pas de remote).

## Prochaine action

1. Demander au joueur les décisions en attente (fin de `ROADMAP.md`).
2. Selon sa réponse :
   - **1c tout de suite** : trouver la fonction de `P3P.exe` appelée par `CALL_BG_IMG` (piste dans
     `MENUS.md`, section « Images des scènes »), annoncer l'image de l'écran muet ;
   - **sinon** : menu pause (tâche 1 de la section « En cours » de `ROADMAP.md`). Commencer par
     lire le code des mods `p3ppc.socialStatTracker` et `p3ppc.unhardcodedNames` sur GitHub, puis
     la méthode de `MENUS.md`.

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
