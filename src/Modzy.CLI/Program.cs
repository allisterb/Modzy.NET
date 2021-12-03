namespace Modzy.CLI;

using System.IO;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

using Modzy;
#region Enums
public enum ExitResult
{
    SUCCESS = 0,
    UNHANDLED_EXCEPTION = 1,
    INVALID_OPTIONS = 2,
    NOT_FOUND = 4,
    SERVER_ERROR = 5,
    ERROR_IN_RESULTS = 6,
    UNKNOWN_ERROR = 7
}
#endregion

class Program : Runtime
{
    #region Constructor
    static Program()
    {
        AppDomain.CurrentDomain.UnhandledException += Program_UnhandledException;
        Console.CancelKeyPress += Console_CancelKeyPress;
        Console.OutputEncoding = Encoding.UTF8;
        foreach (var t in OptionTypes)
        {
            OptionTypesMap.Add(t.Name, t);
        }
    }
    #endregion

    #region Entry point
    static void Main(string[] args)
    {
        if (args.Contains("--debug"))
        {
            SetLogger(new SerilogLogger(console: true, debug: true));
            Info("Debug mode set.");
        }
        else
        {
            SetLogger(new SerilogLogger(console: true, debug: false));
        }
        PrintLogo();
        ParserResult<object> result = new Parser().ParseArguments<Options, ApiOptions, ModelsOptions, JobsOptions>(args);
        result.WithParsed<ApiOptions>(o =>
        {
            if (!string.IsNullOrEmpty(o.ApiKey))
            {
                ApiKey = o.ApiKey!;
            }
            else
            {
                ApiKey = Config("MODZY_API_KEY");
                Info("Using Modzy API key from configuration store.");
            }
            ApiClient = new ApiClient(ApiKey, BaseUrl);
        })
        .WithParsed<ModelsOptions>(o =>
        {
            if (o.List)
            {
                ListModels(o);
                Exit(ExitResult.SUCCESS);
            }
            else if (o.Inspect)
            {
                FailIfNoModelId(o);
                InspectModel(o);
                Exit(ExitResult.SUCCESS);
            }
            else if (o.Run)
            {
                FailIfNoModelId(o);
                RunModel(o);
                Exit(ExitResult.SUCCESS);
            }
            else
            {
                ListModels(o);
                Exit(ExitResult.SUCCESS);
            }
        })
        .WithParsed<JobsOptions>(o =>
        {
            if (o.List || o.Completed || o.Cancelled)
            {
                ListJobs(o);
                Exit(ExitResult.SUCCESS);
            }
            else if (o.Inspect)
            {
                FailIfNoJobId(o);
                InspectJob(o);
                Exit(ExitResult.SUCCESS);
            }
            else if (o.Results)
            {
                FailIfNoJobId(o);
                JobResults(o);
                Exit(ExitResult.SUCCESS);
            }
            else
            {
                ListJobs(o);
                Exit(ExitResult.SUCCESS);
            }
        })
        #region Print options help
        .WithNotParsed((IEnumerable<Error> errors) =>
        {
            HelpText help = GetAutoBuiltHelpText(result);
            help.Heading = new HeadingInfo("Modzy.NET", AssemblyVersion.ToString(3));
            help.Copyright = "";
            if (errors.Any(e => e.Tag == ErrorType.VersionRequestedError))
            {
                help.Heading = new HeadingInfo("Modzy.NET", AssemblyVersion.ToString(3));
                help.Copyright = new CopyrightInfo("Allister Beharry", new int[] { 2021 });
                Info(help);
                Exit(ExitResult.SUCCESS);
            }
            else if (errors.Any(e => e.Tag == ErrorType.HelpVerbRequestedError))
            {
                HelpVerbRequestedError error = (HelpVerbRequestedError)errors.First(e => e.Tag == ErrorType.HelpVerbRequestedError);
                if (error.Type != null)
                {
                    help.AddVerbs(error.Type);
                }
                else
                {
                    help.AddVerbs(OptionTypes);
                }
                Info(help.ToString().Replace("--", ""));
                Exit(ExitResult.SUCCESS);
            }
            else if (errors.Any(e => e.Tag == ErrorType.HelpRequestedError))
            {
                HelpRequestedError error = (HelpRequestedError)errors.First(e => e.Tag == ErrorType.HelpRequestedError);
                help.AddVerbs(result.TypeInfo.Current);
                help.AddOptions(result);
                help.AddPreOptionsLine($"{result.TypeInfo.Current.Name.Replace("Options", "").ToLower()} options:");
                Info(help);
                Exit(ExitResult.SUCCESS);
            }
            else if (errors.Any(e => e.Tag == ErrorType.NoVerbSelectedError))
            {
                help.AddVerbs(OptionTypes);
                Info(help);
                Exit(ExitResult.INVALID_OPTIONS);
            }
            else if (errors.Any(e => e.Tag == ErrorType.MissingRequiredOptionError))
            {
                MissingRequiredOptionError error = (MissingRequiredOptionError)errors.First(e => e.Tag == ErrorType.MissingRequiredOptionError);
                Error("A required option is missing: {0}.", error.NameInfo.NameText);
                Info(help);
                Exit(ExitResult.INVALID_OPTIONS);
            }
            else if (errors.Any(e => e.Tag == ErrorType.UnknownOptionError))
            {
                UnknownOptionError error = (UnknownOptionError)errors.First(e => e.Tag == ErrorType.UnknownOptionError);
                help.AddVerbs(OptionTypes);
                Error("Unknown option: {error}.", error.Token);
                Info(help);
                Exit(ExitResult.INVALID_OPTIONS);
            }
            else
            {
                Error("An error occurred parsing the program options: {errors}.", errors);
                help.AddVerbs(OptionTypes);
                Info(help);
                Exit(ExitResult.INVALID_OPTIONS);
            }
        });
        #endregion;
    }
    #endregion

