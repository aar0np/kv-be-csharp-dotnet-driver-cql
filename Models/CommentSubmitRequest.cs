using Newtonsoft.Json;

namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class CommentSubmitRequest
{
    [JsonProperty("comment_text")]
    public string commentText { get; set; } = string.Empty;
}