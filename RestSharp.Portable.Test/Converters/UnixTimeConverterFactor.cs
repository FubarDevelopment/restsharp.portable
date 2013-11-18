using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitCoin.Trade.MtGox.Converters
{
    public class UnixTimeConverterFactor : JsonConverter
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private double MulFactor { get; set; }
        private double DivFactor { get; set; }
        public UnixTimeConverterFactor(double mulFactor, double divFactor)
        {
            MulFactor = mulFactor;
            DivFactor = divFactor;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(DateTime) == objectType
                || typeof(DateTimeOffset) == objectType
                || typeof(DateTime?) == objectType
                || typeof(DateTimeOffset?) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<long?>(reader);
            if (value == null)
                return null;
            var result = _epoch + TimeSpan.FromMilliseconds(value.Value * MulFactor / DivFactor);
            if (typeof(DateTime) == objectType || typeof(DateTime?) == objectType)
                return result;
            return new DateTimeOffset(result);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
