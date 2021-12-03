namespace Modzy
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class ModelSampleInputSource : Dictionary<string, object>
    {

    }
    
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
        
        public Dictionary<string, Dictionary<string, ModelSampleInputSource>> Sources { get; set; } = new Dictionary<string, Dictionary<string, ModelSampleInputSource>>();
    }

    public partial class ModelSampleInputName
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = "";

        [JsonProperty("version")]
        public string Version { get; set; } = "";
    }

    internal class ModelSampleInputSourceConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(ModelSampleInputSource);

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.String)
            {
                var value = serializer.Deserialize<string>(reader);
                return new ModelSampleInputSource() { { "input", value! } };
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                var value = serializer.Deserialize<Dictionary<string, object>>(reader);
                var o = new ModelSampleInputSource();
                foreach (var key in value!.Keys)
                {
                    o.Add(key, value![key]);
                }
                return o;
            }
            throw new Exception("Cannot unmarshal type ModelSampleInputSource");
        }


        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            ModelSampleInputSource value = (ModelSampleInputSource)untypedValue;
            if (value.Count == 1 && value.Keys.Single() == "input")
            {
                serializer.Serialize(writer, value["value"]);
                return;
            }
            else
            {
                serializer.Serialize(writer, value);
                return;
            }
            throw new Exception("Cannot marshal type ModelSampleInputSource");
        }

        public static readonly ModelSampleInputSourceConverter Singleton = new ModelSampleInputSourceConverter();
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                ModelSampleInputSourceConverter.Singleton,
                JobInputConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
