---
name: snapshot-hunt
description: Trouve où une valeur affichée à l'écran vit en mémoire dans P3P (curseur de menu, contenu d'un panneau, élément sous le curseur en ville…) par la méthode des instantanés, avec le joueur qui fait les actions. À utiliser quand Ghidra et les signatures ne suffisent pas.
---

# Méthode des instantanés (« tu cliques, je scanne »)

Méthode éprouvée sur P4G : lire `C:\Users\asdes.ASUS\Documents\SourceCode\Persona-4-Golden-Access\database\SNAPSHOT_METHOD.md`
avant de commencer. Lecture seule de l'extérieur (`OpenProcess(VM_READ)` + `VirtualQueryEx` +
`ReadProcessMemory`) : pas d'injection, pas de risque de plantage.

## Outils

Scripts Python dans `tools/re/` (numpy). Les scripts de P4G (`cursor_hunt.py`, `peek.py`,
`snap_window.py`, `find_ptrs_to.py`, `arena_compare.py`, `scan_text.py`) ne sont pas dans son
dépôt : les réécrire ici au premier besoin, avec le nom de processus `P3P.exe`. Instantanés dans
`C:\p3p_re\shots\` (hors du dépôt).

## Déroulé

1. Dire au joueur **exactement** où aller et de ne plus bouger. Une consigne à la fois.
2. Instantané 0 (toute la mémoire privée engagée).
3. Le joueur fait **une seule** action (curseur à droite, un pas…) et dit ce que le jeu annonce
   ou affiche. Instantané suivant. Répéter 3 ou 4 fois.
4. Chercher le champ qui suit la séquence attendue (0,1,2,3 pour un curseur ; valeur connue).
   Scanner aussi les zones modifiées en **texte** : les chaînes battent les nombres.
5. **Confirmer sur un deuxième événement** : un seul diff peut être une constante trompeuse.
6. Ancrer sur une structure trouvable par signature : les offsets relatifs à la structure sont
   stables, les adresses absolues du tas non.
7. Consigner le résultat dans `docs/SIGNATURES.md` et le doc du système.
