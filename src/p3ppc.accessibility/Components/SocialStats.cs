using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// The protagonist's social stats panel (shared\shdstatus.c), shown by the pause menu's Status
/// screen: speaks "Savoir : rang, Charme : rang, Courage : rang" when the panel appears or a
/// rank changes. See docs/PAUSE_MENU.md.
///
/// Found 29/09/2026 with Ghidra: DrawSocialStats(position, ?, alpha, panel) 0x1402C8860, called
/// every frame while shown. Panel +0x0A + i × 2 = level (1-6) of stat i (0 Academics, 1 Charm,
/// 2 Courage). Rank name = GetRankName(stat, level) 0x14016A3F0 (community\cmmmisc.c), the
/// game's own text; stat names = hardcoded texts 97-99.
/// </summary>
internal class SocialStats
{
    private IHook<DrawDelegate>? _hook;
    private GetRankDelegate? _getRank;
    private string _last = "";
    private long _lastDrawTick;

    internal SocialStats(IReloadedHooks hooks)
    {
        SigScan("4C 89 48 20 55 56 57 41 54 41 55 41 56 41 57 48 81 EC 30 01 00 00 0F 29 70 B8 4D 8B E9 0F 29 78 A8",
            "SocialStats::Draw", address =>
            {
                _hook = hooks.CreateHook<DrawDelegate>(Draw, address - 7).Activate();
            });
        SigScan("48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 20 0F B7 F9 0F B7 DA 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 35 ?? ?? ?? ?? 48 83 C6 08",
            "SocialStats::GetRankName", address =>
            {
                _getRank = hooks.CreateWrapper<GetRankDelegate>(address, out _);
            });
    }

    private void Draw(long position, long a2, byte alpha, nint panel)
    {
        _hook!.OriginalFunction(position, a2, alpha, panel);
        try { Read(panel); }
        catch (Exception e) { LogError("SocialStats read failed", e); }
    }

    private void Read(nint panel)
    {
        long now = Environment.TickCount64;
        bool opened = now - _lastDrawTick > 500;
        _lastDrawTick = now;

        var parts = new List<string>();
        for (short i = 0; i < 3; i++)
        {
            if (!TryRead(panel + 0x0A + i * 2, out short level) || level < 1 || level > 6) return;
            string stat = GameStrings.Get(97 + i) ?? Loc.F("social_stat_unknown", i + 1);
            string rank = _getRank != null ? GameStrings.ReadGameString(_getRank(i, level), 0x17) ?? "" : "";
            parts.Add(rank.Length > 0 ? Loc.F("social_stat_rank", stat, rank) : Loc.F("social_stat_level", stat, level));
        }
        string text = string.Join(", ", parts);
        if (!opened && text == _last) return;
        _last = text;
        LogDebug($"[SocialStats] {text}");
        Speech.Say(text, true);
    }

    private delegate void DrawDelegate(long position, long a2, byte alpha, nint panel);
    private delegate nint GetRankDelegate(short stat, short level);
}
