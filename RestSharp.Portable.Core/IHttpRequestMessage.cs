using System;

namespace RestSharp.Portable
{
    /// <summary>
    /// Interface to a request message
    /// </summary>
    public interface IHttpRequestMessage
    {
        /// <summary>
        /// Gets the HTTP headers for the request message
        /// </summary>
        IHttpHeaders Headers { get; }

        /// <summary>
        /// Gets or sets the HTTP request method
        /// </summary>
        Method Method { get; set; }

        /// <summary>
        /// Gets or sets the request URI
        /// </summary>
        Uri RequestUri { get; set; }

        /// <summary>
        /// Gets or sets the HTTP protocol version
        /// </summary>
        Version Version { get; set; }

        /// <summary>
        /// Gets or sets the content of the request message
        /// </summary>
        IHttpContent Content { get; set; }
    }
}
