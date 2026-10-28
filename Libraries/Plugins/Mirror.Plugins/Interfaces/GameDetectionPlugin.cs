using Serilog;

using Mirror.Resources.Framework.Base;

namespace Mirror.Plugins.Interfaces;

public interface IGameDetectionPlugin : IPlugin
{
    void Detect(List<BaseProfile> LoadedProfiles, Action<BaseProfile>? onDetected = null);
    
    public static Task TryDetectGameAsync(
        string gameId,
        Func<BaseProfile?> detectFunc,
        Action<BaseProfile>? onDetected,
        List<BaseProfile> LoadedProfiles)
    {
        var detectedProfile = detectFunc();
        if (detectedProfile is null) return Task.CompletedTask;
        
        Log.Information($"Detected {detectedProfile.Name} at {detectedProfile.ArchiveDirectory}");

        LoadedProfiles.Add(detectedProfile);
        onDetected?.Invoke(detectedProfile);

        return Task.CompletedTask;
    }
}
