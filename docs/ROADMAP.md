# Feuille de route et état du chantier

Mise à jour : 29/09/2026. Vue de gestion de projet : ce qui est fait, en cours, à faire et à
venir. Le plan d'origine (faisabilité, risques) est dans [PLAN.md](../PLAN.md) ; le point de
reprise immédiat est dans [HANDOFF.md](HANDOFF.md).

Règle de passage (décision du joueur, 28/09/2026) : **une étape n'est finie que si le test en jeu
est réussi**, et rien après l'étape 1 (ville, combats, Tartarus) ne commence tant que tous les
dialogues et tous les menus ne sont pas lus.

## Diagramme

Version texte juste en dessous (le diagramme est pour un affichage visuel, GitHub ou VS Code).

```mermaid
flowchart TD
    E0["Étape 0 : prérequis<br/>FAIT"]:::done
    E1a["1a : dialogues<br/>FAIT (test 3)"]:::done
    E1b["1b : tous les menus<br/>EN COURS"]:::doing
    E1c["1c : description des images de scène<br/>PROPOSÉ, décision du joueur"]:::proposed
    SOC["Socle P4G à porter :<br/>menu F1, sons, manette,<br/>garde transition de zone"]:::todo
    E3["3 : ville (pointer-cliquer)"]:::todo
    E4["4 : combats"]:::todo
    E5["5 : Tartarus"]:::todo
    E6["6 : Velvet Room, boutiques,<br/>vidéos, requêtes"]:::todo
    E7["7 : guide du joueur, diffusion"]:::todo

    E0 --> E1a --> E1b
    E1b --> E1c
    E1b --> SOC
    E1c --> E3
    SOC --> E3
    E3 --> E4 --> E5 --> E6 --> E7
    SOC --> E4
    SOC --> E5

    classDef done fill:#cfe8cf,stroke:#2e7d32
    classDef doing fill:#fff1c2,stroke:#b58900
    classDef proposed fill:#e3e3f7,stroke:#5c5ca8
    classDef todo fill:#eeeeee,stroke:#777777
```

Lecture du diagramme : 0, puis 1a, puis 1b. Après 1b viennent deux branches, 1c (images) et le
socle P4G, qui mènent toutes deux à l'étape 3. Ensuite 3, 4, 5, 6, 7 dans l'ordre. Le socle P4G
sert aussi aux étapes 4 et 5.

## Fait

- **Étape 0, prérequis** (28/09/2026) : jeu Steam, Reloaded-II, Persona Essentials, variables
  d'environnement, outils de rétro-ingénierie (Ghidra, Atlus Script Tools, CriFsLib).
- **Squelette du mod** : parole Tolk (`Speech.cs`), historique Maj+[ / Maj+] et répétition Maj+P,
  coupure des dialogues Maj+M (`HistoryKeys.cs`), messages FR/EN (`Loc.cs`), lecture mémoire
  gardée (`Utils.cs`), décodage du texte Atlus avec accents (`Native/Text/`).
- **1a, dialogues** (réussi au test 3) : messages, pages suivantes, nom de l'orateur, choix
  Oui / Non (`Dialogue.cs`, doc `DIALOGUE.md`).
- **Titre de fenêtre** relu par NVDA : corrigé (`TitleBar.cs`, validé au test 4).
- **1b, premiers menus** (réussis au test 4) : écran titre (`TitleMenu.cs`), choix du sexe et de
  la difficulté (`SexSelectMenu.cs`), doc `MENUS.md`.
- **Outillage** : extraction CPK/PAK, décompilation des textes, recherche de chaînes et de
  références, désassemblage, conversion d'images, garde-fous des commandes
  (`SHELL_PITFALLS.md`).

## En cours : 1b, tous les menus

Méthode et état détaillé : `MENUS.md`. Tâches, dans l'ordre proposé :

1. **Menu pause** (le plus gros morceau) : liste principale, puis chaque sous-menu :
   compétences, objets, équipement, Persona, statut, liens sociaux, calendrier, configuration.
   Taille : grosse. Sources : mods `p3ppc.*` d'AnimatedSwine37 (menu de statut déjà accroché par
   `p3ppc.socialStatTracker`), tables de noms de `p3ppc.unhardcodedNames`.
2. **Saisie du nom** du héros (au début du jeu). Taille : moyenne.
3. **Sauvegarde et chargement** (écran des emplacements). Taille : moyenne.
4. **Messages système**, écrans de chargement, **argent, date et heure** (annonce du jour et du
   moment de la journée). Taille : moyenne.
5. **Restes de l'écran titre** : Licence, Localisation, entrée « Continuer » grisée ;
   descriptions des difficultés (`sex_select_help.bmd`). Taille : petite.
6. **Écran muet** après « amusez-vous bien en jouant » : probablement une image fixe sans texte
   (voir 1c). Taille : petite si on ne décrit que cette image.

## Proposé : 1c, description des images de scène

Les scènes du jeu sont des images fixes (`CALL_BG_IMG`, 355 images différentes) avec du texte, de
la musique et des fondus. Idée : annoncer une courte description au changement d'image, avant le
texte (nom du lieu pour un décor courant, description plus complète pour une image d'histoire),
avec un réglage de niveau de détail et une touche pour la réentendre. Détails et première piste
technique : `MENUS.md`, section « Images des scènes ». **Décision du joueur en attente** : 1c
complète après les menus, ou seulement l'image de l'écran muet tout de suite.

## Socle P4G à porter (avant ou pendant la fin de 1b)

Code à reprendre de P4G Access, qui ne dépend pas du jeu :

- **Menu de réglages F1 et aide** (`SettingsMenu.cs`, `help_content.json` → `data/`).
- **Sons** (`SoundSettings`, `ToneCue`, NAudio) : bips de menu, balises pour Tartarus.
- **Manette** (`ControllerInput`, couche derrière les gâchettes).
- **Garde « transition de zone »** (cause de plantages dans P4G, `DUNGEON_DOORS_AND_BEACON.md`) :
  indispensable avant l'étape 3.

## À venir

- **Étape 3, ville** : lire ce qui est sous le curseur, liste des personnes et objets avec saut
  direct, carte, lieu, jour, moment, météo, fatigue. Textes des personnages dans
  `field2d/bg/*.abin`. Plus simple que la ville de P4G.
- **Étape 4, combats** : tour, commandes, compétences, cibles, dégâts, faiblesses, All-Out Attack,
  Shuffle Time, résultats. Modèle : `BATTLE_SYSTEM.md` de P4G. Messages de combat déjà repérés
  dans `conver_temp\bf\*.bf`.
- **Étape 5, Tartarus** : étage, navigateur, marche automatique, radar, balises, lobby. Le plus
  gros chantier de navigation.
- **Étape 6** : Velvet Room et fusion, boutiques, requêtes d'Elizabeth / Theodore, vidéos
  (introduction…).
- **Étape 7** : aide F1 complète, guide sans spoiler, README, publication.

## Risques suivis

- Une mise à jour Steam peut casser signatures et adresses : tout est noté dans `SIGNATURES.md`.
- Plantages par lecture mémoire pendant un changement de zone : garde à porter avant l'étape 3.
- Deux protagonistes (masculin / féminin) : textes et liens différents, à tester des deux côtés.
- Travail non versionné : un seul commit (28/09/2026) ; le reste attend d'être enregistré, et le
  dépôt n'a pas de copie distante (pas de remote git).

## Décisions en attente du joueur

1. Étape 1c : maintenant (écran muet seulement) ou après les menus (complète) ?
2. Enregistrer le travail dans git, et créer éventuellement un dépôt GitHub de sauvegarde.
