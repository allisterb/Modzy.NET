namespace Modzy;

using System.Reflection;

using Microsoft.Extensions.Configuration;

public abstract class Runtime
{
    #region Constructors
    static Runtime()
    {
        Logger = new ConsoleLogger();
        IsKubernetesPod = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KUBERNETES_PORT")) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENSHIFT_BUILD_NAMESPACE"));
        if (IsKubernetesPod)
        {
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }
        else if (Assembly.GetEntryAssembly()?.GetName().Name == "Modzy.CLI" && Environment.GetEnvironmentVariable("USERNAME") == "Allister")
        {
            Configuration = new ConfigurationBuilder()
                .AddUserSecrets("c0697968-04fe-49d7-a785-aaa817e38935")
                .Build();
        }
        else if (Assembly.GetEntryAssembly()?.GetName().Name == "Modzy.CLI" && Environment.GetEnvironmentVariable("USERNAME") != "Allister")
        {
            Configuration = new ConfigurationBuilder()
            .AddJsonFile("config.json", optional: true)
            .Build();
        }
        else
        {
            Configuration = new ConfigurationBuilder()
            .AddJsonFile("config.json", optional: true)
            .Build();
        }

        DefaultHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Modzy.NET/0.1");
    }
    public Runtime(CancellationToken ct)
    {
        CancellationToken = ct;
    }
    public Runtime(): this(Cts.Token) {}
    #endregion

    #region Properties
    public static IConfigurationRoot Configuration { get; set; }

    public static string Config(string i) => Configuration[i];

    public static Logger Logger { get; protected set; }

    public static CancellationTokenSource Cts { get; } = new CancellationTokenSource();

    public static CancellationToken Ct { get; } = Cts.Token;

    public static HttpClient DefaultHttpClient { get; } = new HttpClient();

    public static string YY = DateTime.Now.Year.ToString().Substring(2, 2);

    public bool Initialized { get; protected set; }

    public static bool IsKubernetesPod { get; }

    public static bool IsAzureFunction { get; set; }

    public CancellationToken CancellationToken { get; protected set; }
    #endregion

    #region Methods
    public static void SetLogger(Logger logger)
    {
        Logger = logger;
    }

    public static void SetLoggerIfNone(Logger logger)
    {
        if (Logger == null)
        {
            Logger = logger;
        }
    }

    public static void SetDefaultLoggerIfNone()
    {
        if (Logger == null)
        {
            Logger = new ConsoleLogger();
        }
    }

    [DebuggerStepThrough]
    public static void Info(string messageTemplate, params object[] args) => Logger.Info(messageTemplate, args);

    [DebuggerStepThrough]
    public static void Debug(string messageTemplate, params object[] args) => Logger.Debug(messageTemplate, args);

    [DebuggerStepThrough]
    public static void Error(string messageTemplate, params object[] args) => Logger.Error(messageTemplate, args);

    [DebuggerStepThrough]
    public static void Error(Exception ex, string messageTemplate, params object[] args) => Logger.Error(ex, messageTemplate, args);

    [DebuggerStepThrough]
    public static Logger.Op Begin(string messageTemplate, params object[] args) => Logger.Begin(messageTemplate, args);


    public void FailIfNotInitialized()
    {
        if (!this.Initialized) throw new RuntimeNotInitializedException(this);
    }
    #endregion
}
