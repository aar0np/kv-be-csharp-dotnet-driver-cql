using Cassandra;
//using Newtonsoft.Json;

namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class VideoResponse
{
    public Guid key { get; set; } = Guid.NewGuid();
    public Guid userId { get; set; } = Guid.NewGuid();
    public Guid videoId { get; set; } = Guid.NewGuid();
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public HashSet<string> tags { get; set; } = new();
    public string location { get; set; } = string.Empty;
    public int location_type { get; set; } = 0;
    public string thumbnailUrl { get; set; } = string.Empty;
    public DateTimeOffset submittedAt { get; set; } = DateTime.UtcNow;
    public DateTimeOffset uploadDate { get; set; } = DateTime.UtcNow;
    public string creator { get; set; } = string.Empty;
    public int commentCount { get; set; } = 0;
    public int views { get; set; } = 0;
    public string processingStatus { get; set; } = "PENDING";
    public float averageRating { get; set; } = 0.0F;
    public CqlVector<float>? content_features { get; set; }
    public string youtubeVideoId { get; set; } = string.Empty;
    public string contentRating { get; set; } = string.Empty;
    public string category { get; set; } = string.Empty;

    public static VideoResponse fromVideo(Video video)
    {
        VideoResponse response = new VideoResponse();
        response.key = video.videoId;
        response.videoId = video.videoId;
        response.userId = video.userId;
        response.title = video.name;
        response.description = video.description;
        response.tags = video.tags;
        response.location = video.location;
        response.location_type = video.locationType;
        response.thumbnailUrl = video.previewImageLocation;
        response.submittedAt = video.addedDate;
        response.uploadDate = video.addedDate;
        response.content_features = video.contentFeatures;
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
        response.title = video.name;
        response.userId = video.userId;
        response.thumbnailUrl = video.previewImageLocation;
        response.submittedAt = video.addedDate;
        response.uploadDate = video.addedDate;
        response.contentRating = video.contentRating;
        response.category = video.category;

        return response;
    }
}