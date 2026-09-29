using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Save / load screen: speaks the selected save slot (its number, then the texts the game draws
/// in it: date, time of day, level, play time, place… or "AUCUNE DONNÉE"). See docs/SAVE_LOAD.md.
///
/// Found 29/09/2026: DrawSlot(screen, position, z, alpha, slot index, highlighted) (0x140270770)
/// draws one slot; the 6th argument is 1 for the slot under the cursor (it then draws the
/// highlight). Its texts are collected with <see cref="TextCapture"/> while the game draws it.
/// Slot data: screen + index * 0xB0 + 0x51C2 = slot state (0, 1 "no data", 2 saved game).
/// </summary>
internal class SaveSlots
{
    private IHook<DrawSlotDelegate>? _hook;
    private int _lastIndex = -1;
    private string _lastText = "";
    private long _lastSeen;

    internal SaveSlots(IReloadedHooks hooks)
    {
        SigScan("48 63 9D A0 02 00 00 4C 8B F1 48 89 4C 24 60 44 0F 28 C2 48 8D 0D ?? ?? ?? ?? 45 0F B6 E9",
            "SaveLoad::DrawSlot", address =>
            {
                _hook = hooks.CreateHook<DrawSlotDelegate>(DrawSlot, address - 0x39).Activate();
            });
    }

    private nint DrawSlot(nint screen, nint position, float z, byte alpha, int index, int highlighted, nint a7, nint a8)
    {
        if (highlighted != 1) return _hook!.OriginalFunction(screen, position, z, alpha, index, highlighted, a7, a8);

        TextCapture.Begin();
        nint res;
        try { res = _hook!.OriginalFunction(screen, position, z, alpha, index, highlighted, a7, a8); }
        finally
        {
            var texts = TextCapture.End();
            try { Announce(index, texts); }
            catch (Exception e) { LogError("SaveSlots read failed", e); }
        }
        return res;
    }

    private void Announce(int index, List<string> texts)
    {
        long now = Environment.TickCount64;
        bool reopened = now - _lastSeen > 500; // screen (re)opened
        _lastSeen = now;
        string body = string.Join(", ", texts);
        if (!reopened && index == _lastIndex && body == _lastText) return;
        _lastIndex = index;
        _lastText = body;
        string text = Loc.F("save_slot", index + 1) + (body.Length > 0 ? ". " + body : "");
        LogDebug($"[SaveSlots] slot {index}: {body}");
        Speech.Say(text, true);
    }

    private delegate nint DrawSlotDelegate(nint screen, nint position, float z, byte alpha, int index, int highlighted, nint a7, nint a8);
}
