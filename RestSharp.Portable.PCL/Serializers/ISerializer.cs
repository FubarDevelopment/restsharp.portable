using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace RestSharp.Portable.Serializers
{
    /// <summary>
    /// Serializer for a content type
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serialize the object into a byte array
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>Byte array to send in the request body</returns>
        byte[] Serialize(object obj);
        /// <summary>
        /// Content type produced by the serializer
        /// </summary>
        MediaTypeHeaderValue ContentType { get; set; }
    }
}
