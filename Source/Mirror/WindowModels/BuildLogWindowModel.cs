using CommunityToolkit.Mvvm.ComponentModel;

using Mirror.Framework.Models;
using Mirror.Models.Plugins;

namespace Mirror.WindowModels;

public partial class BuildLogWindowModel : WindowModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    private UnrealProject project = new();

    /* Follows the tail unless the reader has scrolled away from it */
    [ObservableProperty] private bool autoScroll = true;

    public string Subtitle => $"{Project.EngineVersionLabel}  ·  {Project.PluginVersionLabel}";
}
