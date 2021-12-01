namespace Modzy.CLI;

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
        foreach (var t in OptionTypes)
        {
            OptionTypesMap.Add(t.Name, t);
        }
        System.Console.OutputEncoding = Encoding.UTF8;
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
            }
            else if (o.Inspect)
            {
                if (!string.IsNullOrEmpty(o.ModelId))
                {
                    InspectModel(o);
                }
                else
                {
                    Error("You must specify a model ID to inspect.");
                    Exit(ExitResult.INVALID_OPTIONS);
                }
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
    static void PrintLogo()
    {
        Con.Write(new FigletText(Font, "Modzy.NET").LeftAligned().Color(Color.OrangeRed1));
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

    private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
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

