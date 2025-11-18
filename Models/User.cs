namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class User
{
    public Guid userid { get; set; } = Guid.NewGuid();
    public DateTimeOffset createdDate { get; set; } = DateTimeOffset.Now;
    public string email { get; set; } = string.Empty;
    public string firstname { get; set; } = string.Empty;
    public string lastname { get; set; } = string.Empty;
    public string accountStatus { get; set; } = string.Empty;
    public DateTimeOffset lastLoginDate { get; set; } = DateTimeOffset.Now;

    public static User fromUserRegistrationRequest(UserRegistrationRequest req)
    {
        User user = new User();

        user.email = req.email;
        user.firstname = req.firstname;
        user.lastname = req.lastname;

        return user;
    }
}