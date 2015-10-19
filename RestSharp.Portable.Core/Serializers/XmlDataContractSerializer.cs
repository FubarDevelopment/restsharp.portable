using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace RestSharp.Portable.Serializers
{
    /// <summary>
    /// Serializes an object using the DataContractSerializer
    /// </summary>
    public class XmlDataContractSerializer : ISerializer
    {
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false);
        private string _contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDataContractSerializer" /> class.
        /// </summary>
        public XmlDataContractSerializer()
        {
            XmlWriterSettings = new XmlWriterSettings
            {
                Encoding = _defaultEncoding,
            };
        }

        /// <summary>
        /// Gets or sets the content type produced by the serializer
        /// </summary>
        /// <remarks>
        /// As long as there is no manually set content type, the content type character set will always reflect the encoding
        /// of the XmlWriterSettings.
        /// </remarks>
        public string ContentType
        {
            get
            {
                if (_contentType == null)
                    return $"text/xml; charset={XmlWriterSettings.Encoding.WebName}";
                return _contentType;
            }
            set
            {
                _contentType = value;
            }
        }

        /// <summary>
        /// Gets the default XML serializer for AddXmlBody
        /// </summary>
        internal static XmlDataContractSerializer Default { get; } = new XmlDataContractSerializer();

        /// <summary>
        /// Gets or sets the configuration used to create an XML writer
        /// </summary>
        protected XmlWriterSettings XmlWriterSettings { get; set; }

        /// <summary>
        /// Serialize the object into a byte array
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>Byte array to send in the request body</returns>
        public byte[] Serialize(object obj)
        {
            var serializer = CreateSerializer(obj);
            var temp = new MemoryStream();
            using (var writer = XmlWriter.Create(temp, XmlWriterSettings))
            {
                serializer.WriteObject(writer, obj);
            }

            var result = temp.ToArray();
            return result;
        }

        /// <summary>
        /// Create a new data contract serializer
        /// </summary>
        /// <param name="obj">The object to create the serializer for</param>
        /// <returns>A new instance of the serializer for the given instance.</returns>
        protected virtual DataContractSerializer CreateSerializer(object obj)
        {
            return new DataContractSerializer(obj.GetType());
        }
    }
}
