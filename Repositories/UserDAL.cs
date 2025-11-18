using Cassandra.Mapping;
using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public class UserDAL : IUserDAL
{
    private readonly Cassandra.ISession _session;
    private readonly IMapper _mapper;

    public UserDAL(ICassandraConnection cassandraConnection)
    {
        _session = cassandraConnection.GetCQLSession();
        _mapper = new Mapper(_session);
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        var user = await _mapper.FirstOrDefaultAsync<User>("WHERE email=?", email);

        if (user is null)
        {
            return false;
        }
        return true;
    }

    public async Task<User?> FindByEmail(string email)
    {
        var user = await _mapper.SingleAsync<User>("WHERE email=?", email);
        return user;
    }

    public async Task<User?> FindByUserId(Guid userid)
    {
        var user = await _mapper.SingleAsync<User>("WHERE userid=?", userid);
        return user;
    }

    public User SaveUser(User user)
    {
        _mapper.Insert(user);
        return user;
    }

    public void UpdateUser(User user)
    {
        _mapper.Update(user);   
    }
}