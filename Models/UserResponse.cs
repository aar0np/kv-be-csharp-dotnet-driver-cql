namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class UserResponse
{
    public Guid userId { get; set; } = Guid.NewGuid();
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.Now;
    public string email { get; set; } = string.Empty;
    public string firstName { get; set; } = string.Empty;
    public string lastName { get; set; } = string.Empty;
    public string accountStatus { get; set; } = string.Empty;
    public DateTimeOffset lastLoginDate { get; set; } = DateTimeOffset.Now;

    public static UserResponse fromUser (User user)
    {
        UserResponse response = new();

        response.userId = user.userid;
        response.createdAt = user.createdDate;
        response.email = user.email;
        response.firstName = user.firstname;
        response.lastName = user.lastname;
        response.accountStatus = user.accountStatus;
        response.lastLoginDate = user.lastLoginDate;

        return response;
    }
}