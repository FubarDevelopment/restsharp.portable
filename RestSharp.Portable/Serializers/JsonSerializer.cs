using System.Net.Http.Headers;
using System.Text;

namespace RestSharp.Portable.Serializers
{
    /// <summary>
    /// The default JSON serializer using Json.Net
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        private static readonly Encoding _encoding = new UTF8Encoding(false);
        private static readonly JsonSerializer _defaultJsonSerializer = new JsonSerializer();

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer" /> class.
        /// </summary>
        public JsonSerializer()
        {
            ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = _encoding.WebName,
            };
        }

        /// <summary>
        /// Gets or sets the content type produced by the serializer
        /// </summary>
        /// <remarks>
        /// This serializer will return application/json
        /// </remarks>
        public MediaTypeHeaderValue ContentType { get; set; }

        /// <summary>
        /// Gets the default JSON serializer for <see cref="RestRequestExtensions.AddJsonBody"/>
        /// </summary>
        internal static JsonSerializer Default
        {
            get { return _defaultJsonSerializer; }
        }

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
