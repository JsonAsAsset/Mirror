using Mirror.Framework.Models;
using Mirror.Services.Framework;
using Mirror.ViewModels.Settings;

namespace Mirror.Views;

public partial class PluginView : ViewBase<PluginSettingsViewModel>
{
    public PluginView() : base(AppServices.Settings.Plugin)
    {
        InitializeComponent();
    }
}
