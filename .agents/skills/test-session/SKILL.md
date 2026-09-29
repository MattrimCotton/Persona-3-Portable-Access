---
name: test-session
description: Prépare un test en jeu pour le joueur aveugle (consignes pas à pas, quoi écouter) et consigne le résultat dans docs/TEST_LOG.md. À utiliser après un déploiement, ou quand le joueur revient avec un retour de test.
---

# Test en jeu

Le joueur est aveugle : il ne peut pas « regarder l'écran ». Les consignes doivent être
entièrement sonores et au clavier (ou à la manette).

## Avant le test

Donner une liste **numérotée et courte** (5 étapes au plus) :
- comment arriver à l'endroit à tester (touches exactes, ou quelle sauvegarde charger) ;
- l'action à faire ;
- **ce qu'il doit entendre** si ça marche, et quoi signaler sinon (silence, texte faux, doublon,
  plantage) ;
- rappel : Maj+P pour répéter, et noter l'heure s'il y a un plantage.

## Après le test

1. Lancer `/check-log` pour croiser son retour avec le journal.
2. Ajouter une entrée en tête de `docs/TEST_LOG.md` : date, version/commit, ce qui a été testé,
   résultat (réussi / partiel / échec), ce qu'il a entendu, erreurs du journal, suite à donner.
3. Si réussi : passer les signatures concernées à `validée en jeu` dans `docs/SIGNATURES.md`, et
   mettre à jour l'étape en cours dans `AGENTS.md` si une étape du plan est franchie.
