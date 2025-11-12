using Cassandra;
using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;
public interface IVideoDAL
{
    Task<Video?> GetVideoByVideoId(Guid videoId);

    Video SaveVideo(Video video);

    void UpdateVideo(Video video);

    Task<IEnumerable<Video>> GetByVector(CqlVector<float> vector, int limit);
}