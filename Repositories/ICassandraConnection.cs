using Cassandra;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public interface ICassandraConnection
{
    Cassandra.ISession GetCQLSession();
}