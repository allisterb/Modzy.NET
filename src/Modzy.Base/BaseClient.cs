namespace Modzy;

public abstract class BaseClient : Runtime, IApiClient
{
    #region Constructors
    public BaseClient(string apiKey, Uri baseUrl) : base()
    {
        ApiKey = apiKey;
        BaseUrl = baseUrl;
        HttpClient.DefaultRequestHeaders.Add("Authorization", "ApiKey " + apiKey);
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));   
        Info("Initialized HTTP client for Modzy API base url {0}.", BaseUrl);
    }
    #endregion

    #region Abstract members
    public abstract Task<T> RestHttpGetAsync<T>(string query);

    public abstract Task<T2> RestHttpPostAsync<T1, T2>(string query, T1 data);
    #endregion

    #region Properties
    public string ApiKey { get; }

    public Uri BaseUrl { get; }
    #endregion

    #region Methods
    #endregion
}
