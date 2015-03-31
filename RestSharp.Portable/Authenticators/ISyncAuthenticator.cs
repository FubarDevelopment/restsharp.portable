using System;
using System.Net.Http;

using JetBrains.Annotations;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The synchronous authenticator interface
    /// </summary>
    public interface ISyncAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        void PreAuthenticate([NotNull] IRestClient client, [NotNull] IRestRequest request);

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        void HandleChallenge([NotNull] IRestClient client, [NotNull] IRestRequest request, [NotNull] HttpResponseMessage response);
    }
}
