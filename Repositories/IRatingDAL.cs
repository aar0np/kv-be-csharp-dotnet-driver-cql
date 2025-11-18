using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public interface IRatingDAL
{
    Task<Rating> SaveRating(Rating rating);
    Task<Rating?> FindByVideoIdAndUserId(Guid videoid, Guid userid);
    Task<IEnumerable<Rating?>> FindByVideoId(Guid videoid);
    Task Update(Rating rating);
}