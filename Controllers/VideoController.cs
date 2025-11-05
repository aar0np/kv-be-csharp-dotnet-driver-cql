using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text.Json;

//using Microsoft.Extensions.AI;
//using HuggingFace;

using kv_be_csharp_dotnet_dataapi_collections.Models;
using kv_be_csharp_dotnet_dataapi_collections.Repositories;

namespace kv_be_csharp_dotnet_dataapi_collections.Controllers;

[ApiController]
[Route("/api/v1/videos")]
[Produces("application/json")]
public class VideosController : Controller
{
    private List<string> _YOUTUBE_PATTERNS = new List<string>();
    private string? _YOUTUBE_API_KEY = System.Environment.GetEnvironmentVariable("YOUTUBE_API_KEY");
    private static readonly string _YOUTUBE_API_URL = "https://www.googleapis.com/youtube/v3/videos?part=snippet&id={YOUTUBE_ID}&key={API_KEY}";

//    private readonly Embedding embeddingModel;

    private readonly IVideoDAL _videoDAL;

    public VideosController(IVideoDAL videoDAL)
    {
        _videoDAL = videoDAL;

        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtu\\.be/(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/watch\\?v=(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/embed/(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/v/(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/shorts/(?<id>[A-Za-z0-9_-]{11})");

        if (string.IsNullOrEmpty(_YOUTUBE_API_KEY))
        {
            Console.WriteLine("ERROR: YOUTUBE_API_KEY must be defined as an environment variable.");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(VideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VideoResponse>> submitVideo(VideoSubmitRequest submitRequest) {

        try
        {
            // Create video with properties from request
            Video video = new Video();

            video.description = submitRequest.description;
            video.tags = submitRequest.tags;
            video.location = submitRequest.youtubeUrl;

            // parse youtube id from url
            string youtubeId = extractYouTubeId(submitRequest.youtubeUrl);
            video.youtubeId = youtubeId;

            // fetch youtube metadata
            YoutubeMetadata? youtubeMetadata = await fetchYoutubeMetadata(youtubeId);

            //logger.info("Youtube metadata.title: {}", youtubeMetadata.getTitle());
            //logger.info("Youtube metadata.thumbnailUrl: {}", youtubeMetadata.getThumbnailUrl());

            if (youtubeMetadata is not null)
            { 
                video.name = youtubeMetadata.title;
                video.previewImageLocation = youtubeMetadata.thumbnailUrl;
            }

            // generate remaining properties
            video.processingStatus = "PENDING";
            video.videoId = Guid.NewGuid();
            video.userId = submitRequest.userId;
            
            // Generate the embedding for the video
            String videoText = video.name;
            //float[] videoVector = embeddingModel.embed(videoText).content().vector();
            //video.videoVector = videoVector;

            // save video to database
            Video savedVideo = _videoDAL.SaveVideo(video);
            
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
        //var video = await _videoSvc.GetVideoByVideoIdAsync(id);
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

            Console.WriteLine("Fetching YouTube metadata for ID: " + youtubeId);

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
                    metadata.thumbnailUrl = thumbH.GetProperty("high").GetProperty("url").GetString();
                } else if (thumbnails.TryGetProperty("medium", out var thumbM)) {
                    metadata.thumbnailUrl = thumbM.GetProperty("medium").GetProperty("url").GetString();
                } else if (thumbnails.TryGetProperty("default", out var thumbD)) {
                    metadata.thumbnailUrl = thumbD.GetProperty("default").GetProperty("url").GetString();
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