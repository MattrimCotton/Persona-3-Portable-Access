---
name: new-component
description: Crée un nouveau lecteur (Component) du mod P3P Access avec son doc de système, en suivant les conventions du modèle P4G. À utiliser quand on ajoute un nouvel écran ou système à rendre accessible (menu, boutique, calendrier, combat…).
---

# Nouveau lecteur

1. Regarder le lecteur P4G le plus proche (agent `p4g-reference`) et un lecteur P3P existant
   dans `src/p3ppc.accessibility/Components/` pour reprendre le style exact (constructeur,
   hooks, journalisation, parole).
2. Créer `src/p3ppc.accessibility/Components/<Nom>.cs` :
   - constructeur qui reçoit les services comme les autres lecteurs, résout ses signatures et
     se désactive proprement si l'une manque ;
   - aucune lecture mémoire non gardée ; rien de lourd dans le thread du jeu ;
   - annonces via `Speech`, anti-répétition sur les curseurs.
3. L'enregistrer dans `Mod.cs` à la bonne place de l'ordre d'initialisation.
4. Ajouter les nouvelles touches et l'explication à l'aide F1 (`data/help_content.json`) et, si
   besoin, un réglage dans le menu F1.
5. Créer ou compléter `docs/<SYSTÈME>.md` : ce que ça lit, comment c'est trouvé (signatures,
   structures, offsets), les touches, les pièges.
6. Relecture `a11y-reviewer`, puis `/build-deploy` et `/test-session`.
