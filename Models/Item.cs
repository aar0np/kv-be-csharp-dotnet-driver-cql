namespace kv_be_csharp_dotnet_dataapi_collections.Models;

public class Item
{
    public Snippet snippet { get; set; } = new();
}

public class Snippet
{
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public HashSet<string> tags { get; set; } = new();
}