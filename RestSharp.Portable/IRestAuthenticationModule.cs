using System;
using System.Net;

namespace RestSharp.Portable
{
    /// <summary>
    /// The authenticator interface
    /// </summary>
    public interface IRestAuthenticationModule
    {
        /// <summary>
        /// Gets the authentication type provided by this authentication module.
        /// </summary>
        string AuthenticationType { get; }

        /// <summary>
        /// Gets a value indicating whether the authentication module supports pre-authentication.
        /// </summary>
        bool CanPreAuthenticate { get; }
    }
}
