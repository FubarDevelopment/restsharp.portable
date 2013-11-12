using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Deserializers
{
    public interface IDeserializer
    {
        T Deserialize<T>(IRestResponse response);
        string DateFormat { get; set; }
    }
}
