using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Name entry keyboard (signing the contract), European variant (nentry_figs.c: French, German,
/// Italian, Spanish). Speaks the key under the cursor and the name typed so far. See
/// docs/NAME_ENTRY.md.
///
/// Found 29/09/2026: the keyboard is a sub-task whose update (0x14028F210) gets the task in rcx;
/// task +0x48 = work struct (0x2218 bytes): +0x08 state (3 = closing), +0x20 column 0-19,
/// +0x24 row 0-5. The game's GetCell(row, col) (0x1402901C0) returns the key's value for the
/// game language (negative = empty cell). Keys are pictures: sprite value + 0x20 of
/// p3p_nameent01.spr (in dict/name.bin), read one by one to build <see cref="Keys"/>.
/// The two names being typed are game strings at 0x1409F37A8 and 0x1409F37C0 (8 characters,
/// 0x80 0x80 = empty slot), found in the outer name entry update (0x14028CC80).
///
/// Automatic signature (asked by the player, 29/09/2026: validating on the letter keyboard is
/// hard): write the names from the settings, then press Start for the game (bit 8 of the pad
/// trigger mask at 0x143624544, PSP button codes): the game's own code checks both names and
/// goes to its confirmation (work +0x18 mode: 1 typing, 3 confirming).
/// </summary>
internal class NameEntry
{
    private IHook<UpdateDelegate>? _hook;
    private GetCellDelegate? _getCell;
    private nint _name1, _name2;

    private nint _lastWork;
    private int _lastCol = -1, _lastRow = -1;
    private string? _lastName1, _lastName2;
    private bool _afterIntro;
    private nint _padTrigger;
    private nint _isFemc;
    private int _autoSignFrames;
    private int _autoSignWait;

    internal NameEntry(IReloadedHooks hooks)
    {
        SigScan("40 53 48 83 EC 20 48 8B D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 43 48 83 78 08 03 74 2B E8 ?? ?? ?? ?? 48 8D 0D 95 0E 00 00 48 8B D0 48 89 48 10 48 8D 0D D7 74 0C 03",
            "NameEntryFigs::KeyboardUpdate", address =>
            {
                _hook = hooks.CreateHook<UpdateDelegate>(Update, address).Activate();
            });
        SigScan("48 89 5C 24 08 57 48 83 EC 20 48 63 F9 48 8D 0D ?? ?? ?? ?? 48 63 DA E8 ?? ?? ?? ?? 44 8B 05 ?? ?? ?? ?? B9 FF FF FF FF 41 83 E8 05",
            "NameEntryFigs::GetCell", address =>
            {
                _getCell = hooks.CreateWrapper<GetCellDelegate>(address, out _);
            });
        SigScan("44 0F B6 0D ?? ?? ?? ?? 4C 8D 1D C7 6A 76 00 44 0F B7 15",
            "NameEntryFigs::NameBuffers", address =>
            {
                _name1 = GetGlobalAddress(address + 0xB);
                _name2 = GetGlobalAddress(address + 0x8C);
                Log($"[NameEntry] name buffers 0x{_name1:X} 0x{_name2:X}");
            });
        // Protagonist is the heroine: signature of p3ppc.visibleRankupReady (AnimatedSwine37);
        // the global is the operand of the instruction just before (movsxd rax, [rip+disp]).
        SigScan("48 8D 35 ?? ?? ?? ?? 0F 28 05 ?? ?? ?? ??", "IsFemc", address =>
            {
                _isFemc = GetGlobalAddress(address - 4);
                Log($"[NameEntry] IsFemc 0x{_isFemc:X}");
            });
        SigScan("F6 05 ?? ?? ?? ?? 08 74 28 45 85 F6 74 23 C7 47 18 03 00 00 00",
            "NameEntry::PadTrigger", address =>
            {
                // test byte ptr [rip+disp32], 8: the instruction ends 5 bytes after the displacement
                if (TryRead(address + 2, out int disp)) _padTrigger = address + 7 + disp;
                Log($"[NameEntry] pad trigger 0x{_padTrigger:X}");
            });
    }

    private nint Update(nint task)
    {
        var res = _hook!.OriginalFunction(task);
        try { Read(task); }
        catch (Exception e) { LogError("NameEntry read failed", e); }
        return res;
    }

    private void Read(nint task)
    {
        nint work = ReadPtr(task + 0x48);
        if (work == 0 || !TryRead(work + 0x08, out int state)) return;
        if (state == 3) { _lastWork = 0; return; } // closing

        if (work != _lastWork)
        {
            _lastWork = work;
            _lastCol = _lastRow = -1;
            _autoSignFrames = 0;
            _autoSignWait = 0;
            if (Config.AutoSignContract && TryAutoFill())
            {
                _autoSignFrames = 60;
                var (last, first) = ChosenNames();
                Speech.Say(Loc.F("name_auto", last.Trim(), first.Trim()), true);
            }
            else
            {
                Speech.Say(Loc.T("name_entry_intro"), true);
                _afterIntro = true;
            }
            _lastName1 = ReadName(_name1);
            _lastName2 = ReadName(_name2);
        }

        if (_autoSignFrames > 0) PressStart(work);

        AnnounceNames();

        if (!TryRead(work + 0x20, out int col) || !TryRead(work + 0x24, out int row)) return;
        if (col < 0 || col > 19 || row < 0 || row > 5) return;
        if (col == _lastCol && row == _lastRow) return;
        _lastCol = col;
        _lastRow = row;

        if (_getCell == null) return;
        short value = _getCell(row, col);
        var key = KeyName(value);
        LogDebug($"[NameEntry] row {row} col {col} value {value} -> {key ?? "?"}");
        if (key != null) Speech.Say(key, !_afterIntro); // the first key waits for the intro
        _afterIntro = false;
    }

    /// <summary>Names of the settings for the protagonist chosen (hero or heroine).</summary>
    private (string Last, string First) ChosenNames()
    {
        bool heroine = _isFemc != 0 && TryRead(_isFemc, out int f) && f != 0;
        return heroine
            ? (Config.HeroineLastName ?? "", Config.HeroineFirstName ?? "")
            : (Config.HeroLastName ?? "", Config.HeroFirstName ?? "");
    }

    /// <summary>Writes the names of the settings into both fields; false if one can't be written.</summary>
    private bool TryAutoFill()
    {
        var (last, firstName) = ChosenNames();
        var first = EncodeName(last);
        var second = EncodeName(firstName);
        if (first == null || second == null || _name1 == 0 || _name2 == 0)
        {
            Log("[NameEntry] automatic signature: a name is empty or cannot be written in the game's letters");
            return false;
        }
        bool ok = TryWriteBytes(_name1, first) && TryWriteBytes(_name2, second);
        Log($"[NameEntry] automatic signature: names written = {ok}");
        return ok;
    }

    /// <summary>
    /// Presses Start for the game until it goes to its confirmation (mode 3). Mode 0 is the
    /// opening animation, where Start is ignored (test 6, 29/09/2026): only the frames in mode
    /// 1 (typing) count. Gives up after about a second of mode 1, or 10 s in all, and tells the
    /// player to press it.
    /// </summary>
    private void PressStart(nint work)
    {
        if (!TryRead(work + 0x18, out int mode)) return;
        if (mode == 3)
        {
            Log("[NameEntry] automatic signature: confirmation reached");
            _autoSignFrames = 0;
            return;
        }
        if (++_autoSignWait > 600) _autoSignFrames = 1; // never reached mode 1: give up now
        if (mode != 1 && _autoSignFrames > 1) return;
        if (--_autoSignFrames == 0)
        {
            Log($"[NameEntry] automatic signature: Start not taken (mode {mode})");
            Speech.Say(Loc.T("name_auto_press_start"), true);
            return;
        }
        if (_padTrigger != 0 && TryRead(_padTrigger, out int pad))
        {
            pad |= 0x8; // PSP Start
            unsafe { TryWriteRaw(_padTrigger, &pad, 4); }
        }
    }

    /// <summary>A name as the field stores it: 8 two-byte characters, empty slots 80 80, then 0.</summary>
    private static byte[]? EncodeName(string? name)
    {
        if (AtlusEncoding.Current is not AtlusEncoding enc || string.IsNullOrWhiteSpace(name)) return null;
        var bytes = new List<byte>();
        foreach (var ch in name.Trim())
        {
            if (bytes.Count == 16) break;
            if (enc.TryGetWideBytes(ch.ToString(), out var hi, out var lo)) { bytes.Add(hi); bytes.Add(lo); }
            else Log($"[NameEntry] letter '{ch}' is not in the game's letters, skipped");
        }
        if (bytes.Count == 0) return null;
        while (bytes.Count < 16) { bytes.Add(0x80); bytes.Add(0x80); }
        bytes.Add(0);
        return bytes.ToArray();
    }

    /// <summary>Speaks a name when a letter is added or removed.</summary>
    private void AnnounceNames()
    {
        var n1 = ReadName(_name1);
        var n2 = ReadName(_name2);
        if (n1 != null && n1 != _lastName1)
            Speech.Say(n1.Length == 0 ? Loc.T("name_empty") : Loc.F("name_typed", n1), true);
        else if (n2 != null && n2 != _lastName2)
            Speech.Say(n2.Length == 0 ? Loc.T("name_empty") : Loc.F("name_typed", n2), true);
        _lastName1 = n1 ?? _lastName1;
        _lastName2 = n2 ?? _lastName2;
    }

    /// <summary>A name buffer as text, empty slots (0x80 0x80) removed; null if unreadable.</summary>
    private static string? ReadName(nint address)
    {
        if (address == 0) return null;
        var bytes = new List<byte>();
        for (int i = 0; i < 0x18; i++)
        {
            if (!TryRead(address + i, out byte b)) return null;
            if (b == 0) break;
            bytes.Add(b);
        }
        for (int i = 0; i + 1 < bytes.Count;)
        {
            if (bytes[i] == 0x80 && bytes[i + 1] == 0x80) bytes.RemoveRange(i, 2);
            else i++;
        }
        if (bytes.Count == 0) return "";
        return AtlusEncoding.Current is AtlusEncoding enc && enc.TryGetString(bytes.ToArray(), out var s)
            ? s.TrimEnd('\0').Trim()
            : null;
    }

    /// <summary>Spoken name of a key: capitals get a prefix, symbols become words.</summary>
    private static string? KeyName(short value)
    {
        if (value < 0 || value >= Keys.Length) return null;
        var ch = Keys[value];
        if (ch == null) return null;
        if (Symbols.TryGetValue(ch, out var symbolKey)) return Loc.T(symbolKey);
        if (char.IsLetter(ch[0]) && char.IsUpper(ch[0])) return Loc.F("key_capital", ch);
        return ch;
    }

    /// <summary>
    /// Key value → character, read from the sprites of p3p_nameent01.spr (data_FR, 29/09/2026).
    /// Values 74-85 and 131-181 are not keys of the grid.
    /// </summary>
    private static readonly string?[] Keys = BuildKeys();

    private static string?[] BuildKeys()
    {
        var k = new string?[216];
        for (int i = 0; i < 26; i++)
        {
            k[i] = ((char)('A' + i)).ToString();
            k[26 + i] = ((char)('a' + i)).ToString();
            k[97 + i] = ((char)('A' + i)).ToString();
        }
        for (int i = 0; i < 10; i++) k[55 + i] = ((char)('0' + i)).ToString();
        void Set(int start, string chars)
        {
            for (int i = 0; i < chars.Length; i++) k[start + i] = chars[i].ToString();
        }
        Set(52, "þÿš");
        Set(65, ".,()/”’:;");
        Set(86, "ÂÀÉÈ#$%&*々—");
        Set(123, "+−×÷=•?!");
        Set(182, "üýþÿšöøùúûñòóôõëìíîïåçèéêàáâãäÊÎŒÇ");
        return k;
    }

    private static readonly Dictionary<string, string> Symbols = new()
    {
        ["."] = "sym_period", [","] = "sym_comma", ["("] = "sym_paren_open", [")"] = "sym_paren_close",
        ["/"] = "sym_slash", ["”"] = "sym_quote", ["’"] = "sym_apostrophe", [":"] = "sym_colon",
        [";"] = "sym_semicolon", ["#"] = "sym_hash", ["$"] = "sym_dollar", ["%"] = "sym_percent",
        ["&"] = "sym_ampersand", ["*"] = "sym_asterisk", ["々"] = "sym_repeat", ["—"] = "sym_dash",
        ["+"] = "sym_plus", ["−"] = "sym_minus", ["×"] = "sym_times", ["÷"] = "sym_divide",
        ["="] = "sym_equals", ["•"] = "sym_dot", ["?"] = "sym_question", ["!"] = "sym_exclamation",
    };

    private delegate nint UpdateDelegate(nint task);
    private delegate short GetCellDelegate(int row, int col);
}
