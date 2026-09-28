using p3ppc.accessibility.Template.Configuration;
using System.ComponentModel;

namespace p3ppc.accessibility.Configuration;
public class Config : Configurable<Config>
{
    [DisplayName("Mod language / Langue du mod")]
    [Description("Language of the mod's own messages. Auto follows the game's Steam language.")]
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

public enum ModLanguage
{
    Auto,
    Francais,
    English,
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    //
}
