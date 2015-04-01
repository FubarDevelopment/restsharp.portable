using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Abstract base class for asynchronous authenticators
    /// </summary>
    public abstract class AsyncAuthenticator : ISyncAuthenticator, IAsyncAuthenticator
    {
        /// <summary>
        /// Dies the authentication module supports pre-authentication?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        public abstract bool CanPreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials);

        /// <summary>
        /// Determines if the authentication module can handle the challenge sent with the response.
        /// </summary>
        /// <param name="client">The REST client the response is assigned to</param>
        /// <param name="request">The REST request the response is assigned to</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <param name="response">The response that returned the authentication challenge</param>
        /// <returns>true when the authenticator can handle the sent challenge</returns>
        public abstract bool CanHandleChallenge(IRestClient client, IRestRequest request, ICredentials credentials, HttpResponseMessage response);

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <param name="response">Response of the failed request</param>
        /// <returns>Task where the handler for a failed authentication gets executed</returns>
        public abstract Task HandleChallenge(IRestClient client, IRestRequest request, ICredentials credentials, HttpResponseMessage response);

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        public abstract Task PreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials);

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        void ISyncAuthenticator.PreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials)
        {
            PreAuthenticate(client, request, credentials).Wait();
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <param name="response">Response of the failed request</param>
        void ISyncAuthenticator.HandleChallenge(IRestClient client, IRestRequest request, ICredentials credentials, HttpResponseMessage response)
        {
            HandleChallenge(client, request, credentials, response).Wait();
        }
    }
}
