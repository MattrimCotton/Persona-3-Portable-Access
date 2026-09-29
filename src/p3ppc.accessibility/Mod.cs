using DavyKager;
using p3ppc.accessibility.Components;
using p3ppc.accessibility.Configuration;
using p3ppc.accessibility.Native.Text;
using p3ppc.accessibility.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility;

/// <summary>
/// P3P Access entry point. Structure copied from P4G Access (Mod.cs); components are added
/// here one by one as each system is ported (see PLAN.md).
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    private readonly IReloadedHooks? _hooks;
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private readonly IModLoader _modLoader;
    private readonly IMod _owner;
    private Config _configuration;

    // Components are kept in fields so their hook delegates stay rooted.
    private TitleBar? _titleBar;
    private Dialogue? _dialogue;
    private SexSelectMenu? _sexSelect;
    private TitleMenu? _titleMenu;
    private HistoryKeys? _historyKeys;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        if (!Initialise(_logger, _configuration, _modLoader))
            return;

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Log($"[CRASH] UnhandledException: {ex?.GetType().Name}: {ex?.Message}");
            if (ex?.StackTrace != null) Log($"[CRASH] {ex.StackTrace}");
        };

        ModDir = _modLoader.GetDirectoryForModId(_modConfig.ModId);

        GameLanguage.Detect();
        Loc.Apply(_configuration.Language);
        Log($"[Language] game language \"{GameLanguage.SteamLanguage}\", text table {GameLanguage.ActiveTable}");
        AtlusEncoding.Initiailse(ModDir, GameLanguage.ActiveTable);

        // Add the mod's folder to the PATH so Tolk finds the screen reader DLLs.
        Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + ModDir,
            EnvironmentVariableTarget.Process);
        Tolk.Load();
        if (!Tolk.IsLoaded())
        {
            LogError("Tolk failed to load, the mod files may be corrupted");
            return;
        }
        Log($"Tolk loaded. HasSpeech={Tolk.HasSpeech()}, ScreenReader={Tolk.DetectScreenReader() ?? "none"}");
        Speech.Say(Loc.T("loaded"), true);

        _titleBar = new TitleBar(_hooks!);
        _dialogue = new Dialogue(_hooks!);
        _sexSelect = new SexSelectMenu(_hooks!);
        _titleMenu = new TitleMenu(_hooks!);
        _historyKeys = new HistoryKeys();
    }

    #region Standard Overrides

    public override void ConfigurationUpdated(Config configuration)
    {
        _configuration = configuration;
        UpdateConfig(configuration);
        Loc.Apply(configuration.Language);
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }

    #endregion

    #region For Exports, Serialization etc.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    public Mod() { }
#pragma warning restore CS8618

    #endregion
}
