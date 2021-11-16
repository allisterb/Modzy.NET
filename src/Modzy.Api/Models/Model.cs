namespace Modzy
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Model
    {
        [JsonProperty("modelId")]
        public string ModelId { get; set; }  = "";

        [JsonProperty("latestVersion")]
        public LatestVersion LatestVersion { get; set; } = LatestVersion.The001;

        [JsonProperty("versions")]
        public string[] Versions { get; set; } = Array.Empty<string>();
    }

    public enum LatestVersion 
    { 
        [EnumMember(Value = "0.0.1")]
        The001,
        [EnumMember(Value = "1.0.2")]
        The102,
        [EnumMember(Value = "4.1.0")]
        The410 
    };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                LatestVersionConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class LatestVersionConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(LatestVersion) || t == typeof(LatestVersion?);

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "0.0.1":
                    return LatestVersion.The001;
                case "1.0.2":
                    return LatestVersion.The102;
                case "4.1.0":
                    return LatestVersion.The410;
            }
            throw new Exception("Cannot unmarshal type LatestVersion");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (LatestVersion)untypedValue;
            switch (value)
            {
                case LatestVersion.The001:
                    serializer.Serialize(writer, "0.0.1");
                    return;
                case LatestVersion.The102:
                    serializer.Serialize(writer, "1.0.2");
                    return;
                case LatestVersion.The410:
                    serializer.Serialize(writer, "4.1.0");
                    return;
            }
            throw new Exception("Cannot marshal type LatestVersion");
        }

        public static readonly LatestVersionConverter Singleton = new LatestVersionConverter();
    }
}
