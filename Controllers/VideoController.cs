using Cassandra;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text.Json;

using kv_be_csharp_dotnet_dataapi_collections.Models;
using kv_be_csharp_dotnet_dataapi_collections.Repositories;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace kv_be_csharp_dotnet_dataapi_collections.Controllers;

[ApiController]
[Route("/api/v1/videos")]
[Produces("application/json")]
public class VideosController : Controller
{
    private List<string> _YOUTUBE_PATTERNS = new List<string>();
    private string? _YOUTUBE_API_KEY = System.Environment.GetEnvironmentVariable("YOUTUBE_API_KEY");
    private string? _HF_API_KEY = System.Environment.GetEnvironmentVariable("HF_API_KEY");
    private static readonly string _YOUTUBE_API_URL = "https://www.googleapis.com/youtube/v3/videos?part=snippet&id={YOUTUBE_ID}&key={API_KEY}";
    private static readonly string _modelId = "ibm-granite/granite-embedding-30m-english";
    
    // https://huggingface.co/spaces/ipepe/nomic-embeddings
    // private static readonly string _HF_IPEPE_SPACE_ENDPOINT = "https://ipepe-nomic-embeddings.hf.space/embed";
    // https://huggingface.co/spaces/aploetz/granite-embeddings
    private static readonly string _HF_APLOETZ_SPACE_ENDPOINT = "https://aploetz-granite-embeddings.hf.space/embed";
    private HttpClient _hFhttpClient;

    private readonly IVideoDAL _videoDAL;
    private readonly ILatestVideosDAL _latestVideosDAL;
    private readonly ICommentDAL _commentDAL;
    private readonly IUserDAL _userDAL;
    private readonly IRatingDAL _ratingDAL;

