using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

using JetBrains.Annotations;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The base authenticator interface.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Gets a value indicating whether the authentication module supports pre-authentication.
        /// </summary>
        bool CanPreAuthenticate { get; }

        /// <summary>
        /// Gets a value indicating whether the authentication module can handle the challenge sent with the response.
        /// </summary>
        bool CanHandleChallenge([NotNull] HttpResponseMessage response);
    }
}
