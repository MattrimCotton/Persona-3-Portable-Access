using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Native.Text;

/// <summary>
/// The game's own short texts, in the game's language: the "hardcoded text" table (142 strings
/// per language: menu help lines, system menu entries, battle commands, times of day, week
/// days, arcana, social stats…). Using them avoids translating what the game already says.
///
/// GetHardcodedText(a, b) = table[language][a + b] (0x140257330, signature of
/// p3ppc.unhardcodedNames by AnimatedSwine37). French table dumped 29/09/2026: see
/// docs/GAME_STRINGS.md for the index of each text.
/// </summary>
internal static class GameStrings
{
    private static GetTextDelegate? _getText;

    internal static void Init(IReloadedHooks hooks)
    {
        SigScan("48 89 5C 24 ?? 57 48 83 EC 20 8B D9 8B FA 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 4C 63 05 ?? ?? ?? ??",
            "GetHardcodedText", address =>
            {
                _getText = hooks.CreateWrapper<GetTextDelegate>(address, out _);
            });
    }

    /// <summary>Hardcoded text number <paramref name="index"/>, decoded; null if unavailable.</summary>
    internal static string? Get(int index)
    {
        if (_getText == null || index < 0 || index >= 142) return null;
        nint p = _getText(index, 0);
        return ReadGameString(p);
    }

    /// <summary>A zero-terminated string of the game (Atlus encoding), decoded; null if unreadable.</summary>
    internal static string? ReadGameString(nint p, int max = 256)
    {
        if (p == 0) return null;
        var bytes = new List<byte>();
        for (int i = 0; i < max; i++)
        {
            if (!TryRead(p + i, out byte b)) return null;
            if (b == 0) break;
            bytes.Add(b);
        }
        if (AtlusEncoding.Current is not AtlusEncoding enc) return null;
        return enc.TryGetString(bytes.ToArray(), out var s) ? s.TrimEnd('\0').Trim() : null;
    }

    private delegate nint GetTextDelegate(int a, int b);
}
