namespace Modzy;

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

    public async Task<List<JobListing>> GetPendingJobsListing() => await RestHttpGetAsync<List<JobListing>>("jobs/history?status=pending&per-page=1000");

    public async Task<List<JobListing>> GetTerminatedJobsListing() => await RestHttpGetAsync<List<JobListing>>("jobs/history?status=terminated&per-page=1000");

    public async Task<Job> GetJob(string jobId) => await RestHttpGetAsync<Job>($"jobs/{jobId}");

    public async Task<Results> GetResults(string jobId) => await RestHttpGetAsync<Results>($"results/{jobId}");
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
        else if (name.EndsWith(".wav"))
        {
            return InputType.AUDIO;
        }
        else if (name == "input")
        {
            return InputType.FILE;
        }
        else 
        {
            throw new Exception($"Could not determine input type from input file name {name}.");
        }
    }

    public async Task<Job> RunModel(Model model, string version, Dictionary<string, Dictionary<string, object>> inputs)
    {
        Dictionary<string, Dictionary<string, object>> sources = new Dictionary<string, Dictionary<string, object>>();
        foreach (var sif in inputs)
        {
            var d = new Dictionary<string, object>();
            foreach (var f in sif.Value)
            {
                d.Add(f.Key, f.Value);
            }
            sources.Add(sif.Key, d);
        }

        Dictionary<string, object> data = new Dictionary<string, object>();
        data.Add("model", new Dictionary<string, string>() { { "identifier", model.ModelId }, { "version", version } });
        data.Add("explain", false);
        object _sources = sources.Count > 1 ? (new Dictionary<string, object>() { { "job", sources } }) : sources;
        data.Add("input", new Dictionary<string, object>() { { "type", "embedded" }, { "sources", _sources } });
        return await this.SubmitJob(data);
    }

    public async Task<Job> RunModelWithText(Model model, string version, string inputName, string inputText)
    {
        Dictionary<string, Dictionary<string, object>> sif = new Dictionary<string, Dictionary<string, object>>()
        {
            {"0001", new Dictionary<string, object>(){ { inputName, "data:text/plain;charset=utf-8;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(inputText)) } } }
        };
        return await RunModel(model, version, sif);
    }

    public async Task<Job> RunModelWithJpg(Model model, string version, string inputName, byte[] inputData)
    {
        Dictionary<string, Dictionary<string, object>> sif = new Dictionary<string, Dictionary<string, object>>()
        {
            {"0001", new Dictionary<string, object>(){ { inputName, "data:image/jpg;charset=utf-8;base64," + Convert.ToBase64String(inputData) } } }
        };
        return await RunModel(model, version, sif);
    }

    public async Task<Job> RunModelWithVideo(Model model, string version, string inputName, byte[] inputData)
    {
        Dictionary<string, Dictionary<string, object>> sif = new Dictionary<string, Dictionary<string, object>>()
        {
            {"0001", new Dictionary<string, object>(){ { inputName, "data:video/mp4;charset=utf-8;base64," + Convert.ToBase64String(inputData) } } }
        };
        return await RunModel(model, version, sif);
    }

    public async Task<Job> RunModelWithWav(Model model, string version, string inputName, byte[] inputData)
    {
        Dictionary<string, Dictionary<string, object>> sif = new Dictionary<string, Dictionary<string, object>>()
        {
            {"0001", new Dictionary<string, object>(){ { inputName, "data:audio/wav;charset=utf-8;base64," + Convert.ToBase64String(inputData) } } }
        };
        return await RunModel(model, version, sif);
    }

    public async Task<Results?> WaitUntilComplete(Job job, Action<Job>? waitAction = null)
    {
        bool done = false;
        bool completed = false;
        int waiting = 0;
        while (!done)
        {
            var j = this.GetJob(job.JobIdentifier).Result;
            if (j == null)
            {
                done = true;
                return null;
            }
            else
            {
                if (j.Status == "COMPLETED")
                {
                    done = true;
                    completed = true;
                }
                else if (j.Status == "CANCELED")
                {
                    done = true;
                    completed = false;
                }
                else
                {
                    waiting += 200;
                    await Task.Delay(200);
                    waitAction?.Invoke(j);
                }
            }
        }
        
        if (completed)
        {
            Info("Job {0} completed.", job.JobIdentifier);
            return await this.GetResults(job.JobIdentifier);

        }
        else
        {
            Info("Job {0} cancelled.", job.JobIdentifier);
            return null;
        }
    }
    #endregion
}
