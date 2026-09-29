using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Native.Text;

/// <summary>
/// The game's own name lookups (item, skill, character, equipped item), in the game's language.
/// Call them from the game thread (inside a hook). See docs/PAUSE_MENU.md and SIGNATURES.md.
///
/// GetItemName 0x14025A090 and GetEquipped 0x140259760 are jumps into the .arch section, where
/// GetSLinkName 0x14BC8ABE0 also lives (callable at run time); GetSkillName 0x1400A7F50 and
/// GetCharacterName 0x14025AD20 are in the main code.
/// </summary>
internal static class GameNames
{
    private static GetNameDelegate? _item, _skill, _character, _socialLink;
    private static GetEquippedDelegate? _equipped;

    internal static void Init(IReloadedHooks hooks)
    {
        // Call found by AnimatedSwine37 (p3ppc.unhardcodedNames): target of the E8 call.
        SigScan("E8 ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ?? 48 8B C8 0F B6 84", "GetItemName call",
            address => _item = hooks.CreateWrapper<GetNameDelegate>(GetGlobalAddress(address + 1), out _));
        SigScan("40 53 48 83 EC 20 0F B7 D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? B8 70 02 00 00 66 3B D8 72 14 8B 15 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? FF C2 E8 ?? ?? ?? ?? 8B 05 ?? ?? ?? ?? 83 F8 03",
            "GetSkillName", address => _skill = hooks.CreateWrapper<GetNameDelegate>(address, out _));
        SigScan("E8 ?? ?? ?? ?? F3 0F 10 0D ?? ?? ?? ?? 8B CF", "GetCharacterName call",
            address => _character = hooks.CreateWrapper<GetNameDelegate>(GetGlobalAddress(address + 1), out _));
        // GetSLinkName (AnimatedSwine37): social link name, in the .arch section (plain bytes, callable).
        SigScan("48 89 5C 24 ?? 57 48 83 EC 20 0F B7 D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 3D ?? ?? ?? ?? 3B 5F ?? 72 ?? 8B 15 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 83 C2 02 E8 ?? ?? ?? ?? C1 E3 06 48 83 C7 08 89 D8",
            "GetSLinkName", address => _socialLink = hooks.CreateWrapper<GetNameDelegate>(address, out _));
        // Equip screen: GetEquipped(character, slot) call.
        SigScan("41 0F B7 D4 41 0F B7 CF E8 ?? ?? ?? ?? B9 FF FF FF FF 66 89 45 A0 66 89 4D A2", "GetEquipped call",
            address => _equipped = hooks.CreateWrapper<GetEquippedDelegate>(GetGlobalAddress(address + 9), out _));
    }

    internal static string? Item(int id)
        => _item == null || id <= 0 || id >= 0x1000 ? null : GameStrings.ReadGameString(_item((short)id), 100);

    /// <summary>Skill names are fixed-width records (0x1D in European languages, 0x13 otherwise), zero-padded.</summary>
    internal static string? Skill(int id)
        => _skill == null || id <= 0 || id >= 0x270 ? null : GameStrings.ReadGameString(_skill((short)id), 0x1D);

    /// <summary>Character 1 is the protagonist (the name typed at the start).</summary>
    internal static string? Character(int id)
        => _character == null || id <= 0 || id > 16 ? null : GameStrings.ReadGameString(_character((short)id), 64);

    /// <summary>Name of the person of a social link (by social link id).</summary>
    internal static string? SocialLink(int id)
        => _socialLink == null || id <= 0 || id > 64 ? null : GameStrings.ReadGameString(_socialLink((short)id), 64);

    /// <summary>Item id equipped by a party member in a slot (0-3), or null.</summary>
    internal static int? Equipped(int character, int slot)
        => _equipped == null || character <= 0 || character > 16 || slot < 0 || slot > 3 ? null : _equipped((ushort)character, (ushort)slot);

    private delegate nint GetNameDelegate(short id);
    private delegate short GetEquippedDelegate(ushort character, ushort slot);
}
