namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class LatestVideosResponse
{
    public List<VideoResponse> data { get; set; }

    public LatestVideosResponse(List<VideoResponse> videoResponses)
    {
        data = videoResponses;
    }
}