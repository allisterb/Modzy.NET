namespace Modzy;

public class ApiClient : BaseClient
{
    #region Constructors
    public ApiClient(string apiKey, Uri baseUrl, HttpClient? c = null) : base(apiKey, baseUrl, c)
    {
        Initialized = true;
    }

    public ApiClient() : this(Config("MODZY_API_KEY"), GetUri(Config("MODZY_BASE_URL"))) { }
    #endregion

    #region Implemented members
    public override async Task<string> RestHttpGetStringAsync(string query)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl.ToString() + query);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Authorization");
        request.Headers.Add("Origin", "https://localhost:7057");
        var response = await RestClient.SendAsync(request);
        Debug("HTTP GET: {0}", BaseUrl.ToString() + query);
        if (response is null)
        {
            throw new Exception("The response for the HTTP GET request is null.");
        }
        else
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP GET request returned code {response.StatusCode}.");
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                Debug("JSON response: {0}", json);
                return json;            }
        }
    }

    public override async Task<string> RestHttpPostStringAsync<T1>(string query, T1 data)
    {
        var c = JsonConvert.SerializeObject(data);
        var content = new StringContent(c, Encoding.UTF8, "application/json");
        var response = await DefaultHttpClient.PostAsync(BaseUrl + query, content);
        if (response is null)
        {
            throw new Exception("The response for the HTTP POST request is null.");
        }
        else
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP POST request returned code {response.StatusCode}.");
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                Debug("JSON response: {0}", json);
                return json;
            }
        }
    }

    public override async Task<T> RestHttpGetAsync<T>(string query) => JsonConvert.DeserializeObject<T>(await RestHttpGetStringAsync(query)) ?? throw new Exception($"Did not successfully read JSON response.");

    public override async Task<T2> RestHttpPostAsync<T1, T2>(string query, T1 data)=> JsonConvert.DeserializeObject<T2>(await RestHttpPostStringAsync(query, data)) ?? throw new Exception($"Did not successfully read JSON response.");
    
    #endregion

}
