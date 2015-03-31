using System.Net.Http;
using System.Threading.Tasks;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Abstract base class for asynchronous authenticators
    /// </summary>
    public abstract class AsyncAuthenticator : IAuthenticator, IAsyncAuthenticator
    {
        /// <summary>
        /// Gets a value indicating whether the authentication module supports pre-authentication.
        /// </summary>
        public abstract bool CanPreAuthenticate { get; }

        /// <summary>
        /// Gets a value indicating whether the authentication module supports pre-authentication.
        /// </summary>
        bool IAuthenticator.CanPreAuthenticate
        {
            get { return CanPreAuthenticate; }
        }

        /// <summary>
        /// Gets a value indicating whether the authentication module can handle the challenge sent with the response.
        /// </summary>
        public abstract bool CanHandleChallenge(HttpResponseMessage response);

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        /// <returns>Task where the handler for a failed authentication gets executed</returns>
        public abstract Task HandleChallenge(IRestClient client, IRestRequest request, HttpResponseMessage response);

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <returns>The task the authentication is performed on</returns>
        public abstract Task PreAuthenticate(IRestClient client, IRestRequest request);

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        void IAuthenticator.PreAuthenticate(IRestClient client, IRestRequest request)
        {
            PreAuthenticate(client, request).Wait();
        }

        /// <summary>
        /// Gets a value indicating whether the authentication module can handle the challenge sent with the response.
        /// </summary>
        bool IAuthenticator.CanHandleChallenge(HttpResponseMessage response)
        {
            return CanHandleChallenge(response);
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        void IAuthenticator.HandleChallenge(IRestClient client, IRestRequest request, HttpResponseMessage response)
        {
            HandleChallenge(client, request, response).Wait();
        }
    }
}
