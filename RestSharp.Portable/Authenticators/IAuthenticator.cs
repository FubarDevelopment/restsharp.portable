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
        /// Determines if the authentication module can handle the challenge sent with the response.
        /// </summary>
        /// <param name="client">The REST client the response is assigned to</param>
        /// <param name="request">The REST request the response is assigned to</param>
        /// <param name="response">The response that returned the authentication challenge</param>
        /// <returns>true when the authenticator can handle the sent challenge</returns>
        bool CanHandleChallenge([NotNull] IRestClient client, [NotNull] IRestRequest request, [NotNull] HttpResponseMessage response);
    }
}
