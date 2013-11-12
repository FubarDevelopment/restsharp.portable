using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace RestSharp.Portable.Serializers
{
    public class JsonSerializer : ISerializer
    {
        private static Encoding _encoding = Encoding.UTF8;

        public JsonSerializer()
        {
            ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = _encoding.WebName,
            };
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

        public MediaTypeHeaderValue ContentType { get; set; }
    }
}
