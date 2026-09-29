using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// New-game screen: choice of protagonist (male / female) then difficulty. The explanations and
/// the Yes / No confirmations are message windows (read by <see cref="Dialogue"/>); the two
/// choices themselves are pictures, so we read the cursors. See docs/MENUS.md.
///
/// Found 28/09/2026 from the source path string "...\src\sex_select\sexselect.c": the per-frame
/// update of the screen (0x1402AA210) gets the task in rcx; task +0x48 = work struct:
///   +0x20 int   state (5 = choosing the protagonist, 9 = choosing the difficulty)
///   +0x24 short protagonist cursor: 0 male, 1 female (message shown = cursor + 2 = msg_HERO / msg_HEROINE)
///   +0x26 short difficulty cursor: 0 Beginner … 4 Maniac, starts on 2 (message = cursor + 5)
/// </summary>
internal class SexSelectMenu
{
    private IHook<UpdateDelegate>? _hook;
    private int _lastState = -1;
    private short _lastCursor = -1;

    private static readonly string[] Genders = { "gender_male", "gender_female" };
    private static readonly string[] Difficulties =
        { "diff_beginner", "diff_easy", "diff_normal", "diff_hard", "diff_maniac" };

    internal SexSelectMenu(IReloadedHooks hooks)
    {
        SigScan("40 53 56 41 56 48 81 EC 90 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 88 00 00 00 48 8B D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 5B 48 33 F6 F6 03 01",
            "SexSelect::Update", address =>
            {
                _hook = hooks.CreateHook<UpdateDelegate>(Update, address).Activate();
            });
    }

    private nint Update(nint task)
    {
        var res = _hook!.OriginalFunction(task);
        try { Read(task); }
        catch (Exception e) { LogError("SexSelectMenu read failed", e); }
        return res;
    }

    private void Read(nint task)
    {
        nint work = ReadPtr(task + 0x48);
        if (work == 0 || !TryRead(work + 0x20, out int state)) return;

        short cursor;
        string[] names;
        if (state == 5 && TryRead(work + 0x24, out cursor)) names = Genders;
        else if (state == 9 && TryRead(work + 0x26, out cursor)) names = Difficulties;
        else { _lastState = state; _lastCursor = -1; return; }

        bool entered = state != _lastState;
        _lastState = state;
        if (!entered && cursor == _lastCursor) return;
        _lastCursor = cursor;
        if (cursor < 0 || cursor >= names.Length) return;

        string text = Loc.T(names[cursor]);
        if (names == Difficulties)
            text += ", " + string.Format(Loc.T("position"), cursor + 1, names.Length);
        // On entering the choice, queue it after the question being read; moving interrupts.
        Speech.Say(text, !entered);
    }

    private delegate nint UpdateDelegate(nint task);
}
