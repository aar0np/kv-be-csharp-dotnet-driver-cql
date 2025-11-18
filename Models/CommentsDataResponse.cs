namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class CommentsDataResponse
{
    public List<CommentResponse> data { get; set; }
    public Pagination pagination { get; set; }

    public CommentsDataResponse(List<CommentResponse> data)
    {
        this.data = data;
        this.pagination = new Pagination(1, data.Count, data.Count);
    }
    
    public CommentsDataResponse(List<CommentResponse> data, Pagination pagination)
    {
        this.data = data;
        this.pagination = pagination;
    }

    public CommentsDataResponse(List<CommentResponse> data, int pages, int pageSize, int totalItems)
    {
        this.data = data;
        this.pagination = new Pagination(pages, pageSize, totalItems);
    }
}