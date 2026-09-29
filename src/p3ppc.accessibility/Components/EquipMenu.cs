using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Pause menu Equip screen (camp\cmpequip.c): speaks the party member being equipped, then the
/// slot under the cursor with the item equipped there. See docs/PAUSE_MENU.md.
///
/// Found 29/09/2026 with Ghidra: Draw(work) 0x1401292F0, every frame. Work: +0x3C selected
/// member (index), +0x52 + i × 2 member character ids, +0x28 flags (0x20 = choosing a slot),
/// +0x3E slot 0-3. Equipped item = GetEquipped(character, slot) (called by the same draw).
/// The list of equipment to choose from is a separate panel (not read yet).
/// </summary>
internal class EquipMenu
{
    private IHook<DrawDelegate>? _hook;
    private int _lastMember = -1, _lastSlot = -1, _lastItem = -1;
    private long _lastDrawTick;

    private static readonly string[] SlotKeys = { "equip_weapon", "equip_armor", "equip_feet", "equip_accessory" };

    internal EquipMenu(IReloadedHooks hooks)
    {
        SigScan("4C 8B E9 41 0F 29 73 C8 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 49 0F BF 45 3C 45 33 E4 66 44 89 65 80",
            "Equip::Draw", address =>
            {
                _hook = hooks.CreateHook<DrawDelegate>(Draw, address - 0x64).Activate();
            });
    }

    private void Draw(nint work)
    {
        _hook!.OriginalFunction(work);
        try { Read(work); }
        catch (Exception e) { LogError("EquipMenu read failed", e); }
    }

    private void Read(nint work)
    {
        if (!TryRead(work + 0x3C, out short member) || member < 0 || member > 7) return;
        if (!TryRead(work + 0x52 + member * 2, out short character) || character <= 0 || character > 16) return;
        if (!TryRead(work + 0x28, out uint flags) || !TryRead(work + 0x3E, out short slot)) return;
        bool choosingSlot = (flags & 0x20) != 0 && slot >= 0 && slot < SlotKeys.Length;

        long now = Environment.TickCount64;
        bool opened = now - _lastDrawTick > 500;
        _lastDrawTick = now;

        int item = choosingSlot ? GameNames.Equipped(character, slot) ?? -1 : -1;
        int shownSlot = choosingSlot ? slot : -1;
        if (!opened && member == _lastMember && shownSlot == _lastSlot && item == _lastItem) return;
        bool memberChanged = opened || member != _lastMember;
        _lastMember = member;
        _lastSlot = shownSlot;
        _lastItem = item;

        var parts = new List<string>();
        if (opened) parts.Add(Loc.T("equip_menu"));
        if (memberChanged || !choosingSlot)
            parts.Add(GameNames.Character(character) ?? Loc.F("character_unknown", character));
        if (choosingSlot)
        {
            string what = item > 0 ? GameNames.Item(item) ?? Loc.F("item_unknown", item) : Loc.T("equip_none");
            parts.Add(Loc.T(SlotKeys[slot]) + ": " + what);
            parts.Add(Loc.F("position", slot + 1, SlotKeys.Length));
        }
        LogDebug($"[EquipMenu] member {member} character {character} slot {shownSlot} item {item}");
        Speech.Say(string.Join(". ", parts), true);
    }

    private delegate void DrawDelegate(nint work);
}
