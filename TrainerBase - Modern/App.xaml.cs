using System.Windows;

namespace TrainerBase;

public partial class App
{
    private const string MutexName = "";
    private Mutex _mutex = null!;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, MutexName, out var createdNew);

        if (createdNew)
        {
            base.OnStartup(e);
        }
        else
        {
            MessageBox.Show("Another instance of the trainer is already running.", "Information", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            Current.Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (HotkeysManager.IsHookSetup)
        {
            HotkeysManager.ShutdownSystemHook();
        }

        _mutex.ReleaseMutex();
        _mutex.Dispose();
        base.OnExit(e);
    }
}