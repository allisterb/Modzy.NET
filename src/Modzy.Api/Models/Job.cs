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
        public Guid JobIdentifier { get; set; }

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
        public object JobInputs { get; set; } = new object();// Array.Empty<JobInput>();

        [JsonProperty("submittedAt")]
        public DateTimeOffset SubmittedAt { get; set; }

        [JsonProperty("hoursDeleteInput")]
        public long HoursDeleteInput { get; set; }

        [JsonProperty("imageClassificationModel")]
        public bool ImageClassificationModel { get; set; }
    }

    public partial class JobInput
    {
        [JsonProperty("identifier")]
        public object Identifier { get; set; } = new object();
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
        public Guid Identifier { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = "";
    }

    public partial class Team
    {
        [JsonProperty("identifier")]
        public Guid Identifier { get; set; }
    }

    public partial class User
    {
        [JsonProperty("identifier")]
        public Guid Identifier { get; set; }

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
}
