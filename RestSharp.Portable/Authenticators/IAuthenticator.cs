using System;
using System.Net.Http;

using JetBrains.Annotations;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The authenticator interface
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Gets a value indicating whether the authentication module supports pre-authentication.
        /// </summary>
        bool CanPreAuthenticate { get; }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        void PreAuthenticate([NotNull] IRestClient client, [NotNull] IRestRequest request);

        /// <summary>
        /// Gets a value indicating whether the authentication module can handle the challenge sent with the response.
        /// </summary>
        bool CanHandleChallenge([NotNull] HttpResponseMessage response);

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        void HandleChallenge([NotNull] IRestClient client, [NotNull] IRestRequest request, [NotNull] HttpResponseMessage response);
    }
}
