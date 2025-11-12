using Cassandra;
using Newtonsoft.Json;

namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class VideoResponse
{
    public Guid key { get; set; } = Guid.NewGuid();
    public Guid userId { get; set; } = Guid.NewGuid();
    public Guid videoId { get; set; } = Guid.NewGuid();

    [JsonProperty("title")]
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public HashSet<string> tags { get; set; } = new();
    public string location { get; set; } = string.Empty;
    [JsonProperty("location_type")]
    public int locationType { get; set; } = 0;

    [JsonProperty("thumbnailUrl")]
    public string previewImageLocation { get; set; } = string.Empty;

    [JsonProperty("submittedAt")]
    public DateTimeOffset addedDate { get; set; } = DateTime.UtcNow;

    [JsonProperty("creator")]
    public string userName { get; set; } = string.Empty;
    public int commentCount { get; set; } = 0;
    public int views { get; set; } = 0;
    public string processingStatus { get; set; } = "PENDING";
    [JsonProperty("averageRating")]
    public float rating { get; set; } = 0F;
    [JsonProperty("content_features")]
    public CqlVector<float>? contentFeatures { get; set; }
    public string youtubeVideoId { get; set; } = string.Empty;
    public string contentRating { get; set; } = string.Empty;
    public string category { get; set; } = string.Empty;
    public static VideoResponse fromVideo(Video video)
    {
        VideoResponse response = new VideoResponse();
        response.key = video.videoId;
        response.videoId = video.videoId;
        response.userId = video.userId;
        response.name = video.name;
        response.description = video.description;
        response.tags = video.tags;
        response.location = video.location;
        response.locationType = video.locationType;
        response.previewImageLocation = video.previewImageLocation;
        response.addedDate = video.addedDate;
        response.contentFeatures = video.contentFeatures;
        response.youtubeVideoId = video.youtubeId;
        response.views = video.views;
        response.contentRating = video.contentRating;
        response.category = video.category;

        return response;
    }

    public static VideoResponse fromLatestVideo(LatestVideo video)
    {
        VideoResponse response = new VideoResponse();
        response.key = video.videoId;
        response.videoId = video.videoId;
        response.name = video.name;
        response.userId = video.userId;
        response.previewImageLocation = video.previewImageLocation;
        response.addedDate = video.addedDate;
        response.contentRating = video.contentRating;
        response.category = video.category;

        return response;
    }
}