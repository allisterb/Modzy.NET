namespace Modzy.CLI;

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
            }
            ApiClient = new ApiClient(ApiKey, BaseUrl);
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
    #endregion

    #region Methods
    static void PrintLogo()
    {
        Con.Write(new FigletText(Font, "Modzy.NET").LeftAligned().Color(Color.OrangeRed1));
        Con.Write(new Text($"v{AssemblyVersion.ToString(3)}").LeftAligned());
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
        Error((Exception)e.ExceptionObject, "Unhandled runtime error occurred. TigerGraph.NET CLI will now shutdown.");
        Exit(ExitResult.UNHANDLED_EXCEPTION);
    }

    private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        Info("Ctrl-C pressed. Exiting.");
        Cts.Cancel();
        Exit(ExitResult.SUCCESS);
    }
    #endregion
}

