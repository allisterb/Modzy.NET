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
        ParserResult<object> result = new Parser().ParseArguments<Options, ApiOptions, ModelsOptions>(args);
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
                if (!string.IsNullOrEmpty(o.ModelId))
                {
                    InspectModel(o);
                    Exit(ExitResult.SUCCESS);
                }
                else
                {
                    Error("You must specify a model ID to inspect.");
                    Exit(ExitResult.INVALID_OPTIONS);
                }
            }
            else if (o.Run)
            {
                FailIfNoModelId(o);
                RunModel(o);
                Exit(ExitResult.SUCCESS);
            }
        });
    }
    #endregion

    #region Properties
    private static Version AssemblyVersion { get; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
    private static FigletFont Font { get; } = FigletFont.Load("chunky.flf");
    static ApiClient? ApiClient { get; set; }
    static Uri BaseUrl { get; } = new Uri("https://app.modzy.com/api/");
    static string ApiKey { get; set; } = "";
    static Type[] OptionTypes = { typeof(Options), typeof(ApiOptions), typeof(ModelsOptions)};
    static Dictionary<string, Type> OptionTypesMap { get; } = new Dictionary<string, Type>();
    static string? ModelId { get; set; }
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
                        var model = models[i];
                        table.AddRow(model.ModelId, model.Name, model.Description, model.Author,
                            model.Versions.Any() ? model.Versions.Aggregate((p, s) => p + "," + s) : "", model.LatestVersion);
                        table.AddEmptyRow();
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
        var inputs = tree.AddNode("[yellow]Inputs[/]");
        foreach(var s in sample!.Input.Sources )
        {
            var sn = inputs.AddNode($"[red]{s.Key}[/]");
            sn.AddNodes(s.Value.Keys.Select(k => ApiClient.InputTypeFromInputFilename(k).ToString()));
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
        
        int ns = 0;
        while(ns < sourceInputTypes.Count)
        {
            var st = sourceInputTypes.Values.ElementAt(ns);
            var si = sourceInputs.Values.ElementAt(ns);
            var sif = sourceInputFiles.Values.ElementAt(ns);
            int nf = 0;
            while (nf < st.Count)
            {
                
                string name = AnsiConsole.Ask<string>($"Enter the [red]{st[nf].ToString()}[/] file for input " + si[nf] + ":");
                if (File.Exists(name))
                {
                    sif.Add(si[nf], Convert.ToBase64String(File.ReadAllBytes(name)));
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
        data.Add("input", new Dictionary<string, object>() { { "type", "text" }, { "sources", _sources }});
        var r = ApiClient!.SubmitJob(data).Result;
    }
    static void PrintLogo()
    {
        Con.Write(new FigletText(Font, "Modzy.NET").LeftAligned().Color(Color.Purple));
        Con.Write(new Text($"v{AssemblyVersion.ToString(3)}\n").LeftAligned());
    }

    public static void Exit(ExitResult result)
    {

        if (Cts != null && !Cts.Token.CanBeCanceled)
        {
            Cts.Cancel();
            Cts.Dispose();
        }
        Environment.Exit((int)result);
    }

    static void FailIfNoModelId(ModelsOptions o)
    {
        if (string.IsNullOrEmpty(o.ModelId))
        {
            Error("You must specify a model ID to inspect.");
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
        Error((Exception)e.ExceptionObject, "Unhandled runtime error occurred. Modzy.NET CLI will now shutdown.");
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

