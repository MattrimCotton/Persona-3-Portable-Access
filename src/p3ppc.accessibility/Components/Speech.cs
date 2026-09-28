using DavyKager;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Central speech output + history, copied from P4G Access (Speech.cs). Every mod announcement
/// goes through <see cref="Say"/> so the last lines are kept for repeat / scroll-back
/// (<see cref="HistoryKeys"/>). <see cref="Record"/> stores a line without speaking it.
/// Not yet ported from P4G: the battle-only anti-spam window and the "protected line" grace
/// rule (they only matter once battle and navigation exist).
/// </summary>
internal static class Speech
{
    private const int Max = 20;
    private static readonly List<string> _hist = new();
    private static int _navIdx = -1;
    private static readonly object _lock = new();

    // The game separates words with the ideographic space (U+3000) in some strings; some
    // screen readers stumble on it, so every line is normalised to a normal space.
    private static string Normalize(string s)
        => s.IndexOf('　') >= 0 ? s.Replace('　', ' ') : s;

    /// <summary>Speak a line and record it to history.</summary>
    internal static void Say(string text, bool interrupt = true,
        [System.Runtime.CompilerServices.CallerFilePath] string callerFile = "")
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        text = Normalize(text);
        // Log every spoken line with the component that spoke it, so a player's log
        // identifies the source of any unwanted speech.
        int cut = callerFile.LastIndexOfAny(new[] { '\\', '/' });
        string src = cut >= 0 ? callerFile.Substring(cut + 1) : callerFile;
        if (src.EndsWith(".cs")) src = src.Substring(0, src.Length - 3);
        Utils.Log($"[Speech] {src}: {text}{(interrupt ? "" : "  (queued)")}");
        Record(text);
        Tolk.Output(text, interrupt);
    }

    /// <summary>Record a line to history without speaking it.</summary>
    internal static void Record(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        text = Normalize(text);
        lock (_lock)
        {
            if (_hist.Count > 0 && _hist[^1] == text) { _navIdx = _hist.Count; return; }
            _hist.Add(text);
            if (_hist.Count > Max) _hist.RemoveAt(0);
            _navIdx = _hist.Count;
        }
    }

    /// <summary>Re-speak the most recent line.</summary>
    internal static void RepeatLast()
    {
        lock (_lock)
        {
            if (_hist.Count == 0) { Tolk.Output(Loc.T("no_history"), true); return; }
            _navIdx = _hist.Count;
            Tolk.Output(_hist[^1], true);
        }
    }

    /// <summary>Browse history. dir = -1 older, +1 newer.</summary>
    internal static void Step(int dir)
    {
        lock (_lock)
        {
            if (_hist.Count == 0) { Tolk.Output(Loc.T("no_history"), true); return; }
            if (_navIdx < 0 || _navIdx > _hist.Count - 1) _navIdx = _hist.Count;
            int next = _navIdx + dir;
            if (next < 0) { _navIdx = 0; Tolk.Output(Loc.T("history_start") + _hist[0], true); return; }
            if (next > _hist.Count - 1) { _navIdx = _hist.Count - 1; Tolk.Output(Loc.T("history_newest") + _hist[^1], true); return; }
            _navIdx = next;
            Tolk.Output(_hist[_navIdx], true);
        }
    }
}
