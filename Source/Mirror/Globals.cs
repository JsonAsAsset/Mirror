global using static Mirror.Services.Framework.AppServices;
global using static Mirror.Resources.Globals;

using Avalonia.Platform.Storage;

namespace Mirror;

public static class Globals
{
    public const bool IsReadyToMeshExport = false;

    public static readonly FilePickerFileType MappingsFileType = new(".USMAP Files") { Patterns = [ "*.usmap" ] };
    public static readonly FilePickerFileType UnrealProjectFileType = new(".UPROJECT Files") { Patterns = [ "*.uproject" ] };

    /* Name of the plugin as it sits inside an Unreal project's Plugins folder */
    public const string UnrealPluginName = "Reflection";

    /* Releases here are GitHub source archives, so they are compiled after being extracted */
    public const string UnrealPluginRepository = $"JsonAsAsset/{UnrealPluginName}";

    public const bool ShowVersion = true;
    public const bool RedactProfiles = false;
    
    public static string LaunchProfileArg = string.Empty;
}
