using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
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
        private static Encoding _defaultEncoding = new UTF8Encoding(false);

        /// <summary>
        /// The configuration used to create an XML writer
        /// </summary>
        protected XmlWriterSettings XmlWriterSettings { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public XmlDataContractSerializer()
        {
            XmlWriterSettings = new System.Xml.XmlWriterSettings
            {
                Encoding = _defaultEncoding,
            };
        }

        /// <summary>
        /// Serialize the object into a byte array
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>Byte array to send in the request body</returns>
        public byte[] Serialize(object obj)
        {
            var serializer = CreateSerializer(obj);
            using (var temp = new MemoryStream())
            {
                using (var writer = System.Xml.XmlWriter.Create(temp, XmlWriterSettings))
                {
                    serializer.WriteObject(temp, obj);
                }
                var result = temp.ToArray();
                return result;
            }
        }

        private MediaTypeHeaderValue _defaultContentType;
        private MediaTypeHeaderValue _contentType;

        /// <summary>
        /// Content type produced by the serializer
        /// </summary>
        /// <remarks>
        /// As long as there is no manually set content type, the content type character set will always reflect the encoding
        /// of the XmlWriterSettings.
        /// </remarks>
        public MediaTypeHeaderValue ContentType
        {
            get
            {
                if (_contentType == null)
                {
                    if (_defaultContentType == null || _defaultContentType.CharSet != XmlWriterSettings.Encoding.WebName)
                    {
                        _defaultContentType = new MediaTypeHeaderValue("text/xml")
                        {
                            CharSet = XmlWriterSettings.Encoding.WebName,
                        };
                    }
                    return _defaultContentType;
                }
                return _contentType;
            }
            set
            {
                _contentType = value;
            }
        }

        /// <summary>
        /// Create a new data contract serializer
        /// </summary>
        /// <param name="obj">The object to create the serializer for</param>
        protected virtual DataContractSerializer CreateSerializer(object obj)
        {
            return new DataContractSerializer(obj.GetType());
        }
    }
}
