using Mirror.API.UEDB.API;

namespace Mirror.Plugins.Fortnite;

public static class Globals
{
    public static UEDB API { get; } = new(Mirror.API.Globals.RestClient, "fortnite");
}
