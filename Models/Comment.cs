using Cassandra;
using Newtonsoft.Json;

namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class Comment
{
    public Guid videoid { get; set; } = Guid.Empty;
    public TimeUuid commentid { get; set; } = TimeUuid.NewId();
    public string comment { get; set; } = string.Empty;
    public Guid userid { get; set; } = Guid.Empty;
    [JsonProperty("sentiment_score")]
    public float sentimentScore { get; set; } = 0.0F;
}