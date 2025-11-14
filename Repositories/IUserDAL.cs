using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public interface IUserDAL
{
    User SaveUser(User user);

    Task<User?> FindByUserId(Guid userid);

    Task<User?> FindByEmail(string email);

    Task<bool> ExistsByEmail(string email);

    void UpdateUser(User user);
}