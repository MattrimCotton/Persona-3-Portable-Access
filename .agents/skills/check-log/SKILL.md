---
name: check-log
description: Lit le dernier journal Reloaded-II de P3P et diagnostique les erreurs du mod (exceptions, signatures introuvables, plantages). À utiliser après un test en jeu, un plantage, ou quand le joueur dit « ça ne parle pas », « le jeu a planté ».
---

# Lire le journal Reloaded de P3P

1. `pwsh -NoProfile -File tools/checklog.ps1 -Tail 150` (ou `-Full` si besoin).
   Les journaux sont dans `%APPDATA%\Reloaded-Mod-Loader-II\Logs`.
2. Si le jeu a planté sans trace dans le journal, regarder aussi
   `D:\SteamLibrary\steamapps\common\P3P\crashdat` (fichiers récents) et l'Observateur
   d'événements Windows (Application, erreurs de `P3P.exe`).
3. Chercher en priorité :
   - le mod est-il chargé (`p3ppc.accessibility`) et ses dépendances aussi ?
   - signatures introuvables (« not found », « pattern ») → noter dans `docs/SIGNATURES.md`
     comme cassées, voir avec l'agent `re-analyst` ;
   - exceptions dans un callback de hook → fichier et ligne ;
   - la dernière chose journalisée avant un plantage (souvent un changement de zone).
4. Rendre un diagnostic court : cause probable, fichier:ligne, correction proposée. Consigner un
   plantage nouveau dans `docs/TEST_LOG.md`.
