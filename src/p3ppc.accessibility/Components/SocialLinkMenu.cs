using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Pause menu Social Link screen (camp\cmpcommu.c): speaks the link under the cursor (arcana,
/// person, rank) and its position. See docs/PAUSE_MENU.md.
///
/// Found 29/09/2026 with Ghidra: Draw(work) 0x14011AE80, every frame. Work: +0x18 flags (2 =
/// list shown), +0x2C cursor in the 5 visible rows, +0x2E first visible row, +0x194 number of
/// links; entry i at +0x44 + i × 0xC: +0x00 arcana + 1 (byte), +0x02 social link id, +0x04 rank.
/// Arcana name = hardcoded text 9 + arcana; person = GetSLinkName(id), both the game's texts.
/// </summary>
internal class SocialLinkMenu
{
    private IHook<DrawDelegate>? _hook;
    private int _lastKey = -1;
    private long _lastDrawTick;

    internal SocialLinkMenu(IReloadedHooks hooks)
    {
        SigScan("48 8B F1 45 0F 29 AB 58 FF FF FF 48 8D 0D ?? ?? ?? ?? 45 0F 29 B3 48 FF FF FF E8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 0F B6 06",
            "SocialLink::Draw", address =>
            {
                _hook = hooks.CreateHook<DrawDelegate>(Draw, address - 0x4B).Activate();
            });
    }

    private void Draw(nint work)
    {
        _hook!.OriginalFunction(work);
        try { Read(work); }
        catch (Exception e) { LogError("SocialLinkMenu read failed", e); }
    }

    private void Read(nint work)
    {
        if (!TryRead(work + 0x18, out uint flags) || (flags & 2) == 0) return;
        if (!TryRead(work + 0x2C, out short cursor) || !TryRead(work + 0x2E, out short top)
            || !TryRead(work + 0x194, out short count)) return;
        int index = top + cursor;
        if (cursor < 0 || cursor > 4 || top < 0 || count <= 0 || count > 28 || index >= count) return;

        nint entry = work + 0x44 + index * 0xC;
        if (!TryRead(entry, out byte arcana) || !TryRead(entry + 2, out short link) || !TryRead(entry + 4, out ushort rank)) return;

        long now = Environment.TickCount64;
        bool opened = now - _lastDrawTick > 500;
        _lastDrawTick = now;
        int key = (index << 20) | (link << 8) | (rank & 0xFF);
        if (!opened && key == _lastKey) return;
        _lastKey = key;

        var parts = new List<string>();
        if (opened) parts.Add(Loc.T("social_link_menu"));
        if (arcana >= 1 && arcana <= 22 && GameStrings.Get(9 + arcana - 1) is { Length: > 0 } arcanaName) parts.Add(arcanaName);
        if (GameNames.SocialLink(link) is { Length: > 0 } person) parts.Add(person);
        parts.Add(Loc.F("social_link_rank", rank));
        parts.Add(Loc.F("position", index + 1, count));
        LogDebug($"[SocialLinkMenu] index {index}/{count} arcana {arcana} link {link} rank {rank}");
        Speech.Say(string.Join(", ", parts), true);
    }

    private delegate void DrawDelegate(nint work);
}
