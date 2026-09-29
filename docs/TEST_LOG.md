# Journal des tests en jeu

Entrée la plus récente en haut. Pour chaque test : date, version ou commit, ce qui a été testé,
résultat (réussi / partiel / échec), ce que le joueur a entendu, erreurs du journal, suite.

_Aucun test pour l'instant._

## 28/09/2026, test 1 : dialogues (étape 1a), version 0.1.0
- **Résultat : échec partiel, corrigé.** Le braille affichait les textes, mais la voix de NVDA
  était coupée sans arrêt : le même message était renvoyé à chaque image (bug de remise à zéro
  dans `Dialogue.ReadWindows`, la fenêtre lue n'était pas marquée « vue »).
- Bon signe : les messages de l'écran de choix du personnage sont lus en entier, accents
  corrects (« Le déroulement du jeu changera selon le sexe du personnage principal. »).
- Suite : correctif déployé, test 2 à faire.

## 28/09/2026, test 2 : dialogues (étape 1a)
- **Résultat : partiel.** Plus de voix coupée. Premier message lu, choix Oui / Non lus, Maj+M
  fonctionne. Mais les pages suivantes du même message (« Dans ce jeu… », « Le déroulement… »)
  n'ont pas été lues : le jeu réutilise la même liste de lignes pour la page suivante.
- Correctif : comparaison de la page (`+0x46`, hypothèse tirée des octets du journal : 1 puis 2),
  du premier glyphe, et du texte complet toutes les 6 images. Test 3 à faire.

## 28/09/2026, test 3 : dialogues (étape 1a)
- **Résultat : dialogues réussis.** Tous les messages lus une fois et en entier, pages
  successives comprises, avec le nom de l'orateur (« Lycéenne : … », « Chef de train : … ») et les
  accents. Choix Oui / Non lus.
- Problèmes :
  - le choix du sexe et de la difficulté n'est pas lu (ce sont des menus graphiques, pas des
    fenêtres de message : relève de l'étape 1b). La question de confirmation qui suit (« Commencer
    la partie avec le personnage masculin ? ») permet déjà de savoir ce qui est sélectionné ;
  - NVDA relit régulièrement le nom de la fenêtre : le jeu réécrit son titre (images par seconde),
    comme P4G. Corrigé par `TitleBar.cs` (même correctif que P4G), à vérifier au prochain test.

## 28/09/2026, test 4 : premiers menus (étape 1b)
- **Résultat : réussi** (« C'est en bonne voie »). Écran titre : « Appuyez sur une touche », puis
  les 5 entrées dans l'ordre, dans les deux sens. Choix du personnage (masculin / féminin) et de
  la difficulté (Normale au départ, puis Facile, Débutant) annoncés ; les confirmations du jeu
  (« Commencer la partie avec le personnage masculin ? », « Tester la difficulté Débutant ? »)
  concordent avec le curseur lu. Titre de fenêtre : plus de plainte (correctif `TitleBar` validé).
- Reste : l'écran muet est **juste après « Maintenant, amusez-vous bien en jouant. »** (environ
  50 s avant « Terminus, dans la soirée... », Entrée une ou deux fois, pas de musique).

## 29/09/2026, test 5 : traduction et saisie du nom (étape 1b)
- **Traduction : réussie.** Messages en français chargés (`[Language] mod messages: french`).
- **Clavier de saisie du nom : réussi, mais valider est difficile** (« comme pour P4G »). Touches,
  majuscules, accents et symboles annoncés ; « Nom : S », « Nom : SI », « Nom : SIM » au fil des
  lettres. Le joueur est resté sur l'écran (nom « SIMèèè », second nom vide).
- Demande du joueur : signer automatiquement avec un nom défini. Fait : réglage
  `AutoSignContract` (voir `NAME_ENTRY.md`), compilé, **pas encore déployé** (jeu ouvert).

## 29/09/2026, test 6 : signature automatique (étape 1b)
- **Résultat : partiel.** Noms « Yuki Makoto » écrits et annoncés, mais Start simulé trop tôt
  (animation d'ouverture, mode 0) : « Noms remplis. Appuyez sur Start » ; le joueur a ensuite
  effacé le nom en tapant (« AAAAAA »). Nom réécrit dans la partie ouverte (`tools/re/p3pmem.py`),
  puis Start pressé par le joueur (touche W) : « Continuer ? » (message du jeu, lu), Oui, la
  suite de l'histoire est lue (Mitsuru, Yukari, choix de réponses).
- Corrigé et déployé : Start simulé seulement quand le clavier est prêt (mode 1). À revérifier à
  la prochaine nouvelle partie, avec le héros et avec l'héroïne.
- La confirmation du jeu ne montre pas le nom : ordre nom / prénom encore inconnu (à repérer
  quand un personnage dira le nom du héros).
- Message suivant du jeu : « Appuyez sur Menu des commandes pour accéder à Config, Charger les
  données et Écran titre » : prochain menu à rendre accessible.
