using System.ComponentModel.DataAnnotations;

namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class UserCredentials
{
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public Guid userid { get; set; } = Guid.Empty;
    public bool accountLocked { get; set; } = false;
}