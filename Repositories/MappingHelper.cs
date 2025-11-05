using Cassandra.Mapping;
using kv_be_csharp_dotnet_dataapi_collections.Models;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public class MappingHelper : Mappings
{
    public MappingHelper()
    {
        For<Video>()
            .TableName("videos")
            .PartitionKey("videoid")
            //.Column(v => v.videoId, cm => cm.WithName("videoid"))
            .Column(v => v.addedDate, cm => cm.WithName("added_date"))
            .Column(v => v.contentFeatures, cm => cm.WithName("content_features"))
            //.Column(v => v.description, cm => cm.WithName("description"))
            //.Column(v => v.location, cm => cm.WithName("location"))
            .Column(v => v.locationType, cm => cm.WithName("location_type"))
            //.Column(v => v.name, cm => cm.WithName("name"))
            .Column(v => v.processingStatus, cm => cm.WithName("processing_status"))
            .Column(v => v.previewImageLocation, cm => cm.WithName("preview_image_location"))
            //.Column(v => v.tags, cm => cm.WithName("tags"))
            //.Column(v => v.userId, cm => cm.WithName("userid"))
            .Column(v => v.videoVector, cm => cm.WithName("video_vector"))
            //.Column(v => v.views, cm => cm.WithName("views"));
            .Column(v => v.youtubeId, cm => cm.WithName("youtube_id"));
    }
}