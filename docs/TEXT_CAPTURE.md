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

## TextSpy (recherche, mode débogage seulement)

F9 (jeu au premier plan, `DebugEnabled`) : pendant 20 s, chaque texte dessiné est écrit dans le
journal, une fois par (couleur, position, texte) : `[TextSpy] v2 colour FFFFFFFF at (512,300): …`.
Sert à repérer les textes d'un écran inconnu et la couleur de la ligne en surbrillance.

Remplace `TextSpy.cs` (supprimé le 29/09/2026), équivalent de `UiTextSpy` de P4G Access.
