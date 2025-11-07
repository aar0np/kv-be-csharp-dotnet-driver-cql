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
    public DateTime addedDate { get; set; } = DateTime.UtcNow;

    [JsonProperty("creator")]
    public string userName { get; set; } = string.Empty;
    public int commentCount { get; set; } = 0;
    public int views { get; set; } = 0;
    public string processingStatus { get; set; } = "PENDING";

    [JsonProperty("averageRating")]
    public float rating { get; set; } = 0F;
    //public float[] videoVector { get; set; } = Array.Empty<float>();
    public CqlVector<float>? videoVector { get; set; }

    [JsonProperty("content_features")]
    public CqlVector<float>? contentFeatures { get; set; }

    public string youtubeVideoId { get; set; } = string.Empty;

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
        response.videoVector = video.videoVector;
        response.contentFeatures = video.contentFeatures;
        response.youtubeVideoId = video.youtubeId;
        response.views = video.views;

        return response;
    }
}