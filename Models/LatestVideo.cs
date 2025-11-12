using Cassandra;

namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class LatestVideo
{
    //public string day { get; set; } = DateTimeOffset.Now.ToString("yyyy-MM-dd");
    public LocalDate day { get; set; } = LocalDate.Parse(DateTimeOffset.Now.Date.ToString("yyyy-MM-dd"));
    public DateTimeOffset addedDate { get; set; } = DateTimeOffset.Now;
    public Guid videoId { get; set; } = Guid.NewGuid();
    public string name { get; set; } = string.Empty;
    public string previewImageLocation { get; set; } = string.Empty;
    public Guid userId { get; set; } = Guid.NewGuid();
    public string contentRating { get; set; } = string.Empty;
    public string category { get; set; } = string.Empty;

    public static LatestVideo fromVideo(Video video)
    {
        LatestVideo response = new LatestVideo();
        response.videoId = video.videoId;
        response.name = video.name;
        response.previewImageLocation = video.previewImageLocation;
        response.addedDate = video.addedDate;
        response.userId = video.userId;
        response.contentRating = video.contentRating;
        response.category = video.category;

        return response;
    }
}