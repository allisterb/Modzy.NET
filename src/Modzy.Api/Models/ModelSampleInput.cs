namespace Modzy
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class ModelSampleInput
    {
        [JsonProperty("model")]
        public ModelSampleInputName Model { get; set; } = new ModelSampleInputName();

        [JsonProperty("input")]
        public Input Input { get; set; } = new Input();
    }

    public partial class Input
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("accessKeyID")]
        public string AccessKeyId { get; set; } = "";

        [JsonProperty("secretAccessKey")]
        public string SecretAccessKey { get; set; } = "";

        [JsonProperty("region")]
        public string Region { get; set; } = "";

        [JsonProperty("sources")]
        public Dictionary<string, Dictionary<string, object>> Sources { get; set; } = new Dictionary<string, Dictionary<string, object>>();
    }

    public partial class ModelSampleInputName
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = "";

        [JsonProperty("version")]
        public string Version { get; set; } = "";
    }
}
