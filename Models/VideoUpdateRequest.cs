public class VideoUpdateRequest
{
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public HashSet<string> tags { get; set; } = new();
}