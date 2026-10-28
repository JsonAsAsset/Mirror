using Mirror.Plugins.EpicGames.API;
using Mirror.Plugins.EpicGames.API.Responses;

namespace Mirror.Plugins.EpicGames;

public static class Globals
{
    public static EpicAuthResponse? EpicAuth;
    
    public static EpicGamesAPI API { get; } = new(Mirror.API.Globals.RestClient);
}
