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
        var response = await RestClient.GetAsync(BaseUrl + query);
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
        var c = JsonConvert.SerializeObject(data, ModelSampleInputSourceConverter.Singleton);
        var content = new StringContent(c, Encoding.UTF8, "application/json");
        Debug("JSON request: {0}", c);
        var response = await RestClient.PostAsync(BaseUrl.ToString() + query, content);
        if (response is null)
        {
            throw new Exception("The response for the HTTP POST request is null.");
        }
        else
        {
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Accepted)
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

    public override async Task<T> RestHttpGetAsync<T>(string query) => JsonConvert.DeserializeObject<T>(await RestHttpGetStringAsync(query), ModelSampleInputSourceConverter.Singleton, JobInputConverter.Singleton, ResultsJsonConverter.Singleton) ?? throw new Exception($"Did not successfully read JSON response.");

    public override async Task<T2> RestHttpPostAsync<T1, T2>(string query, T1 data)=> JsonConvert.DeserializeObject<T2>(await RestHttpPostStringAsync(query, data), ModelSampleInputSourceConverter.Singleton, JobInputConverter.Singleton, ResultsJsonConverter.Singleton) ?? throw new Exception($"Did not successfully read JSON response.");
    
    #endregion

}
