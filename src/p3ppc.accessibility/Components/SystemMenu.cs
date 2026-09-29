using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Simple system menu of the start of the game ("Menu des commandes": Config, Load, Title...),
/// task "cmpSimpleSystem" (camp\cmpsimplesystem.c). See docs/MENUS.md.
///
/// Found 29/09/2026: update 0x1401517D0, task +0x48 = work: +0x00 state (5 = list active),
/// +0x38 cursor (short), +0x6D0 number of entries, +0x6D4 entry ids (int each). Entry id n is
/// drawn with sprite table[23 + n] of c_main_01.spr (init\camp.bin): 0 CONFIG, 1 QUICK SAVE,
/// 2 SUPPRIMER (data screen in mode 4), 3 DONNÉES CHARGER (data screen, mode 0), 4 RETOUR AU
/// TITRE, 5 END GAME (4 and 5 open a question read by Dialogue).
/// </summary>
internal class SystemMenu
{
    private IHook<UpdateDelegate>? _hook;
    private int _lastState = -1;
    private int _lastCursor = -1;

    private static readonly string[] EntryKeys =
        { "sys_config", "sys_quick_save", "sys_delete", "sys_load", "sys_title", "sys_end_game" };

    /// <summary>The game's own label of each entry (hardcoded texts, docs/GAME_STRINGS.md).</summary>
    private static readonly int[] GameText = { 51, 136, 52, 53, 54, 133 };

    internal SystemMenu(IReloadedHooks hooks)
    {
        SigScan("40 53 48 83 EC 20 48 8B D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 5B 48 48 8B CB E8 ?? ?? ?? ?? 85 C0 74 0D 48 C7 C0 FF FF FF FF",
            "SimpleSystem::Update", address =>
            {
                _hook = hooks.CreateHook<UpdateDelegate>(Update, address).Activate();
            });
    }

    private nint Update(nint task)
    {
        var res = _hook!.OriginalFunction(task);
        try { Read(task); }
        catch (Exception e) { LogError("SystemMenu read failed", e); }
        return res;
    }

    private void Read(nint task)
    {
        nint work = ReadPtr(task + 0x48);
        if (work == 0 || !TryRead(work, out int state)) return;
        bool entered = state == 5 && _lastState != 5;
        _lastState = state;
        if (state != 5) { _lastCursor = -1; return; }

        if (!TryRead(work + 0x38, out short cursor) || !TryRead(work + 0x6D0, out int count)) return;
        if (count < 1 || count > 8 || cursor < 0 || cursor >= count) return;
        if (!entered && cursor == _lastCursor) return;
        _lastCursor = cursor;

        if (!TryRead(work + 0x6D4 + cursor * 4, out int id)) return;
        string name = id >= 0 && id < EntryKeys.Length
            ? GameStrings.Get(GameText[id]) is { Length: > 0 } g ? g : Loc.T(EntryKeys[id])
            : Loc.F("sys_unknown", id);
        string text = name + ", " + Loc.F("position", cursor + 1, count);
        if (entered) text = Loc.T("sys_menu") + ". " + text;
        LogDebug($"[SystemMenu] cursor {cursor}/{count} id {id}");
        Speech.Say(text, true);
    }

    private delegate nint UpdateDelegate(nint task);
}
