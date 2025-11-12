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
    
    public async Task<IEnumerable<UserComment?>> GetCommentsByUserId(Guid userId)
    {
        var userComments = await _mapper.FetchAsync<UserComment>("WHERE userid=?", userId);
        return userComments;
    }

    public async Task<IEnumerable<Comment?>> GetCommentsByVideoId(Guid videoId)
    {
        var comments = await _mapper.FetchAsync<Comment>("WHERE videoid=?", videoId);
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
}