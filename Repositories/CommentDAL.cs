using Cassandra;
using Cassandra.Mapping;
using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public class CommentDAL : ICommentDAL
{
    private readonly Cassandra.ISession _session;
    private readonly IMapper _mapper;

    public CommentDAL(ICassandraConnection cassandraConnection)
    {
        _session = cassandraConnection.GetCQLSession();
        _mapper = new Mapper(_session);
    }

    public async Task DeleteComment(Guid videoid, TimeUuid commentid)
    {
        await _mapper.DeleteAsync<Comment>("WHERE videoid=? AND commentid=?", videoid, commentid);
    }

    public async Task DeleteUserComment(Guid userid, TimeUuid commentid)
    {
        await _mapper.DeleteAsync<UserComment>("WHERE userid=? AND commentid=?", userid, commentid);
    }

    public async Task<Comment?> GetCommentById(TimeUuid commentid)
    {
        // better way to do this?
        var comment = await _mapper.SingleAsync<Comment>("WHERE commentid=? ALLOW FILTERING", commentid);
        return comment;
    }

    public async Task<IEnumerable<UserComment?>> GetCommentsByUserId(Guid userId)
    {
        var userComments = await _mapper.FetchAsync<UserComment>("WHERE userid=?", userId);
        return userComments;
    }

    public async Task<IEnumerable<Comment?>> GetCommentsByVideoId(Guid videoId, int limit)
    {
        var comments = await _mapper.FetchAsync<Comment>("WHERE videoid=? LIMIT ?", videoId, limit);
        return comments;
    }

    public Comment SaveComment(Comment comment)
    {
        _mapper.Insert(comment);
        return comment;
    }

    public Comment UpdateComment(Comment comment)
    {
        _mapper.Update(comment);
        return comment;
    }

    public UserComment SaveUserComment(UserComment comment)
    {
        _mapper.Insert(comment);
        return comment;
    }
    
    public UserComment UpdateUserComment(UserComment comment)
    {
        _mapper.Update(comment);
        return comment;
    }
}