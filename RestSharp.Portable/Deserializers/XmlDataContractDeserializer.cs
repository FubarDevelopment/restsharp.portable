using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace RestSharp.Portable.Deserializers
{
    /// <summary>
    /// Deserializes a XML using the DataContractSerializer
    /// </summary>
    public class XmlDataContractDeserializer : IDeserializer
    {
        /// <summary>
        /// Deserialize the response
        /// </summary>
        /// <typeparam name="T">Object type to deserialize the result to</typeparam>
        /// <param name="response">The response to deserialize the result from</param>
        /// <returns>The deserialized object</returns>
        public T Deserialize<T>(IRestResponse response)
        {
            var serializer = CreateSerializer(typeof(T));
            using (var input = new MemoryStream(response.RawBytes))
            {
                var result = serializer.ReadObject(input);
                return (T)result;
            }
        }

        /// <summary>
        /// Create a new data contract serializer
        /// </summary>
        /// <param name="t">The type to create the serializer for</param>
        protected virtual DataContractSerializer CreateSerializer(Type t)
        {
            return new DataContractSerializer(t);
        }
    }
}
