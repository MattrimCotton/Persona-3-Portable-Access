using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Reads dialogue messages and choice lists aloud. Ported from P4G Access (Dialogue.cs); see
/// docs/DIALOGUE.md for how the P3P function was found.
///
/// P4G hooks MsgWindow::DrawDialog(dialogInfo), called once per message window. In P3P that
/// body is INLINED in a function that walks the list of open message windows and draws each one
/// (0x14023B510 on 28/09/2026). So we hook that function, let it draw, then walk the same list
/// ourselves: node +0x08 next, node +0x18 dialogInfo.
///
/// dialogInfo (P3P offsets, P4G in brackets):
///   +0x00 flags: bits 0-2 = message state (text drawn when >= 3), bit 17 = message hidden,
///         bits 3-5 = choice state (list drawn when >= 3), bit 18 = choice hidden
///   +0x20 speaker name text [0x20]   +0x38 message text [0x38]
///   +0x60 choice list text [0x70]    +0x6E selected option, short, -1 = none yet [0x7E]
/// </summary>
internal unsafe class Dialogue
{
    private IHook<DrawWindowsDelegate>? _hook;
    private nint _windowList;   // address of the global that holds the first node

    /// <summary>When false, dialogue text is recorded to history but not spoken (Shift+M).
    /// Choice lists are always spoken.</summary>
    internal static bool ReaderEnabled = true;

    // Message: last window, text object and string spoken.
    private nint _lastInfo, _lastLines, _lastGlyph;
    private short _lastPage = -1;
    private int _framesSinceCheck;
    private string _lastText = "";
    private string _lastSpeaker = "";

    // Choice list: the window that owns it and the last option spoken.
    private nint _selectionInfo;
    private short _lastSelected = -1;

    internal Dialogue(IReloadedHooks hooks)
    {
        SigScan("40 55 48 83 EC 30 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 8B E8 48 85 C0 0F 84",
            "MsgWindow::DrawAll", address =>
            {
                // mov rax, [rip+disp32] at +0x12: displacement at +0x15
                _windowList = GetGlobalAddress(address + 0x15);
                Log($"Found message window list at 0x{_windowList:X}");
                _hook = hooks.CreateHook<DrawWindowsDelegate>(DrawWindows, address).Activate();
            });
    }

    internal static void ToggleReader()
    {
        ReaderEnabled = !ReaderEnabled;
        Speech.Say(Loc.T(ReaderEnabled ? "dialogue_on" : "dialogue_off"), true);
    }

    private nint DrawWindows(nint a1, nint a2)
    {
        var res = _hook!.OriginalFunction(a1, a2);
        try
        {
            if (Config.AnnounceDialogue) ReadWindows();
        }
        catch (Exception e)
        {
            LogError("Dialogue read failed", e);
        }
        return res;
    }

    private void ReadWindows()
    {
        bool lastSeen = false, selectionSeen = false;
        int n = 0;
        for (nint node = ReadPtr(_windowList); node != 0 && n < 32; node = ReadPtr(node + 8), n++)
        {
            nint info = ReadPtr(node + 0x18);
            if (info == 0 || !TryRead(info, out uint flags)) continue;

            bool msgShown = (flags & (1u << 17)) == 0 && (flags & 7) >= 3;
            nint text = ReadPtr(info + 0x38);
            if (msgShown && text != 0) SpeakMessage(info, text);

            bool choiceShown = (flags & (1u << 18)) == 0 && (flags & 0x38) >= 0x18;
            nint choices = ReadPtr(info + 0x60);
            if (choiceShown && choices != 0)
            {
                SpeakSelection(info, choices);
                if (info == _selectionInfo) selectionSeen = true;
            }

            // Checked AFTER reading: a window read for the first time this frame counts as seen,
            // otherwise the reset below forgets it and it is re-spoken every frame (bug of the
            // first test, 28/09/2026: NVDA's voice cut 60 times a second).
            if (info == _lastInfo) lastSeen = true;
        }

        // The window we last read closed: the next message is new even if its text is the same.
        if (!lastSeen) { _lastInfo = 0; _lastLines = 0; _lastGlyph = 0; _lastPage = -1; _lastText = ""; _lastSpeaker = ""; }
        if (!selectionSeen) { _selectionInfo = 0; _lastSelected = -1; }
    }

    private void SpeakMessage(nint info, nint textObj)
    {
        nint lines = ReadPtr(textObj + 0x40);
        if (lines == 0) return;
        TryRead(info + 0x46, out short page);

        // This runs every frame. The game REUSES the same line list for the next page (test 2,
        // 28/09/2026: pages 2 and 3 were skipped), so a same pointer doesn't mean same text.
        // Cheap check every frame (line list, page, first glyph); full text compare a few
        // times a second as a safety net.
        nint firstGlyph = ReadPtr(lines + 0x20);
        bool sameKey = info == _lastInfo && lines == _lastLines && page == _lastPage && firstGlyph == _lastGlyph;
        if (sameKey && ++_framesSinceCheck < 6) return;
        _framesSinceCheck = 0;

        string body = MsgText.Read(textObj);
        if (string.IsNullOrWhiteSpace(body)) return;
        _lastLines = lines;
        _lastPage = page;
        _lastGlyph = firstGlyph;
        if (info == _lastInfo && body == _lastText) return;

        string speaker = "";
        nint nameObj = ReadPtr(info + 0x20);
        if (nameObj != 0) speaker = MsgText.Read(nameObj);

        string line = body.StartsWith(">") ? body.Substring(1).Trim() : body;
        // Speaker name only when it changes, like P4G.
        if (!string.IsNullOrWhiteSpace(speaker) && (info != _lastInfo || speaker != _lastSpeaker))
            line = speaker + " : " + line;

        if (Config.DebugEnabled)
        {
            TryRead(info + 0x40, out ulong raw40);
            TryRead(info + 0x48, out ulong raw48);
            LogDebug($"[Dialogue] info 0x{info:X} text 0x{textObj:X} lines 0x{lines:X} +40={raw40:X16} +48={raw48:X16}");
        }

        if (ReaderEnabled) Speech.Say(line, true);
        else Speech.Record(line);

        _lastInfo = info;
        _lastText = body;
        _lastSpeaker = speaker;
    }

    private void SpeakSelection(nint info, nint choices)
    {
        if (!TryRead(info + 0x6E, out short selected) || selected < 0) return;

        bool newMenu = info != _selectionInfo;
        if (!newMenu && selected == _lastSelected) return;
        _selectionInfo = info;
        _lastSelected = selected;

        string option = MsgText.ReadOption(choices, selected);
        // The first option of a new list is queued so it doesn't cut the question;
        // moving the cursor interrupts.
        if (!string.IsNullOrWhiteSpace(option))
            Speech.Say(option, !newMenu);
    }

    private delegate nint DrawWindowsDelegate(nint a1, nint a2);
}
