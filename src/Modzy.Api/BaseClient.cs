﻿namespace Modzy;

using System.Threading.Tasks;
public abstract class BaseClient : Runtime, IApiClient
{
    #region Constructors
    public BaseClient(string apiKey, Uri baseUrl, HttpClient? client = null) : base()
    {
        ApiKey = apiKey;
        BaseUrl = baseUrl;
        RestClient = client ?? DefaultHttpClient;
        if (RestClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            RestClient.DefaultRequestHeaders.Remove("Authorization");
        }
        RestClient.DefaultRequestHeaders.Add("Authorization", "ApiKey " + apiKey);
        Info("Initialized HTTP client for Modzy API base url {0}.", BaseUrl);
    }
    #endregion

    #region Abstract members
    public abstract Task<T> RestHttpGetAsync<T>(string query);

    public abstract Task<T2> RestHttpPostAsync<T1, T2>(string query, T1 data);

    public abstract Task<string> RestHttpGetStringAsync(string query);

    public abstract Task<string> RestHttpPostStringAsync<T1>(string query, T1 data);
    #endregion

    #region Properties
    public string ApiKey { get; }

    public Uri BaseUrl { get; }

    public HttpClient RestClient { get; init; }
    #endregion

    #region Methods
    protected static Uri GetUri(string u)
    {
        if (!Uri.TryCreate(u, UriKind.Absolute, out Uri? uri))
        {
            throw new ArgumentException($"The string {u} is not a valid URI.");
        }
        else return uri!;
    }

    #region Modzy API
    public async Task<List<ModelListing>> GetModelsListing() => await RestHttpGetAsync<List<ModelListing>>("models?per-page=1000");

    public async Task<Model> GetModel(string modelId) => await RestHttpGetAsync<Model>($"models/{modelId}");

    public async Task<MinimumProcessingEnginesSum> GetMinimumProcessingEnginesSum() => await RestHttpGetAsync<MinimumProcessingEnginesSum>("models/processing-engines");

    public async Task<List<ModelVersion>> GetModelVersions(string modelId) => await RestHttpGetAsync<List<ModelVersion>>($"models/{modelId}/versions");

    public async Task<ModelSampleInput> GetModelSampleInput(string modelId, string version) => await RestHttpGetAsync<ModelSampleInput>($"models/{modelId}/versions/{version}/sample-input");

    public async Task<Job> SubmitJob(Dictionary<string, object> data) => await RestHttpPostAsync<Dictionary<string, object>, Job>("jobs", data);

    public async Task<List<JobListing>> GetJobsListing() => await RestHttpGetAsync<List<JobListing>>("jobs?per-page=1000");

    public async Task<Job> GetJob(string jobId) => await RestHttpGetAsync<Job>($"jobs/{jobId}");
    #endregion

    public static InputType InputTypeFromInputFilename(string name)
    {
        name = name.ToLower();
        if (name.EndsWith(".jpg") || name == "image")
        {
            return InputType.IMAGE;
        }
        else if (name.EndsWith(".mp4"))
        { 
            return InputType.VIDEO;
        }
        else if (name.EndsWith(".txt"))
        {
            return InputType.TEXT;
        }
        else if (name.EndsWith(".json"))
        {
            return InputType.JSON;
        }
        else if (name.EndsWith(".mp3"))
        {
            return InputType.AUDIO;
        }
        else
        {
            throw new Exception($"Could not determine input type from input file name {name}.");
        }
    }
    #endregion
}
