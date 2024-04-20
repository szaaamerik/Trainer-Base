using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace TrainerBase;

public sealed partial class MainWindow
{
    public static string ProcessName => "";
    public static string GameName => "";
    public static string GameVersion => "";

    private GlobalHotkey _currentHotkey = null!;
    
    private readonly Cheats _cheats;
    public MainWindowViewModel ViewModel { get; }

    
    public MainWindow()
    {
        ViewModel = new MainWindowViewModel();
        DataContext = this;
        
        InitializeComponent();
        _cheats = new Cheats(this);
        _cheats.SetupAttach();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || ViewModel.HotkeysVisible) return;
        Cursor = Cursors.SizeAll;
        DragMove();
        Cursor = Cursors.Arrow;
    }

    private void Minimize_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || ViewModel.HotkeysVisible) return;
        SystemCommands.MinimizeWindow(this);
    }

    private void ExitButton_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || ViewModel.HotkeysVisible) return;
        SystemCommands.CloseWindow(this);
        _cheats.TrainerClose();
        Application.Current.Shutdown();
    }

    private void RectangleOpenLink_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Rectangle rectangle)
        {
            return;
        }
        
        if (e.ChangedButton != MouseButton.Left || ViewModel.HotkeysVisible) return;
        var processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = rectangle.ToolTip as string
        };
        Process.Start(processStartInfo);
    }

    private async void OpenHotkeysPrompt()
    {
        HotKeyBox.HotKey = null;
        ViewModel.HotkeysVisible = true;

        if (!HotKeyBox.Focus())
        {
            HotKeyBox.Focus();
        }
        
        while (ViewModel is { HotkeysOpacity: < 1, HotkeysBackgroundOpacity: < 0.5 })
        {
            ViewModel.HotkeysOpacity += 0.05;
            ViewModel.HotkeysBackgroundOpacity = ViewModel.HotkeysOpacity / 2;
            await Task.Delay(10);
        }
    }

    private async void CloseHotkeysPrompt()
    {
        while (ViewModel is { HotkeysOpacity: > 0, HotkeysBackgroundOpacity: > 0 })
        {
            ViewModel.HotkeysOpacity -= 0.05;
            ViewModel.HotkeysBackgroundOpacity = ViewModel.HotkeysOpacity / 2;
            await Task.Delay(10);
        }
        ViewModel.HotkeysVisible = false;
    }

    private void HotKeyBox_OnHotKeyChanged(object sender, RoutedPropertyChangedEventArgs<HotKey?> e)
    {
        if (e.NewValue == null)
        {
            return;
        }

        if (HotkeysManager.DoesTheSameHotkeyExist(e.NewValue.ModifierKeys, e.NewValue.Key))
        {
            MessageBox.Show("This hotkey is already registered!", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            CloseHotkeysPrompt();
        }
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        OpenHotkeysPrompt();
    }
}