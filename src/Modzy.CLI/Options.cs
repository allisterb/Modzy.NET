using CommandLine;
using CommandLine.Text;

namespace Modzy.CLI
{
    public class Options
    {
        [Option('d', "debug", Required = false, HelpText = "Enable debug mode.")]
        public bool Debug { get; set; }

        public static Dictionary<string, object> Parse(string o)
        {
            Dictionary<string, object> options = new Dictionary<string, object>();
            Regex re = new Regex(@"(\w+)\=([^\,]+)", RegexOptions.Compiled);
            string[] pairs = o.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in pairs)
            {
                Match m = re.Match(s);
                if (!m.Success)
                {
                    options.Add("_ERROR_", s);
                }
                else if (options.ContainsKey(m.Groups[1].Value))
                {
                    options[m.Groups[1].Value] = m.Groups[2].Value;
                }
                else
                {
                    options.Add(m.Groups[1].Value, m.Groups[2].Value);
                }
            }
            return options;
        }
    }

    public class ApiOptions : Options
    {
        [Option("key", Required = false, HelpText = "Your Modzy API key. If none is specified then the environment variable MODZY_APIKEY will be used.")]
        public string? ApiKey { get; set; }
    }

    [Verb("models", HelpText = "Perform model operations.")]
    public class ModelsOptions : ApiOptions 
    {
        [Option('l', "list", Required = false, HelpText = "List all models.")]
        public bool List { get; set; }

        [Option('i', "inspect", Required = false, HelpText = "Inspect a model with the specified model ID.")]
        public bool Inspect { get; set; }

        [Option('r', "run", Required = false, HelpText = "Run a model with the specified model ID.")]
        public bool Run { get; set; }

        [Option('f', "input-files", Required = false, HelpText = "Comma-delimited list of input files for a run operation.")]
        public string? Input { get; set; }

        [Option('v', "version", Required = false, HelpText = "The model version.")]
        public string? Version { get; set; }

        [Option('s', "search", Required = false, HelpText = "Search for a model that contains the specified text in the title or description.")]
        public string? Search { get; set; }

        [Option('t', "text", Required = false, HelpText = "Indicates plain text input should be sent to model.")]
        public bool PlainText { get; set; }

        [Option('w', "wait", Required = false, HelpText = "When submitting a job wait for the job to complete.")]
        public bool WaitForCompletetion { get; set; }

        [Value(0, Required = false)]
        public string? ModelId { get; set; }

    }

    [Verb("jobs", HelpText = "Perform operations on jobs.")]
    public class JobsOptions : ApiOptions
    {
        [Option('l', "list", Required = false, HelpText = "List all pending jobs.")]
        public bool List { get; set; }

        [Option('i', "inspect", Required = false, HelpText = "Inspect a job with the specified job ID.")]
        public bool Inspect { get; set; }

        [Option('r', "results", Required = false, HelpText = "Inspect a job with the specified job ID.")]
        public bool Results { get; set; }

        [Option("cancelled", Required = false, HelpText = "List only cancelled jobs.")]
        public bool Cancelled { get; set; }

        [Option("completed", Required = false, HelpText = "List only completed jobs.")]
        public bool Completed { get; set; }

        [Value(0, Required = false)]
        public string? JobId { get; set; }
    }

    [Verb("results", HelpText = "Perform operations on jobs.")]
    public class ResultsOptions : ApiOptions
    {
        [Value(0, Required = true)]
        public string JobId { get; set; } = String.Empty;
    }
}
