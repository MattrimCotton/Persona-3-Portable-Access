# Persona 3 Portable Access — guide pour Claude

Mod d'accessibilité qui rend **Persona 3 Portable (PC, Steam)** jouable par un joueur aveugle avec
NVDA. Mod **Reloaded-II** (C#, .NET) chargé avec **Persona Essentials**, sur le modèle de
**Persona 4 Golden Access**. Licence GPL-3.0 (crédits : AnimatedSwine37, Haru).

Le plan complet (faisabilité, étapes, risques) est dans [PLAN.md](PLAN.md). **Le lire avant toute
décision d'architecture.**

## État actuel

- **Étape 0 (prérequis)** : P3P Steam installé (`D:\SteamLibrary\steamapps\common\P3P`, le 28/09/2026,
  avec les langues `data_EN`, `data_FR`…). `P3P.exe` ajouté dans Reloaded-II le 28/09/2026
  (`Reloaded-II\Apps\p3p.exe\AppConfig.json`, écrit à la main) avec Persona Essentials
  (`p5rpc.modloader` 2.9.4) et ses dépendances ; premier lancement par Reloaded le même jour.
  Variables vérifiées : `RELOADEDIIMODS`, `GHIDRA_HOME`, `JAVA_HOME` (JDK 25).
- **Étape 1 (le socle : dialogues puis tous les menus)** : en cours. Décision du joueur
  (28/09/2026) : rien d'autre (ville, combats, Tartarus) tant que les dialogues (1a) **et** tous
  les menus (1b) ne sont pas lus correctement, chaque sous-étape testée en jeu.
  - **1a (dialogues)** : **réussi au test 3** (28/09/2026) : messages, pages, orateur, accents,
    choix Oui / Non (`docs/TEST_LOG.md`). `Components/Dialogue.cs` accroche `MsgWindow::DrawAll` (voir `docs/DIALOGUE.md`). Le mod est
    activé dans `Apps\p3p.exe\AppConfig.json` ; mode débogage activé dans
    `Reloaded-II\User\Mods\p3ppc.accessibility\Config.json` pour les tests.
  - **1b (menus)** : à commencer. Premiers écrans non lus : choix du sexe et de la difficulté
    (menus graphiques, pas des fenêtres de message), écran titre.
  - Déjà en place : parole et historique (`Speech.cs`, Maj+P répéter, Maj+[ / Maj+] historique,
    Maj+M couper les dialogues, dans `HistoryKeys.cs`), messages du mod en FR/EN (`Loc.cs`),
    correction du titre fenêtre pour éviter que NVDA le ré-annonce à chaque image (`TitleBar.cs`,
    test 3, 28/09/2026).
- Mettre à jour cette section à chaque étape franchie (voir « Tenir ce fichier à jour »).

## Chemins utiles

| Quoi | Où |
|---|---|
| Projet de référence P4G | `C:\Users\asdes.ASUS\Documents\SourceCode\Persona-4-Golden-Access` |
| Code du mod P4G (modèle) | `...\Persona-4-Golden-Access\p4g64.accessibility-master\p4g64.accessibility` |
| Docs techniques P4G | `...\Persona-4-Golden-Access\database\*.md` (BATTLE_SYSTEM, SNAPSHOT_METHOD, DUNGEON_*…) |
| Jeu P3P | `D:\SteamLibrary\steamapps\common\P3P` (exe : `P3P.exe`) |
| Reloaded-II | `D:\SteamLibrary\Reloaded-II` (mods : `Mods\`, variable `RELOADEDIIMODS`, corrigée le 28/09/2026 : elle pointait vers un ancien dossier du Bureau) |
| Lancer le jeu moddé | raccourci Bureau « Persona 3 Portable (Reloaded) » = `Reloaded-II.exe --launch "...\P3P\P3P.exe"` |
| Journaux Reloaded | `%APPDATA%\Reloaded-Mod-Loader-II\Logs` |
| Ghidra 12.1.4 (Java 25) | `C:\Users\asdes.ASUS\Tools\ghidra_12.1.4_PUBLIC` (variable `GHIDRA_HOME`, mode sans interface : `support\analyzeHeadless.bat`) |
| Tolk (DLL à copier) | `D:\SteamLibrary\Reloaded-II\Mods\p4g.golden.access` (`Tolk.dll`, `TolkDotNet.dll`, `SAAPI64.dll`) ; le sous-module des sources P4G est incomplet |

## Organisation du dépôt

`lib/tolk/` contient Tolk et les pilotes des lecteurs d'écran (versionnés malgré `*.dll` dans `.gitignore`).

| Chemin | Contenu |
|---|---|
| `src/p3ppc.accessibility/` | Le mod (projet SDK .NET, point d'entrée `Mod.cs`, fonctions sous `Components/`, accès mémoire sous `Native/`, localization via `Loc.cs`). |
| `src/p3ppc.accessibility/Loc.cs` | Messages propres au mod en FR/EN (réglage Langue, Auto = langue Steam du jeu). Aucun message codé en dur ailleurs. |
| `docs/` | Documents « source de vérité » par système (un `.md` par système : `DIALOGUE.md`, `BATTLE_SYSTEM.md`, `TARTARUS.md`…), plus `SIGNATURES.md` (toutes les signatures et adresses trouvées) et `TEST_LOG.md` (résultats des tests en jeu). |
| `data/` | Fichiers JSON chargés par le mod à l'exécution (aide F1, tables de noms…). |
| `tools/` | Scripts : `checklog.ps1` (journal Reloaded), `re/sigscan.py` (tester une signature dans un exe), `re/disasm.py` (désassembler à une adresse, `pip install capstone`). |

## Commandes

```bash
# Compiler
dotnet build src/p3ppc.accessibility/p3ppc.accessibility.csproj -c Debug
# Journal Reloaded le plus récent de P3P, avec les erreurs
powershell -NoProfile -File tools/checklog.ps1
```

Déployer : skill `/build-deploy`. Le jeu doit être **fermé** (sinon la DLL est verrouillée).
Il n'y a pas de tests automatiques : **toute vérification se fait en jeu par le joueur** (skill
`/test-session`).

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
- **Hooks appelés à chaque image** : ne parler que quand l'état change (leçon du test 1 : un
  message renvoyé 60 fois par seconde coupe la voix de NVDA, seul le braille suit). Vérifier dans
  le journal qu'une ligne `[Speech]` n'apparaît qu'une fois.
- **Mods d'AnimatedSwine37 (`p3ppc.*`)** : source de signatures déjà validées sur P3P Steam. Les
  consulter sur GitHub avant de chercher une fonction à la main.
- **Parole** : phrases courtes, l'info importante en premier, pas de répétition inutile ; ne pas
  couper une annonce importante. Le jeu existe en plusieurs langues (dont le français, `data_FR`) :
  le décodage du texte doit gérer les accents ; les messages propres au mod suivent la langue
  choisie dans les réglages.
- **Une étape à la fois** : on ne passe à l'étape suivante du plan que si le test en jeu est réussi.
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
  - `Stop` : exécute `claude-md-check.ps1` (rappel de mettre à jour `CLAUDE.md` si la structure du
    projet a changé).

## Tenir ce fichier à jour

Mettre à jour CLAUDE.md quand change : l'étape en cours, l'organisation du dépôt, les commandes,
les dépendances Reloaded, une règle ou une leçon importante (plantage, piège de signature…).
Rester court : les détails vont dans `docs/`.
