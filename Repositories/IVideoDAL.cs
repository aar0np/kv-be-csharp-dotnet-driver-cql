using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;
public interface IVideoDAL
{
    Video SaveVideo(Video video);
    
    Task<Video?> GetVideoByVideoId(Guid videoId);
}