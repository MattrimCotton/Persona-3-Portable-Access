using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using p3ppc.accessibility.Configuration;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace p3ppc.accessibility;

/// <summary>
/// Shared helpers, copied from P4G Access (Utils.cs) and trimmed to what P3P uses so far.
/// </summary>
internal class Utils
{
    // Guarded reads through ReadProcessMemory on our own process (from P4G, 2026-07-27):
    // an AccessViolationException cannot be caught in .NET 9, so EVERY read of game memory
    // whose validity isn't guaranteed goes through these. No VirtualQuery (VAD lock stalls).
    [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
    private static extern unsafe bool Rpm(nint h, nint addr, void* buf, nint size, out nint read);
    [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcess")]
    private static extern nint SelfProc();

    /// <summary>Copy <paramref name="size"/> bytes from <paramref name="addr"/> into
    /// <paramref name="dst"/> — false (nothing copied) on any unreadable page.</summary>
    internal static unsafe bool TryReadRaw(nint addr, void* dst, int size)
        => addr >= 0x10000 && (ulong)addr <= 0x00007FFFFFFFFFFFUL
           && Rpm(SelfProc(), addr, dst, size, out nint got) && got == (nint)size;

    [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
    private static extern unsafe bool Wpm(nint h, nint addr, void* buf, nint size, out nint written);

    /// <summary>Guarded write into game memory (same reason as <see cref="TryReadRaw"/>):
    /// false, nothing written, on any unwritable page.</summary>
    internal static unsafe bool TryWriteRaw(nint addr, void* src, int size)
        => addr >= 0x10000 && (ulong)addr <= 0x00007FFFFFFFFFFFUL
           && Wpm(SelfProc(), addr, src, size, out nint done) && done == (nint)size;

    internal static unsafe bool TryWriteBytes(nint addr, byte[] bytes)
    {
        fixed (byte* p = bytes) return TryWriteRaw(addr, p, bytes.Length);
    }

    internal static unsafe bool TryRead<T>(nint addr, out T value) where T : unmanaged
    {
        T tmp;
        bool ok = TryReadRaw(addr, &tmp, sizeof(T));
        value = ok ? tmp : default;
        return ok;
    }

    internal static nint ReadPtr(nint addr) => TryRead(addr, out nint v) ? v : 0;

    private static ILogger _logger = null!;
    private static Config _config = null!;
    internal static Config Config => _config;
    private static IStartupScanner _startupScanner = null!;
    internal static nint BaseAddress { get; private set; }

    /// <summary>This mod's own folder (set in Mod.cs); data files and charsets live here.</summary>
    internal static string ModDir { get; set; } = "";

    internal static bool Initialise(ILogger logger, Config config, IModLoader modLoader)
    {
        _logger = logger;
        _config = config;
        using var thisProcess = Process.GetCurrentProcess();
        BaseAddress = thisProcess.MainModule!.BaseAddress;

        var startupScannerController = modLoader.GetController<IStartupScanner>();
        if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner!))
        {
            LogError("Unable to get controller for Reloaded SigScan Library, nothing will work");
            return false;
        }

        return true;
    }

    internal static void UpdateConfig(Config config) => _config = config;

    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll", SetLastError = true)] private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentProcessId();

    /// <summary>True when the focused window belongs to the game. Key-poll threads must
    /// do nothing when false, so hotkeys don't fire while the player is alt-tabbed.</summary>
    internal static bool GameHasFocus()
    {
        nint hwnd = GetForegroundWindow();
        if (hwnd == 0) return false;
        GetWindowThreadProcessId(hwnd, out uint pid);
        return pid == GetCurrentProcessId();
    }

    internal static void LogDebug(string message)
    {
        if (_config.DebugEnabled)
            _logger.WriteLine($"[P3P Access] {message}");
    }

    internal static void Log(string message) => _logger.WriteLine($"[P3P Access] {message}");

    internal static void LogError(string message, Exception e)
        => _logger.WriteLine($"[P3P Access] {message}: {e.Message}", Color.Red);

    internal static void LogError(string message)
        => _logger.WriteLine($"[P3P Access] {message}", Color.Red);

    internal static void SigScan(string pattern, string name, Action<nint> action)
    {
        _startupScanner.AddMainModuleScan(pattern, result =>
        {
            if (!result.Found)
            {
                LogError($"Unable to find {name}, it won't work");
                return;
            }

            Log($"Found {name} at 0x{result.Offset + BaseAddress:X}");
            action(result.Offset + BaseAddress);
        });
    }

    /// <summary>
    /// Gets the address of a global from the rel32 displacement of an instruction that references it.
    /// </summary>
    /// <param name="ptrAddress">Address of the 4-byte displacement (the end of the instruction must follow it)</param>
    internal static unsafe nint GetGlobalAddress(nint ptrAddress)
        => (nint)((*(int*)ptrAddress) + ptrAddress + 4);
}
