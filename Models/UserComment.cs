using Cassandra;
using Newtonsoft.Json;

namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class UserComment
{
    public Guid videoid { get; set; } = Guid.NewGuid();
    public TimeUuid commentid { get; set; } = Guid.NewGuid();
    public string comment { get; set; } = string.Empty;
    public Guid userid { get; set; } = Guid.NewGuid();
    [JsonProperty("sentiment_score")]
    public float sentimentScore { get; set; } = 0.0F;
}