# Menu Config (réglages PC)

Lecteur : `src/p3ppc.accessibility/Components/ConfigMenu.cs`. Écrit le 29/09/2026 (Claude Code),
**compilé, pas encore testé en jeu**.

## Ce que le joueur entend

- À l'ouverture : « Configuration. Onglet Audio. Confirmer, 1 sur 7 ».
- En changeant de ligne : « Musique : 8, 2 sur 7. » suivi de la ligne d'aide du jeu si elle existe.
- Gauche / droite sur une ligne : seulement la nouvelle valeur (« 9 », « OUI »…).
- Changement d'onglet (touches d'onglet du jeu) : « Onglet Jeu. Texte auto : OUI, 2 sur 8 ».
- Onglets Clavier et Manette : le nom de l'action seulement ; les touches sont dessinées en
  icônes, pas en texte (à faire plus tard).

Libellés, valeurs, aides et noms d'onglets sont les textes du jeu, dans sa langue (encodage
Atlus, accents compris). Seuls « Configuration », « Onglet {0} » et « Réglage {0} » (secours si
le libellé est illisible) viennent de `lang/*.json`.

## Rétro-ingénierie (Ghidra, voir `GHIDRA.md`)

Textes des réglages dans l'exe, en 9 langues (encodage Atlus), dans des tables de pointeurs par
langue. `xrefs` sur la table des tables de libellés (`0x1407824C0`) mène à `GetLabel` et à
`DrawList` (`0x140122070`), appelée chaque image par le dessin de l'écran (`0x140120EE0`).

Structure de travail (argument de `DrawList`) :

| Champ | Sens |
|---|---|
| +0x3C | onglet : 0 Audio, 1 Jeu, 2 Graphismes, 3 Affichage, 4 Clavier, 5 Manette |
| +0x38 | première ligne visible (défilement ; 7 lignes visibles) |
| +0x34 | curseur dans la fenêtre visible ; ligne = +0x38 + +0x34 |
| +0xA54 + n × 0x20 | entrée du réglage de libellé n (+0x00 valeur courte, +0x14 type) |

Tables (données) : lignes par onglet `{7, 8, 8, 6, 31, 31}` (`0x1407822C8`), premier libellé par
onglet `{0, 7, 15, 23, 29, 60}` (`0x1407822E0`) : libellé de la ligne r de l'onglet t = premier[t] + r.
Le lecteur retrouve ces tables à partir du déplacement lu dans `GetLabel` (vérifié : premier[0] = 0).

`DrawValue` (`0x140127460`) dessine le texte de la valeur de chaque ligne des onglets 0 à 3 (types
1 à 4 et 6 : choix, nombre, résolution `%dx%d`, périphérique audio). Le lecteur l'accroche et,
pour la seule ligne sous le curseur, recueille les textes avec `TextCapture`. Le jeu mesure puis
dessine le texte : les doublons sont retirés.

La barre des onglets est dessinée par `0x140123670` (onglet surligné = +0x3C).

## Pas encore traité

- Fenêtre « Enregistrer les modifications ? » et compte à rebours de l'affichage : créées par du
  code de `.arch` (`0x140127BF0`, fonctions `thunk_…`), non lisible. À observer au test (TextSpy F9).
- Touches du clavier et de la manette (icônes, `0x1401266B0`).
