using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Media;
using Memory;

namespace TrainerBase;

public class Cheats(MainWindow mainWindow)
{
    public readonly Mem Mem = new();

    public void SetupAttach()
    {
        Process.EnterDebugMode();
        if (Mem.OpenProcess(MainWindow.ProcessName) == Mem.OpenProcessResults.Success)
        {
            HandleOpenGame();
            SetupExit();
        }
        Process.LeaveDebugMode();

        var watcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
        watcher.EventArrived += (_, e) =>
        {
            if (mainWindow.ViewModel.Attached)
            {
                return;
            }
            
            var name = e.NewEvent.Properties["ProcessName"].Value.ToString()?.ToLower();
            if (name == null || name != MainWindow.ProcessName)
            {
                return;
            }

            Process.EnterDebugMode();
            var result = Mem.OpenProcess(MainWindow.ProcessName);
            Process.LeaveDebugMode();
            if (result != Mem.OpenProcessResults.Success) return;
            HandleOpenGame();
            SetupExit();
        };

        watcher.Start();
    }
    
    private void SetupExit()
    {
        Mem.MProc.Process.EnableRaisingEvents = true;
        Mem.MProc.Process.Exited += (_, _) => HandleCloseGame();
    }
    
    private void HandleOpenGame()
    {
        if (mainWindow.ViewModel.Attached)
        {
            return;
        }

        mainWindow.Dispatcher.Invoke(() =>
        {
            mainWindow.GameStatusLabel.Foreground = Brushes.Green;
            mainWindow.GameStatusLabel.Text = "On";
            mainWindow.ProcessIdLabel.Text = Mem.MProc.ProcessId.ToString();
        });
        
        mainWindow.ViewModel.Attached = true;
    }

    private void HandleCloseGame()
    {
        if (!mainWindow.ViewModel.Attached)
        {
            return;
        }

        mainWindow.Dispatcher.Invoke(() =>
        {
            mainWindow.GameStatusLabel.Foreground = Brushes.Red;
            mainWindow.GameStatusLabel.Text = "Off";
            mainWindow.ProcessIdLabel.Text = "0";
        });
        
        ResetTrainer();
        mainWindow.ViewModel.Attached = false;
    }

    private void ResetTrainer()
    {
    }

    public void TrainerClose()
    {
        Imports.CloseHandle(Mem.MProc.Handle);
    }

    private async Task<nuint> SmartAobScan(string search, UIntPtr? start = null, UIntPtr? end = null)
    {
        var handle = Mem.MProc.Handle;
        var minRange = (long)Mem.MProc.Process.MainModule!.BaseAddress;
        var maxRange = minRange + Mem.MProc.Process.MainModule!.ModuleMemorySize;

        if (start != null)
        {
            minRange = (long)start;
        }
        
        if (end != null)
        {
            maxRange = (long)end;
        }
        
        var scanStartAddr = minRange;
        var address = (UIntPtr)minRange;

        try
        {
            Imps.GetSystemInfo(out var info);
            while (address < (ulong)maxRange)
            {
                Imps.Native_VirtualQueryEx(handle, address, out Imps.MemoryBasicInformation64 memInfo, info.PageSize);
                if (address == memInfo.BaseAddress + memInfo.RegionSize)
                {
                    break;
                }

                var scanEndAddr = (long)memInfo.BaseAddress + (long)memInfo.RegionSize;

                nuint retAddress;
                if (scanEndAddr - scanStartAddr > 500000000)
                {
                    retAddress = await ScanRange(search, scanStartAddr, scanEndAddr);
                }
                else
                {
                    retAddress = (await Mem.AoBScan(scanStartAddr, scanEndAddr, search)).FirstOrDefault();
                }

                if (retAddress != 0)
                {
                    return retAddress;
                }

                scanStartAddr = scanEndAddr;
                address = memInfo.BaseAddress + checked((UIntPtr)memInfo.RegionSize);
            }
        }
        catch
        {
            // ignored
        }

        return 0;
    }

    private async Task<nuint> ScanRange(string search, long startAddr, long endAddr)
    {
        var end = startAddr + (endAddr - startAddr) / 2;
        var retAddress = (await Mem.AoBScan(startAddr, end, search)).FirstOrDefault();
        return retAddress;
    }

    private static void ShowError(string feature, string sig)
    {
        MessageBox.Show(
            $"Address for this feature wasn't found!\nPlease try to activate the cheat again or try to restart the game and the trainer.\n\nIf this error still occur, please (Press Ctrl+C) to copy, and make an issue on the github repository.\n\nFeature: {feature}\nSignature: {sig}\n\nTrainer Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}",
            "Error", 0, MessageBoxImage.Error
        );
    }
}
