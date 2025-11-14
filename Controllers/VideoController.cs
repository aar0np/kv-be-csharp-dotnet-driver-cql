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
    private static readonly string _HF_IPEPE_SPACE_ENDPOINT = "https://ipepe-nomic-embeddings.hf.space/embed";
    private HttpClient _hFhttpClient;

    private readonly IVideoDAL _videoDAL;
    private readonly ILatestVideosDAL _latestVideosDAL;
    private readonly ICommentDAL _commentDAL;

    public VideosController(IVideoDAL videoDAL, ILatestVideosDAL latestVideosDAL, ICommentDAL commentDAL)
    {
        // videoDAL instantiation
        _videoDAL = videoDAL;
        _latestVideosDAL = latestVideosDAL;
        _commentDAL = commentDAL;

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
    public async Task<ActionResult<VideoResponse>> SubmitVideo(VideoSubmitRequest submitRequest) {
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
            var hFRequestMessage = new HttpRequestMessage(HttpMethod.Post, _HF_IPEPE_SPACE_ENDPOINT)
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

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Video), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Video>> GetVideo(string id)
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

        //Console.WriteLine(video);

        return Ok(video);
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
    public async Task<ActionResult<List<VideoResponse>>> GetLatestVideos(int page, int pageSize)
    {
        if (page <= 0 || pageSize <= 0 || pageSize > 100)
        {
            pageSize = 10;
        }

        LocalDate today = LocalDate.Parse(DateTimeOffset.Now.Date.ToString("yyyy-MM-dd"));

        var latestVideos = await _latestVideosDAL.GetLatestVideosToday(today, pageSize);
        
        if (!latestVideos.Any())
        {
            // latestVideos is empty for today - try again with only the LIMIT
            latestVideos = await _latestVideosDAL.GetLatestVideos(pageSize);
        }

        List<VideoResponse> response = new();

        foreach (LatestVideo video in latestVideos)
        {
            response.Add(VideoResponse.fromLatestVideo(video));
        }

        return response;
    }

    [HttpGet("id/{id}/related")]
    [ProducesResponseType(typeof(VideoResponse), StatusCodes.Status200OK)]
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
                response.Add(VideoResponse.fromVideo(video));
            }
        }

        return response;
    }

    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(VideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult<CommentResponse>> SubmitComment(Guid id, [FromBody] CommentSubmitRequest req)
    {
        Guid userId = getUserIdFromAuth(HttpContext.User);

        Comment comment = new Comment();
        comment.videoid = id;
        comment.comment = req.commentText;

        _commentDAL.SaveComment(comment);

        return CommentResponse.fromComment(comment);
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