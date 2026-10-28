using System.IO;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

using Mirror.Extensions;
using Mirror.Framework.Models;
using Mirror.Services;
using Mirror.Services.Framework;
using Mirror.ViewModels.Settings;
using Mirror.Windows;

namespace Mirror.Views.Settings;

public partial class DebugSettingsView : ViewBase<DebugSettingsViewModel>
{
    public DebugSettingsView() : base(AppServices.Settings.Debug)
    {
        InitializeComponent();
    }

    private void OpenLogsFolder(object? sender, RoutedEventArgs e)
    {
        var folder = LogsFolder.FullName;

        if (Directory.Exists(folder))
        {
            AppService.OpenLink(folder);
        }
    }

    private void OpenStartup(object? sender, RoutedEventArgs e)
    {
        var win = new OnboardingWindow();

        if (this.GetVisualRoot() is not Window window) return;
        
        win.CenterToScreen(window);
        win.Show();
    }
    
    private void OpenGallery(object? sender, RoutedEventArgs e)
    {
        var win = new GalleryWindow();

        if (this.GetVisualRoot() is not Window window) return;
        
        win.CenterToScreen(window);
        win.Show();
    }
}
