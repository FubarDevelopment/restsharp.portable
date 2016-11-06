using System;
using System.Net;

using JetBrains.Annotations;

namespace RestSharp.Portable
{
    /// <summary>
    /// The generic REST response
    /// </summary>
    public interface IRestResponse
    {
        /// <summary>
        /// Gets the Request that resulted in this response
        /// </summary>
        IRestRequest Request { get; }

        /// <summary>
        /// Gets the full response URL
        /// </summary>
        Uri ResponseUri { get; }

        /// <summary>
        /// Gets the raw data
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Required for RestSharp compatibility")]
        byte[] RawBytes { get; }

        /// <summary>
        /// Gets the content type of the raw data
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets the collection of cookies
        /// </summary>
        [CanBeNull]
        CookieCollection Cookies { get; }

        /// <summary>
        /// Gets the response headers (without content headers)
        /// </summary>
        IHttpHeaders Headers { get; }

        /// <summary>
        /// Gets a value indicating whether the request was successful.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Gets the HTTP status code
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the description for the HTTP status code
        /// </summary>
        string StatusDescription { get; }

        /// <summary>
        /// Gets the string representation of response content
        /// </summary>
        string Content { get; }
    }
}
