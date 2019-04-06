using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace PortfolioFunction.Models
{

    public partial class Transaction
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("transactiontype")]
        public string Transactiontype { get; set; }
    }

    public partial class Transaction
    {
        public static Transaction FromJson(string json) => JsonConvert.DeserializeObject<Transaction>(json, Models.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Transaction self) => JsonConvert.SerializeObject(self, Models.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}

