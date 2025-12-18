using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace kv_be_csharp_dotnet_dataapi_collections.Repositories;

public class AstraHelperService : IAstraHelperService
{
    private readonly HttpClient _httpClient;
    private readonly string? _astraDbEndPoint;
    private readonly string? _astraDbApplicationToken;
    private readonly string? _astraDbNamespace;
    private readonly string? _baseUrl;

    public AstraHelperService(HttpClient httpClient)
    {
        // get env vars
        _astraDbEndPoint = System.Environment.GetEnvironmentVariable("ASTRA_DB_API_ENDPOINT");
        _astraDbApplicationToken = System.Environment.GetEnvironmentVariable("ASTRA_DB_APPLICATION_TOKEN");
        _astraDbNamespace = System.Environment.GetEnvironmentVariable("ASTRA_DB_NAMESPACE");

        // check each for null
        if (string.IsNullOrEmpty(_astraDbEndPoint))
        {
            Console.WriteLine("ERROR: ASTRA_DB_API_ENDPOINT must be defined as an environment variable.");
        }

        if (string.IsNullOrEmpty(_astraDbApplicationToken))
        {
            Console.WriteLine("ERROR: ASTRA_DB_APPLICATION_TOKEN must be defined as an environment variable.");
        }

        if (string.IsNullOrEmpty(_astraDbNamespace))
        {
            Console.WriteLine("ERROR: ASTRA_DB_NAMESPACE must be defined as an environment variable.");
        }

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _httpClient = httpClient;
        _baseUrl = _astraDbEndPoint + "/api/json/v1/";
    }

    public async Task<string?> PostDataAsyncAstra(string table, string query)
    {
        string uri = _astraDbNamespace + "/" + table;
        var json = JsonConvert.SerializeObject(query);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseUrl + uri)
        {
            Content = data
        };
        requestMessage.Headers.Add("Token", _astraDbApplicationToken);

        Console.WriteLine("request = " + requestMessage.Content);

        var response = await _httpClient.SendAsync(requestMessage);

        if (response.IsSuccessStatusCode)
        {
            return response.Content.ReadAsStringAsync().Result;
        }
        else
        {
            string message = "ERROR communicating with Astra Data API: " + response.StatusCode;
            Console.WriteLine("StatusCode = " + response.StatusCode);
            return "{ \"description\": \"" + message + "\" }";
        }
    }
    
    public async Task<string?> FindByKeyValue(string table, string key, string value)
    {
        string query = "{ \"find\": { \"filter\": { \"" + key + "\": \"" + value + "\" } } }";
        string? result = await PostDataAsyncAstra(table, query);

        return result;
    }
}