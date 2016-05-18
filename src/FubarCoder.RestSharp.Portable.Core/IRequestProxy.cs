using System;
using System.Net;

namespace RestSharp.Portable
{
    /// <summary>
    /// Interface that defines a proxy for a request
    /// </summary>
    public interface IRequestProxy
    {
        /// <summary>
        /// Gets or sets the credentials to be queried by the authentication modules
        /// </summary>
        ICredentials Credentials { get; set; }

        /// <summary>
        /// Returns the URI for the proxy for a given destination
        /// </summary>
        /// <param name="destination">The destination URL</param>
        /// <returns>The proxy to use for the given destination (or null)</returns>
        Uri GetProxy(Uri destination);

        /// <summary>
        /// Is the proxy bypassed for the given URL?
        /// </summary>
        /// <param name="host">The host to be tested</param>
        /// <returns>true if the host isn't passed through a proxy</returns>
        bool IsBypassed(Uri host);
    }
}
