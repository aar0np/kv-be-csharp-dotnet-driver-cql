namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class UserUpdateRequest
{
    public string firstName { get; set; } = string.Empty;
    public string lastName { get; set; } = string.Empty;
    public String email { get; set; } = string.Empty;
    public String password { get; set; } = string.Empty;
    public String roles { get; set; } = string.Empty;
}