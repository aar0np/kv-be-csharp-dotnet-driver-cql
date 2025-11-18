namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class RatingSummaryResponse
{
    public RatingSummaryConversion data { get; set; }
    public float averageRating { get; set; } = 0f;
    public float currentUserRating { get; set; } = 0f;

    public RatingSummaryResponse(RatingSummary data)
    {
        this.data = new RatingSummaryConversion(data);
        this.averageRating = float.Parse(data.averageRating);
    }
}

public class RatingSummaryConversion
{
    public Guid videoid { get; set; } = Guid.Empty;
    public float averageRating { get; set; } = 0f;
    public int ratingCount { get; set; } = 0;

    public RatingSummaryConversion(RatingSummary rating)
    {
        this.videoid = rating.videoid;
        this.averageRating = float.Parse(rating.averageRating);
        this.ratingCount = rating.ratingCount;
    }
}