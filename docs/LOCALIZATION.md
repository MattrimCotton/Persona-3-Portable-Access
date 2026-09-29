# Traduction des messages du mod

Mis en place le 29/09/2026, à la demande du joueur : le mod doit pouvoir être traduit facilement
par des contributeurs. Le **français reste la langue prioritaire** : toujours complet, testé en
premier.

## Deux sortes de texte

- **Texte du jeu** (dialogues, choix, descriptions du jeu) : lu dans la mémoire du jeu, dans la
  langue du jeu, décodé avec la table de glyphes correspondante (`Native/Text/GameLanguage.cs`,
  `Charsets/P3P_*.tsv`). Rien à traduire.
- **Messages du mod** (entrées de menus dessinées en images, « 2 sur 5 », annonces, aide…) :
  dans `lang/<id>.json`, un fichier par langue.

## Fichiers (`lang/` à la racine du dépôt)

- `english.json` : référence, toute clé y est créée d'abord. `french.json` : toujours complet.
- `context.json` : explication de chaque clé pour les traducteurs (en anglais), non chargé.
- Nom de fichier = identifiant de langue Steam (`french`, `german`, `koreana`, `schinese`…).
- Clés commençant par `_` (`_meta` : nom de la langue, traducteurs) : ignorées par le mod.
- Guide des contributeurs, en anglais : `lang/README.md`.

La compilation copie `lang/*.json` (sauf `context.json`) dans le sous-dossier `Lang\` du mod
(`p3ppc.accessibility.csproj`).

## Fonctionnement (`Loc.cs`)

- Réglage **Langue du mod** (`Config.cs`, énumération `ModLanguage`) : Auto (langue Steam du jeu)
  ou une des 9 langues du jeu. Les numéros de l'énumération sont enregistrés dans `Config.json` :
  **ne jamais les réordonner, seulement en ajouter à la fin**.
- `Loc.Apply` charge au démarrage et à chaque changement de réglage : la langue choisie, et
  l'anglais en secours. Langue sans fichier : anglais, avec une ligne `[Language]` au journal.
- `Loc.T("clé")` : texte dans la langue active, sinon anglais, sinon la clé (clé manquante notée
  une fois au journal).
- `Loc.F("clé", a, b)` : texte avec `{0}`, `{1}`. Si une traduction a des marqueurs cassés, retour
  à l'anglais au lieu d'une exception dans un hook du jeu.
- Lecture JSON tolérante : commentaires et virgule finale acceptés.

## Règles pour ajouter un message (Claude, Codex, contributeurs)

1. Aucun texte parlé écrit en dur dans le code : toujours `Loc.T` / `Loc.F`.
2. Ajouter la clé dans `english.json`, `french.json` **et** `context.json` (où et quand elle est
   dite).
3. Lancer `python tools/lang_check.py --complete french` : doit afficher `OK`. La même
   vérification tourne sur GitHub (`.github/workflows/lang-check.yml`).
4. Contenus longs à venir (aide F1, descriptions des images de scène de l'étape 1c, noms) : même
   principe, un fichier par langue avec repli sur l'anglais, par exemple `lang/<id>/help.json`.
   À concevoir quand le contenu arrive.
