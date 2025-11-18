using Newtonsoft.Json;

namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class RatingSummary
{
    [JsonProperty("video_id")]
    public Guid videoid { get; set; } = Guid.Empty;

    [JsonProperty("average_rating")]
    public String averageRating { get; set; } = string.Empty;

    [JsonProperty("rating_count")]
    public int ratingCount { get; set; } = 0;
    
    [JsonProperty("current_user_rating")]
    public int currentUserRating { get; set; } = 0;
}