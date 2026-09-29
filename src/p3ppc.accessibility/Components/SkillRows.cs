using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Skill lists drawn with the shared skill row (shared\shdskill.c): the pause menu's Skill screen
/// and the Persona skill panels. Speaks the skill under the cursor when it changes.
/// See docs/PAUSE_MENU.md.
///
/// Found 29/09/2026 with Ghidra: DrawSkillRow(position, float, alpha, row, …) 0x1402C3400
/// dispatches on row kind. Row (shorts): +0x00 kind (0 name only, 1 skill with cost, 2 other),
/// +0x02 state (3 = row under the cursor in the Skill screen, 2 normal, 4 unusable),
/// +0x0A skill id. Name: GetSkillName(id) 0x1400A7F50 (battle\data\datcalc.c), fixed-width
/// record in the game's language.
/// </summary>
internal class SkillRows
{
    private IHook<DrawRowDelegate>? _hook;
    private int _lastId = -1;
    private long _lastSeen;

    internal SkillRows(IReloadedHooks hooks)
    {
        SigScan("48 89 5C 24 10 57 48 81 EC 80 00 00 00 0F 29 74 24 70 49 8B F9 66 48 0F 6E F1 0F 29 7C 24 60 48 8D 0D ?? ?? ?? ?? F2 0F 11 74 24 50 41 0F B6 D8",
            "Skill::DrawRow", address =>
            {
                _hook = hooks.CreateHook<DrawRowDelegate>(DrawRow, address).Activate();
            });
    }

    private void DrawRow(long position, float a2, byte alpha, nint row, long a5, float a6)
    {
        _hook!.OriginalFunction(position, a2, alpha, row, a5, a6);
        try { Read(row); }
        catch (Exception e) { LogError("SkillRows read failed", e); }
    }

    private void Read(nint row)
    {
        if (!TryRead(row, out short kind) || kind != 1) return;
        if (!TryRead(row + 2, out short state) || state != 3) return;
        if (!TryRead(row + 0x0A, out ushort id) || id == 0 || id >= 0x270) return;

        long now = Environment.TickCount64;
        bool reopened = now - _lastSeen > 500;
        _lastSeen = now;
        if (!reopened && id == _lastId) return;
        _lastId = id;

        string name = GameNames.Skill(id) ?? "";
        if (name.Length == 0) name = Loc.F("skill_unknown", id);
        LogDebug($"[SkillRows] skill {id}");
        Speech.Say(name, true);
    }

    private delegate void DrawRowDelegate(long position, float a2, byte alpha, nint row, long a5, float a6);
}
