using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Item lists drawn with the shared item row (shared\item.c): the pause menu's Item screen.
/// Speaks the selected item's name and quantity when the selection changes. See docs/PAUSE_MENU.md.
///
/// Found 29/09/2026 with Ghidra: DrawItemRow(position, int, alpha, row) 0x1402AD4F0 draws one
/// visible row. Row (shorts): +0x00 item id, +0x02 quantity (-1 = none), +0x16 state (1 = the
/// row under the cursor). The name comes from GetItemName(id) (call found by AnimatedSwine37's
/// p3ppc.unhardcodedNames; the function itself is in the encrypted .arch section, callable).
/// </summary>
internal class ItemRows
{
    private IHook<DrawRowDelegate>? _hook;
    private int _lastKey = -1;
    private long _lastSeen;

    internal ItemRows(IReloadedHooks hooks)
    {
        SigScan("49 89 73 C8 49 8B F9 4D 89 73 C0 45 0F B6 F8 45 0F 29 93 68 FF FF FF 0F 28 F9 48 89 4C 24 78",
            "Item::DrawRow", address =>
            {
                _hook = hooks.CreateHook<DrawRowDelegate>(DrawRow, address - 0x3F).Activate();
            });
    }

    private void DrawRow(long position, int a2, byte alpha, nint row)
    {
        _hook!.OriginalFunction(position, a2, alpha, row);
        try { Read(row); }
        catch (Exception e) { LogError("ItemRows read failed", e); }
    }

    private void Read(nint row)
    {
        if (!TryRead(row + 0x16, out short state) || state != 1) return;
        if (!TryRead(row, out short id) || !TryRead(row + 2, out short count)) return;
        if (id <= 0 || id >= 0x1000) return;

        long now = Environment.TickCount64;
        bool reopened = now - _lastSeen > 500; // no selected row drawn for half a second
        _lastSeen = now;
        int key = (id << 16) | (ushort)count;
        if (!reopened && key == _lastKey) return;
        _lastKey = key;

        string name = GameNames.Item(id) ?? "";
        if (name.Length == 0) name = Loc.F("item_unknown", id);
        string text = count >= 0 ? Loc.F("item_with_count", name, count) : name;
        LogDebug($"[ItemRows] item {id} count {count}");
        Speech.Say(text, true);
    }

    private delegate void DrawRowDelegate(long position, int a2, byte alpha, nint row);
}
