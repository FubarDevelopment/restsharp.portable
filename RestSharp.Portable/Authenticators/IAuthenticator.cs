using System;
using System.Net;
using System.Net.Http;

using JetBrains.Annotations;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The base authenticator interface.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Dies the authentication module supports pre-authentication?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        bool CanPreAuthenticate([NotNull] IRestClient client, [NotNull] IRestRequest request, ICredentials credentials);

        /// <summary>
        /// Determines if the authentication module can handle the challenge sent with the response.
        /// </summary>
        /// <param name="client">The REST client the response is assigned to</param>
        /// <param name="request">The REST request the response is assigned to</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <param name="response">The response that returned the authentication challenge</param>
        /// <returns>true when the authenticator can handle the sent challenge</returns>
        bool CanHandleChallenge([NotNull] IRestClient client, [NotNull] IRestRequest request, ICredentials credentials, [NotNull] HttpResponseMessage response);
    }
}
