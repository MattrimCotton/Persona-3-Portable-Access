using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace p3ppc.accessibility.Native.Text;

/// <summary>
/// The game's language, adapted from P4G Access. P3P uses one glyph numbering per script
/// family (the CJK families reuse numbers for different characters), so the decoder loads the
/// matching Atlus Script Tools charset (P3P_*.tsv, bundled flat in the mod folder).
/// The language is Steam's per-app choice for P3P (app id 1809700).
/// </summary>
internal static class GameLanguage
{
    private const string AppManifest = "appmanifest_1809700.acf";

    /// <summary>Steam language id of the game ("french", "english"…), detected once at startup.</summary>
    internal static string SteamLanguage { get; private set; } = "english";

    internal static string ActiveTable { get; private set; } = "P3P_EFIGS.tsv";

    internal static void Detect()
    {
        SteamLanguage = DetectSteamLanguage();
        ActiveTable = SteamLanguage switch
        {
            "japanese" => "P3P_JP.tsv",
            "schinese" => "P3P_CHS.tsv",
            "tchinese" => "P3P_CHT.tsv",
            "koreana" or "korean" => "P3P_Korean.tsv",
            _ => "P3P_EFIGS.tsv",
        };
    }

    private static string DetectSteamLanguage()
    {
        try
        {
            // <library>/steamapps/common/P3P/P3P.exe → <library>/steamapps/appmanifest_1809700.acf
            var gameDir = Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory;
            var acf = Path.GetFullPath(Path.Combine(gameDir, "..", "..", AppManifest));
            if (File.Exists(acf))
            {
                var lang = ParseAcfLanguage(File.ReadAllText(acf));
                if (lang != null) return lang;
            }
        }
        catch (Exception e) { Utils.Log($"[Language] appmanifest read failed: {e.Message}"); }

        try
        {
            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "Language", null) is string s
                && !string.IsNullOrWhiteSpace(s))
                return s.Trim().ToLowerInvariant();
        }
        catch (Exception e) { Utils.Log($"[Language] registry read failed: {e.Message}"); }

        return "english";
    }

    private static readonly Regex UserConfigLang = new(
        "\"UserConfig\"\\s*\\{[^}]*?\"language\"\\s*\"([^\"]+)\"", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex AnyLang = new("\"language\"\\s*\"([^\"]+)\"", RegexOptions.Compiled);

    private static string? ParseAcfLanguage(string acfText)
    {
        var m = UserConfigLang.Match(acfText);
        if (!m.Success) m = AnyLang.Match(acfText);
        return m.Success ? m.Groups[1].Value.Trim().ToLowerInvariant() : null;
    }
}
