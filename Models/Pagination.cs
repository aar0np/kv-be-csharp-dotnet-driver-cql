namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class Pagination
{
    public int totalPages { get; set; } = 0;
    public int pageSize { get; set; } = 0;
    public int totalItems { get; set; } = 0;
    public int currentPage { get; set; } = 0;

    public Pagination(int currentPage, int totalPages, int pageSize, int totalItems)
    {
        this.currentPage = currentPage;
        this.totalPages = totalPages;
        this.pageSize = pageSize;
        this.totalItems = totalItems;
    }
    
    public Pagination(int currentPage, int pageSize, int totalItems)
    {
        this.currentPage = currentPage;
        this.pageSize = pageSize;
        this.totalItems = totalItems;

        if (pageSize > 0) {
            this.totalPages = totalItems / pageSize;
        } else {
            this.totalPages = 1;
        }
    }
}