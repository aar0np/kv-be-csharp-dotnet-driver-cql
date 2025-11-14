using Cassandra.Mapping;
using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public class UserCredentialsDAL : IUserCredentialsDAL
{
    private readonly Cassandra.ISession _session;
    private readonly IMapper _mapper;

    public UserCredentialsDAL(ICassandraConnection cassandraConnection)
    {
        _session = cassandraConnection.GetCQLSession();
        _mapper = new Mapper(_session);
    }
    
    public UserCredentials? FindByEmail(string email)
    {
        return _mapper.Single<UserCredentials>("WHERE email=?", email);
    }

    public UserCredentials SaveUserCreds(UserCredentials user)
    {
        _mapper.Insert(user);
        return user;
    }

    public void UpdateUserCreds(UserCredentials user)
    {
        _mapper.Update(user);
    }
}