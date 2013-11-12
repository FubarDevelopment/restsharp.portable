using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Serializers
{
    public class JsonSerializer : ISerializer
    {
        public JsonSerializer()
        {
            ContentType = "application/json";
        }

        public byte[] Serialize(object obj)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var output = new System.IO.MemoryStream();
            using (var writer = new System.IO.StreamWriter(output))
                serializer.Serialize(writer, obj);
            return output.ToArray();
        }

        public string DateFormat { get; set; }

        public string ContentType { get; set; }
    }
}