    public VideosController(IVideoDAL videoDAL, ILatestVideosDAL latestVideosDAL,
     ICommentDAL commentDAL, IUserDAL userDAL, IRatingDAL ratingDAL)
    {
        // videoDAL instantiation
        _videoDAL = videoDAL;
        _latestVideosDAL = latestVideosDAL;
        _commentDAL = commentDAL;
        _userDAL = userDAL;
        _ratingDAL = ratingDAL;

        // YouTube regex patterns
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtu\\.be/(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/watch\\?v=(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/embed/(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/v/(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/shorts/(?<id>[A-Za-z0-9_-]{11})");

        // check YouTube API KEY from env var
        if (string.IsNullOrEmpty(_YOUTUBE_API_KEY))
        {
            Console.WriteLine("ERROR: YOUTUBE_API_KEY must be defined as an environment variable.");
        }

        // check HuggingFace API KEY from env var
        if (string.IsNullOrEmpty(_HF_API_KEY))
        {
            Console.WriteLine("ERROR: HF_API_KEY must be defined as an environment variable.");
        }

        // define HTTP client to hit HuggingFace embedding model
        _hFhttpClient = new HttpClient();
    }

    [HttpPost]
    [ProducesResponseType(typeof(VideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult<VideoResponse>> SubmitVideo(VideoSubmitRequest submitRequest)
    {
        try
        {
            var userId = getUserIdFromAuth(HttpContext.User);

            // Create video with properties from request
            Video video = new Video();

            video.description = submitRequest.description;
            video.tags = submitRequest.tags;
            video.location = submitRequest.youtubeUrl;

            // parse youtube id from url
            string youtubeId = extractYouTubeId(submitRequest.youtubeUrl);
            //video.youtubeId = youtubeId;

            // fetch youtube metadata
            YoutubeMetadata? youtubeMetadata = await fetchYoutubeMetadata(youtubeId);

            if (youtubeMetadata is not null)
            { 
                video.name = youtubeMetadata.title;
                video.previewImageLocation = youtubeMetadata.thumbnailUrl;
            }

            // generate remaining properties
            //video.processingStatus = "PENDING";
            video.videoId = Guid.NewGuid();
            video.userId = userId;

            // Generate the embedding for the video
            var req = new HuggingFaceRequest();
            req.text = video.name;
            req.model = _modelId;

            var json = JsonConvert.SerializeObject(req);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var hFRequestMessage = new HttpRequestMessage(HttpMethod.Post, _HF_APLOETZ_SPACE_ENDPOINT)
            {
                Content = data
            };
            HttpResponseMessage hFResponse = await _hFhttpClient.SendAsync(hFRequestMessage);
            string jsonResponse = await hFResponse.Content.ReadAsStringAsync();
            HuggingFaceResponse hFResp = JsonConvert.DeserializeObject<HuggingFaceResponse>(jsonResponse);

            //float[] videoVector = embeddingModel.embed(videoText).content().vector();
            video.contentFeatures = (CqlVector<float>)hFResp.embedding;

            // save video to two tables in database
            Video savedVideo = _videoDAL.SaveVideo(video);
            LatestVideo latestVideo = _latestVideosDAL.SaveLatestVideo(LatestVideo.fromVideo(video));
            
            VideoResponse response = VideoResponse.fromVideo(savedVideo);
            response.processingStatus = "PENDING";
            
            Console.WriteLine("Video submitted successfully. ID: " + savedVideo.videoId);
            return response;

        } catch (Exception e) {
            Console.WriteLine("Error processing video submission: " + e.GetBaseException());
            return NotFound("Error processing video submission");
        }
    }

    [HttpGet("id/{id}")]
    [ProducesResponseType(typeof(VideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VideoResponse>> GetVideo(string id)
    {
        Guid videoid = Guid.Parse(id);
        var video = await _videoDAL.GetVideoByVideoId(videoid);

        if (video == null)
        {
            return NotFound($"Video with ID " + id + " returned null.");
        }
        else if (string.IsNullOrEmpty(video.name))
        {
            return NotFound($"Video with ID " + id + " not found.");
        }

        // make sure that we have a YouTubeID
        if (string.IsNullOrEmpty(video.youtubeId) && !string.IsNullOrEmpty(video.location))
        {
            video.youtubeId = extractYouTubeId(video.location);
        }

        return Ok(VideoResponse.fromVideo(video));
    }

    [HttpPut("id/{id}")]
    [ProducesResponseType(typeof(VideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult<VideoResponse>> UpdateVideo(string id, VideoUpdateRequest videoUpdateRequest)
    {
        Guid videoid = Guid.Parse(id);
        var video = await _videoDAL.GetVideoByVideoId(videoid);

        if (video == null)
        {
            return NotFound($"Video with ID " + id + " returned null.");
        }
        else if (string.IsNullOrEmpty(video.name))
        {
            return NotFound($"Video with ID " + id + " not found.");
        }

        if (!string.IsNullOrEmpty(videoUpdateRequest.name))
        {
            video.name = videoUpdateRequest.name;
        }

        if (!string.IsNullOrEmpty(videoUpdateRequest.description))
        {
            video.description = videoUpdateRequest.description;
        }

        if (videoUpdateRequest.tags is not null && videoUpdateRequest.tags.Count > 0)
        {
            video.tags = videoUpdateRequest.tags;
        }

        _videoDAL.UpdateVideo(video);

        return VideoResponse.fromVideo(video);
    }

    [HttpPost("id/{id}/view")]
    [ProducesResponseType(typeof(VideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VideoResponse>> RecordVideoView(string id)
    {
        Guid videoid = Guid.Parse(id);
        var video = await _videoDAL.GetVideoByVideoId(videoid);

        if (video == null)
        {
            return NotFound($"Video with ID " + id + " returned null.");
        }
        else if (string.IsNullOrEmpty(video.name))
        {
            return NotFound($"Video with ID " + id + " not found.");
        }

        int views = video.views + 1;
        video.views = views;

        _videoDAL.UpdateVideo(video);

        return VideoResponse.fromVideo(video);
    }

    [HttpGet("latest")]
    [ProducesResponseType(typeof(VideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LatestVideosResponse>> GetLatestVideos(int page, int pageSize)
    {
        if (page <= 0 || pageSize <= 0 || pageSize > 100)
        {
            pageSize = 10;
        }

        LocalDate today = LocalDate.Parse(DateTimeOffset.Now.Date.ToString("yyyy-MM-dd"));

        var latestVideos = await _latestVideosDAL.GetLatestVideosToday(today, pageSize);
        //latestVideos.TryGetNonEnumeratedCount(out int count);
        var latestVideosList = latestVideos.ToList();
        int count = latestVideosList.Count;

        if (count < pageSize)
        {
            Console.WriteLine(count + " latestVideos returned for " + today);            
            var additionalVideos = await _latestVideosDAL.GetLatestVideos(pageSize - count);

            // combine latestVideosList and additionalVideos for processing
            latestVideos = latestVideosList.Concat(additionalVideos);
        }

        List<VideoResponse> response = new();
        
        foreach (LatestVideo video in latestVideos)
        {
            VideoResponse videoResponse = VideoResponse.fromLatestVideo(video);

            // get views for video
            var videoData = await _videoDAL.GetVideoByVideoId(video.videoId);

            videoResponse.views = videoData.views;

            // Get all ratings for the video
            var ratings = await _ratingDAL.FindByVideoId(video.videoId);

            if (ratings is not null)
            {
                int ratingCount = 0;
                int totalRating = 0;
                foreach (var rating in ratings)
                {
                    totalRating += rating.rating;
                    ratingCount++;
                }

                if (ratingCount > 0)
                {
                    videoResponse.averageRating = totalRating / ratingCount;
                }
                else
                {
                    videoResponse.averageRating = 0.0f;
                }
            }
            else
            {
                videoResponse.averageRating = 0.0f;
            }

            response.Add(videoResponse);
        }

        return new LatestVideosResponse(response);
    }

    [HttpGet("id/{id}/related")]
    [ProducesResponseType(typeof(List<VideoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<VideoResponse>>> GetSimilarVideos(string id, int requestedLimit)
    {
        // if requestedLimit is an invalid number, default it to 5
        int limit = requestedLimit <= 0 || requestedLimit > 20 ? 5 : requestedLimit;
        Guid videoid = Guid.Parse(id);

        var originalVideo = await _videoDAL.GetVideoByVideoId(videoid);

        if (originalVideo is null)
        {
            return NotFound($"Video with ID " + id + " not found.");
        }

        if (originalVideo.contentFeatures is null)
        {
            return NotFound($"Video with ID " + id + " does not have a valid vector for content_features.");
        }

        var similarVideos = await _videoDAL.GetByVector(originalVideo.contentFeatures, limit + 1);
        List<VideoResponse> response = new();

        foreach (Video video in similarVideos)
        {
            if (!video.videoId.Equals(videoid))
            {
                // Get views for the video
                var videoData = await _videoDAL.GetVideoByVideoId(video.videoId);

                // Get all ratings for the video
                var ratings = await _ratingDAL.FindByVideoId(video.videoId);

                int ratingCount = 0;
                int totalRating = 0;
                
                foreach (var rating in ratings) {
                    ratingCount++;
                    totalRating += rating.rating;
                }

                VideoResponse videoResponse = VideoResponse.fromVideo(video);

                videoResponse.views = videoData.views;

                if (ratingCount > 0)
                {
                    videoResponse.averageRating = totalRating / ratingCount;
                }
                else
                {
                    videoResponse.averageRating = 0.0f;
                }

                response.Add(videoResponse);
            }
        }

        return response;
    }

    [HttpPost("{videoid}/comments")]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult<CommentResponse>> SubmitComment(Guid videoid, [FromBody] CommentSubmitRequest req)
    {
        Guid userId = getUserIdFromAuth(HttpContext.User);

        Comment comment = new Comment();
        comment.videoid = videoid;
        comment.comment = req.text;
        comment.userid = userId;

        _commentDAL.SaveComment(comment);
        _commentDAL.SaveUserComment(UserComment.fromComment(comment));

        return CommentResponse.fromComment(comment);
    }

    [HttpGet("{videoid}/comments")]
    [ProducesResponseType(typeof(CommentsDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CommentsDataResponse>>> GetCommentsForVideo(Guid videoid,
            int page, int pageSize)
    {
        // rudimentary limit calculation, for now
        int limit = pageSize * page;
        
        var comments = await _commentDAL.GetCommentsByVideoId(videoid, limit);
        List<CommentResponse> response = new ();

        foreach (var comment in comments)
        {
            var poster = await _userDAL.FindByUserId(comment.userid);
            var commentResp = CommentResponse.fromComment(comment);

            if (poster is null)
            {
                commentResp.user_name = "anonymous user";
            }
            else
            {
                if (!string.IsNullOrEmpty(poster.firstname))
                {
                    // firstname exists
                    if (!string.IsNullOrEmpty(poster.lastname))
                    {
                        // lastname exists
                        commentResp.user_name = poster.firstname + " " + poster.lastname;
                    }
                    else
                    {
                        // lastname null
                        commentResp.user_name = poster.firstname;
                    }
                }
                else
                {
                    // firstname null
                    if (!string.IsNullOrEmpty(poster.lastname))
                    {
                        // lastname exists
                        commentResp.user_name = poster.lastname;
                    }
                    else
                    {
                        // firstname and lastname are both null
                        commentResp.user_name = "anonymous user";
                    }
                }
            }

            response.Add(commentResp);
        }

        return Ok(new CommentsDataResponse(response));
    }

    [HttpDelete("comment/{commentid}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(TimeUuid commentid)
    {
        // make sure that the comment exists
        var comment = await _commentDAL.GetCommentById(commentid);

        if (comment is not null)
        {
            // future - check for ADMIN role

            // delete comment
            await _commentDAL.DeleteComment(comment.videoid, commentid);
            await _commentDAL.DeleteUserComment(comment.userid, commentid);
        }

        return BadRequest("Comment does not exist.");
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
    
    private string extractYouTubeId(string youtubeUrl)
    {

        foreach (string pattern in _YOUTUBE_PATTERNS)
        {
            MatchCollection matches = Regex.Matches(youtubeUrl, pattern);

            if (matches.Any())
            {
                Match match = matches.First();
                GroupCollection group = match.Groups;
                return group["id"].ToString();
            }
        }
        return string.Empty;
    }
    
    private async Task<YoutubeMetadata?> fetchYoutubeMetadata(String youtubeId) {
        try {
            HttpClient httpClient = new HttpClient();

            String url = _YOUTUBE_API_URL
                    .Replace("{YOUTUBE_ID}", youtubeId)
                    .Replace("{API_KEY}", _YOUTUBE_API_KEY);

            var json = await httpClient.GetStringAsync(url);

            if (json == null)
            {
                Console.WriteLine("Received null response from YouTube API for ID: " + youtubeId);
                return null;
            }
            else if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine("Received invalid response from YouTube API for ID: " + youtubeId);
                return null;
            }
            
            //List<Item> youtubeResponse = JsonSerializer.Deserialize<List<Item>>(json);
            using var resp = JsonDocument.Parse(json);
            var items = resp.RootElement.GetProperty("items");
            var videoInfo = items[0];
            var snippet = videoInfo.GetProperty("snippet");

            YoutubeMetadata metadata = new YoutubeMetadata();

            // Extract title
            if (snippet.TryGetProperty("title", out var title)) {
                metadata.title = title.GetString();
            }
            // Extract description
            if (snippet.TryGetProperty("description", out var desc)) {
                metadata.description = desc.GetString();
            }
            
            // Extract thumbnail URL (prefer high quality, fallback to default)
            if (snippet.TryGetProperty("thumbnails", out var thumbnails)) {
                if (thumbnails.TryGetProperty("high", out var thumbH)) {
                    metadata.thumbnailUrl = thumbH.GetProperty("url").GetString();
                } else if (thumbnails.TryGetProperty("medium", out var thumbM)) {
                    metadata.thumbnailUrl = thumbM.GetProperty("url").GetString();
                } else if (thumbnails.TryGetProperty("default", out var thumbD)) {
                    metadata.thumbnailUrl = thumbD.GetProperty("url").GetString();
                }
            }
            
            // Extract tags
            if (snippet.TryGetProperty("tags", out var tags)) {
                var tagsList = new HashSet<String>();
                foreach (var tag in tags.EnumerateArray())
                {
                    if (!string.IsNullOrEmpty(tag.GetString()))
                    {
                        tagsList.Add(tag.GetString());
                    }
                }
                
                metadata.tags = tagsList;
            }

            Console.WriteLine("Successfully fetched YouTube metadata for ID: " + youtubeId);
            return metadata;

        } catch (Exception e) {
            Console.WriteLine("Error fetching YouTube metadata for ID " + youtubeId + " : " + e.GetBaseException());
            return null;
        }
    }
}