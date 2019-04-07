using Newtonsoft.Json;
using System;

namespace PortfolioFunction.Models.CoinMarketCap
{

    public partial class ResponseHeader
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("error_code")]
        public long ErrorCode { get; set; }

        [JsonProperty("error_message")]
        public object ErrorMessage { get; set; }

        [JsonProperty("elapsed")]
        public long Elapsed { get; set; }

        [JsonProperty("credit_count")]
        public long CreditCount { get; set; }
    }

    public partial class ResponseHeader
    {
        public static ResponseHeader FromJson(string json) => JsonConvert.DeserializeObject<ResponseHeader>(json, Models.Converter.Settings);
    }

    //public static class Serialize
    //{
    //    public static string ToJson(this ResponseHeader self) => JsonConvert.SerializeObject(self, Models.Converter.Settings);
    //}

    //internal static class Converter
    //{
    //    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    //    {
    //        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
    //        DateParseHandling = DateParseHandling.None,
    //        Converters =
    //        {
    //            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
    //        },
    //    };
    //}
}
