# Écran de sauvegarde et de chargement

Lecteur : `src/p3ppc.accessibility/Components/SaveSlots.cs` (29/09/2026, Claude Code),
**compilé, pas encore testé en jeu**.

## Ce que le joueur entend

À chaque déplacement du curseur : « Emplacement 3. » suivi des textes que le jeu dessine dans
l'emplacement (date, moment de la journée, niveau, temps de jeu, lieu…, ou « AUCUNE DONNÉE »).
L'annonce se répète quand l'écran se rouvre.

## Rétro-ingénierie

- Module `memcard\mcpanel.c`. `DrawSlot(écran, position, z, alpha, numéro, surligné, …)`
  (`0x140270770`) dessine un emplacement ; le 6e argument vaut 1 pour l'emplacement sous le
  curseur. Les textes sont recueillis avec `TextCapture` pendant cet appel.
- État d'un emplacement : écran + numéro × 0xB0 + 0x51C2 (0, 1 « pas de données », 2 partie).
- Machine d'états de l'écran : `0x140271750` (étape à +0x51B2 : 0-3 chargement des images,
  5 choix, 8-9 confirmation et résultat). Mode (sauvegarde, chargement…) : +0x2C (sens exact à
  confirmer).
- Questions et résultats (« Charger ce fichier ? », « Les données de sauvegarde existent déjà.
  Voulez-vous remplacer ces données ? »…) : messages intégrés à l'exe (`0x14080B480` et suivants,
  format des fichiers de messages, noms `…_MSG`, `…CONF`), affichés par des fonctions de `.arch`.
  Ils devraient passer par les fenêtres de message lues par `Dialogue.cs` : **à vérifier au test**.

## Pas encore traité

- Titre de l'écran (Sauvegarder / Charger / Effacer) à l'ouverture.
- Sauvegarde rapide (`memcard\quicksave.c`, `0x1402736D0`…).
