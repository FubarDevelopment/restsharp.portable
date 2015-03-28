using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Abstract base class for asynchronous authenticators
    /// </summary>
    public abstract class AsyncAuthenticationModule : IRestAuthenticationModuleSync, IRestAuthenticationModuleAsync
    {
        /// <summary>
        /// Gets the authentication type provided by this authentication module.
        /// </summary>
        public abstract string AuthenticationType { get; }

        /// <summary>
        /// Gets a value indicating whether the authentication module supports pre-authentication.
        /// </summary>
        public abstract bool CanPreAuthenticate { get; }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        public abstract Task PreAuthenticate(
            IRestClient client,
            IRestRequest request,
            AuthHeader header,
            NetworkCredential credential);

        /// <summary>
        /// Will be called in response to an authentication request.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        /// <returns>The task the authentication is performed on, which returns true when the authentication succeeded</returns>
        public abstract Task<bool> Authenticate(
            IRestClient client,
            IRestRequest request,
            HttpResponseMessage response,
            AuthHeader header,
            NetworkCredential credential);

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        void IRestAuthenticationModuleSync.PreAuthenticate(
            IRestClient client,
            IRestRequest request,
            AuthHeader header,
            NetworkCredential credential)
        {
            PreAuthenticate(client, request, header, credential).Wait();
        }

        /// <summary>
        /// Will be called in response to an authentication request.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        /// <returns>true when the authentication succeeded</returns>
        bool IRestAuthenticationModuleSync.Authenticate(
            IRestClient client,
            IRestRequest request,
            HttpResponseMessage response,
            AuthHeader header,
            NetworkCredential credential)
        {
            return Authenticate(client, request, response, header, credential).Result;
        }
    }
}
