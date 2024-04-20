using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace TrainerBase;

// ReSharper disable InconsistentNaming
public static partial class HotkeysManager
{
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private static readonly LowLevelKeyboardProc LowLevelProc = HookCallback;
        
    private static List<GlobalHotkey> Hotkeys { get; set; }
    private const int WH_KEYBOARD_LL = 13;

    private static IntPtr _hookId = IntPtr.Zero;
    public static bool IsHookSetup { get; private set; }

    static HotkeysManager()
    {
        Hotkeys = new List<GlobalHotkey>();
    }

    private static void SetupSystemHook()
    {
        _hookId = SetHook(LowLevelProc);
        IsHookSetup = true;
    }
    
    public static void ShutdownSystemHook()
    {
        UnhookWindowsHookEx(_hookId);
        IsHookSetup = false;
    }

    public static void AddHotkey(GlobalHotkey hotkey)
    {
        if (!IsHookSetup)
        {
            SetupSystemHook();
        }
        
        Hotkeys.Add(hotkey);
    }

    public static bool DoesTheSameHotkeyExist(ModifierKeys modifier, Key key)
    {
        return Hotkeys.Any(globalHotkey => globalHotkey.Key == key && globalHotkey.Modifier == modifier);
    }

    private static void CheckHotkeys()
    {
        foreach (var hotkey in Hotkeys.Where(hotkey => Keyboard.Modifiers == hotkey.Modifier && Keyboard.IsKeyDown(hotkey.Key)).Where(hotkey => hotkey.CanExecute))
        {
            hotkey.Callback();
        }
    }
    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        return curModule == null ? 0 : SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            CheckHotkeys();
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    #region Native Methods

    [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExA", SetLastError = true)]
    private static partial IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial void UnhookWindowsHookEx(IntPtr hhk);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    
    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint GetModuleHandle(string lpModuleName);

    #endregion
}