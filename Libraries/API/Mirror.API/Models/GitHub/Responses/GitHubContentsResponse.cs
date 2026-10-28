using Newtonsoft.Json;

namespace Mirror.API.Models.GitHub.Responses;

public class GitHubContentsResponse
{
    [JsonProperty("name")] public string Name = null!;
    [JsonProperty("download_url")] public string DownloadURL = null!;
}
