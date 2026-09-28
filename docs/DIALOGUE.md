# Dialogues et choix de réponse

Composant : `src/p3ppc.accessibility/Components/Dialogue.cs`, lecture du texte :
`Native/Text/MsgText.cs`. Modèle : `Dialogue.cs` et `Native/Text/Text.cs` de P4G Access.

## Comment la fonction a été trouvée (28/09/2026)

1. Les signatures P4G de `MsgWindow::DrawDialog` et `StartDialog` ne donnent **aucun résultat**
   dans `P3P.exe` (outil `tools/re/sigscan.py`, qui cherche dans le fichier sur disque).
2. Désassemblage du `DrawDialog` de P4G (`tools/re/disasm.py --exe ...P4G.exe 0x140461880`),
   puis recherche dans P3P de petits fragments caractéristiques. Le fragment
   `83 23 E7 83 0B 20` (`and [rbx],~0x18` puis `or [rbx],0x20`, fin de l'affichage d'une liste
   de choix) donne un seul résultat, dans la fonction `0x14023B510`.
3. Cette fonction contient **le même code que `DrawDialog` de P4G**, mais à l'intérieur d'une
   boucle : elle parcourt la liste chaînée des fenêtres de message ouvertes et dessine chacune.
   P3P n'a donc pas de fonction « dessiner une fenêtre » à accrocher : on accroche la boucle
   (`MsgWindow::DrawAll`), et après l'appel original on parcourt nous-mêmes la même liste.

Signatures : voir `SIGNATURES.md`.

## Structures (P3P, avec l'équivalent P4G entre crochets)

Nœud de la liste (tête dans le global `0x1409F11D0`) : `+0x08` nœud suivant, `+0x10` numéro
d'exécution, `+0x18` pointeur vers la fenêtre (`dialogInfo`).

`dialogInfo` :

| Offset | Contenu |
|---|---|
| `+0x00` | drapeaux. Bits 0-2 : état du message (texte dessiné si ≥ 3). Bit 17 : message caché. Bits 3-5 : état de la liste de choix (dessinée si ≥ 3). Bit 18 : liste cachée |
| `+0x20` | texte du nom de l'orateur [0x20] |
| `+0x38` | texte du message [0x38] |
| `+0x4C` | remis à 0 quand le message passe à l'état 4 [0x50] |
| `+0x60` | texte de la liste de choix [0x70] |
| `+0x6E` | option sélectionnée, `short`, -1 = pas encore de curseur [0x7E] |

La page courante et le nombre de pages (P4G : `+0x48` / `+0x4A`) ne sont **pas encore
identifiés** dans P3P. En mode débogage, le mod écrit dans le journal les octets `+0x40` à `+0x4F`
à chaque nouveau message, pour les retrouver lors d'un test.

Texte mis en page (identique à P4G, vérifié dans la fonction de dessin du texte `0x140231380`) :
texte `+0x40` première ligne ; ligne `+0x08` position Y, `+0x20` premier glyphe, `+0x38` ligne
suivante ; glyphe `+0x00` deux octets (octet bas d'abord), `+0x38` glyphe suivant.

## Fonctionnement du mod

- Après chaque appel de `DrawAll` (une fois par image), on parcourt la liste (32 fenêtres au
  plus). Toutes les lectures passent par `ReadProcessMemory` (`Utils.TryRead`) et toutes les
  boucles sont bornées.
- **Message** : le jeu **réutilise la même liste de lignes** pour la page suivante (test 2). On compare donc à chaque image la liste, la page supposée (`+0x46`, `short`, 1 puis 2 dans le journal du test 1) et le premier glyphe, et le texte complet toutes les 6 images ; on parle quand le texte change.
  Le nom de l'orateur est ajouté devant (« Yukari : … ») seulement quand il change.
  Le `>` en début de certains messages est retiré, comme dans P4G.
- **Choix** : l'option sous le curseur est lue à chaque déplacement. La première option d'une
  nouvelle liste est mise en file d'attente pour ne pas couper la question.
- Quand la fenêtre lue se ferme, on oublie son texte : un même texte dans une nouvelle fenêtre
  sera relu.
- Décodage : tables de glyphes d'Atlus Script Tools (`P3P_EFIGS.tsv` pour le français et les
  autres langues européennes), choisies d'après la langue Steam du jeu.
- Maj+M coupe ou rétablit la lecture des dialogues (le texte reste dans l'historique).
  Maj+P répète, Maj+[ et Maj+] parcourent l'historique.

## Inconnues à vérifier au premier test

- Le texte est-il complet dès la première image (pas d'effet machine à écrire dans la liste de
  lignes) ? Si le mod lit des bouts de phrase, il faudra attendre que le texte soit stable.
- Les accents du français sont-ils bien décodés avec `P3P_EFIGS.tsv` ?
- Pas de `StartDialog` accroché (P4G s'en sert pour relire un même message rouvert) ; remplacé
  par l'oubli du texte à la fermeture de la fenêtre.
