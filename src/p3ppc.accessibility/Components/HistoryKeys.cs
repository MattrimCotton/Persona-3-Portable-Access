using System.Runtime.InteropServices;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Keyboard shortcuts for the speech history, copied from P4G Access. Shift is the modifier
/// so the base keys keep their normal meaning:
///   Shift+P — repeat the last spoken line
///   Shift+[ / Shift+] — step back / forward through history
///   Shift+M — toggle the dialogue reader
/// </summary>
internal class HistoryKeys
{
    private const int PollMs = 40;
    private const int VK_SHIFT = 0x10, VK_P = 0x50, VK_M = 0x4D, VK_OEM_4 = 0xDB, VK_OEM_6 = 0xDD;

    private bool _pWas, _lbWas, _rbWas, _mWas;

    public HistoryKeys()
    {
        new Thread(Poll) { IsBackground = true, Name = "HistoryKeys" }.Start();
        Log("[HistoryKeys] ready (Shift+P repeat, Shift+[ / Shift+] history, Shift+M dialogue toggle)");
    }

    private void Poll()
    {
        while (true)
        {
            Thread.Sleep(PollMs);
            try { Tick(); }
            catch (Exception ex) { Log($"[HistoryKeys] poll error: {ex.Message}"); }
        }
    }

    private void Tick()
    {
        if (!GameHasFocus()) return;
        bool shift = Down(VK_SHIFT);

        bool sp = shift && Down(VK_P);
        if (sp && !_pWas) Speech.RepeatLast();
        _pWas = sp;

        bool sl = shift && Down(VK_OEM_4);
        if (sl && !_lbWas) Speech.Step(-1);
        _lbWas = sl;

        bool sr = shift && Down(VK_OEM_6);
        if (sr && !_rbWas) Speech.Step(+1);
        _rbWas = sr;

        bool sm = shift && Down(VK_M);
        if (sm && !_mWas) Dialogue.ToggleReader();
        _mWas = sm;
    }

    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
    private static bool Down(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;
}
