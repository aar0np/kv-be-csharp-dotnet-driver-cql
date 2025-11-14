using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public interface IUserCredentialsDAL
{
    UserCredentials SaveUserCreds(UserCredentials user);

    UserCredentials? FindByEmail(string email);

    void UpdateUserCreds(UserCredentials user);
}