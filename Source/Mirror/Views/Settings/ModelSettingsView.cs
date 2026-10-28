using Mirror.Framework.Models;
using Mirror.Services.Framework;
using Mirror.ViewModels.Settings;

namespace Mirror.Views.Settings;

public partial class ModelSettingsView : ViewBase<ModelSettingsViewModel>
{
    public ModelSettingsView() : base(AppServices.Settings.Model)
    {
        InitializeComponent();
    }
}
