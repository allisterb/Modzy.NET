namespace Modzy;

public class ApiClient : BaseClient
{
    #region Constructors
    public ApiClient(string apiKey, Uri baseUrl) : base(apiKey, baseUrl)
    {
        Initialized = true;
    }

    public ApiClient() : this(Config("MODZY_API_KEY"), GetUri(Config("MODZY_BASE_URL"))) { }
    #endregion

    #region Implemented members
    public override async Task<T> RestHttpGetAsync<T>(string query)
    {
        var response = await HttpClient.GetAsync(BaseUrl + query);
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
                return JsonConvert.DeserializeObject<T>(json) ?? throw new Exception($"Did not successfully deserialize JSON response as type {typeof(T).Name}");
            }
        }
    }

    public override async Task<T2> RestHttpPostAsync<T1, T2>(string query, T1 data)
    {
        var c = JsonConvert.SerializeObject(data);
        var content = new StringContent(c, Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync(BaseUrl + query, content);
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
                return JsonConvert.DeserializeObject<T2>(json) ?? throw new Exception($"Did not successfully deserialize JSON response as type {typeof(T2).Name}");
            }
        }
    }

    #endregion

    #region Properties

    #endregion

    #region Methods
    private static Uri GetUri(string u)
    {
        if (!Uri.TryCreate(u, UriKind.Absolute, out Uri? uri))
        {
            throw new ArgumentException($"The string {u} is not a valid URI.");
        }
        else return uri!;
    }

    public async Task<List<ModelListing>> GetAllModels() => await RestHttpGetAsync<List<ModelListing>>("models?per-page=1000");

    public async Task<Model> GetModel(string modelId) => await RestHttpGetAsync<Model>($"models/{modelId}");
    #endregion
}
