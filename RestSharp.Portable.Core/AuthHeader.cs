using System;

namespace RestSharp.Portable
{
    /// <summary>
    /// The authentication/authorization header to use
    /// </summary>
    public enum AuthHeader
    {
        /// <summary>
        /// Authentication/authorization header for web sites
        /// </summary>
        Www,

        /// <summary>
        /// Authentication/authorization header for proxies
        /// </summary>
        Proxy
    }
}
