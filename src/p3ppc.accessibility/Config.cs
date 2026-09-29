using p3ppc.accessibility.Template.Configuration;
using System.ComponentModel;

namespace p3ppc.accessibility.Configuration;
public class Config : Configurable<Config>
{
    [DisplayName("Mod language / Langue du mod")]
    [Description("Language of the mod's own messages (the game's text is read in the game's language). Auto follows the game's Steam language. A language without a translation file uses English: see lang/README.md to add one.")]
    [DefaultValue(ModLanguage.Auto)]
    public ModLanguage Language { get; set; } = ModLanguage.Auto;

    [DisplayName("Announce Dialogue")]
    [Description("Reads dialogue lines and choices aloud via screen reader.")]
    [DefaultValue(true)]
    public bool AnnounceDialogue { get; set; } = true;

    [DisplayName("Debug Mode")]
    [Description("Logs additional information to the console that is useful for debugging.")]
    [DefaultValue(false)]
    public bool DebugEnabled { get; set; } = false;
}

/// <summary>
/// Languages of the game. The numbers are stored in Config.json: never reorder, only append.
/// Each maps to a Steam language id and a translation file lang/&lt;id&gt;.json (Loc.SteamId).
/// </summary>
public enum ModLanguage
{
    Auto = 0,
    French = 1,
    English = 2,
    German = 3,
    Italian = 4,
    Spanish = 5,
    Japanese = 6,
    Korean = 7,
    ChineseSimplified = 8,
    ChineseTraditional = 9,
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    //
}
