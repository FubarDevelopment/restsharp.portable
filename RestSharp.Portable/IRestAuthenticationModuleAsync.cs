using System;
using System.Net;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// The async authenticator interface
    /// </summary>
    public interface IRestAuthenticationModuleAsync : IRestAuthenticationModule
    {
        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        Task PreAuthenticate(IRestClient client, IRestRequest request, AuthHeader header, NetworkCredential credential);

        /// <summary>
        /// Will be called in response to an authentication request.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        /// <returns>The task the authentication is performed on, which returns true when the authentication succeeded</returns>
        Task<bool> Authenticate(IRestClient client, IRestRequest request, IRestResponse response, AuthHeader header, NetworkCredential credential);
    }
}
