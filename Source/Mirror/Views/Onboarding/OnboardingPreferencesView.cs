using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

using Mirror.Windows;

namespace Mirror.Views.Onboarding;

public partial class OnboardingPreferencesView : UserControl
{
    public OnboardingPreferencesView()
    {
        InitializeComponent();
    }

    private void Save(object? sender, RoutedEventArgs e)
    {
        var window = VisualRoot as OnboardingWindow;
        window!.GoNext();
    }
}
