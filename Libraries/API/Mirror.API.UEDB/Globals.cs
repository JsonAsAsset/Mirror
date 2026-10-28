namespace Mirror.API.UEDB;

public static class Globals
{
    /* Use mappings from FN */
    public static API.UEDB API { get; } = new(Mirror.API.Globals.RestClient, "fortnite");
}
