using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// The PC settings screen ("Config", from the title screen or the system menus), camp\cmpconfig.c.
/// Speaks the tab, the highlighted setting with its value and its help line, and the new value
/// when left / right changes it. See docs/CONFIG.md.
///
/// Found 29/09/2026 with Ghidra (docs/GHIDRA.md): DrawList(work) 0x140122070 draws the 7
/// visible rows every frame. Work: +0x3C tab (0 Audio … 5 Controller), +0x38 first visible row,
/// +0x34 cursor within the visible rows. Row r of tab t has label number first[t] + r.
/// Label / help / tab name getters (0x140126360, 0x140126320, 0x1401263E0) return the game's
/// own text in its language. DrawValue(work, entry, label, pos, alpha, selected) 0x140127460
/// draws the value text of a row (tabs 0-3); its texts are collected with TextCapture.
/// Keyboard and controller tabs draw their keys as icons: only the action name is spoken.
/// </summary>
internal class ConfigMenu
{
    private IHook<DrawListDelegate>? _drawHook;
    private IHook<DrawValueDelegate>? _valueHook;
    private GetTextDelegate? _getLabel, _getHelp, _getName;
    private nint _counts, _first; // int[6] each: rows per tab, first label number per tab

    private const int MaxLabels = 91;     // rows of all tabs (7 + 8 + 8 + 6 + 31 + 31)
    private int _wantLabel = -1;          // label whose value the current frame should capture
    private string? _frameValue;          // value captured this frame
    private int _lastTab = -1, _lastRow = -1;
    private string? _lastValue;
    private long _lastDrawTick;

    internal ConfigMenu(IReloadedHooks hooks)
    {
        SigScan("44 0F 29 B8 38 FF FF FF 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 08 4C 8B F9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 45 32 ED E8 ?? ?? ?? ?? 33 C9",
            "Config::DrawList", address =>
            {
                _drawHook = hooks.CreateHook<DrawListDelegate>(DrawList, address - 0x5D).Activate();
            });
        SigScan("4C 89 4D A0 4C 8B F1 F3 0F 10 7D A0 48 8D 0D ?? ?? ?? ?? F3 44 0F 10 45 A4 41 8B F8 48 8B DA E8",
            "Config::DrawValue", address =>
            {
                _valueHook = hooks.CreateHook<DrawValueDelegate>(DrawValue, address - 0x34).Activate();
            });
        SigScan("40 53 48 83 EC 20 48 63 D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 63 05 ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ?? 48 0F BE 8C 9A ?? ?? ?? ?? 48 8B 84 C2 C0 24 78 00",
            "Config::GetLabel", address =>
            {
                _getLabel = hooks.CreateWrapper<GetTextDelegate>(address, out _);
                // The name getter follows 0x80 bytes later (same prologue, checked).
                if (TryRead(address + 0x80, out uint p) && p == 0x83485340u)
                    _getName = hooks.CreateWrapper<GetTextDelegate>(address + 0x80, out _);
                // Tables next to the label tables: rows per tab and first label of each tab.
                if (TryRead(address + 0x30, out int disp))
                {
                    nint labels = BaseAddress + disp;   // table of label tables per language
                    _counts = labels - 0x1F8;
                    _first = labels - 0x1E0;
                    if (!TryRead(_counts, out int c0) || c0 < 1 || c0 > 40 || !TryRead(_first, out int f0) || f0 != 0)
                    {
                        Log("[ConfigMenu] row tables not recognised, positions will not be spoken");
                        _counts = _first = 0;
                    }
                }
            });
        SigScan("40 53 48 83 EC 20 48 63 D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 63 05 ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ?? 48 0F BE 8C 9A ?? ?? ?? ?? 48 8B 84 C2 10 25 78 00",
            "Config::GetHelp", address =>
            {
                _getHelp = hooks.CreateWrapper<GetTextDelegate>(address, out _);
            });
    }

    private void DrawList(nint work)
    {
        int tab = -1, row = -1;
        try
        {
            if (TryRead(work + 0x3C, out tab) && TryRead(work + 0x38, out int top) && TryRead(work + 0x34, out int cur)
                && tab >= 0 && tab < 6 && top >= 0 && cur >= 0 && cur < 7)
            {
                row = top + cur;
                // The getters index the game's tables without checks: only ask for a real row.
                bool known = Count(tab) is int n && row < n && top < n;
                _wantLabel = known && FirstLabel(tab) is int f && f >= 0 && f + row < MaxLabels ? f + row : -1;
            }
            else tab = -1;
        }
        catch (Exception e) { LogError("ConfigMenu read failed", e); tab = -1; }

        _frameValue = null;
        _drawHook!.OriginalFunction(work);
        int want = _wantLabel;
        _wantLabel = -1;
        if (tab < 0) return;
        try { Announce(tab, row, want, _frameValue); }
        catch (Exception e) { LogError("ConfigMenu announce failed", e); }
    }

    private void DrawValue(nint work, nint entry, int label, long pos, byte alpha, int selected)
    {
        if (label != _wantLabel || _wantLabel < 0)
        {
            _valueHook!.OriginalFunction(work, entry, label, pos, alpha, selected);
            return;
        }
        TextCapture.Begin();
        try { _valueHook!.OriginalFunction(work, entry, label, pos, alpha, selected); }
        finally
        {
            // The game measures the text before drawing it: drop repeated texts.
            var texts = TextCapture.End().Distinct().ToList();
            _frameValue = texts.Count > 0 ? string.Join(" ", texts) : null;
        }
    }

    private void Announce(int tab, int row, int label, string? value)
    {
        long now = Environment.TickCount64;
        bool opened = now - _lastDrawTick > 500; // not drawn for half a second: screen (re)opened
        _lastDrawTick = now;

        bool tabChanged = opened || tab != _lastTab;
        bool rowChanged = tabChanged || row != _lastRow;
        if (!rowChanged)
        {
            // Same row: speak only a new value (left / right).
            if (value != null && value != _lastValue)
            {
                _lastValue = value;
                LogDebug($"[ConfigMenu] value {value}");
                Speech.Say(value, true);
            }
            return;
        }
        _lastTab = tab;
        _lastRow = row;
        _lastValue = value;

        var parts = new List<string>();
        if (opened) parts.Add(Loc.T("config_menu"));
        if (tabChanged && Text(_getName, tab) is { Length: > 0 } tabName) parts.Add(Loc.F("config_tab", tabName));
        string name = label >= 0 ? Text(_getLabel, label) ?? "" : "";
        if (name.Length == 0) name = Loc.F("config_row", row + 1);
        parts.Add(value != null ? name + ": " + value : name);
        if (Count(tab) is int n && row < n) parts.Add(Loc.F("position", row + 1, n));
        if (label >= 0 && tab < 4 && Text(_getHelp, label) is { Length: > 0 } help && help != name) parts.Add(help);

        LogDebug($"[ConfigMenu] tab {tab} row {row} label {label} value {value}");
        Speech.Say(string.Join(". ", parts), true);
    }

    private int? FirstLabel(int tab) => _first != 0 && TryRead(_first + tab * 4, out int f) ? f : null;
    private int? Count(int tab) => _counts != 0 && TryRead(_counts + tab * 4, out int c) && c > 0 && c < 64 ? c : null;

    private static string? Text(GetTextDelegate? getter, int index)
    {
        if (getter == null) return null;
        return GameStrings.ReadGameString(getter(index), 200);
    }

    private delegate void DrawListDelegate(nint work);
    private delegate void DrawValueDelegate(nint work, nint entry, int label, long pos, byte alpha, int selected);
    private delegate nint GetTextDelegate(int index);
}
