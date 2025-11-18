namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class RatingResponse
{
    public List<RatingConversion> data { get; set; }
    public string averageRating { get; set; } = string.Empty;

    public RatingResponse(IEnumerable<Rating> ratings)
    {
        List<RatingConversion> dataResponse = new();
        float totalRating = 0;
        foreach (Rating rating in ratings)
        {
            RatingConversion localRating = new RatingConversion(rating);
            dataResponse.Add(localRating);
            totalRating += localRating.averageRating;
        }

        float averageRatingFlt = totalRating / ratings.Count();
        this.averageRating = averageRatingFlt.ToString("0.0");
        this.data = dataResponse;
    }
}

public class RatingConversion
{
    public Guid videoid { get; set; } = Guid.Empty;
    public float averageRating { get; set; } = 0f;
    public int ratingCount { get; set; } = 0;

    public RatingConversion(Rating rating)
    {
        this.videoid = rating.videoid;
        this.averageRating = rating.rating;
    }
}