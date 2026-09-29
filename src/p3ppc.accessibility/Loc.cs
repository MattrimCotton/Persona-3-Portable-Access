using p3ppc.accessibility.Configuration;
using p3ppc.accessibility.Native.Text;

namespace p3ppc.accessibility;

/// <summary>
/// The mod's own messages, in the language chosen in the settings (Auto = the game's Steam
/// language: French for "french", English otherwise). Every mod message goes through
/// <see cref="T"/> so a new language only needs a new column here.
/// </summary>
internal static class Loc
{
    private static bool _french;

    internal static void Apply(ModLanguage setting)
        => _french = setting switch
        {
            ModLanguage.Francais => true,
            ModLanguage.English => false,
            _ => GameLanguage.SteamLanguage == "french",
        };

    private static readonly Dictionary<string, (string Fr, string En)> Strings = new()
    {
        ["loaded"] = ("P3P Access chargé.", "P3P Access loaded."),
        ["no_history"] = ("Aucun historique.", "No history."),
        ["history_start"] = ("Début de l'historique. ", "Start of history. "),
        ["history_newest"] = ("Plus récent. ", "Newest. "),
        ["dialogue_on"] = ("Lecture des dialogues activée.", "Dialogue reader on."),
        ["dialogue_off"] = ("Lecture des dialogues désactivée.", "Dialogue reader off."),
        ["position"] = ("{0} sur {1}", "{0} of {1}"),
        ["press_any_key"] = ("Appuyez sur une touche.", "Press any key."),
        ["title_new_game"] = ("Nouvelle partie", "New game"),
        ["title_load"] = ("Charger", "Load game"),
        ["title_continue"] = ("Continuer", "Continue"),
        ["title_config"] = ("Config", "Config"),
        ["title_quit"] = ("Quitter", "Quit"),
        ["gender_male"] = ("Personnage masculin", "Male protagonist"),
        ["gender_female"] = ("Personnage féminin", "Female protagonist"),
        ["diff_beginner"] = ("Débutant", "Beginner"),
        ["diff_easy"] = ("Facile", "Easy"),
        ["diff_normal"] = ("Normale", "Normal"),
        ["diff_hard"] = ("Difficile", "Hard"),
        ["diff_maniac"] = ("Maniaque", "Maniac"),
    };

    internal static string T(string key)
        => Strings.TryGetValue(key, out var s) ? (_french ? s.Fr : s.En) : key;
}
