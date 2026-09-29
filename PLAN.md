# Persona 3 Portable Access : étude de faisabilité et plan

Rédigé le 28 septembre 2026. Objectif : un mod qui rend **Persona 3 Portable (PC, Steam)** jouable
par un joueur aveugle avec NVDA, sur le modèle de **Persona 4 Golden Access**
(`Documents\SourceCode\Persona-4-Golden-Access`).

## Verdict

**Faisable, et c'est le meilleur candidat après P4G.** Toute la chaîne technique existe déjà pour
P3P PC. Plusieurs petits mods publiés prouvent qu'on peut accrocher le code du jeu, et une partie du
jeu (la ville) est même plus simple à rendre accessible que dans P4G. Le gros du travail reste la
rétro-ingénierie propre à P3P : adresses, structures et signatures. Rien ne se copie tel quel du
code de P4G, mais la méthode et la conception se transposent.

---

## 1. Ce qui existe déjà pour P3P PC

| Élément | État | Source |
|---|---|---|
| Reloaded-II + **Persona Essentials** (chargeur de mods) | prend officiellement en charge P3P, P4G 64 bits et P5R. Fusion automatique des fichiers BF / BMD / PAK / SPD / tables, rechargement à chaud | [Sewer56/p5rpc.modloader](https://github.com/Sewer56/p5rpc.modloader) |
| File Emulation Framework (BF / BMD) | le même que pour P4G ; permet d'ajouter du FlowScript et des messages | idem |
| Mods de code P3P avec signatures publiques | 12 mods d'AnimatedSwine37 (auteur du modèle d'accessibilité de P4G) : liens sociaux, fusion, menu de statut, noms, films… | [AnimatedSwine37 (GitHub)](https://github.com/AnimatedSwine37?tab=repositories&q=p3ppc) |
| Accès aux tables de noms (objets, personnages, liens, glossaire) | déjà accrochées par `p3ppc.unhardcodedNames` | [p3ppc.unhardcodedNames](https://github.com/AnimatedSwine37/p3ppc.unhardcodedNames) |
| Menu de statut et points de stats sociales | accroché par `p3ppc.socialStatTracker` (signature `E8 ?? ?? ?? ?? 48 63 0D ?? ?? ?? ?? 33 FF`) | [p3ppc.socialStatTracker](https://github.com/AnimatedSwine37/p3ppc.socialStatTracker) |
| Fusion, choix des compétences héritées | `p3ppc.manualSkillInheritance` | [dépôt](https://github.com/AnimatedSwine37/p3ppc.manualSkillInheritance) |
| Décompilation des scripts et messages (FlowScript / BMD) | Atlus Script Tools, bibliothèque « P3 » | [tge-was-taken/Atlus-Script-Tools](https://github.com/tge-was-taken/Atlus-Script-Tools), [Tupelov/Atlus-Library](https://github.com/Tupelov/Atlus-Library) |
| Adresses mémoire (PV, stats, argent…) | tables Cheat Engine publiques pour P3P Steam | [FearLess](https://fearlessrevolution.com/viewtopic.php?t=23053) |
| Mod d'accessibilité existant pour P3P | **aucun trouvé** | recherche web du 28/09/2026 |

⚠ Les mods P3P d'AnimatedSwine37 sont testés **uniquement sur la version Steam** ; la version
Game Pass a un exécutable différent. **Acheter la version Steam.**

---

## 2. Ce qui se réutilise depuis P4G Access

Licence GPL-3.0 : réutilisation libre, à condition de garder le code ouvert et les crédits (Haru,
AnimatedSwine37).

**Tel quel (ne dépend pas du jeu) :**
- `Speech.cs` + Tolk (NVDA / SAPI), historique vocal (`HistoryKeys`), répétition (Maj+P).
- `SoundSettings`, `ModSettings`, `ToneCue` / NAudio (mixeur de sons, balises, bips).
- Menu de réglages F1 et aide à partir de `help_content.json` (`SettingsMenu.cs`).
- `ControllerInput` (couche manette derrière les gâchettes), garde de lecture mémoire
  `IsReadable`, `SigScan`, `GetGlobalAddress`, squelette du modèle Reloaded.

**À adapter (même principe, adresses et signatures différentes) :**
- `Native/Text/AtlusEncoding.cs`, `GameText.cs`, `Dialog.cs` : le format de texte Atlus est de la
  même famille ; le décodage des caractères est probablement très proche, à vérifier.
- `Dialogue.cs` : accroche `MsgWindow::DrawDialog` et `StartDialog` dans P4G. P3P a un système de
  fenêtres de messages équivalent : il faut retrouver les deux fonctions.
- Lecteurs de combat (`Battle/*`), boutiques, Velvet Room, calendrier, Shuffle Time, résultats.
- Logique de navigation en donjon : navigateur par catégories, marche automatique, radar des
  Ombres, balises, bruit des murs, curseur de carte (**Tartarus**).

**À concevoir pour P3P :**
- **La ville en « pointer-cliquer »** : dans P3P, hors du Tartarus, on ne marche pas. On déplace un
  **curseur** sur les personnes et les objets d'une image 2D. Il suffit de lire ce qui est sous le
  curseur et de proposer une liste des éléments présents : **c'est bien plus simple** que la
  navigation libre en ville de P4G (OverworldNav, 130 Ko de code).
- **Le Tartarus** : une tour de plus de 250 étages aléatoires, avec des téléporteurs (« Access
  Point », points de téléportation), des barrières jusqu'à certaines dates, des Ombres qui
  errent, des coffres, et le Reaper. Même principe que les donjons de P4, mais c'est ici le cœur
  du jeu.
- **Deux protagonistes** (masculin / féminin) : les textes, les liens sociaux et certains lieux
  diffèrent.
- **Combats** : l'option de contrôle direct de l'équipe (« Direct Commands ») doit être activée et
  lue ; il y a aussi les tactiques par membre.
- La **Pleine Lune** (boss de chaque mois), la **Dark Hour** (le soir), l'**état de fatigue**
  (Tired / Sick) : à annoncer.

---

## 3. Plan par étapes

Chaque étape se termine par un test réel en jeu par le joueur aveugle. On passe à la suivante
seulement si ça marche.

### Étape 0 : prérequis (joueur)
- Acheter P3P **Steam**, installer Reloaded-II + Persona Essentials, lancer le jeu une fois par
  Reloaded (avec aide voyante ou Claude si l'installateur de Reloaded n'est pas accessible).

### Étape 1 : le socle, dialogues et menus (décision du joueur, 28/09/2026)
Le mod doit lire **tous les dialogues et tous les menus** avant de passer à la ville, aux combats
ou à Tartarus. L'étape se fait en deux temps, chacun testé en jeu :

**1a. Dialogues (le test décisif)**
- Recibler le modèle `p4g64.accessibility` en `p3ppc.accessibility` (ID d'appli `p3p.exe`,
  dépendances Persona Essentials + SigScan + Hooks).
- Retrouver dans P3P les équivalents de `MsgWindow::DrawDialog` / `StartDialog` (Ghidra :
  chercher les fonctions qui lisent les structures de message BMD ; partir des signatures P4G et
  des chaînes de caractères).
- Résultat attendu : **les dialogues et les choix de réponse sont lus par NVDA.**
- Si ça marche en quelques séances : feu vert pour la suite. Sinon : revoir l'approche.

**1b. Tous les menus**
- Écran titre, menu pause (compétences, objets, équipement, Persona, statut, liens sociaux,
  calendrier, réglages), messages système, saisie du nom, écrans de chargement, argent, date et
  heure.
- L'étape 1 n'est finie que quand chaque menu a été testé en jeu et lu correctement.

### Étape 2 : (fusionnée dans l'étape 1)

### Étape 3 : la ville (pointer-cliquer)
- Lire l'élément sous le curseur ; liste des personnes et objets de la zone avec saut direct ;
  carte de la ville ; annonces de lieu, jour, moment, météo, fatigue.

### Étape 4 : combats
- Tour de qui, commandes, compétences (coût, description), cibles, dégâts, faiblesses découvertes,
  état de l'équipe et des ennemis, All-Out Attack, Shuffle Time, résultats. Reprendre la
  conception de `BATTLE_SYSTEM.md` de P4G.

### Étape 5 : Tartarus
- Nom et numéro de l'étage, navigateur (escalier, téléporteur, coffres, Ombres), marche
  automatique, radar, balises, bruit des murs ; lobby (Velvet Room, point de sauvegarde, sortie,
  choix de l'équipe).

### Étape 6 : Velvet Room, boutiques, quêtes d'Elizabeth / Theodore, cinématiques
- Fusion avec prévisions, compendium, boutiques (Paulownia Mall, pharmacie, police), sous-titres
  et descriptions des cinématiques.

### Étape 7 : guide du joueur et diffusion
- Aide F1 complète, guide « sans spoiler » par date (comme P4G), README, publication.

---

### Traduction (tout au long du projet, décision du joueur du 29/09/2026)
- Le **français est la langue prioritaire** : toujours complet et testé en premier.
- Le mod doit pouvoir être traduit facilement par des contributeurs : chaque message du mod est
  dans un fichier par langue (`lang/<langue>.json`, référence anglaise, contexte pour les
  traducteurs, vérification automatique sur GitHub, repli sur l'anglais pour une traduction
  partielle). Le texte du jeu, lui, est lu dans la langue du jeu. Détails : `docs/LOCALIZATION.md`,
  guide des contributeurs : `lang/README.md`.
- Chaque nouvelle étape suit la même règle ; les contenus longs à venir (aide F1, descriptions
  des images de scène, noms) auront aussi un fichier par langue.

## 4. Risques et inconnues

- **Rétro-ingénierie** : c'est l'essentiel du travail, et il est propre à P3P (structures de
  combat, de menu, de carte). La méthode de P4G (`SNAPSHOT_METHOD.md` : comparer la mémoire avant
  et après une action, puis analyser avec Ghidra) s'applique, mais demande du temps.
- **ASLR** : sur P4G, l'ASLR est désactivé, donc les adresses sont fixes. Il faut vérifier sur
  `P3P.exe` ; sinon, tout passer par signatures et adresses relatives (le modèle le fait déjà).
- **Plantages** : les lectures mémoire pendant les changements de zone ont fait planter P4G
  (voir `DUNGEON_DOORS_AND_BEACON.md`, « CRASH WATCH »). Reprendre dès le départ la garde
  « transition de zone ».
- **Mises à jour du jeu** : une mise à jour Steam peut casser les signatures.
- **Tartarus** : très grand, avec des étages spéciaux et des événements ; la navigation doit être
  robuste.

---

## 5. Prochaine action

Quand le jeu est acheté et installé : lancer l'étape 1. Claude peut préparer à l'avance le squelette
du projet `p3ppc.accessibility` (copie du modèle, renommage, Speech, Settings, SigScan, sans aucun
hook spécifique au jeu), pour que le premier test porte seulement sur les dialogues.

## Sources
- [Sewer56/p5rpc.modloader (Persona Essentials)](https://github.com/Sewer56/p5rpc.modloader)
- [Dépôts P3P d'AnimatedSwine37](https://github.com/AnimatedSwine37?tab=repositories&q=p3ppc)
- [p3ppc.unhardcodedNames](https://github.com/AnimatedSwine37/p3ppc.unhardcodedNames)
- [p3ppc.manualSkillInheritance](https://github.com/AnimatedSwine37/p3ppc.manualSkillInheritance)
- [Atlus Script Tools](https://github.com/tge-was-taken/Atlus-Script-Tools)
- [Tupelov/Atlus-Library](https://github.com/Tupelov/Atlus-Library)
- [Persona Modding : P3P](https://personamodding.com/p3p/)
- [FearLess : table Cheat Engine P3P](https://fearlessrevolution.com/viewtopic.php?t=23053)
- [CBR : différences entre P3P et P3 FES](https://www.cbr.com/persona-3-portable-vs-fes-differences/)
- [PCGamingWiki : Persona 3 Portable](https://www.pcgamingwiki.com/wiki/Persona_3_Portable)
