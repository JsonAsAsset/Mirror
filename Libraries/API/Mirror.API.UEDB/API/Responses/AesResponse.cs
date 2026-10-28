using Newtonsoft.Json;

using Mirror.Resources.Framework.CUEParse;

namespace Mirror.API.UEDB.API.Responses;

public class AesResponse
{
    [JsonProperty("mainKey")] public string MainKey = null!;
    [JsonProperty("dynamicKeys")] public List<EncryptionKey> DynamicKeys = null!;
}
