using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Deserializers
{
    /// <summary>
    /// The default JSON deserializer using Json.Net
    /// </summary>
    public class JsonDeserializer : IDeserializer
    {
        /// <summary>
        /// Deserialize the response
        /// </summary>
        /// <typeparam name="T">Object type to deserialize the result to</typeparam>
        /// <param name="response">The response to deserialize the result from</param>
        /// <returns>The deserialized object</returns>
        public T Deserialize<T>(IRestResponse response)
        {
            var input = new MemoryStream(response.RawBytes);
            using (var reader = new StreamReader(input))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(new JsonTextReader(reader));
            }
        }

        /// <summary>
        /// The date format to use during the deserialization
        /// </summary>
        public string DateFormat { get; set; }
    }
}
