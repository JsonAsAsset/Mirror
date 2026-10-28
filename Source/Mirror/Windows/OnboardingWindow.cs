using Avalonia.Controls;
using Avalonia.Interactivity;

using Mirror.Application;
using Mirror.Framework.Models;
using Mirror.Services;
using Mirror.Views.Onboarding;
using Mirror.WindowModels;

namespace Mirror.Windows;

public partial class OnboardingWindow : WindowBase<OnboardingWindowModel>
{
    public OnboardingWindow()
    {
        InitializeComponent();
        captionButtons.Attach((VisualRoot as Window)!);
        
        DataContext = WindowModel;

        WindowModel.CurrentPageIndex = 0;
        
        Navigation.Onboarding = new NavigatorContext();
        Navigation.Onboarding.Initialize(OnboardingNavigationView);
        Navigation.Onboarding.Open(typeof(OnboardingWelcomeView));
    }
    
    private void Back(object? sender, RoutedEventArgs e)
    {
        if (!WindowModel.CanGoBack) return;

        WindowModel.GoBack();
        
        var type = WindowModel.CurrentPageType;
        Navigation.Onboarding.Open(type);
    }

    public void GoNext()
    {
        if (!WindowModel.CanGoNext) return;

        WindowModel.GoNext();
        
        var type = WindowModel.CurrentPageType;

        if (type == typeof(MainWindow))
        {
            CompleteOnboarding();
            
            return;
        }
        
        Navigation.Onboarding.Open(type);
    }

    private void CompleteOnboarding()
    {
        Settings.Application.CompletedOnboarding = true;
        Settings.Save();
        
        var app = (AppInstance)Avalonia.Application.Current!;
            
        if (app.savedWindow.GetType() != typeof(MainWindow))
        {
            app.SpawnWindow(new MainWindow());
        }

        Close();
    }
        
    private void OpenDonateLink(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(DONATE_LINK);
    }
    
    private void OpenDiscordLink(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(DISCORD_LINK);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        CompleteOnboarding();
    }
}