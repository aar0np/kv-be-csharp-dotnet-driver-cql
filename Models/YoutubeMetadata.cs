namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class YoutubeMetadata
{
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string thumbnailUrl { get; set; } = string.Empty;
    public HashSet<string> tags { get; set; } = new();
}