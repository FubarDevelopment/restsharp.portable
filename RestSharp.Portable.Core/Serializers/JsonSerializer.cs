using System.Text;

namespace RestSharp.Portable.Serializers
{
    /// <summary>
    /// The default JSON serializer using Json.Net
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        private static readonly Encoding _encoding = new UTF8Encoding(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer" /> class.
        /// </summary>
        public JsonSerializer()
        {
            ContentType = string.Format("application/json; charset={0}", _encoding.WebName);
        }

        /// <summary>
        /// Gets or sets the content type produced by the serializer
        /// </summary>
        /// <remarks>
        /// This serializer will return application/json
        /// </remarks>
        public string ContentType { get; set; }

        /// <summary>
        /// Serialize the object into a byte array
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>Byte array to send in the request body</returns>
        public byte[] Serialize(object obj)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            ConfigureSerializer(serializer);
            var output = new System.IO.MemoryStream();
            using (var writer = new System.IO.StreamWriter(output))
                serializer.Serialize(writer, obj);
            return output.ToArray();
        }

        /// <summary>
        /// Configure the <see cref="JsonSerializer"/>
        /// </summary>
        /// <param name="serializer">The serializer to configure</param>
        protected virtual void ConfigureSerializer(Newtonsoft.Json.JsonSerializer serializer)
        {
        }
    }
}
