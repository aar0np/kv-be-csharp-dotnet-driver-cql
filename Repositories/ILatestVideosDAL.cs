using Cassandra;
using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public interface ILatestVideosDAL
{
    LatestVideo SaveLatestVideo(LatestVideo video);

    Task<IEnumerable<LatestVideo>> GetLatestVideosToday(LocalDate day, int limit);
    Task<IEnumerable<LatestVideo>> GetLatestVideos(int limit);
}