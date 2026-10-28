using CommunityToolkit.Mvvm.ComponentModel;

using Mirror.Framework.Models;
using Mirror.Models.Profiles;

namespace Mirror.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private Profile? currentProfile = new();
    
    [ObservableProperty]
    private bool isOnboarding;
}
