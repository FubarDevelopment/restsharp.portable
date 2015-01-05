using System;
using System.Net;
using System.Net.Http.Headers;

namespace RestSharp.Portable
{
    /// <summary>
    /// The generic REST response
    /// </summary>
    public interface IRestResponse
    {
        /// <summary>
        /// Request that resulted in this response
        /// </summary>
        IRestRequest Request { get; }
        /// <summary>
        /// The full response URL
        /// </summary>
        Uri ResponseUri { get; }
        /// <summary>
        /// The raw data
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        byte[] RawBytes { get; }
        /// <summary>
        /// Content type of the raw data
        /// </summary>
        string ContentType { get; }
        /// <summary>
        /// Response headers (without content headers)
        /// </summary>
        HttpHeaders Headers { get; }
        /// <summary>
        /// HTTP status code
        /// </summary>
        HttpStatusCode StatusCode { get; }
        /// <summary>
        /// Description for the HTTP status code
        /// </summary>
        string StatusDescription { get; }
    }

    /// <summary>
    /// Typed response
    /// </summary>
    /// <typeparam name="T">Type of the object to deserialize from the raw data</typeparam>
    public interface IRestResponse<T> : IRestResponse
    {
        /// <summary>
        /// Deserialized object of type T
        /// </summary>
        T Data { get; }
    }
}
