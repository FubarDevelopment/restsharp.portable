using System;
using System.Net;

namespace RestSharp.Portable.WebRequest.Impl.Http
{
    /// <summary>
    /// This class wraps a <see cref="IRequestProxy" /> and provides a <see cref="IWebProxy" /> compatible interface.
    /// </summary>
    public class RequestProxyWrapper : IWebProxy
    {
        private readonly IRequestProxy _proxy;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestProxyWrapper"/> class.
        /// </summary>
        /// <param name="proxy">The request proxy to pass all properties and functions to.</param>
        public RequestProxyWrapper(IRequestProxy proxy)
        {
            _proxy = proxy;
        }

        /// <summary>
        /// Gets or sets the authentication credentials that will be sent to the proxy.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Net.ICredentials"/> instance that will be used to determine the authentication credentials sent to the proxy.
        /// </returns>
        public ICredentials Credentials
        {
            get { return _proxy.Credentials; }
            set { _proxy.Credentials = value; }
        }

        /// <summary>
        /// Returns the URI for the proxy for a given destination
        /// </summary>
        /// <param name="destination">The destination URL</param>
        /// <returns>The proxy to use for the given destination (or null)</returns>
        public Uri GetProxy(Uri destination)
        {
            return _proxy.GetProxy(destination);
        }

        /// <summary>
        /// Is the proxy bypassed for the given URL?
        /// </summary>
        /// <param name="host">The host to be tested</param>
        /// <returns>true if the host isn't passed through a proxy</returns>
        public bool IsBypassed(Uri host)
        {
            return _proxy.IsBypassed(host);
        }
    }
}
