using System.Runtime.InteropServices;
using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Hooks the game's shared text drawing (six variants of DrawText, one per alignment, found
/// 29/09/2026 at 0x1402320E0…0x140232710) and lets a reader collect the texts drawn while the
/// game draws something it knows (a save slot, a menu entry): <see cref="Begin"/> before calling
/// the game's draw, <see cref="End"/> after. That avoids decoding each screen's data: the game
/// has already turned it into text. Game thread only (the draw functions run there).
///
/// Research (debug mode only): F9 logs every drawn text for 20 s with colour and position
/// ([TextSpy] lines), like P4G Access's UiTextSpy.
///
/// DrawText(x, y, z, colour RGBA, byte, byte, text, int, …): floats in xmm0-xmm2, colour in r9,
/// text pointer = 7th argument. Up to 11 arguments: the 9th and 10th are optional OUTPUT pointers
/// (the first variant writes the text width to *arg9), so the hooks declare and forward all
/// 11 (forwarding stack slots a variant does not read is harmless; dropping them was not).
/// Also hooked: two more variants (+0x890, +0x9E0, same prologue). NOT hooked: the 5-byte jump at
/// +0xCE0 to the fit-to-width text drawing in .arch (used by long French names): the bytes after
/// it are protection code that a longer hook jump would overwrite. Readers use ids instead.
/// See docs/TEXT_CAPTURE.md.
/// </summary>
internal class TextCapture
{
    private const int VK_F9 = 0x78;
    private static readonly int[] VariantOffsets = { 0x0, 0x110, 0x2D0, 0x3E0, 0x520, 0x630, 0x890, 0x9E0 };
    private const string Prologue = "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 70 0F 29 74 24 60 48 8D 0D";

    private readonly List<IHook<DrawTextDelegate>> _hooks = new();

    // Capture for a reader (game thread only).
    private static List<string>? _capture;

    // Research log.
    private long _spyUntil;
    private bool _keyWas;
    private readonly HashSet<string> _seen = new();

    internal TextCapture(IReloadedHooks hooks)
    {
        SigScan("48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 70 0F 29 74 24 60 48 8D 0D ?? ?? ?? ?? 0F 29 7C 24 50 41 8B ?? 44 0F 29 44 24 40 0F 28 F2 44 0F 28 C0 0F 28 F9 E8 ?? ?? ?? ?? 0F B7 05 ?? ?? ?? ?? 41 B9 FF FF FF FF 0F B6 BC 24 A0 00 00 00",
            "DrawText (first variant)", address =>
            {
                var expected = Prologue.Split(' ').Select(h => Convert.ToByte(h, 16)).ToArray();
                foreach (var off in VariantOffsets)
                {
                    nint va = address + off;
                    bool ok = true;
                    for (int i = 0; i < expected.Length && ok; i++)
                        ok = TryRead(va + i, out byte b) && b == expected[i];
                    if (!ok) { Log($"[TextCapture] variant 0x{va:X} does not match, skipped"); continue; }
                    AddHook(hooks, va);
                }

                Log($"[TextCapture] {_hooks.Count} text draw functions hooked");
            });
        new Thread(Poll) { IsBackground = true, Name = "TextCapture" }.Start();
    }

    private void AddHook(IReloadedHooks hooks, nint va)
    {
        int index = _hooks.Count;
        var hook = hooks.CreateHook<DrawTextDelegate>(
            (x, y, z, c, a5, a6, text, a8, a9, a10, a11) => DrawText(index, x, y, z, c, a5, a6, text, a8, a9, a10, a11), va);
        _hooks.Add(hook); // stored before activation: the game may call it at once
        hook.Activate();
    }

    /// <summary>Starts collecting the texts drawn from now on (game thread).</summary>
    internal static void Begin() => _capture = new List<string>();

    /// <summary>Stops collecting and returns the texts drawn since <see cref="Begin"/>, in order.</summary>
    internal static List<string> End()
    {
        var list = _capture ?? new List<string>();
        _capture = null;
        return list;
    }

    private nint DrawText(int variant, float x, float y, float z, uint colour, byte a5, byte a6, nint text, long a8, nint a9, nint a10, nint a11)
    {
        if (_capture != null || _spyUntil != 0)
        {
            try
            {
                var s = GameStrings.ReadGameString(text, 200);
                if (!string.IsNullOrWhiteSpace(s))
                {
                    _capture?.Add(s);
                    if (_spyUntil != 0) Spy(variant, x, y, colour, s);
                }
            }
            catch (Exception e) { Log($"[TextCapture] {e.Message}"); }
        }
        return _hooks[variant].OriginalFunction(x, y, z, colour, a5, a6, text, a8, a9, a10, a11);
    }

    private void Spy(int variant, float x, float y, uint colour, string s)
    {
        if (Environment.TickCount64 > _spyUntil)
        {
            _spyUntil = 0;
            Log("[TextSpy] capture ended");
            return;
        }
        string key = $"{colour:X8}|{(int)x},{(int)y}|{s}";
        if (_seen.Add(key)) Log($"[TextSpy] v{variant} colour {colour:X8} at ({x:0},{y:0}): {s}");
    }

    private void Poll()
    {
        while (true)
        {
            Thread.Sleep(80);
            bool down = (GetAsyncKeyState(VK_F9) & 0x8000) != 0;
            if (down && !_keyWas && Config.DebugEnabled && GameHasFocus())
            {
                _seen.Clear();
                _spyUntil = Environment.TickCount64 + 20000;
                Log("[TextSpy] capture started (20 s)");
                Speech.Say("TextSpy", true);
            }
            _keyWas = down;
        }
    }

    private delegate nint DrawTextDelegate(float x, float y, float z, uint colour, byte a5, byte a6, nint text, long a8, nint a9, nint a10, nint a11);

    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
}
