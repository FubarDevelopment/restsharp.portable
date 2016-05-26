using System.IO;

using Newtonsoft.Json;

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
                ConfigureSerializer(serializer);
                return serializer.Deserialize<T>(new JsonTextReader(reader));
            }
        }

        /// <summary>
        /// Configure the JsonSerializer
        /// </summary>
        /// <param name="serializer">The serializer to configure</param>
        protected virtual void ConfigureSerializer(JsonSerializer serializer)
        {
        }
    }
}
