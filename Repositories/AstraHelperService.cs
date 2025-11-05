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

    // ASTRA_DB_API_ENDPOINT="https://cab00884-not-real-fb25536-us-east1.apps.astra.datastax.com"
    // ASTRA_DB_APPLICATION_TOKEN=AstraTOKEN:nNotRealTefrhIQa:8c435dc8b3eb41af1NotReal4dd8ba2b70bdfd7f912d96a2fa969
    // ASTRA_DB_NAMESPACE=method param
    // TABLE=method param
    // QUERY=method param - '{ "find": { "filter": { "video_id": "900c1236-55ae-4f05-a7fb-d566d603a2ae" } } }'
    //
    // curl -sS -L 
    // -X POST "https://$ASTRA_DB_API_ENDPOINT/$KEYSPACE/$TABLE"
    // --header "Token: $ASTRA_DB_APPLICATION_TOKEN"
    // --header "Content-Type: application/json" 
    // --data $QUERY

    public AstraHelperService(HttpClient httpClient)
    {
        // get env vars
        _astraDbEndPoint = System.Environment.GetEnvironmentVariable("ASTRA_DB_API_ENDPOINT");
        _astraDbApplicationToken = System.Environment.GetEnvironmentVariable("ASTRA_DB_APPLICATION_TOKEN");
        _astraDbNamespace = System.Environment.GetEnvironmentVariable("ASTRA_DB_NAMESPACE");


        //Console.WriteLine("ASTRA_DB_API_ENDPOINT = " + _astraDbEndPoint);
        //Console.WriteLine("ASTRA_DB_APPLICATION_TOKEN = " + _astraDbApplicationToken);
        //Console.WriteLine("ASTRA_DB_NAMESPACE = " + _astraDbNamespace);

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

        //httpClient.BaseAddress = new Uri(_astraDbEndPoint + "/api/json/v1/");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        //httpClient.DefaultRequestHeaders.Add("Token", _astraDbApplicationToken);

        _httpClient = httpClient;
        _baseUrl = _astraDbEndPoint + "/api/json/v1/";

        //Console.WriteLine("url = " + httpClient.BaseAddress);
    }

    public async Task<string?> PostDataAsyncAstra(string table, string query)
    {
        string uri = _astraDbNamespace + "/" + table;
        //string url = _astraDbEndPoint + "/api/json/v1/" + _astraDbNamespace + "/" + table;
        //var json = JsonSerializer.Serialize(query);
        var json = JsonConvert.SerializeObject(query);
        //Console.WriteLine("query json = " + json);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseUrl + uri)
        {
            Content = data
        };
        requestMessage.Headers.Add("Token", _astraDbApplicationToken);

        Console.WriteLine("request = " + requestMessage.Content);

        var response = await _httpClient.SendAsync(requestMessage);
        //var response = _httpClient.PostAsync(url, content).Result;

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
        //Console.WriteLine("query = " + query);
        string? result = await PostDataAsyncAstra(table, query);

        return result;
    }
}