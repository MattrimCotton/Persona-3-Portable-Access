using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Stops the game from rewriting its window title (it adds the frame rate every second or so),
/// which made NVDA re-announce the window name (test 3, 28/09/2026). Same fix as P4G Access
/// (TitleBar.cs): the update is guarded by a "jge skip"; forcing the flags to "equal" before it
/// always takes the skip. P3P's code is the same as P4G's (0x140353E2A on 28/09/2026).
/// </summary>
internal class TitleBar
{
    private IAsmHook? _titleBarUpdateHook;

    internal TitleBar(IReloadedHooks hooks)
    {
        SigScan("0F 8D ?? ?? ?? ?? 33 D2 0F 29 B4 24 ?? ?? ?? ?? 41 B8 00 01 00 00", "UpdateTitleBar", address =>
        {
            string[] function =
            {
                "use64",
                "cmp rax, rax" // equal → the following jge skips the title update
            };
            _titleBarUpdateHook = hooks.CreateAsmHook(function, address, AsmHookBehaviour.ExecuteFirst).Activate();
        });
    }
}
