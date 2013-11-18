using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitCoin.Trade.MtGox.Converters
{
    public class UnixTimeConverterMicroSec : UnixTimeConverterFactor
    {
        public UnixTimeConverterMicroSec()
            : base(1.0, 1000.0)
        {

        }
    }
}
