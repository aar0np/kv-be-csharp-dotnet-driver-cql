using Newtonsoft.Json;

namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class User
{
    public Guid userid { get; set; } = Guid.NewGuid();
    [JsonProperty("created_date")]
    public DateTimeOffset createdDate { get; set; } = DateTimeOffset.Now;
    public string email { get; set; } = string.Empty;
    public string firstname { get; set; } = string.Empty;
    public string lastname { get; set; } = string.Empty;
    [JsonProperty("account_status")]
    public string accountStatus { get; set; } = string.Empty;
    [JsonProperty("last_login_date")]
    public DateTimeOffset lastLoginDate { get; set; } = DateTimeOffset.Now;
}