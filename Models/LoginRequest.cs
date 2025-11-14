namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class LoginRequest
{
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}