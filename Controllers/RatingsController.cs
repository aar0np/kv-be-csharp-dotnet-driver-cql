using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using kv_be_csharp_dotnet_dataapi_collections.Repositories;
using kv_be_csharp_dotnet_dataapi_collections.Models;
using System.Security.Claims;

namespace kv_be_csharp_dotnet_dataapi_collections.Controllers;

[ApiController]
[Route("/api/v1/videos")]
[Produces("application/json")]
public class RatingsController : Controller
{
    //private readonly IVideoDAL _videoDAL;
    private readonly IRatingDAL _ratingDAL;

    public RatingsController(IRatingDAL ratingDAL)
    {
        //_videoDAL = videoDAL;
        _ratingDAL = ratingDAL;
    }

    [HttpPost("{videoid}/ratings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult> SubmitRating(Guid videoid, [FromBody] RatingRequest ratingRequest)
    {
        var userId = getUserIdFromAuth(HttpContext.User);

        // check for existing rating
        var existingRating = await _ratingDAL.FindByVideoIdAndUserId(videoid, userId);

        if (existingRating is not null)
        {
            // update existing rating
            existingRating.rating = ratingRequest.rating;
            existingRating.ratingDate = DateTimeOffset.Now;
            await _ratingDAL.Update(existingRating);
        }
        else
        {
            Rating rating = new Rating();
            rating.videoid = videoid;
            rating.userid = userId;
            rating.rating = ratingRequest.rating;
            await _ratingDAL.SaveRating(rating);
        }

        return Ok();
    }

    [HttpGet("{videoid}/ratings")]
    [ProducesResponseType(typeof(RatingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RatingResponse>> GetVideoRating(Guid videoid)
    {
        IEnumerable<Rating> ratings = await _ratingDAL.FindByVideoId(videoid);
        var response = new RatingResponse(ratings);
        return Ok(response);
    }

    [HttpGet("id/{videoid}/rating")]
    [ProducesResponseType(typeof(RatingSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RatingSummaryResponse>> GetAggregateVideoRating(Guid videoid)
    {
        IEnumerable<Rating> ratings = await _ratingDAL.FindByVideoId(videoid);
        RatingSummary summary = new();
        summary.videoid = videoid;

        if (ratings is null)
        {
            summary.averageRating = "0.0";
        }
        else
        {
            int ratingSum = 0;
            int ratingCount = 0;
            foreach (Rating rating in ratings)
            {
                ratingSum += rating.rating;
                ratingCount++;
            }

            if (ratingCount == 0)
            {
                summary.averageRating = "0.0";
            }
            else
            {
                summary.averageRating = (ratingSum / ratingCount).ToString("0.0");
                summary.ratingCount = ratingCount;
            }
        }

        return Ok(new RatingSummaryResponse(summary));
    }

    [HttpGet("{videoid}/ratings/user/{userid}")]
    [ProducesResponseType(typeof(RatingSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RatingSummaryResponse>> GetUserRating(Guid videoid, Guid userid)
    {
        var userRating = await _ratingDAL.FindByVideoIdAndUserId(videoid, userid);

        RatingSummary summary = new();
        summary.videoid = videoid;

        if (userRating is not null)
        {
            summary.ratingCount = 1;
            summary.averageRating = userRating.rating.ToString() + ".0";
            summary.currentUserRating = userRating.rating;
        }
        else
        {
            summary.averageRating = "0.0";
        }

        return Ok(new RatingSummaryResponse(summary));
    }

    private Guid getUserIdFromAuth(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is not null)
        {
            return Guid.Parse(userIdClaim.Value);
        }

        return Guid.Empty;
    }
}