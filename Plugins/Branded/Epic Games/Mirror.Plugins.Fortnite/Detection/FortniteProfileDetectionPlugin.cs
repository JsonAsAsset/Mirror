using CUE4Parse.UE4.Versions;

using Mirror.Plugins.EpicGames.Detection;
using Mirror.Resources.Framework.Base;
using Mirror.Plugins.Interfaces;

namespace Mirror.Plugins.Fortnite.Detection;

public sealed class FortniteProfileDetectionPlugin : IEpicGamesDetection, IGameVersionUpdatePlugin
{
    public string GameId => "Fortnite";
    public string Name => "Fortnite Profile Detection";
    public EGame TargetVersion => EGame.GAME_UE6_LATEST;
    
    public async void Detect(List<BaseProfile> LoadedProfiles, Action<BaseProfile>? onDetected = null)
    {
        await IEpicGamesDetection.TryDetectGameAsync(
            gameId: "Fortnite",
            detectFunc: () => IEpicGamesDetection.DetectGame("Fortnite", @"\FortniteGame\Content\Paks", TargetVersion, "Fortnite"),
            onDetected,
            LoadedProfiles
        );
    }
}
