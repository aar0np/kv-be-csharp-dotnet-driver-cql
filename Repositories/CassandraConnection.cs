using Cassandra;
using Cassandra.Mapping;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public class CassandraConnection : ICassandraConnection
{
    private readonly string? _astraDbApplicationToken;
    private readonly string? _astraDbKeyspace;
    private readonly string? _secureBundleLocation;

    public CassandraConnection()
    {
        _astraDbApplicationToken = System.Environment.GetEnvironmentVariable("ASTRA_DB_APPLICATION_TOKEN");
        _astraDbKeyspace = System.Environment.GetEnvironmentVariable("ASTRA_DB_KEYSPACE");
        _secureBundleLocation = System.Environment.GetEnvironmentVariable("ASTRA_DB_SECURE_BUNDLE_LOCATION");
        MappingConfiguration.Global.Define<MappingHelper>();
    }

    public Cassandra.ISession GetCQLSession()
    {
        Cassandra.ISession session =
            Cluster.Builder()
                   .WithCloudSecureConnectionBundle(_secureBundleLocation)
                   .WithCredentials("token", _astraDbApplicationToken)
                   .WithDefaultKeyspace(_astraDbKeyspace)
                   .Build()
                   .Connect();

        return session;
    }
}