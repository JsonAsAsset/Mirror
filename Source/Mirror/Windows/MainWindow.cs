using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Mirror.Extensions;
using Mirror.Framework.Models;
using Mirror.WindowModels;

namespace Mirror.Windows;

public partial class MainWindow : WindowBase<MainWindowModel>
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = WindowModel;
        
        Navigation.App.Initialize(MainNavigationView);
        Navigation.App.OnNavigate += WindowModel.OnNavigationItemSelected;

        WindowModel.Initialize();
    }

    public void OnEditProfile(object? sender, RoutedEventArgs e)
    {  
        WindowModel.RequestEditProfile();
    }
    
    public void OnEditLinkedProfile(object? sender, RoutedEventArgs e)
    {  
        WindowModel.RequestEditLinkedProfile();
    }
}
