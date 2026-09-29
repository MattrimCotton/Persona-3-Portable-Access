---
name: port-hook
description: Porte une fonction de Persona 4 Golden Access vers P3P Access — trouver le code P4G, retrouver la fonction ou la structure équivalente dans P3P.exe, écrire le hook et le documenter. À utiliser pour « fais lire les dialogues / menus / la boutique… comme dans P4G ».
---

# Porter un hook de P4G vers P3P

1. **Référence** : lancer l'agent `p4g-reference` sur la fonction voulue. Il rend les fichiers,
   hooks, signatures P4G et le classement *tel quel / à transposer / à concevoir*.
2. **Déjà trouvé ?** Lire `docs/SIGNATURES.md` et le doc du système dans `docs/`.
3. **Retrouver dans P3P** : lancer l'agent `re-analyst` avec la liste « à transposer ». Ordre :
   mods `p3ppc.*` d'AnimatedSwine37 → signature P4G essayée telle quelle → chaînes et scripts
   décompilés → Ghidra → méthode des instantanés (`/snapshot-hunt`) si la donnée est sur le tas.
4. **Écrire le code** dans `src/p3ppc.accessibility/` en suivant le modèle P4G :
   - signature résolue au démarrage, journal clair si introuvable, fonction désactivée sinon ;
   - callback : try/catch, toujours appeler l'original, lectures gardées par `IsReadable` ;
   - annonce via `Speech`, info importante en premier ; ajout des touches dans l'aide F1.
5. **Documenter** : ligne(s) dans `docs/SIGNATURES.md` (statut `non testée`) et section dans
   `docs/<SYSTÈME>.md` (comment c'est trouvé, comment ça marche, pièges).
6. **Relire** avec l'agent `a11y-reviewer`, corriger les points bloquants.
7. `/build-deploy`, puis `/test-session`. Passer la signature à `validée en jeu` seulement après
   le retour du joueur.
