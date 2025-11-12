using Cassandra;
using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public interface ICommentDAL
{
    Comment SaveComment(Comment comment);
    Comment UpdateComment(Comment comment);
    Task<IEnumerable<Comment?>> GetCommentsByVideoId(Guid videoId);
    Task<IEnumerable<UserComment?>> GetCommentsByUserId(Guid userId);

}