    #region Properties
    private static Version AssemblyVersion { get; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
    private static FigletFont Font { get; } = FigletFont.Load("chunky.flf");
    static ApiClient? ApiClient { get; set; }
    static Uri BaseUrl { get; } = new Uri("https://app.modzy.com/api/");
    static string ApiKey { get; set; } = "";

    static Type[] OptionTypes = { typeof(Options), typeof(ApiOptions), typeof(ModelsOptions), typeof(JobsOptions)};
    static Dictionary<string, Type> OptionTypesMap { get; } = new Dictionary<string, Type>();
    #endregion

    #region Methods
    static void ListModels(ModelsOptions o)
    {
        ModelListing[]? modelsListing = Con.Status().Spinner(Spinner.Known.Dots).Start("Fetching models listing...", ctx => ApiClient!.GetModelsListing().Result.ToArray());
        Info("Got {0} models.", modelsListing.Length);
        var models = new Model[modelsListing.Length];
        var table = new Table();
        table.AddColumns("[green]Id[/]", "[green]Name[/]", "[green]Description[/]", "[green]Author[/]", "[green]Versions[/]", "[green]Latest Version[/]");
        using (var op = Begin("Fetching details for {0} models", modelsListing.Length))
        {
            AnsiConsole.Progress().Start(ctx =>
            {
                var task1 = ctx.AddTask("[green]Fetching details[/]");
                Parallel.For(0, modelsListing.Length, i =>
                {
                    models[i] = ApiClient!.GetModel(modelsListing[i].ModelId).Result;
                    lock (_uilock)
                    {
                        if (o.Search is null || (o.Search is not null && (models[i].Name.ToLower().Contains(o.Search.ToLower()))))
                        {
                            var model = models[i];
                            table.AddRow(model.ModelId, model.Name, model.Description, model.Author,
                                model.Versions.Any() ? model.Versions.Aggregate((p, s) => p + "," + s) : "", model.LatestVersion);
                            table.AddEmptyRow();
                        }
                        task1.Increment(100.0 / modelsListing.Length);
                    }
                });
            });
            op.Complete();
        }
        Con.Write(table);    
    }

    static void InspectModel(ModelsOptions o)
    {
        var model = Con.Status().Spinner(Spinner.Known.Dots).Start($"Fetching model details for model {o.ModelId!}...", ctx => ApiClient!.GetModel(o.ModelId!).Result);
        if (model == null)
        {
            Error("Could not find model with ID {0}.", o.ModelId!);
            Exit(ExitResult.ERROR_IN_RESULTS);
        }
        Info("Model name is {0}, latest version is {1}.", model!.Name, model!.LatestVersion);   
        var sample = Con.Status().Spinner(Spinner.Known.Dots).Start($"Fetching input details for model {o.ModelId} at version {model!.LatestVersion}...", ctx => ApiClient!.GetModelSampleInput(o.ModelId!, model!.LatestVersion).Result);
        if (sample == null)
        {
            {
                Error("Could not get input details for model with ID {0}.", o.ModelId!);
                Exit(ExitResult.ERROR_IN_RESULTS);
            }
        }
        var tree = new Tree($"{model.Name}({model.ModelId})");
        var metadata = tree.AddNode("[yellow]Metadata[/]");
        metadata.AddNode($"[green]Description:[/] {model.Description}");
        metadata.AddNode($"[green]Author:[/] {model.Author}");
        var tags = model.Tags.Any() ? model.Tags.Select(k => k.Name).Aggregate((p, s) => p + ", " + s) : "";
        metadata.AddNode($"[green]Tags:[/] {tags}");
        var versions = model.Versions.Any() ? model.Versions.Aggregate((p, s) => p + ", " + s) : "";
        metadata.AddNode($"[green]Versions:[/] {versions}");
        var inputs = tree.AddNode("[yellow]Inputs[/]");
        foreach(var s in sample!.Input.Sources )
        {
            var sn = inputs.AddNode($"[green]{s.Key}[/]");
            sn.AddNodes(s.Value.Keys.Select(k => "[red]" + ApiClient.InputTypeFromInputFilename(k).ToString() + "[/]" + ": " + k));
        }
        Con.Write(tree);
    }

    static void RunModel(ModelsOptions o)
    {
        var model = Con.Status().Spinner(Spinner.Known.Dots).Start($"Fetching model details for model {o.ModelId!}...", ctx => ApiClient!.GetModel(o.ModelId!).Result);
        if (model == null)
        {
            Error("Could not find model with ID {0}.", o.ModelId!);
            Exit(ExitResult.ERROR_IN_RESULTS);
        }
        var version = o.Version ?? model!.LatestVersion;
        if (!model!.Versions.Contains(version))
        {
            Error("The model version specified {0} does not exist. Model versions are: {1}.", version, model.Versions.Aggregate((p, n) => p + "," + n));
            Exit(ExitResult.INVALID_OPTIONS);
        }
        Info("Model name is {0}, version requested is {1}.", model!.Name, version);
        var sample = Con.Status().Spinner(Spinner.Known.Dots).Start($"Fetching input details for model {o.ModelId} at version {version}...", ctx => ApiClient!.GetModelSampleInput(o.ModelId!, version).Result);
        if (sample == null)
        {
            {
                Error("Could not get input details for model with ID {0}.", o.ModelId!);
                Exit(ExitResult.ERROR_IN_RESULTS);
            }
        }
        var sourceInputTypes = new Dictionary<string, List<InputType>>();
        var sourceInputs = new Dictionary<string, List<string>>();
        var sourceInputFiles = new Dictionary<string, Dictionary<string, string>>();
        foreach (var s in sample!.Input.Sources)
        {
            sourceInputTypes.Add(s.Key, s.Value.Keys.Select(k => ApiClient.InputTypeFromInputFilename(k)).ToList());
            sourceInputs.Add(s.Key, s.Value.Keys.ToList());
            sourceInputFiles.Add(s.Key, new Dictionary<string, string>(s.Value.Keys.Count));
        }
        Info("Model has {0} input groups with {1} total inputs.", sourceInputs.Keys.Count, sourceInputs.Values.SelectMany(x => x).Count());
        int ns = 0;
        while(ns < sourceInputTypes.Count)
        {
            var st = sourceInputTypes.Values.ElementAt(ns);
            var si = sourceInputs.Values.ElementAt(ns);
            var sif = sourceInputFiles.Values.ElementAt(ns);
            int nf = 0;
            while (nf < st.Count)
            {
                string name = !o.PlainText ? 
                        AnsiConsole.Ask<string>($"{nf + 1}. Enter the [red]{st[nf].ToString()}[/] file for input parameter [green]" + si[nf] + "[/]:")
                        : AnsiConsole.Ask<string>($"{nf + 1}. Enter the [red]{st[nf].ToString()}[/] for input parameter [green]" + si[nf] + "[/]:");
                if (o.PlainText)
                {
                    sif.Add(si[nf], name);
                    nf++;
                }

                else if (File.Exists(name))
                {
                    if (name.EndsWith(".txt"))
                    {
                        sif.Add(si[nf], "data:text/plain;charset=utf-8;base64," + Convert.ToBase64String(File.ReadAllBytes(name)));
                    }
                    else if (name.EndsWith(".jpg"))
                    {
                        sif.Add(si[nf], "data:image/jpg;base64," + Convert.ToBase64String(File.ReadAllBytes(name)));
                    }
                    else if (name.EndsWith(".png"))
                    {
                        sif.Add(si[nf], "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(name)));
                    }
                    else if (name.EndsWith(".mp3"))
                    {
                        sif.Add(si[nf], "data:audio/mp3;base64," + Convert.ToBase64String(File.ReadAllBytes(name)));
                    }
                    nf++;
                }
                else
                {
                    Error("The input file {0} does not exist.", name);
                }
            }
            ns++;
        }
        Dictionary<string, Dictionary<string, object>> sources = new Dictionary<string, Dictionary<string,object>>();
        foreach (var sif in sourceInputFiles)
        {
            var d = new Dictionary<string, object>();
            foreach (var f in sif.Value)
            {
                d.Add(f.Key, f.Value);
            }
            sources.Add(sif.Key, d);
        }
        
        Dictionary<string, object> data = new Dictionary<string, object>();
        data.Add("model", new Dictionary<string, string>() { { "identifier", model.ModelId}, { "version", version } });
        data.Add("explain", false);
        object _sources = sources.Count > 1 ? (new Dictionary<string, object>() { { "job", sources } }) : sources;
        data.Add("input", new Dictionary<string, object>() { { "type", o.PlainText ? "text" : "embedded" }, { "sources", _sources }});
        var job = Con.Status().Spinner(Spinner.Known.Dots).Start($"Running model {o.ModelId} at version {version} on input files...", ctx => ApiClient!.SubmitJob(data).Result);
        if (job == null)
        {
            Error("Failed to submit job.");
            Exit(ExitResult.ERROR_IN_RESULTS);
        }
        else
        {
            Info("Successfully submitted job with ID {0}.", job.JobIdentifier);
        }
        var tree = new Tree($"Job [green]{job!.JobIdentifier.ToString()}[/]");
    }

    static void ListJobs(JobsOptions o)
    {
        JobListing[]? jobsListing;
        string js = "pending";
        if (o.Completed)
        {
            js = "completed";
            jobsListing = Con.Status().Spinner(Spinner.Known.Dots).Start("Fetching completed jobs listing...", ctx => ApiClient!.GetTerminatedJobsListing().Result.ToArray());
            Info("Got {0} {1} job(s).", jobsListing.Where(j => j.Status == "COMPLETED").Count(), js);
        }
        else if (o.Cancelled)
        {
            js = "cancelled";
            jobsListing = Con.Status().Spinner(Spinner.Known.Dots).Start("Fetching cancelled jobs listing...", ctx => ApiClient!.GetTerminatedJobsListing().Result.ToArray());
            Info("Got {0} {1} job(s).", jobsListing.Where(j => j.Status == "CANCELED").Count(), js);
        }
        else
        {
            jobsListing = Con.Status().Spinner(Spinner.Known.Dots).Start("Fetching pending jobs listing...", ctx => ApiClient!.GetPendingJobsListing().Result.ToArray());
            Info("Got {0} {1} job(s).", jobsListing.Count(), js);
        }
        
        var jobs = new Job[jobsListing.Length];
        var table = new Table();
        table.AddColumns("[green]Id[/]", "[green]Status[/]", "[green]Model[/]", "[green]Model Version[/]");
        var getStatusText = (string s) =>
        {
            switch (s)
            {
                case "SUBMITTED": return "[purple]SUBMITTED[/]";
                case "CANCELED": return "[yellow]CANCELLED[/]";
                case "COMPLETED": return "[green]COMPLETED[/]";
                default: return s;
            }
        };
        foreach(var job in jobsListing)
        {
            if (!(o.Cancelled || o.Completed) || (o.Cancelled && job.Status == "CANCELED") || (o.Completed && job.Status == "COMPLETED"))
            {
                table.AddRow(job.JobIdentifier.ToString(), getStatusText(job.Status), job.Model.Identifier, job.Model.Version);
                table.AddEmptyRow();
            }
        }
        Con.Write(table);        
    }

    static void InspectJob(JobsOptions o)
    {
        var job = Con.Status().Spinner(Spinner.Known.Dots).Start($"Fetching job details for job {o.JobId!}...", ctx => ApiClient!.GetJob(o.JobId!).Result);
        if (job == null)
        {
            Error("Could not find job with ID {0}.", o.JobId!);
            Exit(ExitResult.ERROR_IN_RESULTS);
        }
        Info("Job {0} was submitted at {1}.", job!.JobIdentifier, job!.SubmittedAt);
        var model = Con.Status().Spinner(Spinner.Known.Dots).Start($"Fetching model details for model {job!.Model.Identifier}...", ctx => ApiClient!.GetModel(job!.Model.Identifier).Result);
        if (model == null)
        {
            Error("Could not find model with ID {0}.", job!.Model.Identifier);
            Exit(ExitResult.ERROR_IN_RESULTS);
        }
        var tree = new Tree($"Job {job.JobIdentifier}");
        var metadata = tree.AddNode("[yellow]Model metadata[/]");
        metadata.AddNode($"[green]Submitted:[/] {job!.SubmittedAt}");
        metadata.AddNode($"[green]Status:[/] {job!.Status}");
        var tags = model!.Tags.Any() ? model.Tags.Select(k => k.Name).Aggregate((p, s) => p + ", " + s) : "";
        metadata.AddNode($"[green]Tags:[/] {tags}");
        //var inputs = tree.AddNode("[yellow]Inputs[/]");
        //foreach (var s in sample!.Input.Sources)
        //{
        //    var sn = inputs.AddNode($"[red]{s.Key}[/]");
        //    sn.AddNodes(s.Value.Keys.Select(k => ApiClient.InputTypeFromInputFilename(k).ToString()));
        //}
        Con.Write(tree);
        
    }

    static void JobResults(JobsOptions o)
    {
        var job = Con.Status().Spinner(Spinner.Known.Dots).Start($"Fetching job details for job {o.JobId!}...", ctx => ApiClient!.GetJob(o.JobId!).Result);
        if (job == null)
        {
            Error("Could not find job with ID {0}.", o.JobId!);
            Exit(ExitResult.ERROR_IN_RESULTS);
        }
        Info("Job {0} was submitted at {1}.", job!.JobIdentifier, job!.SubmittedAt);
        if (job.Status == "CANCELED")
        {
            Error("Job {0} was cancelled.", job.JobIdentifier);
            Exit(ExitResult.ERROR_IN_RESULTS);
        }
        else if (job.Status != "COMPLETED")
        {
            Info("Job {0} status is currently {1}.", job.JobIdentifier, job.Status);
            Exit(ExitResult.SUCCESS);
        }
        var model = Con.Status().Spinner(Spinner.Known.Dots).Start($"Fetching model details for model {job!.Model.Identifier}...", ctx => ApiClient!.GetModel(job!.Model.Identifier).Result);
        if (model == null)
        {
            Error("Could not find model with ID {0}.", job!.Model.Identifier);
            Exit(ExitResult.ERROR_IN_RESULTS);
        }
        var results = Con.Status().Spinner(Spinner.Known.Dots).Start($"Fetching results for job {o.JobId!}...", ctx => ApiClient!.GetResults(o.JobId!).Result);
        if (results == null)
        {
            Error("Could not get results for job with ID {0}.", o.JobId!);
            Exit(ExitResult.ERROR_IN_RESULTS);
        }
        var tree = new Tree($"Results for Job {job.JobIdentifier}");
        
        TreeNode timings = tree.AddNode("[yellow]Timings[/]");
        timings.AddNode($"[green]Job Submitted:[/] {results!.SubmittedAt}");
        timings.AddNode($"[green]Total Elapsed Time:[/] {results!.ElapsedTime}[red]s[/]");
        timings.AddNode($"[green]Total Model Latency:[/] {results!.TotalModelLatency}[red]s[/]");
        timings.AddNode($"[green]Total Queue Time:[/] {results!.TotalQueueTime}[red]s[/]");
        
        TreeNode predictions = tree.AddNode("[yellow]Predictions[/]");
        if (results.ResultsResults is not null)
        {
            foreach (var i in results.ResultsResults)
            {
                var inp = predictions.AddNode($"[red]{i.Key}[/]");
                foreach (var p in i.Value.ResultsJson.Data.Result.ClassPredictions)
                {
                    inp.AddNode($"Class: {p.Class}");
                    inp.AddNode($"Score: {p.Score}");

                }
            }
        }
        Con.Write(tree);
    }
    static void PrintLogo()
    {
        Con.Write(new FigletText(Font, "Modzy.NET").LeftAligned().Color(Color.Blue));
        Con.Write(new Text($"v{AssemblyVersion.ToString(3)}\n").LeftAligned());
    }

    public static void Exit(ExitResult result)
    {
        if (Cts != null && !Cts.Token.CanBeCanceled)
        {
            Cts.Cancel();
            Cts.Dispose();
        }
        Serilog.Log.CloseAndFlush();
        Environment.Exit((int)result);
    }

    static void FailIfNoModelId(ModelsOptions o)
    {
        if (string.IsNullOrEmpty(o.ModelId))
        {
            Error("You must specify a model ID for this operation.");
            Exit(ExitResult.INVALID_OPTIONS);
        }
    }

    static void FailIfNoJobId(JobsOptions o)
    {
        if (string.IsNullOrEmpty(o.JobId))
        {
            Error("You must specify a job ID for this operation.");
            Exit(ExitResult.INVALID_OPTIONS);
        }
    }
    static HelpText GetAutoBuiltHelpText(ParserResult<object> result)
    {
        return HelpText.AutoBuild(result, h =>
        {
            h.AddOptions(result);
            return h;
        },
        e =>
        {
            return e;
        });
    }
    #endregion

    #region Event Handlers
    private static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Serilog.Log.CloseAndFlush();
        Error("Unhandled runtime error occurred. Modzy.NET CLI will now shutdown.");
        Con.WriteException((Exception) e.ExceptionObject);
        Exit(ExitResult.UNHANDLED_EXCEPTION);
    }

    private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        Info("Ctrl-C pressed. Exiting.");
        Cts.Cancel();
        Exit(ExitResult.SUCCESS);
    }
    #endregion

    #region Fields
    private static object _uilock = new object();
    #endregion
}

