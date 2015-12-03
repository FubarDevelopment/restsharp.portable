using System;
using System.Net;

using JetBrains.Annotations;

namespace RestSharp.Portable
{
    /// <summary>
    /// The HTTP response message
    /// </summary>
    public interface IHttpResponseMessage : IDisposable
    {
        /// <summary>
        /// Gets the HTTP headers returned by the response
        /// </summary>
        IHttpHeaders Headers { get; }

        /// <summary>
        /// Gets a value indicating whether the request was successful
        /// </summary>
        bool IsSuccessStatusCode { get; }

        /// <summary>
        /// Gets the reason phrase returned together with the status code
        /// </summary>
        string ReasonPhrase { get; }

        /// <summary>
        /// Gets the request message this response was returned for
        /// </summary>
        IHttpRequestMessage RequestMessage { get; }

        /// <summary>
        /// Gets the status code
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the content of the response
        /// </summary>
        [CanBeNull]
        IHttpContent Content { get; }

        /// <summary>
        /// Throws an exception when the status doesn't indicate success.
        /// </summary>
        void EnsureSuccessStatusCode();
    }
}
