namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class JwtResponse
{
    public string token { get; set; } = string.Empty;
    public string type { get; set; } = "Bearer";
    public string userId { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;

    public JwtResponse(string jwtToken, string jwtUserid, string jwtEmail)
    {
        token = jwtToken;
        userId = jwtUserid;
        email = jwtEmail;
    }
}