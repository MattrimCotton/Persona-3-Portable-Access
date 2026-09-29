# Sources fiables sur P3P

Sources choisies le 28/09/2026 pour la rétro-ingénierie, le contenu du jeu et l'aide au joueur.
Les classer par fiabilité : **les fichiers du jeu eux-mêmes** d'abord, puis le code des mods publiés
(signatures déjà validées), puis les guides.

## Le jeu lui-même (source la plus exacte)

| Fichier | Contenu |
|---|---|
| `P3P\data\umd0.cpk` (5 Go) | fichiers principaux : scripts FlowScript (BF), modèles, textes japonais |
| `P3P\data_FR\umd0.cpk` (154 Mo) | **tous les textes français** : messages (BMD), noms, menus sous forme de tables |
| `P3P\data\umd1.cpk` | audio et vidéos |

Extraction (outils installés le 28/09/2026 dans `%USERPROFILE%\Tools\AtlusScriptTools` et
`...\Tools\CriFsLib.GUI`) :

```bash
# 1. Extraire une archive CPK (sans filtre : liste le contenu)
dotnet run --project tools/re/CpkExtract -- D:\SteamLibrary\steamapps\common\P3P\data_FR\umd0.cpk extracted\data_FR "."
# 2. Décompiler tous les .bf / .bmd en .flow / .msg lisibles (table P3P_EFIGS, accents corrects)
powershell -File tools/re/decompile-texts.ps1
```

`data_FR\umd0.cpk` contient 6 189 fichiers, dont 3 452 scripts `.bf` et 9 `.bmd` à la racine des
dossiers ; d'autres messages sont dans les archives `.pac` / `.bin` (à ouvrir avec Amicitia ou
PackTools). `conver_temp\txt\` contient des dumps de messages encodés (fichiers de traduction).
Piège : les 2 149 fichiers `conver_temp\bf\*.bf` ne sont **pas** des scripts mais des messages
(en-tête `MSG1` à +8) : messages de combat (« Un ennemi détecté ! »…), utiles pour l'étape 4.
`decompile-texts.ps1` les reconnaît à leur en-tête et les décompile en `.msg`.
Sites qui refusent la lecture automatique (GameFAQs, Game UI Database) : les lire avec Claude in
Chrome.
Méthode générale : doc « Extracting The Game's Files » ci-dessous.
Les fichiers extraits sont protégés par le droit d'auteur : dans `extracted/` ou `decompiled/`
(déjà ignorés par git), jamais versionnés.

## Modding et rétro-ingénierie

- [Persona Modding Docs](https://animatedswine37.github.io/persona-modding-docs/) (AnimatedSwine37) :
  extraction des fichiers, FlowScript, ordre de chargement Reloaded. Sources :
  [GitHub](https://github.com/AnimatedSwine37/persona-modding-docs).
- [Mods p3ppc d'AnimatedSwine37](https://github.com/AnimatedSwine37?tab=repositories&q=p3ppc) :
  signatures validées sur P3P Steam (noms, liens sociaux, menu de statut, fusion, films).
- [Persona Essentials](https://github.com/Sewer56/p5rpc.modloader) (Sewer56) : chargeur de mods.
- [Atlus Script Tools](https://github.com/tge-was-taken/Atlus-Script-Tools) : tables de
  caractères `P3P_*.tsv` (utilisées par le mod) et bibliothèque des fonctions FlowScript de P3P.
- [The Cutting Room Floor, P3P](https://tcrf.net/Persona_3_Portable) : contenu inutilisé,
  différences de versions.
- [Guide de modding P3P PC (GameBanana)](https://gamebanana.com/tuts/15677).

## Contenu du jeu (aide au joueur, ordre des écrans)

- [GameFAQs, guide de Lukaight (PC, v1.61, 30/06/2026)](https://gamefaqs.gamespot.com/pc/370651-persona-3-portable/faqs/80884) :
  le plus récent ; côtés masculin et féminin, liens sociaux, Shuffle Time. (Le site bloque la
  lecture automatique : à consulter dans un navigateur.)
- [GameFAQs, guide de KADFC](https://gamefaqs.gamespot.com/pc/370651-persona-3-portable/faqs/60441) :
  différences entre P3 FES et P3P.
- [Megami Tensei Wiki, P3P](https://megamitensei.fandom.com/wiki/Persona_3_Portable).
- [Guide Steam pour débutants, sans spoiler](https://steamcommunity.com/sharedfiles/filedetails/?id=3117592332).

Archives internes (`.bin`, `.pak`, `.pac`) : `python tools/re/pakunpack.py <archive> <sortie> -r`.
Exemple : `title\sex_select.bin` contient `sex_select.bmd` (messages du choix du sexe et de la
difficulté) et `sex_select_help.bmd` (descriptions des difficultés, non lues par le lecteur de
dialogues : affichées par un autre chemin, à traiter avec les menus).
Piège : le compilateur écrit `AtlusScriptCompiler.log` dans le dossier courant ; deux instances
dans le même dossier plantent (d'où un dossier de travail par appel dans `decompile-texts.ps1`).

État au 28/09/2026 (`data_FR`) : 1 261 scripts d'événements (`event\`, un `.flow` + un `.msg` par
script, versions masculine `_0xx` et féminine `_3xx`), 39 scripts de lieux (`field\`), et 36 fichiers
de messages dans les archives : boutiques et Velvet Room (`facility\`), mode personnalisé
(`camp\mode_custom`), liens sociaux (`commu\`), Shuffle Time (`battle\result\`), carte
(`lmap\`), choix du sexe et de la difficulté (`title\`). Les libellés des menus (Compétences,
Objets…) ne sont pas dans ces messages : probablement dans des tables ou des images, à chercher
pour l'étape 1b.
