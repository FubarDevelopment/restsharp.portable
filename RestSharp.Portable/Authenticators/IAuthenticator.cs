using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The base authenticator interface.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Does the authentication module supports pre-authentication for the given <see cref="IRestRequest" />?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        bool CanPreAuthenticate([NotNull] IRestClient client, [NotNull] IRestRequest request, ICredentials credentials);

        /// <summary>
        /// Does the authentication module supports pre-authentication for the given <see cref="HttpRequestMessage" />?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        bool CanPreAuthenticate([CanBeNull] HttpClient client, [NotNull] HttpRequestMessage request, ICredentials credentials);

        /// <summary>
        /// Determines if the authentication module can handle the challenge sent with the response.
        /// </summary>
        /// <param name="client">The HTTP client the response is assigned to</param>
        /// <param name="request">The HTTP request the response is assigned to</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <param name="response">The response that returned the authentication challenge</param>
        /// <returns>true when the authenticator can handle the sent challenge</returns>
        bool CanHandleChallenge([CanBeNull] HttpClient client, [NotNull] HttpRequestMessage request, ICredentials credentials, [NotNull] HttpResponseMessage response);

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        Task PreAuthenticate([NotNull] IRestClient client, [NotNull] IRestRequest request, ICredentials credentials);

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        Task PreAuthenticate([CanBeNull] HttpClient client, [NotNull] HttpRequestMessage request, ICredentials credentials);

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <param name="response">Response of the failed request</param>
        /// <returns>Task where the handler for a failed authentication gets executed</returns>
        Task HandleChallenge([CanBeNull] HttpClient client, [NotNull] HttpRequestMessage request, ICredentials credentials, [NotNull] HttpResponseMessage response);
    }
}
