using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Pause menu Persona screen (camp\cmppersona.c): speaks the Persona under the cursor in the
/// protagonist's stock (name, level, position). See docs/PAUSE_MENU.md.
///
/// Found 29/09/2026 with Ghidra: DrawRow(work, row) 0x1401497E0 draws one stock row. Work:
/// +0x50 number of Persona, +0x54 cursor, +0x38 + i × 2 stock slot. The row reads its record with
/// GetStockPersona(slot) (0x1402649B0, jump into .arch): +0x02 Persona id, +0x04 level (byte).
/// Name: GetPersonaName(id) (battle\data\datpersona.c).
/// </summary>
internal class PersonaMenu
{
    private IHook<DrawRowDelegate>? _hook;
    private GetStockDelegate? _getStock;
    private int _lastKey = -1;
    private long _lastSeen;

    internal PersonaMenu(IReloadedHooks hooks)
    {
        SigScan("48 8B F9 48 63 F2 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 85 F6 78 0C E8 ?? ?? ?? ?? 0F B7 C0 3B F0",
            "Persona::DrawRow", address =>
            {
                _hook = hooks.CreateHook<DrawRowDelegate>(DrawRow, address - 0x2E).Activate();
            });
        SigScan("0F B7 4C 77 38 B8 02 00 00 00 66 89 44 24 70 E8",
            "GetStockPersona call", address =>
            {
                _getStock = hooks.CreateWrapper<GetStockDelegate>(GetGlobalAddress(address + 0x10), out _);
            });
    }

    private void DrawRow(nint work, int row)
    {
        _hook!.OriginalFunction(work, row);
        try { Read(work, row); }
        catch (Exception e) { LogError("PersonaMenu read failed", e); }
    }

    private void Read(nint work, int row)
    {
        if (_getStock == null) return;
        if (!TryRead(work + 0x54, out short cursor) || row != cursor) return;
        if (!TryRead(work + 0x50, out short count) || count <= 0 || count > 12 || row < 0 || row >= count) return;
        if (!TryRead(work + 0x38 + row * 2, out ushort slot)) return;
        nint record = _getStock(slot);
        if (record == 0 || !TryRead(record + 2, out ushort persona) || !TryRead(record + 4, out byte level)) return;

        long now = Environment.TickCount64;
        bool opened = now - _lastSeen > 500;
        _lastSeen = now;
        int key = (row << 16) | (persona << 4) ^ level;
        if (!opened && key == _lastKey) return;
        _lastKey = key;

        var parts = new List<string>();
        if (opened) parts.Add(Loc.T("persona_menu"));
        parts.Add(GameNames.Persona(persona) ?? Loc.F("persona_unknown", persona));
        parts.Add(Loc.F("persona_level", level));
        parts.Add(Loc.F("position", row + 1, count));
        LogDebug($"[PersonaMenu] row {row}/{count} slot {slot} persona {persona} level {level}");
        Speech.Say(string.Join(", ", parts), true);
    }

    private delegate void DrawRowDelegate(nint work, int row);
    private delegate nint GetStockDelegate(ushort slot);
}
