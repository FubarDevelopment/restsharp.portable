using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitCoin.Trade.MtGox.Requests
{
    public class TickerData
    {
        [JsonProperty("high")]
        public CurrencyValue High { get; set; }
        [JsonProperty("low")]
        public CurrencyValue Low { get; set; }
        [JsonProperty("avg")]
        public CurrencyValue Average { get; set; }
        [JsonProperty("vwap")]
        public CurrencyValue VolumeWeightedAveragePrice { get; set; }
        [JsonProperty("vol")]
        public CurrencyValue Volume { get; set; }
        [JsonProperty("last_local")]
        public CurrencyValue LastLocal { get; set; }
        [JsonProperty("last_orig")]
        public CurrencyValue LastOriginal { get; set; }
        [JsonProperty("last_all")]
        public CurrencyValue LastAll { get; set; }
        [JsonProperty("last")]
        public CurrencyValue Last { get; set; }
        [JsonProperty("buy")]
        public CurrencyValue Buy { get; set; }
        [JsonProperty("sell")]
        public CurrencyValue Sell { get; set; }
        [JsonProperty("item")]
        public Currency Item { get; set; }
        [JsonProperty("now")]
        [JsonConverter(typeof(Converters.UnixTimeConverterMicroSec))]
        public DateTimeOffset Timestamp { get; set; }
    }
}
