using Mirror.Framework.Models;
using Mirror.Services.Framework;
using Mirror.ViewModels.Settings;

namespace Mirror.Views.Settings;

public partial class ApplicationSettingsView : ViewBase<ApplicationSettingsViewModel>
{
    public ApplicationSettingsView() : base(AppServices.Settings.Application)
    {
        InitializeComponent();
    }
}
