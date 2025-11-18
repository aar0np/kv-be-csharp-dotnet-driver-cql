using Cassandra;

namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class CommentResponse
{
    public Guid videoid { get; set; } = Guid.Empty;
    public TimeUuid commentid { get; set; } = TimeUuid.NewId();
    public string comment { get; set; } = string.Empty;
    public Guid userid { get; set; } = Guid.Empty;
    public float sentiment_score { get; set; } = 0.0F;
    public string user_name { get; set; } = string.Empty;
    public DateTimeOffset timestamp { get; set; } = DateTimeOffset.Now;

    public static CommentResponse fromComment(Comment comment)
    {
        CommentResponse response = new CommentResponse();
        response.videoid = comment.videoid;
        response.comment = comment.comment;
        response.commentid = comment.commentid;
        response.userid = comment.userid;
        response.timestamp = comment.commentid.GetDate();

        string strUserid = comment.userid.ToString();
        int firstDash = strUserid.IndexOf('-');
        response.user_name = strUserid[..firstDash];

        return response;
    }
}