using Mirror.Framework.Models;
using Mirror.Services.Framework;
using Mirror.ViewModels.Settings;

namespace Mirror.Views.Settings;

public partial class ConnectionsSettingsView : ViewBase<ConnectionsSettingsViewModel>
{
    public ConnectionsSettingsView() : base(AppServices.Settings.Connections)
    {
        InitializeComponent();
    }
}
