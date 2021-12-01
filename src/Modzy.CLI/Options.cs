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
        [Option("key", Required = false, HelpText = "Your Modzy API key. If none is specified then use the environment variable MODZY_APIKEY.")]
        public string? ApiKey { get; set; }
    }

    [Verb("models", HelpText = "Work with models.")]
    public class ModelsOptions : ApiOptions 
    {
        [Option('l', "list", Required = false, HelpText = "List all models.")]
        public bool List { get; set; }

        [Option("inspect", Required = false, HelpText = "Inspect a model with the specified model ID.")]
        public bool Inspect { get; set; }

        [Option('r', "run", Required = false, HelpText = "Run a model with the specified model ID.")]
        public bool Run { get; set; }

        [Option('i', "input", Required = false, HelpText = "Comma-delimited list of input files for a run operation.")]
        public string? Input { get; set; }

        [Value(0, Required = false)]
        public string? ModelId { get; set; }


    }
}
