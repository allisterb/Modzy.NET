namespace Modzy
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Model
    {
        [JsonProperty("modelId")]
        public string ModelId { get; set; } = "";

        [JsonProperty("latestVersion")]
        public string LatestVersion { get; set; } = "";

        [JsonProperty("latestActiveVersion")]
        public string LatestActiveVersion { get; set; } = "";

        [JsonProperty("versions")]
        public string[] Versions { get; set; } = Array.Empty<string>();

        [JsonProperty("author")]
        public string Author { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonProperty("permalink")]
        public string Permalink { get; set; } = "";

        [JsonProperty("features")]
        public object[] Features { get; set; } = Array.Empty<object>();

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("isRecommended")]
        public bool IsRecommended { get; set; }

        [JsonProperty("isCommercial")]
        public bool IsCommercial { get; set; }

        [JsonProperty("tags")]
        public Tag[] Tags { get; set; } = Array.Empty<Tag>();

        [JsonProperty("images")]
        public Image[] Images { get; set; } = Array.Empty<Image>();

        [JsonProperty("snapshotImages")]
        public object[] SnapshotImages { get; set; } = Array.Empty<object>();

        [JsonProperty("lastActiveDateTime")]
        public DateTimeOffset LastActiveDateTime { get; set; }

        [JsonProperty("visibility")]
        public Visibility? Visibility { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        [JsonProperty("caption")]
        public string Caption { get; set; } = "";

        [JsonProperty("relationType")]
        public string RelationType { get; set; } = "";
    }

    public partial class Tag
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("dataType")]
        public string DataType { get; set; } = "";

        [JsonProperty("isCategorical")]
        public bool IsCategorical { get; set; }
    }

    public partial class Visibility
    {
        [JsonProperty("scope")]
        public string Scope { get; set; } = "";
    }
}
