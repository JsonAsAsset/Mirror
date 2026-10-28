global using static Mirror.Resources.Globals;
using Mirror.API.Models.Base;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Mirror.API.Models.GitHub;

namespace Mirror.API;

public static class Globals
{
    public static RestClient RestClient { get; } = new(
        new RestClientOptions
        {
            UserAgent = $"{APP_NAME}/{VERSION}"
        },
        configureSerialization: s => s.UseSerializer<JsonNetSerializer>());
    
    public static GitHubAPI GitHub { get; } = new(RestClient);
    public static APIBase API { get; } = new(RestClient);
}
