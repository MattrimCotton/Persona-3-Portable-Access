---
name: build-deploy
description: Compile le mod p3ppc.accessibility et le déploie dans le dossier Mods de Reloaded-II. À utiliser quand on veut essayer une modification en jeu (« compile », « déploie », « mets à jour le mod »).
---

# Compiler et déployer P3P Access

1. **Le jeu doit être fermé** (sinon la DLL est verrouillée et la copie échoue) :
   `tasklist //NH //FO CSV | grep -q '"P3P.exe"' && echo lance || echo ferme` (Bash). S'il tourne, demander au joueur de quitter le
   jeu ; ne jamais le fermer de force sans son accord (partie non sauvegardée).
2. Compiler en Debug pour voir les erreurs :
   `dotnet build src/p3ppc.accessibility/p3ppc.accessibility.csproj -c Debug`
   Corriger toutes les erreurs ; signaler les nouveaux avertissements.
3. Déployer (publication Release dans le dossier du mod, comme `BuildLinked.ps1` de P4G) :
   `dotnet publish src/p3ppc.accessibility/p3ppc.accessibility.csproj -c Release -o "$env:RELOADEDIIMODS/p3ppc.accessibility" /p:OutputPath="./bin/Release"`
   (`RELOADEDIIMODS` = `D:\SteamLibrary\Reloaded-II\Mods`, défini dans `.Codex/settings.json`.)
4. Lancer `python tools/lang_check.py --complete french` (doit afficher OK). Vérifier que
   `ModConfig.json`, les fichiers de `data/` et les traductions `Lang\*.json` sont bien présents dans le dossier du
   mod, et que les dépendances listées dans `ModConfig.json` existent dans `Mods\`.
5. Dire au joueur, en une ou deux phrases : c'est déployé, relancer le jeu par Reloaded-II, et
   **quoi tester** (skill `test-session` si le test a plusieurs étapes).

Si le projet `src/p3ppc.accessibility` n'existe pas encore : le dire, et proposer de le créer à
partir du modèle P4G (étape 1 de `PLAN.md`).
