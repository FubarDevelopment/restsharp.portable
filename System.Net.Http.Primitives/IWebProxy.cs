using System;

namespace System.Net
{
    /// <summary>
    /// Provides the base interface for implementation of proxy access for the WebRequest class.
    /// </summary>
    public interface IWebProxy
    {
        /// <summary>
        /// Gets or sets the credentials to submit to the proxy server for authentication.
        /// </summary>
        ICredentials Credentials { get; set; }

        /// <summary>
        /// Returns the URI of a proxy.
        /// </summary>
        /// <param name="destination">A <see cref="Uri"/> that specifies the requested Internet resource. </param>
        /// <returns>A <see cref="Uri"/> instance that contains the URI of the proxy used to contact <paramref name="destination"/>.</returns>
        Uri GetProxy(Uri destination);

        /// <summary>
        /// Indicates that the proxy should not be used for the specified host.
        /// </summary>
        /// <param name="host">The <see cref="Uri"/> of the host to check for proxy use.</param>
        /// <returns><code>true</code> if the proxy server should not be used for <paramref name="host"/>; otherwise, <code>false</code>.</returns>
        bool IsBypassed(Uri host);
    }
}
