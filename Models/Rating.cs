using Newtonsoft.Json;

namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class Rating
{
    [JsonProperty("video_id")]
    public Guid videoid { get; set; } = Guid.Empty;

    [JsonProperty("user_id")]
    public Guid userid { get; set; } = Guid.Empty;

    public int rating { get; set; } = 0;

    [JsonProperty("rating_date")]
    public DateTimeOffset ratingDate { get; set; } = DateTimeOffset.Now;
}