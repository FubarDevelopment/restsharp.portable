using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitCoin.Trade.MtGox.Requests
{
    public class CurrencyValue
    {
        [JsonIgnore()]
        public decimal Value
        {
            get
            {
                return Converters.PriceConverter.ToDecimal(ValueInt, Currency);
            }
            set
            {
                ValueInt = Converters.PriceConverter.ToLong(value, Currency);
            }
        }

        [JsonProperty("value_int")]
        protected long ValueInt { get; set; }
        [JsonProperty("display")]
        public string Display { get; set; }
        [JsonProperty("display_short")]
        public string DisplayShort { get; set; }
        [JsonProperty("currency")]
        public Currency Currency { get; set; }
    }
}
