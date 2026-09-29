# Persona 3 Portable Access : consignes pour Codex

Ce dépôt est développé en alternance par Claude Code et Codex. **Au démarrage, lire dans l'ordre :**

1. [docs/HANDOFF.md](docs/HANDOFF.md) : où en est le chantier et la prochaine action.
2. [CLAUDE.md](CLAUDE.md) : guide complet du projet (état, chemins, organisation, commandes,
   règles). **Il s'applique entièrement à Codex**, sauf les outils propres à Claude Code
   (section « Équivalences » ci-dessous).
3. [docs/ROADMAP.md](docs/ROADMAP.md) : feuille de route, fait / en cours / à venir.
4. La doc du système touché (`docs/<SYSTÈME>.md`) avant de modifier son code.

**À la fin de chaque session**, mettre à jour `docs/HANDOFF.md` (date, « Codex », fait, prochaine
action), `docs/ROADMAP.md` si une tâche change d'état, et la section « État actuel » de
`CLAUDE.md` si une étape est franchie. C'est ce qui permet à l'agent suivant de reprendre.

## Le joueur

- Joueur **aveugle**, lecteur d'écran **NVDA**. Répondre **en français**, phrases courtes, pas de
  grands tableaux ni de symboles décoratifs, l'essentiel d'abord.
- Il ne peut pas utiliser les interfaces graphiques (Reloaded-II…) : faire la configuration
  directement dans les fichiers.
- **Toute vérification se fait en jeu par lui** : lui donner des consignes de test pas à pas, et
  consigner le résultat dans `docs/TEST_LOG.md`.
- Écran ou texte inconnu : chercher d'abord dans les fichiers du jeu (`decompiled/`, images via
  `tools/re/spr2png.py`) et en ligne (`docs/REFERENCES.md`) ; ne demander une capture d'écran
  qu'après.
- Une étape à la fois : on ne passe à la suivante que si le test en jeu est réussi.

## Équivalences des outils Claude Code

Les dossiers `.claude/` ne sont pas chargés par Codex, mais ce sont des fichiers Markdown lisibles :

- **Skills** = procédures à suivre à la main : `.claude/skills/<nom>/SKILL.md`
  (`build-deploy`, `check-log`, `test-session`, `new-component`, `port-hook`, `snapshot-hunt`).
- **Agents** = rôles spécialisés : `.claude/agents/*.md` (`re-analyst` pour la rétro-ingénierie,
  `p4g-reference` pour retrouver le code P4G, `a11y-reviewer` pour la revue avant déploiement,
  `p3p-assistant` pour l'aide de jeu). Lire le fichier et appliquer sa méthode.
- **Hooks** : non actifs sous Codex. Faire soi-même leur travail : contexte de départ
  (git status, dernier journal via `tools/checklog.ps1`) et règles de commandes ci-dessous.
- **Mémoire de Claude** : son contenu utile est repris dans ce fichier.

## Commandes sous Windows (important)

Pièges vécus, détail dans [docs/SHELL_PITFALLS.md](docs/SHELL_PITFALLS.md) :

- Modifier les fichiers avec l'outil d'édition de Codex (apply_patch), **jamais** par
  `Set-Content` / `Out-File` / chaîne PowerShell entre guillemets doubles : l'accent grave y est
  un caractère d'échappement et abîme les chemins et le Markdown.
- Pas de `python -c "…"` dans PowerShell : écrire le script dans un fichier temporaire, ou passer
  par Git Bash : `& "C:\Program Files\Git\bin\bash.exe" -lc '…'`. **Attention** : `bash` seul
  lance WSL (`C:\Windows\System32\bash.exe`), pas Git Bash.
- Scripts `.ps1` du projet : `pwsh -NoProfile -File <script>` (PowerShell 7). Windows
  PowerShell 5 (`powershell`) casse les accents des fichiers UTF-8.
- Code de sortie 1 ne veut pas toujours dire échec : `Get-Process` quand le jeu est fermé,
  `Select-String` sans résultat. Jeu lancé ? `tasklist /NH /FO CSV /FI "IMAGENAME eq P3P.exe"`.
- Pas de chemin `$env:TEMP` (forme courte `ASDES~1.ASU` mal résolue) : chemin complet.
- Accents dans la sortie Python : variables `PYTHONUTF8=1` et `PYTHONIOENCODING=utf-8`.
- Modules Python des outils : `python -m pip install -r tools/re/requirements.txt`.

## Rappels techniques essentiels (détails dans CLAUDE.md)

- Compiler : `dotnet build src/p3ppc.accessibility/p3ppc.accessibility.csproj -c Debug`.
- Déployer : procédure `.claude/skills/build-deploy/SKILL.md`, **jeu fermé**, dossier des mods
  `D:\SteamLibrary\Reloaded-II\Mods` (variable `RELOADEDIIMODS`).
- Journal du jeu : `pwsh -NoProfile -File tools/checklog.ps1`.
- Signatures plutôt qu'adresses, toutes notées dans `docs/SIGNATURES.md`.
- Lectures mémoire toujours gardées (`Utils.cs`) : une violation d'accès fait planter le jeu.
- Hooks appelés à chaque image : ne parler que quand l'état change (sinon NVDA est coupé sans
  arrêt) ; vérifier qu'une ligne `[Speech]` n'apparaît qu'une fois dans le journal.
- Messages du mod uniquement dans `Loc.cs` (FR/EN).
- Fichiers extraits du jeu (`extracted/`, `decompiled/`) : droit d'auteur, jamais versionnés.
- git : faire un commit seulement quand le joueur le demande.
