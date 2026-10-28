using Mirror.Framework.Models;
using Mirror.Services.Framework;
using Mirror.ViewModels.Settings;

namespace Mirror.Views.Settings;

public partial class UpdatesSettingsView : ViewBase<UpdatesSettingsViewModel>
{
    public UpdatesSettingsView() : base(AppServices.Settings.Updates)
    {
        InitializeComponent();
    }
}
