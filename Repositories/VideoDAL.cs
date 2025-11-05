using Cassandra.Mapping;
using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public class VideoDAL : IVideoDAL
{
    private readonly Cassandra.ISession _session;
    private readonly IMapper _mapper;

    public VideoDAL(ICassandraConnection cassandraConnection)
    {
        _session = cassandraConnection.GetCQLSession();
        MappingConfiguration.Global.Define<MappingHelper>();
        _mapper = new Mapper(_session);
    }

    public Video SaveVideo(Video video)
    {
        _mapper.Insert(video);

        return video;
    }
    
    public async Task<Video?> GetVideoByVideoId(Guid videoId)
    {
        Video video = await _mapper.SingleAsync<Video>("WHERE videoid=?", videoId);

        return video;
    }
}