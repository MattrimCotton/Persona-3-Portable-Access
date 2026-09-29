using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using static p3ppc.accessibility.Utils;

namespace p3ppc.accessibility.Components;

/// <summary>
/// Controller support for the mod's own functions, on the model of P4G Access (ControllerInput.cs):
/// the two triggers held together (LT + RT, PlayStation L2 + R2) are the modifier, read through
/// XInput. See docs/CONTROLLER.md.
///   LT + RT + d-pad up          repeat the last line          (Shift+P)
///   LT + RT + d-pad left/right  speech history back / forward (Shift+[ / Shift+])
///   LT + RT + d-pad down        dialogue reader on / off      (Shift+M)
///
/// While the modifier is held, the game must not react to the same buttons: we hook the game's
/// pad update (0x1403A68B0, found 29/09/2026) and zero its button masks right after it runs
/// (pad block 0x143624540: +0x00..+0x0C and +0xE0..+0xFC are button masks, +0x10..+0x15 the
/// sticks, left alone). Nothing happens while the game window is not in front.
/// </summary>
internal class ControllerInput
{
    private const int PollMs = 33;
    private const byte TrigOn = 45, TrigOff = 20; // hysteresis
    private const ushort DPAD_UP = 0x0001, DPAD_DOWN = 0x0002, DPAD_LEFT = 0x0004, DPAD_RIGHT = 0x0008;

    private static readonly int[] ButtonMaskOffsets =
        { 0x00, 0x04, 0x08, 0x0C, 0xE0, 0xE4, 0xE8, 0xEC, 0xF0, 0xF4, 0xF8, 0xFC };

    /// <summary>True while LT + RT are both held: game buttons are suppressed.</summary>
    internal static volatile bool ChordHeld;

    private IHook<PadUpdateDelegate>? _padHook;
    private nint _padBlock;
    private int _slot = -1, _rescanWait;
    private bool _ltHeld, _rtHeld;
    private ushort _lastButtons;
    private bool _connectedLogged;

    internal ControllerInput(IReloadedHooks hooks)
    {
        SigScan("40 57 48 83 EC 30 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 3D ?? ?? ?? ?? 48 85 FF 0F 84 ?? ?? ?? ?? 48 89 6C 24 48 48 8D 2D",
            "Pad::Update", address =>
            {
                _padBlock = GetGlobalAddress(address + 0x2A); // lea rbp, [pad block]
                Log($"[Controller] pad block 0x{_padBlock:X}");
                _padHook = hooks.CreateHook<PadUpdateDelegate>(PadUpdate, address).Activate();
            });
        new Thread(Poll) { IsBackground = true, Name = "ControllerInput" }.Start();
    }

    private nint PadUpdate(nint a1)
    {
        var res = _padHook!.OriginalFunction(a1);
        if (ChordHeld && _padBlock != 0)
        {
            int zero = 0;
            foreach (var off in ButtonMaskOffsets)
                unsafe { TryWriteRaw(_padBlock + off, &zero, 4); }
        }
        return res;
    }

    private void Poll()
    {
        while (true)
        {
            Thread.Sleep(PollMs);
            try { Tick(); }
            catch (Exception e) { Log($"[Controller] poll error: {e.Message}"); }
        }
    }

    private void Tick()
    {
        if (!GameHasFocus() || !TryGetState(out var st))
        {
            ChordHeld = _ltHeld = _rtHeld = false;
            _lastButtons = 0;
            return;
        }
        var pad = st.Gamepad;
        _ltHeld = _ltHeld ? pad.LeftTrigger > TrigOff : pad.LeftTrigger > TrigOn;
        _rtHeld = _rtHeld ? pad.RightTrigger > TrigOff : pad.RightTrigger > TrigOn;
        ChordHeld = _ltHeld && _rtHeld;

        ushort pressed = (ushort)(pad.Buttons & ~_lastButtons);
        _lastButtons = pad.Buttons;
        if (!ChordHeld || pressed == 0) return;

        if ((pressed & DPAD_UP) != 0) Speech.RepeatLast();
        if ((pressed & DPAD_LEFT) != 0) Speech.Step(-1);
        if ((pressed & DPAD_RIGHT) != 0) Speech.Step(+1);
        if ((pressed & DPAD_DOWN) != 0) Dialogue.ToggleReader();
    }

    /// <summary>
    /// Reads the connected controller. XInputGetState on an empty slot enumerates devices (slow,
    /// can stutter the game, lesson of P4G): poll only the known slot, re-scan every ~2 s.
    /// </summary>
    private bool TryGetState(out XInputState st)
    {
        if (_slot >= 0)
        {
            if (XInputGetState((uint)_slot, out st) == 0) return true;
            _slot = -1;
        }
        if (_rescanWait > 0) { _rescanWait--; st = default; return false; }
        _rescanWait = 60;
        for (uint i = 0; i < 4; i++)
        {
            if (XInputGetState(i, out st) == 0)
            {
                _slot = (int)i;
                if (!_connectedLogged) { Log($"[Controller] controller {i} connected"); _connectedLogged = true; }
                return true;
            }
        }
        st = default;
        return false;
    }

    private delegate nint PadUpdateDelegate(nint a1);

    [StructLayout(LayoutKind.Sequential)]
    private struct XInputGamepad
    {
        public ushort Buttons;
        public byte LeftTrigger;
        public byte RightTrigger;
        public short ThumbLX, ThumbLY, ThumbRX, ThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XInputState
    {
        public uint PacketNumber;
        public XInputGamepad Gamepad;
    }

    [DllImport("xinput1_4.dll")]
    private static extern uint XInputGetState(uint dwUserIndex, out XInputState pState);
}
