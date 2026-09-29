using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Pause menu ("camp") main list: Skill, Item, Persona, Equip, Status, Social link, System.
/// See docs/MENUS.md.
///
/// Found 29/09/2026 (camp\cmproot.c): the camp's task is created from the .arch section of the
/// exe, so we hook the root DRAW function (0x14014B350, rcx = work struct), called every frame
/// while the menu is shown. Work +0x20 = cursor 0-6: the draw asks the help line
/// GetHardcodedText(44 + cursor) ("Utiliser des compétences"…), which gives the entry order.
/// +0x28 = a mode or member index (logged, not used yet).
/// </summary>
internal class CampMenu
{
    private IHook<DrawDelegate>? _hook;
    private int _lastCursor = -1;
    private long _lastDrawTick;

    private static readonly string[] EntryKeys =
        { "camp_skill", "camp_item", "camp_persona", "camp_equip", "camp_status", "camp_social_link", "camp_system" };

    internal CampMenu(IReloadedHooks hooks)
    {
        SigScan("48 8B F1 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? BB 01 00 00 00 44 8B F3 89 5C 24 70 E8 ?? ?? ?? ?? 48 63 46 28",
            "CampRoot::Draw", address =>
            {
                _hook = hooks.CreateHook<DrawDelegate>(Draw, address - 0x51).Activate();
            });
    }

    private nint Draw(nint work, nint a2, nint a3, nint a4)
    {
        var res = _hook!.OriginalFunction(work, a2, a3, a4);
        try { Read(work); }
        catch (Exception e) { LogError("CampMenu read failed", e); }
        return res;
    }

    private void Read(nint work)
    {
        long now = Environment.TickCount64;
        bool reopened = now - _lastDrawTick > 500; // not drawn for half a second: menu (re)opened
        _lastDrawTick = now;
        if (!TryRead(work + 0x20, out int cursor) || cursor < 0 || cursor >= EntryKeys.Length) return;
        if (!reopened && cursor == _lastCursor) return;
        _lastCursor = cursor;

        TryRead(work + 0x28, out int mode);
        LogDebug($"[CampMenu] cursor {cursor} mode {mode}");
        string text = Loc.T(EntryKeys[cursor]);
        var help = GameStrings.Get(44 + cursor);
        if (!string.IsNullOrEmpty(help)) text += ". " + help;
        text += ", " + Loc.F("position", cursor + 1, EntryKeys.Length);
        if (reopened) text = Loc.T("camp_menu") + ". " + text;
        Speech.Say(text, true);
    }

    private delegate nint DrawDelegate(nint work, nint a2, nint a3, nint a4);
}
