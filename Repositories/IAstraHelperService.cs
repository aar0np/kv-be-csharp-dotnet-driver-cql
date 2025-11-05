using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public interface IAstraHelperService
{
    Task<string?> PostDataAsyncAstra(string table, string query);

    Task<string?> FindByKeyValue(string table, string key, string value);
}