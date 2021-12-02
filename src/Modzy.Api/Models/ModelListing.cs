namespace Modzy
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class ModelListing
    {
        [JsonProperty("modelId")]
        public string ModelId { get; set; }  = "";

        [JsonProperty("latestVersion")]
        public string LatestVersion { get; set; } = "";

        [JsonProperty("versions")]
        public string[] Versions { get; set; } = Array.Empty<string>();
    }


}
