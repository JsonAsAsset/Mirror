using CommunityToolkit.Mvvm.ComponentModel;

using Mirror.Framework.Models;
using Mirror.Services.Framework;

namespace Mirror.ViewModels.Settings;

public partial class CloudSettingsViewModel : ViewModelBase
{
    [ObservableProperty] private bool _runHostedAPI = true;
    
    partial void OnRunHostedAPIChanged(bool value)
    {
        if (value)
        {
            AppServices.Cloud.Initialize();
        }
        else
        {
            AppServices.Cloud.Stop();
        }
        
        MainWM.UpdateAPIServiceEnabled();
    }
}
