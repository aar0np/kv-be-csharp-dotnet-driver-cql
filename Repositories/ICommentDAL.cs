using Cassandra;
using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public interface ICommentDAL
{
    Comment SaveComment(Comment comment);
    Comment UpdateComment(Comment comment);
    UserComment SaveUserComment(UserComment comment);
    UserComment UpdateUserComment(UserComment comment);
    Task<Comment?> GetCommentById(TimeUuid commentid);
    Task<IEnumerable<Comment?>> GetCommentsByVideoId(Guid videoId, int limit);
    Task<IEnumerable<UserComment?>> GetCommentsByUserId(Guid userId);
    Task DeleteComment(Guid videoid, TimeUuid commentid);
    Task DeleteUserComment(Guid userid, TimeUuid commentid);
}