using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace RestSharp.Portable.Serializers
{
    /// <summary>
    /// The default JSON serializer using Json.Net
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        private static Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// Constructor which initializes this serializer
        /// </summary>
        public JsonSerializer()
        {
            ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = _encoding.WebName,
            };
        }

        /// <summary>
        /// Serialize the object into a byte array
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>Byte array to send in the request body</returns>
        public byte[] Serialize(object obj)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var output = new System.IO.MemoryStream();
            using (var writer = new System.IO.StreamWriter(output))
                serializer.Serialize(writer, obj);
            return output.ToArray();
        }

        /// <summary>
        /// The date format to use during the serialization
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Content type produced by the serializer
        /// </summary>
        /// <remarks>
        /// This serializer will return application/json
        /// </remarks>
        public MediaTypeHeaderValue ContentType { get; set; }
    }
}
