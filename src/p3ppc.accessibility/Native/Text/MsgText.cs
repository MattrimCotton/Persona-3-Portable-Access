using System.Text;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Native.Text;

/// <summary>
/// Reads the game's laid-out message text (a TextStruct: linked list of lines, each a linked
/// list of glyphs). Layout is the same as P4G (Native/Text/Text.cs of P4G Access), checked in
/// P3P.exe on 28/09/2026: the text draw function at 0x140231380 walks [text+0x40] then the
/// lines' +0x38 links. Unlike the P4G version, every read is guarded (no raw pointer walks),
/// and the walks are bounded so a corrupt list can never hang the game thread.
///   TextStruct : +0x40 first line
///   TextLine   : +0x08 Y position (int), +0x20 first glyph, +0x38 next line
///   Glyph      : +0x00 2 bytes (low byte first), +0x38 next glyph
/// </summary>
internal static class MsgText
{
    private const int MaxLines = 64;
    private const int MaxGlyphsPerLine = 512;

    /// <summary>The whole text, lines joined with spaces. Empty if unreadable.</summary>
    internal static string Read(nint textStruct)
    {
        var sb = new StringBuilder();
        int n = 0;
        for (nint line = ReadPtr(textStruct + 0x40); line != 0 && n < MaxLines; line = ReadPtr(line + 0x38), n++)
        {
            AppendLine(sb, line);
            sb.Append(' ');
        }
        return Clean(sb.ToString());
    }

    /// <summary>The text of one option of a choice list: options are the groups of lines that
    /// share a Y position (same rule as P4G's GetSelection).</summary>
    internal static string ReadOption(nint textStruct, int option)
    {
        nint first = ReadPtr(textStruct + 0x40);
        if (first == 0 || !TryRead(first + 8, out int lastY)) return "";
        var sb = new StringBuilder();
        int cur = 0, n = 0;
        for (nint line = first; line != 0 && n < MaxLines; line = ReadPtr(line + 0x38), n++)
        {
            if (!TryRead(line + 8, out int y)) break;
            if (y != lastY) { lastY = y; cur++; }
            if (cur == option) AppendLine(sb, line);
            else if (cur > option) break;
        }
        return Clean(sb.ToString());
    }

    private static void AppendLine(StringBuilder sb, nint line)
    {
        var enc = AtlusEncoding.Current;
        var pair = new byte[2];
        int n = 0;
        for (nint g = ReadPtr(line + 0x20); g != 0 && n < MaxGlyphsPerLine; g = ReadPtr(g + 0x38), n++)
        {
            if (!TryRead(g, out ushort code)) break;
            byte lo = (byte)code, hi = (byte)(code >> 8);
            if (hi == 0 && lo == 0) continue;
            string s;
            if (hi == 0)
            {
                pair[0] = lo;
                s = enc.GetString(pair, 0, 1);
            }
            else
            {
                pair[0] = hi;
                pair[1] = lo;
                s = enc.GetString(pair, 0, 2);
            }
            foreach (char c in s)
                if (c != '\0') sb.Append(c);
        }
    }

    private static string Clean(string s)
    {
        s = s.Replace('　', ' ').Trim();
        while (s.Contains("  ")) s = s.Replace("  ", " ");
        return s;
    }
}
