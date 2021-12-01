namespace Modzy
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class JobListing
    {
        [JsonProperty("jobIdentifier")]
        public Guid JobIdentifier { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("model")]
        public JobModel Model { get; set; } = new JobModel();
    }
}
