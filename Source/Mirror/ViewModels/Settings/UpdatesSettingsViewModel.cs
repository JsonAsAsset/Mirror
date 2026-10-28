using System;
using System.Text.Json.Serialization;

using Avalonia;

using CommunityToolkit.Mvvm.ComponentModel;

using CUE4Parse.UE4.Versions;

using FluentAvalonia.UI.Media.Animation;

using Mirror.Framework.Models;

namespace Mirror.ViewModels.Settings;

public partial class UpdatesSettingsViewModel : ViewModelBase
{
    [ObservableProperty] private bool _developmentUpdateMode;
}
