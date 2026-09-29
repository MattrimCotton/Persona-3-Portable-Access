# Capture des textes dessinés (TextCapture)

Composant : `src/p3ppc.accessibility/Components/TextCapture.cs` (29/09/2026, Claude Code),
**compilé, pas encore validé en jeu** (utilisé par `SaveSlots` et `ConfigMenu`).

## Principe

Le jeu dessine ses textes courts (menus, valeurs, emplacements de sauvegarde) avec six variantes
de `DrawText`, une par alignement (`0x1402320E0`, `+0x110`, `+0x2D0`, `+0x3E0`, `+0x520`,
`+0x630`, signature dans `SIGNATURES.md`). Le 7e argument est le texte (encodage Atlus).

Plutôt que de décoder les données de chaque écran, un lecteur encadre l'appel à une fonction de
dessin du jeu qu'il connaît :

```csharp
TextCapture.Begin();
original(...);                 // le jeu dessine
var texts = TextCapture.End(); // textes dessinés entre-temps, dans l'ordre
```

Uniquement sur le fil du jeu (les fonctions de dessin y tournent). Pas d'imbrication.

## Fonctions accrochées et arguments (corrigé le 29/09/2026)

- 8 variantes de même prologue : écarts 0, 0x110, 0x2D0, 0x3E0, 0x520, 0x630, 0x890, 0x9E0 depuis
  `0x1402320E0`.
- **Non accroché** : le saut (`jmp`, 5 octets) en +0xCE0 (`0x140232DC0`) vers le dessin de texte
  « resserré » de `.arch`, par lequel le français fait passer les noms trop longs (objets,
  libellés). Les octets qui le suivent sont du code de protection, qu'une accroche plus longue
  que 5 octets écraserait. Ces textes échappent donc à la capture : les lecteurs lisent les
  numéros (objet, compétence…) et demandent le nom au jeu (`GameNames`).
- **Jusqu'à 11 arguments** : arguments 5 à 11 sur la pile (`[rsp+0xA0]` à `[rsp+0xD0]` dans
  les variantes). Le 9e et le 10e sont des pointeurs de sortie facultatifs : la première variante
  écrit la largeur du texte dans `*arg9` s'il n'est pas nul. La première version de l'accroche ne
  déclarait que 8 arguments et transmettait donc un 9e argument quelconque : **risque de
  plantage**, corrigé avant tout test en jeu. Règle générale : une accroche déclare au moins
  tous les arguments que la fonction lit (vérifier avec Ghidra ou `disasm.py`).

## TextSpy (recherche, mode débogage seulement)

F9 (jeu au premier plan, `DebugEnabled`) : pendant 20 s, chaque texte dessiné est écrit dans le
journal, une fois par (couleur, position, texte) : `[TextSpy] v2 colour FFFFFFFF at (512,300): …`.
Sert à repérer les textes d'un écran inconnu et la couleur de la ligne en surbrillance.

Remplace `TextSpy.cs` (supprimé le 29/09/2026), équivalent de `UiTextSpy` de P4G Access.
