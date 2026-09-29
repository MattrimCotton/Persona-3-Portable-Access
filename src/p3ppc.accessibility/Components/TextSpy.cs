using System.Runtime.InteropServices;
using p3ppc.accessibility.Native.Text;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Research tool (debug mode only), on the model of P4G Access's UiTextSpy: F9 logs for 20 s
/// every text the game draws with its shared text function (0x1402323B0, found 29/09/2026),
/// with colour and position, once per (colour, text, position). Menus mark the selected entry
/// with a colour: this shows which one, to write the readers. Log prefix [TextSpy].
///
/// DrawText(x, y, z, colour RGBA, byte, byte, text, int): floats in xmm0-xmm2, colour in r9,
/// text pointer = 7th argument.
/// </summary>
internal class TextSpy
{
    private const int VK_F9 = 0x78;
    private long _until;
    private bool _keyWas;
    private readonly HashSet<string> _seen = new();

    // The six variants of DrawText (one per text alignment), same arguments. Offsets from the
    // first one, checked against the start of the signature before hooking.
    private static readonly int[] VariantOffsets = { 0x0, 0x110, 0x2D0, 0x3E0, 0x520, 0x630 };
    private const string Prologue = "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 70 0F 29 74 24 60 48 8D 0D";
    private readonly List<IHook<DrawTextDelegate>> _hooks = new();

    internal TextSpy(IReloadedHooks hooks)
    {
        if (!Config.DebugEnabled) return; // research tool: no hooks in normal play
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
                    if (!ok) { Log($"[TextSpy] variant 0x{va:X} does not match, skipped"); continue; }
                    int index = _hooks.Count;
                    var hook = hooks.CreateHook<DrawTextDelegate>(
                        (x, y, z, c, a5, a6, text, a8) => DrawText(index, x, y, z, c, a5, a6, text, a8), va);
                    _hooks.Add(hook); // stored before activation: the game may call it at once
                    hook.Activate();
                }
                Log($"[TextSpy] {_hooks.Count} text draw functions hooked (F9 = 20 s capture)");
            });
        new Thread(Poll) { IsBackground = true, Name = "TextSpy" }.Start();
    }

    private nint DrawText(int variant, float x, float y, float z, uint colour, byte a5, byte a6, nint text, int a8)
    {
        if (_until != 0)
        {
            try { Capture(variant, x, y, colour, text); }
            catch (Exception e) { Log($"[TextSpy] {e.Message}"); }
        }
        return _hooks[variant].OriginalFunction(x, y, z, colour, a5, a6, text, a8);
    }

    private void Capture(int variant, float x, float y, uint colour, nint text)
    {
        if (Environment.TickCount64 > _until)
        {
            _until = 0;
            Log("[TextSpy] capture ended");
            return;
        }
        var s = GameStrings.ReadGameString(text, 200);
        if (string.IsNullOrEmpty(s)) return;
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
                _until = Environment.TickCount64 + 20000;
                Log("[TextSpy] capture started (20 s)");
                Speech.Say("TextSpy", true);
            }
            _keyWas = down;
        }
    }

    private delegate nint DrawTextDelegate(float x, float y, float z, uint colour, byte a5, byte a6, nint text, int a8);

    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
}
