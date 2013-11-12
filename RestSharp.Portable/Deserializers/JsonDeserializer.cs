using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Deserializers
{
    public class JsonDeserializer : IDeserializer
    {
        public T Deserialize<T>(IRestResponse response)
        {
            var input = new MemoryStream(response.RawBytes);
            using (var reader = new StreamReader(input))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(new JsonTextReader(reader));
            }
        }

        public string DateFormat { get; set; }
    }
}
