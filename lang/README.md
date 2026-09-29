# Translating P3P Access

P3P Access speaks two kinds of text:

- **The game's own text** (dialogue, choices…): read from the game, in the language the game
  runs in. Nothing to translate.
- **The mod's own messages** (menu items drawn as pictures, "2 of 5", "Press any key", help…):
  these live in this folder, one file per language. **This is what you translate.**

French and English are maintained by the project (French is the priority language). Any other
language is welcome.

## Files

| File | Role |
|---|---|
| `english.json` | Reference: every message exists here first. |
| `french.json` | French, always complete. |
| `context.json` | Where and when each message is spoken. Read it while translating. Not loaded by the mod. |
| `<language>.json` | One file per language, named with the **Steam language id** (below). |

Steam language ids for the languages of Persona 3 Portable: `english`, `french`, `german`,
`italian`, `spanish`, `japanese`, `koreana`, `schinese` (Simplified Chinese), `tchinese`
(Traditional Chinese).

## Add or update a translation

1. Copy `english.json` to `<language>.json` (for example `german.json`).
2. In `_meta`, set `language` to the language's own name and add your name to `translators`.
3. Translate the **values**, never the keys (the part before the colon).
4. Keep placeholders exactly: `{0}`, `{1}` are replaced by numbers or names. Example:
   `"position": "{0} of {1}"` becomes `"position": "{0} von {1}"`.
5. Keep leading or trailing spaces when the English text has them.
6. Keep messages short, most important word first: they are spoken by a screen reader during
   play. For menu items, use the game's official wording in your language.
7. Check your file: `python tools/lang_check.py`. It lists missing keys, unknown keys and broken
   placeholders. The same check runs on GitHub for every pull request.
8. Open a pull request, or send the file to the maintainers.

A partial translation works: any missing message is spoken in English until it is translated.
When new messages are added to `english.json`, the check lists them as missing for your
language.

## Try it in the game

Copy your file into the mod's `Lang` folder (`Reloaded-II\Mods\p3ppc.accessibility\Lang`), then
in the mod's settings set **Mod language** to your language (or **Auto** to follow the game's
Steam language), and restart the game.
