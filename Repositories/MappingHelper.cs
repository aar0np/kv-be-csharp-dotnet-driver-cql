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
            .Column(v => v.addedDate, cm => cm.WithName("added_date"))
            .Column(v => v.contentFeatures, cm => cm.WithName("content_features"))
            .Column(v => v.locationType, cm => cm.WithName("location_type"))
            //.Column(v => v.processingStatus, cm => cm.WithName("processing_status"))
            .Column(v => v.previewImageLocation, cm => cm.WithName("preview_image_location"))
            .Column(v => v.youtubeId, cm => cm.WithName("youtube_id"))
            .Column(v => v.contentRating, cm => cm.WithName("content_rating"));
        For<LatestVideo>()
            .TableName("latest_videos")
            .PartitionKey("day")
            .ClusteringKey("added_date", "videoid")
            .Column(lv => lv.addedDate, cm => cm.WithName("added_date"))
            .Column(lv => lv.previewImageLocation, cm => cm.WithName("preview_image_location"))//;
            .Column(lv => lv.contentRating, cm => cm.WithName("content_rating"));
        For<Comment>()
            .TableName("comments")
            .PartitionKey("videoid")
            .ClusteringKey("commentid")
            .Column(c => c.sentimentScore, cm => cm.WithName("sentiment_score"));
        For<UserComment>()
            .TableName("comments_by_user")
            .PartitionKey("userid")
            .ClusteringKey("commentid")
            .Column(c => c.sentimentScore, cm => cm.WithName("sentiment_score"));
        For<User>()
            .TableName("users")
            .PartitionKey("userid")
            .Column(u => u.accountStatus, cm => cm.WithName("account_status"))
            .Column(u => u.createdDate, cm => cm.WithName("created_date"))
            .Column(u => u.lastLoginDate, cm => cm.WithName("last_login_date"));
        For<UserCredentials>()
            .TableName("user_credentials")
            .PartitionKey("email")
            .Column(uc => uc.accountLocked, cm => cm.WithName("account_locked"));
        For<Rating>()
            .TableName("video_ratings_by_user")
            .PartitionKey("videoid")
            .ClusteringKey("userid")
            .Column(r => r.ratingDate, cm => cm.WithName("rating_date"));
    }
}