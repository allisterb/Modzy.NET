using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        [Option('t', "token", Required = false, HelpText = "Your TigerGraph server instance access token. If none is specified then use the environment variable TG_TOKEN.")]
        public string? Token { get; set; }

        [Option('r', "rest", Required = false, HelpText = "Your TigerGraph REST++ server instance URL. If none is specified then use the environment variable TG_REST_SERVER_URL.")]
        public string? RestServerUrl { get; set; }

        [Option('q', "gsql", Required = false, HelpText = "Your TigerGraph GSQL server instance URL. If none is specified then use the environment variable TG_GSQL_SERVER_URL.")]
        public string? GsqlServerUrl { get; set; }

        [Option('u', "user", Required = false, HelpText = "Your TigerGraph server user name. If none is specified then use the environment variable TG_USR.")]
        public string? User { get; set; }

        [Option("pass", Required = false, HelpText = "Your TigerGraph server user password. If none is specified then use the environment variable TG_PASS.")]
        public string? Pass { get; set; }
    }

    [Verb("ping", HelpText = "Ping a TigerGraph server instance using the specified server URL and access token.")]
    public class PingOptions : ApiOptions {}

    [Verb("endpoints", HelpText = "Get the endpoints of the specified server.")]
    public class EndpointsOptions : ApiOptions {}

    [Verb("schema", HelpText = "Get the schema of the specified graph or of a specified vertex or edge type.")]
    public class SchemaOptions : ApiOptions 
    {
        [Option('g', "graph", Required = false, Default = "MyGraph", HelpText = "The name of the graph.")]
        public string? Graph { get; set; }

        [Option('v', "vertex", Required = false, HelpText = "The vertex type to retrieve the schema for.")]
        public string? Vertex { get; set; }

        [Option('e', "edge", Required = false, HelpText = "The edge type to retrieve the schema for.")]
        public string? Edge { get; set; }
    }

    [Verb("vertices", HelpText = "Get the vertices of the specified graph of a specified vertex type and optionally with a specified vertex id.")]
    public class VerticesOptions : ApiOptions
    {
        [Option('g', "graph", Required = false, Default = "MyGraph", HelpText = "The name of the graph.")]
        public string? Graph { get; set; }

        [Option('v', "vertex", Required = true, HelpText = "The vertex type to retrieve the data for.")]
        public string? Vertex { get; set; }

        [Option('i', "id", Required = false, HelpText = "A specific vertex or edge id to retrieve.")]
        public string? Id { get; set; }

        [Option('c', "count", Required = false, HelpText = "Only count the number of vertices of the specified type.")]
        public bool Count { get; set; }
    }
    
    [Verb("builtin", HelpText = "Execute a builtin function on the specified graph.")]
    public class BuiltinOptions : ApiOptions
    {
        [Option('g', "graph", Required = false, Default = "MyGraph", HelpText = "The name of the graph.")]
        public string? Graph { get; set; }

        [Option('f', "func", Required = true, HelpText = "The name of the builtin function to execute.")]
        public string? Fn { get; set; }

        [Option('t', "function", Required = false, Default = "", HelpText = "The vertex or edge type to execute the function against.")]
        public string? FnType { get; set; }
    }
}
