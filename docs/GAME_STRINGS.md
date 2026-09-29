# Textes courts du jeu (« hardcoded text »)

Table de 142 textes par langue, lue par `Native/Text/GameStrings.cs` (29/09/2026). Les utiliser
au lieu de messages du mod : ils sont déjà dans la langue du jeu, sans traduction à maintenir.

## Accès

- `GetHardcodedText(a, b)` = `table[langue][a + b]`, fonction `0x140257330` (signature de
  `p3ppc.unhardcodedNames`, AnimatedSwine37). Tables par langue en `0x1407D7BE0` (pointeurs),
  langue courante : global `0x142F81298`. Numéros de langue : 0 japonais, 1 anglais,
  5 **français**, 6 allemand, 7 italien, 8 espagnol (2, 3, 4 : chinois et coréen).
- Décodage : encodage Atlus (`AtlusEncoding`). Attention : caractère sur deux octets
  `h l` = glyphe `(h & 0x7F) × 0x80 + l`, index de la table `P3P_EFIGS.tsv` = glyphe − 0x60.
- Pour relire la table : script de lecture dans l'historique du 29/09/2026 (lire les
  pointeurs de la langue 5, décoder chaque chaîne). Ne pas versionner le texte complet (droit
  d'auteur) : seulement les numéros ci-dessous.

## Contenu (numéros)

| Numéros | Contenu |
|---|---|
| 0-4 | tous les membres, renfort, Oui, Non, choix d'une Persona à abandonner |
| 5-8 | couleurs des cartes (épées, deniers, bâtons, coupes) |
| 9-30 | arcanes (Le Mat… L'Æon) |
| 37-43 | questions et aides des sous-menus (lien social de qui, équipement de qui, Personae) |
| **44-50** | **aides du menu pause** : compétences, objets, Personae, équipement, état, liens sociaux, paramètres |
| **51-54** | **menu système** : Config, Effacer les données, Charger les données, Écran titre |
| 55-58 | questions de l'utilisation d'une compétence (qui, laquelle, sur qui, sur tous) |
| 59-65 | sous-menu système du menu pause : confirmation de l'état, Quête, Glossaire, Config, Effacer, Charger, Écran titre |
| 66-72 | jours de la semaine (abrégés, dimanche d'abord) |
| 73-77 | stats sociales (courage, connaissance, discipline, empathie, éloquence) |
| 78-79 | sauvegarde réussie / échouée (`%s`) |
| 80-85 | lieux (lycée, centre commercial, gare, rue commerçante, sanctuaire, dortoir) |
| 86-90 | difficultés (Débutant… Maniaque) |
| 91 | « Sélectionnez le personnage principal. » |
| 92-99 | abréviations des caractéristiques (Fo, Ma, En, Ag, Ch) et stats sociales du héros |
| 100-116 | Oui, Non, **commandes de combat** (Attaque, Compétence, Objet, Tactique, Changer de Persona, Courir, Défense, Attendre, Invocation, Analyse, Ruée, Demander de l'aide) |
| **117-125** | **moments de la journée** (tôt le matin… tard le soir, journée, Heure sombre) |
| 126-129 | formats de la date et des sauvegardes (niveau, temps de jeu, difficulté) |
| 132-138 | niveau de difficulté, Quitter le jeu, Personnalisé, Sauvegarde rapide, AUCUNE DONNÉE, NOUVELLE PARTIE |
| 140 | manette déconnectée |

Autre table voisine : `GetGlossaryText(a, b)` (349 textes du glossaire, même source), non
encore utilisée.
