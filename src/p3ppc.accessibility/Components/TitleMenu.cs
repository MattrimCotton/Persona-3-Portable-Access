using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Title screen: "press any key" then the menu. The entries are pictures (title/menu.spr), so we
/// read the cursor and speak our own labels. See docs/MENUS.md.
///
/// Found 28/09/2026 (titledraw.c): the title update (0x14024BCA0) gets the task in rcx;
/// task +0x48 = work struct: +0x00 state (7 = "press any key", 9 = menu), +0x28 cursor 0-4.
/// Order of the entries, from the table of sprite ids drawn top to bottom (0x1407D6F60 =
/// 3, 4, 12, 5, 11 = NEW GAME, LOAD GAME, RESUME, config, EXIT in menu.spr): New game, Load,
/// Continue, Config, Quit. Default cursor: Continue if there is a quick save, else Load if
/// there are saves, else New game.
/// </summary>
internal class TitleMenu
{
    private IHook<UpdateDelegate>? _hook;
    private int _lastState = -1;
    private int _lastCursor = -1;

    private static readonly string[] Entries =
        { "title_new_game", "title_load", "title_continue", "title_config", "title_quit" };

    internal TitleMenu(IReloadedHooks hooks)
    {
        SigScan("40 53 41 54 41 56 41 57 48 81 EC 88 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 78 4C 8B F1 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 49 8B CE E8 ?? ?? ?? ?? 48 8B D8 48 63 00 83 F8 1C",
            "Title::Update", address =>
            {
                _hook = hooks.CreateHook<UpdateDelegate>(Update, address).Activate();
            });
    }

    private nint Update(nint task)
    {
        var res = _hook!.OriginalFunction(task);
        try { Read(task); }
        catch (Exception e) { LogError("TitleMenu read failed", e); }
        return res;
    }

    private void Read(nint task)
    {
        nint work = ReadPtr(task + 0x48);
        if (work == 0 || !TryRead(work, out int state)) return;
        bool entered = state != _lastState;
        _lastState = state;

        if (state == 7)
        {
            if (entered) Speech.Say(Loc.T("press_any_key"), true);
            return;
        }
        if (state != 9) { _lastCursor = -1; return; }

        if (!TryRead(work + 0x28, out int cursor)) return;
        if (!entered && cursor == _lastCursor) return;
        _lastCursor = cursor;
        if (cursor < 0 || cursor >= Entries.Length) return;
        Speech.Say(Loc.T(Entries[cursor]) + ", " + Loc.F("position", cursor + 1, Entries.Length), true);
    }

    private delegate nint UpdateDelegate(nint task);
}
