using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitCoin.Trade.MtGox.Converters
{
    public static class PriceConverter
    {
        public static decimal GetFactor(Currency currency)
        {
            switch (currency)
            {
                case Currency.BTC:
                    return 100000000m;
                case Currency.JPY:
                case Currency.SEK:
                    return 1000m;
            }
            return 100000m;
        }

        public static decimal ToDecimal(long value, Currency currency)
        {
            return (decimal)value / GetFactor(currency);
        }

        public static long ToLong(decimal value, Currency currency)
        {
            return (long)(value / GetFactor(currency));
        }
    }
}
