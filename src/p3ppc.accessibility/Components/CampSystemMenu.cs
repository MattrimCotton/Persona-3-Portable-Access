using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Pause menu System submenu (camp\cmpsystem.c): Quest, Glossary, Config, Difficulty, Quick Save,
/// Delete, Load, Title screen, Quit. Speaks the entry under the cursor with its help line.
/// See docs/PAUSE_MENU.md.
///
/// Found 29/09/2026 with Ghidra: Draw(work) 0x14015A150, every frame. Work: +0x18 flags (2 =
/// list shown), +0x36 first visible entry, +0x2C cursor in the visible entries. Entry labels are
/// hardcoded texts (table 0x1405DF5F0…: 60, 61, 62, 132, 136, 63, 64, 65, 133); help line =
/// GetHelp(entry, 0) 0x14015DBE0. The second list of the same work (+0x30 / +0x32, +0x58) is a
/// sub-list (not read yet).
/// </summary>
internal class CampSystemMenu
{
    private IHook<DrawDelegate>? _hook;
    private GetHelpDelegate? _getHelp;
    private int _lastEntry = -1;
    private long _lastDrawTick;

    private static readonly int[] EntryText = { 60, 61, 62, 132, 136, 63, 64, 65, 133 };

    internal CampSystemMenu(IReloadedHooks hooks)
    {
        SigScan("49 89 73 10 4C 8B F1 4D 89 6B 18 48 8D 0D ?? ?? ?? ?? 4D 89 7B 20 45 0F 29 4B 98",
            "CampSystem::Draw", address =>
            {
                _hook = hooks.CreateHook<DrawDelegate>(Draw, address - 0x42).Activate();
            });
        SigScan("48 89 5C 24 08 57 48 83 EC 70 48 63 F9 48 8D 0D ?? ?? ?? ?? 48 63 DA E8 ?? ?? ?? ?? 66 0F 6F 05 ?? ?? ?? ?? 48 8D 0C DB",
            "CampSystem::GetHelp", address =>
            {
                _getHelp = hooks.CreateWrapper<GetHelpDelegate>(address, out _);
            });
    }

    private void Draw(nint work)
    {
        _hook!.OriginalFunction(work);
        try { Read(work); }
        catch (Exception e) { LogError("CampSystemMenu read failed", e); }
    }

    private void Read(nint work)
    {
        if (!TryRead(work + 0x18, out uint flags) || (flags & 2) == 0) return;
        if (!TryRead(work + 0x36, out short top) || !TryRead(work + 0x2C, out short cursor)) return;
        int entry = top + cursor;
        if (top < 0 || cursor < 0 || entry >= EntryText.Length) return;

        long now = Environment.TickCount64;
        bool opened = now - _lastDrawTick > 500;
        _lastDrawTick = now;
        if (!opened && entry == _lastEntry) return;
        _lastEntry = entry;

        var parts = new List<string>();
        if (opened) parts.Add(Loc.T("camp_system"));
        parts.Add(GameStrings.Get(EntryText[entry]) ?? Loc.F("sys_unknown", entry));
        if (_getHelp != null && GameStrings.ReadGameString(_getHelp(entry, 0), 200) is { Length: > 0 } help) parts.Add(help);
        parts.Add(Loc.F("position", entry + 1, EntryText.Length));
        LogDebug($"[CampSystemMenu] entry {entry}");
        Speech.Say(string.Join(". ", parts), true);
    }

    private delegate void DrawDelegate(nint work);
    private delegate nint GetHelpDelegate(int entry, int variant);
}
