using System.Text.Json;
using p3ppc.accessibility.Configuration;
using p3ppc.accessibility.Native.Text;

namespace p3ppc.accessibility;

/// <summary>
/// The mod's own messages, loaded from <c>Lang/&lt;steam language id&gt;.json</c> in the mod folder
/// (sources in the repo's <c>lang/</c> folder, see <c>lang/README.md</c> and docs/LOCALIZATION.md).
/// Language = the one chosen in the settings; Auto = the game's Steam language.
/// Lookup order: chosen language, then English, then the key itself, so a partial translation
/// still works. Every mod message goes through <see cref="T"/> or <see cref="F"/>.
/// </summary>
internal static class Loc
{
    internal const string Fallback = "english";

    private static Dictionary<string, string> _strings = new();
    private static Dictionary<string, string> _english = new();
    private static readonly HashSet<string> _reportedMissing = new();

    /// <summary>Steam language id of the active translation ("french", "english"…).</summary>
    internal static string Active { get; private set; } = Fallback;

    internal static void Apply(ModLanguage setting)
    {
        var wanted = setting == ModLanguage.Auto ? GameLanguage.SteamLanguage : SteamId(setting);
        var dir = Path.Combine(Utils.ModDir, "Lang");
        _english = Load(dir, Fallback) ?? new();
        var chosen = wanted == Fallback ? _english : Load(dir, wanted);
        if (chosen == null)
        {
            Utils.Log($"[Language] no translation \"{wanted}\" in {dir}, using English");
            wanted = Fallback;
            chosen = _english;
        }
        _strings = chosen;
        Active = wanted;
        _reportedMissing.Clear();
        Utils.Log($"[Language] mod messages: {Active} ({_strings.Count} strings)");
    }

    /// <summary>Message for <paramref name="key"/> in the active language.</summary>
    internal static string T(string key)
    {
        if (_strings.TryGetValue(key, out var s)) return s;
        if (_reportedMissing.Add(key)) Utils.Log($"[Language] missing \"{key}\" in {Active}");
        return _english.TryGetValue(key, out var e) ? e : key;
    }

    /// <summary>
    /// Formatted message ({0}, {1}… placeholders). A translation with broken placeholders falls
    /// back to English instead of throwing inside a game hook.
    /// </summary>
    internal static string F(string key, params object[] args)
    {
        try { return string.Format(T(key), args); }
        catch (FormatException)
        {
            if (_reportedMissing.Add("format:" + key)) Utils.Log($"[Language] bad placeholders in \"{key}\" ({Active})");
            try { return string.Format(_english.TryGetValue(key, out var e) ? e : key, args); }
            catch (FormatException) { return key; }
        }
    }

    internal static string SteamId(ModLanguage setting) => setting switch
    {
        ModLanguage.French => "french",
        ModLanguage.German => "german",
        ModLanguage.Italian => "italian",
        ModLanguage.Spanish => "spanish",
        ModLanguage.Japanese => "japanese",
        ModLanguage.Korean => "koreana",
        ModLanguage.ChineseSimplified => "schinese",
        ModLanguage.ChineseTraditional => "tchinese",
        _ => Fallback,
    };

    /// <summary>
    /// Reads a flat JSON object of string values. Keys starting with "_" (such as "_meta") are
    /// information for translators and are skipped. Comments and trailing commas are tolerated.
    /// </summary>
    private static Dictionary<string, string>? Load(string dir, string id)
    {
        var path = Path.Combine(dir, id + ".json");
        if (!File.Exists(path)) return null;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path),
                new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
            var result = new Dictionary<string, string>();
            foreach (var p in doc.RootElement.EnumerateObject())
                if (!p.Name.StartsWith('_') && p.Value.ValueKind == JsonValueKind.String)
                    result[p.Name] = p.Value.GetString()!;
            return result;
        }
        catch (Exception e)
        {
            Utils.Log($"[Language] cannot read {path}: {e.Message}");
            return null;
        }
    }
}
