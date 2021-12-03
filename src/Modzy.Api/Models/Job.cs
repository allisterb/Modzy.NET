namespace Modzy
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Job
    {
        [JsonProperty("model")]
        public JobModel Model { get; set; } = new JobModel();

        [JsonProperty("status")]
        public string Status { get; set; } = "";

        [JsonProperty("totalInputs")]
        public long TotalInputs { get; set; }

        [JsonProperty("jobIdentifier")]
        public string JobIdentifier { get; set; } = "";

        [JsonProperty("accessKey")]
        public string AccessKey { get; set; } = "";

        [JsonProperty("explain")]
        public bool Explain { get; set; }

        [JsonProperty("jobType")]
        public string JobType { get; set; } = "";

        [JsonProperty("accountIdentifier")]
        public string AccountIdentifier { get; set; } = "";

        [JsonProperty("team")]
        public Team Team { get; set; } = new Team();

        [JsonProperty("user")]
        public User User { get; set; } = new User();

        [JsonProperty("project")]
        public Project Project { get; set; } = new Project();

        [JsonProperty("jobInputs")]
        public object JobInputs { get; set; } = new object();

        [JsonProperty("submittedAt")]
        public DateTimeOffset SubmittedAt { get; set; }

        [JsonProperty("hoursDeleteInput")]
        public long HoursDeleteInput { get; set; }

        [JsonProperty("imageClassificationModel")]
        public bool ImageClassificationModel { get; set; }
    }

    public partial class JobInput 
    {
        
        public JobInputIdentifier[] Identifier { get; set; } = Array.Empty<JobInputIdentifier>();
    }

    public partial class JobModel
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = "";

        [JsonProperty("version")]
        public string Version { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";
    }

    public partial class Project
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";
    }

    public partial class Team
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = "";
    }

    public partial class User
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = "";

        [JsonProperty("externalIdentifier")]
        public string ExternalIdentifier { get; set; } = "";

        [JsonProperty("firstName")]
        public string FirstName { get; set; } = "";

        [JsonProperty("lastName")]
        public string LastName { get; set; } = "";

        [JsonProperty("email")]
        public string Email { get; set; } = "";

        [JsonProperty("status")]
        public string Status { get; set; } = "";
    }

    public class JobInputIdentifier
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = "";

    }

    internal class JobInputConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(JobInput);

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                var value = serializer.Deserialize<JobInputIdentifier>(reader);
                return new JobInput() { Identifier = new JobInputIdentifier[] { value! } } ;
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                var value = serializer.Deserialize<JobInputIdentifier[]>(reader);
                var o = new JobInput() { Identifier = value!};
                return o;
            }
            throw new Exception("Cannot unmarshal type JobInput");
        }


        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            JobInput value = (JobInput) untypedValue;
            if (value.Identifier.Length == 1)
            {
                serializer.Serialize(writer, value.Identifier[0]);
                return;
            }
            else
            {
                serializer.Serialize(writer, value);
                return;
            }
            throw new Exception("Cannot marshal type JobInput");
        }

        public static readonly JobInputConverter Singleton = new JobInputConverter();
    }

}
