# Lecture détaillée de la documentation

29/09/2026, Codex. Lecture des 35 fichiers Markdown du projet présents au début de cette
revue : racine, `docs/`, `lang/README.md`, six procédures dans chacune des arborescences
`.claude/skills` et `.agents/skills`, quatre rôles dans `.claude/agents`.
Les index générés sous `extracted/`, les fichiers du jeu et les documents du dépôt P4G
ne sont pas inclus dans ce nombre. Les procédures ont été lues, pas exécutées.

## Plan retenu pour la suite

1. Étape 1b : terminer les menus généraux. Racine du menu pause, listes, détails,
   sélection d'un personnage ou d'une cible, valeurs de configuration, confirmations,
   sauvegarde/chargement, messages système et informations de date/heure/argent.
   Lire la racine ne suffit pas à rendre les sous-menus accessibles.
2. Étape 1c : descriptions des images de scène, avec niveau de détail et répétition.
   Cette fonctionnalité est approuvée, mais pas implémentée. Le catalogue graphique
   actuel fournit des ressources de recherche ; il n'implémente pas cette étape.
3. Socle complémentaire P4G : réglages et aide F1, sons, extension des commandes et
   protection des transitions. À intégrer selon les besoins de la fin de 1b et avant
   les systèmes qui en dépendent. La croix avec deux gâchettes existe déjà dans P3P.
4. Ville : curseur 2D, éléments présents, carte et informations de contexte.
5. Combat : commandes, compétences, cibles, états, résultats et Shuffle Time.
6. Tartarus : navigation, cibles, balises, radar et marche automatique.
7. Velvet Room, boutiques, requêtes, cinématiques, puis aide complète et diffusion.

« Tous les menus » en 1b désigne ici les menus généraux énumérés par le plan. Les menus
spécifiques de combat, de boutique et de fusion restent explicitement dans leurs étapes
respectives. Ne pas avancer ces étapes par simple interprétation du mot « tous ».

La précision récente du joueur remplace l'ancienne attente après chaque sous-étape :
développer des lots cohérents, contrôler hors jeu ce qui peut l'être, puis proposer un
test court quand il y a de la matière. Un résultat technique ne devient pas pour autant
une validation en jeu. Les deux protagonistes et la priorité au français restent à couvrir.

## Écarts documentaires constatés

- `PLAN.md` est l'étude initiale : achat et installation encore présentés comme prochaine
  action, ASLR encore à vérifier, étape 1c absente. Pour la reprise, suivre `HANDOFF.md`
  et la roadmap actualisée ; conserver le plan comme référence de conception datée.
- `ROADMAP.md` présente encore traduction et clavier du nom comme à tester. Le test 5
  les donne réussis. La racine `CampMenu` et `SaveSlots` existent déjà mais restent non
  validés en jeu ; ils ne sont pas encore reflétés précisément dans les tâches restantes.
- `CLAUDE.md` indique « réussi au test 6 » pour l'auto-signature ; `TEST_LOG.md` indique
  un résultat partiel, puis une correction de Start à retester. Ce dernier fait foi.
- `NAME_ENTRY.md` commence par « pas encore testé » mais décrit ensuite les tests 5 et 6.
  La mise à jour des tampons pendant la frappe a été observée ; l'ordre nom/prénom et
  la correction automatique restent à distinguer de cette réussite.
- `DIALOGUE.md` conserve des questions du premier test sur les accents et les phrases
  complètes, résolues dans le périmètre du test 3. Le champ de page +0x46 reste décrit
  comme une hypothèse : ne pas transformer la réussite du lecteur en preuve de son sens exact.
- `MENUS.md` remet la saisie du nom à tester et ne décrit pas encore `CampMenu`.
  La section scènes contient une ancienne demande de validation de l'idée déjà approuvée.
  Son hypothèse sur l'écran muet reste une hypothèse, pas une identification confirmée.
- `TEST_LOG.md` conserve « Aucun test pour l'instant » malgré six entrées et annonce
  l'ordre récent-en-premier alors que les entrées vont de 1 à 6. Préserver les résultats
  historiques ; ne pas inventer un nouveau test pour résoudre cette présentation.
- `SIGNATURES.md` ne contient pas encore les entrées de `CampMenu`, `GameStrings`,
  `TextCapture` et `SaveSlots`. Certains lecteurs testés conservent un statut de seule
  unicité statique. Compléter avec une preuve traçable, sans déclarer toutes les variantes
  ou branches validées parce qu'un écran a fonctionné une fois.
- `SAVE_LOAD.md` et `TEXT_CAPTURE.md`, cités par le code ou le guide, sont absents.
  Les exemples `BATTLE_SYSTEM.md` et `TARTARUS.md` du guide désignent des docs futures.
- Le socle P4G est parfois présenté comme copiable « tel quel ». Les composants actuels
  de parole, commandes et réglages dépendent aussi de contextes P4G ; réutiliser leur
  conception et isoler les dépendances avant le portage.
- Les procédures de nouveaux lecteurs et la revue d'accessibilité demandent d'ajouter
  les touches à F1 alors que ce menu n'existe pas encore dans P3P. Documenter les commandes
  dans la doc du système en attendant ; ne pas prétendre que l'aide est déjà chargée.
- Les procédures se terminent par déploiement et test à chaque lecteur : appliquer la
  consigne plus récente de regroupement par lots. Les instantanés avec actions du joueur
  restent un recours ciblé lorsque l'analyse locale ne suffit pas.
- `REFERENCES.md` utilise encore `powershell -File` dans son exemple de décompilation,
  contrairement à la règle `pwsh -NoProfile -File`. Les chemins Windows de l'exemple Bash
  doivent aussi être correctement cités ou écrits avec des barres obliques.
- Les deux procédures de déploiement mélangent un contexte Bash et `$env:RELOADEDIIMODS`,
  syntaxe PowerShell. La copie Codex cite `.Codex/settings.json`, absent de l'inventaire
  local (le dossier présent contient `config.toml`). Ne pas exécuter cet exemple tel quel.
- `README.md` n'explicite pas que la compilation Debug écrit déjà dans Reloaded-II.
  Vérifié dans le projet : `OutputPath` pointe sur `$(RELOADEDIIMODS)/p3ppc.accessibility`.
  Une lecture de documentation ne nécessite donc aucune compilation du mod.

## Sources et règles techniques à conserver

`REFERENCES.md` donne la bonne hiérarchie : données du jeu, code des mods P3P, puis guides.
Les outils locaux permettent de partir des chemins C conservés dans l'exécutable,
retrouver les références, désassembler, vérifier une signature et relier un dessin à
ses sprites ou textes. `GAME_STRINGS.md` offre les indices des textes du jeu réutilisables.
Les images extraites et les fichiers décompilés restent exclus du dépôt public.

`LOCALIZATION.md` et `lang/README.md` distinguent correctement texte du jeu et messages
du mod : clés anglais/français/contexte, marqueurs conservés, repli et contrôle automatique.
La présence de neuf langues dans le réglage ne signifie pas neuf traductions complètes.

`CONTROLLER.md` décrit les commandes réellement présentes, le masquage des boutons et
les vérifications restantes. `SHELL_PITFALLS.md` documente les pièges vécus ; les outils
propres à Claude se traduisent selon `AGENTS.md`, sans supposer qu'ils sont actifs dans Codex.

Cette revue ne change aucun statut de validation. Aucun code du mod, déploiement ou test
joueur nouveau. Les écarts sont recensés ici pour leur correction lors de la reprise des
documents concernés, sans réécrire l'historique des sessions.
