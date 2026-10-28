using Avalonia.Controls;
using Avalonia.Interactivity;
using Mirror.Framework.Models;
using Mirror.WindowModels;

namespace Mirror.Windows;

/* ~~~ GalleryWindow ~~~ */
public partial class GalleryWindow : WindowBase<GalleryWindowModel>
{
    public GalleryWindowModel WM => WindowModel;
    
    public GalleryWindow()
    {
        Setup();
    }
    
    /* ~~~ Setup Logic ~~~ */
    private void Setup()
    {
        InitializeComponent();
        captionButtons.Attach((VisualRoot as Window)!);
        WindowModel = new GalleryWindowModel();

        _ = WindowModel.Initialize();
        DataContext = WindowModel;
    }

    private void Close(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void PrimaryButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
        WindowModel.InvokePrimaryButtonClick();
    }
}
