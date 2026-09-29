# Persona 3 Portable Access — guide pour Claude

Mod d'accessibilité qui rend **Persona 3 Portable (PC, Steam)** jouable par un joueur aveugle avec
NVDA. Mod **Reloaded-II** (C#, .NET) chargé avec **Persona Essentials**, sur le modèle de
**Persona 4 Golden Access**. Licence GPL-3.0 (crédits : AnimatedSwine37, Haru).

Le plan complet (faisabilité, étapes, risques) est dans [PLAN.md](PLAN.md). **Le lire avant toute
décision d'architecture.**

**Reprise et relais avec Codex** (29/09/2026) : le chantier alterne entre Claude Code et Codex.
Au début d'une session, lire [docs/HANDOFF.md](docs/HANDOFF.md) (point de reprise) et
[docs/ROADMAP.md](docs/ROADMAP.md) (fait / en cours / à venir, diagramme). **À la fin de chaque
session, mettre à jour `HANDOFF.md`** (date, agent, fait, prochaine action) et `ROADMAP.md` si une
tâche change d'état. Codex lit [AGENTS.md](AGENTS.md), qui renvoie ici : y reporter toute règle
nouvelle qui vaut aussi pour lui.

## État actuel

- **Images de menus préparées en lot** (29/09/2026, Codex) : catalogue local sous
  `extracted/menu_catalog/` (1 112 planches, 4 542 éléments découpés, index et provenance),
  outils `tools/re/menu_catalog.py` / `check_menu_catalog.py`. Lire `docs/MENU_IMAGES.md`
  avant une nouvelle recherche d'image ; repères des deux projets dans `docs/WORKSPACE_GUIDE.md`.
  Ressources extraites et contrôlées hors jeu, sans nouveau déploiement ni test de lecteur.
- **Étape 0 (prérequis)** : P3P Steam installé (`D:\SteamLibrary\steamapps\common\P3P`, le 28/09/2026,
  avec les langues `data_EN`, `data_FR`…). `P3P.exe` ajouté dans Reloaded-II le 28/09/2026
  (`Reloaded-II\Apps\p3p.exe\AppConfig.json`, écrit à la main) avec Persona Essentials
  (`p5rpc.modloader` 2.9.4) et ses dépendances ; premier lancement par Reloaded le même jour.
  Variables vérifiées : `RELOADEDIIMODS`, `GHIDRA_HOME`, `JAVA_HOME` (JDK 25).
- **Étape 1 (le socle : dialogues puis tous les menus)** : en cours. Décision du joueur
  (28/09/2026) : rien d'autre (ville, combats, Tartarus) tant que les dialogues (1a) **et** tous
  les menus (1b) ne sont pas lus correctement. Précision du 29/09/2026 : développer et
  contrôler des lots cohérents avant de demander un test joueur, sans arrêt à chaque sous-étape.
  - **1a (dialogues)** : **réussi au test 3** (28/09/2026) : messages, pages, orateur, accents,
    choix Oui / Non (`docs/TEST_LOG.md`). `Components/Dialogue.cs` accroche `MsgWindow::DrawAll` (voir `docs/DIALOGUE.md`). Le mod est
    activé dans `Apps\p3p.exe\AppConfig.json` ; mode débogage activé dans
    `Reloaded-II\User\Mods\p3ppc.accessibility\Config.json` pour les tests.
  - **1b (menus)** : en cours, méthode et état dans `docs/MENUS.md`. Écran titre (`TitleMenu.cs`)
    et choix du sexe / difficulté (`SexSelectMenu.cs`) **réussis au test 4** (28/09/2026). Saisie du nom (`NameEntry.cs`) **réussi au test 5** (29/09/2026) : clavier d'entrée du nom au début du jeu, variante
    européenne (FR, DE, IT, ES), annonce de la touche sous le curseur et du nom saisi. **Auto-signature** (`AutoSignContract`, activée par défaut, **partiel au test 6** (29/09/2026) : noms écrits, Start automatique corrigé ensuite, à retester) : noms selon le personnage choisi (héros **ou** héroïne, via l'indicateur `IsFemc`, `0x143387510`) = `HeroLastName`/`HeroFirstName` (Yuki / Makoto, héros) ou `HeroineLastName`/`HeroineFirstName` (Shiomi / Kotone, héroïne, défaut 2023) ; le mod écrit les deux noms et le joueur presse Start ; délai Start corrigé le 29/09/2026 (documentation complète : `docs/NAME_ENTRY.md` ; anglais et langues asiatiques non faits).
  - **Lot du 29/09/2026** (**à tester ensemble**) : ensemble de composants compilés / compilés et déployés, tous accessibles dès le début du jeu, préparés pour validation en jeu :
    - **Menu système** (`SystemMenu.cs`) : bouton du menu des commandes ; entrées annoncées avec leur position (doc : `docs/MENUS.md` § « Menu système du début de partie »).
    - **Menu camp** (`CampMenu.cs`) : menu pause avec liste principale (Skill, Item, Persona, Equip, Status, Social link, System) ; annonce l'entrée courante avec sa position (doc : `docs/MENUS.md`).
    - **Menu Config** (`ConfigMenu.cs`) : réglages PC (onglets Audio, Jeu, Graphismes, Affichage, Clavier, Manette). Lit l'onglet, le réglage sous le curseur avec sa valeur et sa ligne d'aide (textes du jeu), puis la nouvelle valeur à gauche / droite. Touches Clavier/Manette : nom de l'action seulement (doc : `docs/CONFIG.md`).
    - **Écran Sauvegarde/Chargement** (`SaveSlots.cs`) : annonce le slot sélectionné (numéro, puis date, heure, niveau, durée, lieu… ou « AUCUNE DONNÉE »). Accroche `DrawSlot` (0x140270770) via signature ; collecte les textes avec `TextCapture` pendant le dessin du jeu (doc : `docs/SAVE_LOAD.md`).
    - **Écran Équipement** (`EquipMenu.cs`) : écran pause Équip (camp\cmpequip.c) annonce le personnage équipé, puis l'emplacement sous le curseur et l'objet équipé. Hook `Draw` (0x1401292F0 − 0x64) sur la fonction de dessin (doc : `docs/PAUSE_MENU.md`).
    - **Statistiques sociales** (`SocialStats.cs`) : panneau des stats sociales (Savoir/Academics, Charme/Charm, Courage/Courage) à l'écran pause Status, annonce les rangs quand le panneau apparaît ou qu'un rang change (doc : `docs/PAUSE_MENU.md`).
    - **Écran Liens sociaux** (`SocialLinkMenu.cs`) : écran pause Liens sociaux (camp\cmpcommu.c) annonce l'arcane, le personnage et le rang du lien sous le curseur, ainsi que sa position. Hook `Draw` (0x14011AE80 − 0x4B) sur la fonction de dessin (doc : `docs/PAUSE_MENU.md`).
    - **Capture de texte** (`TextCapture.cs`) : intercepte les six variantes de `DrawText` du jeu pour lire les menus sans décoder les données. Inclut **TextSpy** (mode débogage) : F9 enregistre les textes dessinés pendant 20 s. Détail : `docs/TEXT_CAPTURE.md`.
    - **Manette** (`ControllerInput.cs`) : LT + RT pour la parole/historique/muet, croix haut pour répéter (doc : `docs/CONTROLLER.md`).
  - **Ghidra** (29/09/2026) : projet analysé de P3P.exe (sans la section chiffrée `.arch`), décompilation en une commande : `sh tools/re/ghidra/gh.sh decomp|xrefs SORTIE ADRESSES` (doc : `docs/GHIDRA.md`). Une partie du code des menus est dans `.arch` (illisible).
  - Déjà en place : parole et historique (`Speech.cs`, Maj+P répéter, Maj+[ / Maj+] historique,
    Maj+M couper les dialogues, dans `HistoryKeys.cs`), contrôle manette (`ControllerInput.cs`,
    LT + RT modifier pour les mêmes fonctions, voir `docs/CONTROLLER.md`), messages du mod en FR/EN
    (`Loc.cs`), correction du titre fenêtre pour éviter que NVDA le ré-annonce à chaque image
    (`TitleBar.cs`, test 3, 28/09/2026).
- Mettre à jour cette section à chaque étape franchie (voir « Tenir ce fichier à jour »).

## Chemins utiles

| Quoi | Où |
|---|---|
| Dépôt GitHub (public) | https://github.com/MattrimCotton/Persona-3-Portable-Access (branche `master`, pousser après chaque commit ; jamais de fichiers du jeu) |
| Projet de référence P4G | `C:\Users\asdes.ASUS\Documents\SourceCode\Persona-4-Golden-Access` |
| Code du mod P4G (modèle) | `...\Persona-4-Golden-Access\p4g64.accessibility-master\p4g64.accessibility` |
| Docs techniques P4G | `...\Persona-4-Golden-Access\database\*.md` (BATTLE_SYSTEM, SNAPSHOT_METHOD, DUNGEON_*…) |
| Jeu P3P | `D:\SteamLibrary\steamapps\common\P3P` (exe : `P3P.exe`) |
| Reloaded-II | `D:\SteamLibrary\Reloaded-II` (mods : `Mods\`, variable `RELOADEDIIMODS`, corrigée le 28/09/2026 : elle pointait vers un ancien dossier du Bureau) |
| Lancer le jeu moddé | raccourci Bureau « Persona 3 Portable (Reloaded) » = `Reloaded-II.exe --launch "...\P3P\P3P.exe"` |
| Journaux Reloaded | `%APPDATA%\Reloaded-Mod-Loader-II\Logs` |
| Réglages et touches du jeu | `%LOCALAPPDATA%\SEGA\P3P\steam\1541465250\P3P.ini` (`[ActionConfig_P3P]` : touches par défaut, Start = W ou X en AZERTY ; détail dans `docs/NAME_ENTRY.md`) |
| Ghidra 12.1.4 (Java 25) | `C:\Users\asdes.ASUS\Tools\ghidra_12.1.4_PUBLIC` (variable `GHIDRA_HOME`, mode sans interface : `support\analyzeHeadless.bat`) |
| Atlus Script Tools, CriFsLib.GUI | `C:\Users\asdes.ASUS\Tools\AtlusScriptTools`, `...\Tools\CriFsLib.GUI` (installés le 28/09/2026). Textes extraits dans `extracted/` et `decompiled/` (ignorés par git, droit d'auteur) : méthode dans `docs/REFERENCES.md` |
| Tolk (DLL à copier) | `D:\SteamLibrary\Reloaded-II\Mods\p4g.golden.access` (`Tolk.dll`, `TolkDotNet.dll`, `SAAPI64.dll`) ; le sous-module des sources P4G est incomplet |

## Organisation du dépôt

`lib/tolk/` contient Tolk et les pilotes des lecteurs d'écran (versionnés malgré `*.dll` dans `.gitignore`).

| Chemin | Contenu |
|---|---|
| `src/p3ppc.accessibility/` | Le mod (projet SDK .NET, point d'entrée `Mod.cs`, fonctions sous `Components/`, accès mémoire sous `Native/`, localization via `Loc.cs`). |
| `src/p3ppc.accessibility/Loc.cs` | Messages du mod chargés depuis `lang/<steam_language_id>.json` (FR, EN, DE, IT, ES, JA, KO, ZH). Réglage Langue, Auto = langue Steam du jeu. Lookup: langue choisie → EN → clé. Aucun message codé en dur. Méthodes : `T(key)` (texte brut), `F(key, args)` (avec placeholders). Voir `docs/LOCALIZATION.md` pour les règles d'ajout et la validation. |
| `docs/` | Documents « source de vérité » par système (un `.md` par système : `DIALOGUE.md`, `MENUS.md`, `PAUSE_MENU.md`, `BATTLE_SYSTEM.md`, `TARTARUS.md`…), plus `SIGNATURES.md` (toutes les signatures et adresses trouvées), `TEST_LOG.md` (résultats des tests en jeu), `REFERENCES.md` (sources fiables : fichiers du jeu, docs de modding, guides ; à consulter avant de chercher ailleurs), `LOCALIZATION.md` (système de traduction : fichiers, règles, validation), `WORKSPACE_GUIDE.md` (repères des projets P3P et P4G, commandes), `DOC_REVIEW.md` (revue des docs et contradictions relevées), `SHELL_PITFALLS.md` (pièges des commandes sous Windows et parades). |
| `lang/` | Traductions des messages du mod, un fichier par langue (`english.json` référence, `french.json` toujours complet, `context.json` pour les traducteurs, guide `lang/README.md`). Vérifier : `python tools/lang_check.py --complete french` (aussi lancé sur GitHub). |
| `data/` | Fichiers JSON chargés par le mod à l'exécution (aide F1, tables de noms…). |
| `tools/` | Scripts : `checklog.ps1` (journal Reloaded), `lang_check.py` (valider les fichiers de traduction `lang/*.json` contre la référence anglaise, `python tools/lang_check.py [--complete fr] [--complete <id> ...]`), `re/CpkExtract/` (extraire une archive CPK du jeu, `dotnet run`), `re/pakunpack.py` (ouvrir les archives PAK `.bin`/`.pak`, `-r` récursif), `re/decompile-texts.ps1` (pwsh 7, décompiler les scripts `.bf` et messages `.bmd` d'une archive P3P avec Atlus Script Tools), `re/sigscan.py` (tester une signature dans un exe), `re/strfind.py` + `re/xref.py` (trouver une chaîne puis le code qui l'utilise), `re/spr2png.py` (convertir une image du jeu `.spr`/`.tmx` en PNG pour lire un texte dessiné), `re/disasm.py` (désassembler à une adresse), `re/p3pmem.py` (lire / écrire la mémoire du jeu en cours, fonctions `rd` / `wr` via `from p3pmem import rd, wr`), `re/menu_catalog.py` + `re/check_menu_catalog.py` (produire / vérifier le catalogue d'images de menus, voir `docs/MENU_IMAGES.md`), `re/ghidra/prepare.py` (préparer P3P.exe pour Ghidra : section `.arch` déclarée vide, usage : `python tools/re/ghidra/prepare.py [--exe CHEMIN] [--out DOSSIER]`), `re/ghidra/gh.sh` (décompiler ou lister les références dans le projet Ghidra analysé, `sh tools/re/ghidra/gh.sh decomp|xrefs FICHIER_SORTIE 0x... [0x...]`, ≈ 15s/appel, usage : `docs/GHIDRA.md`), `re/requirements.txt` (dépendances Python pour `disasm.py`, `xref.py`, `spr2png.py` : `python -m pip install -r tools/re/requirements.txt`). |

## Commandes

```bash
# Compiler
dotnet build src/p3ppc.accessibility/p3ppc.accessibility.csproj -c Debug
# Journal Reloaded le plus récent de P3P, avec les erreurs
pwsh -NoProfile -File tools/checklog.ps1
# Valider les traductions (local)
python tools/lang_check.py --complete french
```

Déployer : skill `/build-deploy`. Le jeu doit être **fermé** (sinon la DLL est verrouillée).
Après publication, le skill exécute `python tools/lang_check.py --complete french` pour valider
les traductions avant de lancer le jeu. Effectuer les contrôles techniques disponibles
(signatures dans l'exécutable, structures, compilation, traductions) avant de solliciter le
joueur. Regrouper la validation en jeu sur un lot substantiel (skill `/test-session`).
Un contrôle hors jeu ne vaut pas validation de l'expérience NVDA. Les fichiers de traduction sont validés
automatiquement par le workflow GitHub Actions `.github/workflows/lang-check.yml` à chaque push/PR.

## Règles du projet

- **Réutiliser avant d'écrire.** Pour toute fonction, regarder d'abord comment P4G Access l'a fait
  (code + doc dans `database/`). Ce qui ne dépend pas du jeu (Speech/Tolk, réglages, sons, menu F1,
  entrée manette, `IsReadable`, SigScan) se copie tel quel ; le reste se transpose.
- **Signatures plutôt qu'adresses en dur.** `P3P.exe` n'a pas d'ASLR (base fixe `0x140000000`,
  vérifié le 28/09/2026) mais une mise à jour Steam déplace les adresses. Toute signature ou adresse trouvée est notée dans `docs/SIGNATURES.md` (fonction, signature,
  source, date, vérifiée oui/non).
- **Lectures mémoire toujours gardées** : `AccessViolationException` ne se rattrape pas en .NET 9.
  Passer par `IsReadable` / ReadProcessMemory sur soi-même, et reprendre dès le départ la garde
  « transition de zone » de P4G (cause de plantages, voir `DUNGEON_DOORS_AND_BEACON.md`).
- **Accroche = tous les arguments** (leçon du 29/09/2026, `TEXT_CAPTURE.md`) : le délégué d'un
  hook déclare au moins tous les arguments que la fonction lit (pile comprise, vérifier avec
  Ghidra / `disasm.py`), sinon l'appel à l'original transmet des valeurs quelconques (plantage
  si c'est un pointeur de sortie). Types : float si la fonction lit un registre `xmm`.
- **Hooks appelés à chaque image** : ne parler que quand l'état change (leçon du test 1 : un
  message renvoyé 60 fois par seconde coupe la voix de NVDA, seul le braille suit). Vérifier dans
  le journal qu'une ligne `[Speech]` n'apparaît qu'une fois.
- **Écran ou texte inconnu** (demande du joueur, 28/09/2026) : chercher d'abord soi-même dans les
  fichiers du jeu (`decompiled/`, images via `spr2png.py`) et en ligne (`docs/REFERENCES.md`) ;
  la capture d'écran en jeu ne vient qu'après.
- **Commandes** (22 échecs le 28/09/2026, détail dans `docs/SHELL_PITFALLS.md`) : fichiers avec
  Read / Grep / Edit / Write ; git, python, dotnet et scripts dans **Bash** (Python en heredoc) ;
  PowerShell seulement pour ce qui l'exige, en commandes courtes, et les `.ps1` via
  `pwsh -NoProfile -File`. Tout `.ps1` force l'UTF-8 (`[Console]::OutputEncoding`,
  `-Encoding UTF8`). Relire un fichier juste avant de l'éditer. Pour tout texte contenant des
  chemins Windows, utiliser Edit / Write plutôt que générer le texte en Bash (la barre oblique
  inverse est réduite, cassant les chemins et le Markdown). Un outil différé (Monitor…) se
  charge avec ToolSearch avant l'appel. Le hook `shell-guard.py` refuse les pièges connus.
- **Mods d'AnimatedSwine37 (`p3ppc.*`)** : source de signatures déjà validées sur P3P Steam. Les
  consulter sur GitHub avant de chercher une fonction à la main.
- **Traduction** (29/09/2026) : aucun texte parlé en dur ; tout nouveau message = une clé dans
  `lang/english.json`, `lang/french.json` et `lang/context.json`, puis `tools/lang_check.py`
  (détail : `docs/LOCALIZATION.md`). Ne jamais réordonner l'énumération `ModLanguage`.
- **Parole** : phrases courtes, l'info importante en premier, pas de répétition inutile ; ne pas
  couper une annonce importante. Le jeu existe en plusieurs langues (dont le français, `data_FR`) :
  le décodage du texte doit gérer les accents ; les messages propres au mod suivent la langue
  choisie dans les réglages.
- **Travail par lots** (précision du joueur, 29/09/2026) : les tests en jeu sont difficiles
  pour lui. Avancer sur plusieurs sous-menus et faire les vérifications techniques de façon
  autonome, puis proposer un test court quand il y a suffisamment de matière. Ne pas attendre
  son retour après chaque modification. Conserver la priorité aux menus et distinguer les
  fonctions implémentées, contrôlées hors jeu et validées en jeu.
- **Sources locales P3P** : utiliser directement l'exécutable, les archives, les textes
  décompilés et les outils installés. P4G Access guide les comportements et commandes ;
  les structures et signatures doivent être établies dans P3P. Les deux jeux sont installés.
- **Joueur aveugle** : les réponses à l'utilisateur sont en français, lisibles au lecteur d'écran
  (pas de grands tableaux ni de symboles décoratifs dans les réponses courtes).
- Documenter chaque système dans `docs/<SYSTÈME>.md` : comment il a été trouvé (rétro-ingénierie)
  et comment le code du mod fonctionne. Lire ce doc avant de toucher au système.

## Outils Claude du projet (`.claude/`)

- **Skills** : `/build-deploy`, `/check-log`, `/port-hook` (porter une fonction de P4G vers P3P),
  `/snapshot-hunt` (trouver une valeur en mémoire avec le joueur), `/new-component` (nouveau
  lecteur + doc), `/test-session` (préparer et consigner un test en jeu).
- **Agents** : `p4g-reference` (trouve et explique le code P4G équivalent), `re-analyst`
  (signatures, Ghidra, mods p3ppc), `a11y-reviewer` (revue parole + sécurité mémoire, avant déploiement),
  `p3p-assistant` (aide de jeu pour le joueur).
- **Hooks** :
  - `SessionStart` : exécute `session-context.ps1` (résumé du contexte : état git, jeu installé,
    erreurs du dernier journal).
  - `PreToolUse` (PowerShell | Bash) : `shell-guard.py` bloque les commandes dangereuses
    (Start-Sleep, Select-String -Recurse, Set-Content, python -c en PowerShell, git en PowerShell,
    powershell -Command en Bash) et suggère les outils corrects (Monitor, Grep, Write/Edit, Bash,
    etc.). Règles et historique : `docs/SHELL_PITFALLS.md`.
  - `Stop` : exécute `claude-md-check.ps1` (rappel de mettre à jour `CLAUDE.md` si la structure du
    projet a changé).

## Tenir ce fichier à jour

Mettre à jour CLAUDE.md quand change : l'étape en cours, l'organisation du dépôt, les commandes,
les dépendances Reloaded, une règle ou une leçon importante (plantage, piège de signature…).
Rester court : les détails vont dans `docs/`.
