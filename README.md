# Persona 3 Portable Access

A screen-reader accessibility mod for the Steam version of **Persona 3 Portable**, built as a
[Reloaded II](https://reloaded-project.github.io/Reloaded-II/) mod loaded with
[Persona Essentials](https://github.com/Sewer56/p5rpc.modloader). It speaks the game through the
user's screen reader (NVDA / SAPI via Tolk), so the game can be played without sight. It follows
the design of [Persona 4 Golden Access](https://github.com/AquaRose7/Persona-4-Golden-Access).

**Status: early development, not yet released.** Tested in-game by a blind player at each step.

## What works so far

- Dialogue: every message and page, the speaker's name, accents, and Yes / No choices.
- Title screen, protagonist (male / female) and difficulty selection.
- Speech history (Shift+[ / Shift+]), repeat last (Shift+P), mute dialogue (Shift+M).
- Mod messages in French and English (follows the game's Steam language by default).

Next: every in-game menu (pause menu, save / load, name entry, system messages), then the town,
battles and Tartarus. Full roadmap: [docs/ROADMAP.md](docs/ROADMAP.md) (in French).

## Repository layout

| Path | What it is |
|---|---|
| `src/p3ppc.accessibility/` | The mod (.NET Reloaded II project, entry point `Mod.cs`, features under `Components/`). |
| `docs/` | Per-system source-of-truth docs: how each part was reverse-engineered and how the code works. |
| `tools/` | Log reader and reverse-engineering scripts (CPK / PAK extraction, text decompilation, signature and xref search). |
| `lib/tolk/` | Tolk and screen-reader client DLLs. |

Project notes and working docs are written in French.

## Building

```bash
dotnet build src/p3ppc.accessibility/p3ppc.accessibility.csproj -c Debug
```

Deploying copies the Release build into the Reloaded II `Mods` folder (see
`.claude/skills/build-deploy/SKILL.md`). The game must be closed while deploying.

## Credits and license

GPL-3.0, see [LICENSE](LICENSE). Based on the work of Haru and AnimatedSwine37 (Persona 4 Golden
Access, `p3ppc.*` mods). Game text decoding uses the character tables of
[Atlus Script Tools](https://github.com/tge-was-taken/Atlus-Script-Tools). No game files are
included in this repository.
