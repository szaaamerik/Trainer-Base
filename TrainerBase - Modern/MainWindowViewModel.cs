using CommunityToolkit.Mvvm.ComponentModel;

namespace TrainerBase;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]    
    private bool _attached;

    [ObservableProperty]    
    private bool _hotkeysVisible;

    [ObservableProperty]    
    private double _hotkeysOpacity;
    
    [ObservableProperty]
    private double _hotkeysBackgroundOpacity;
}