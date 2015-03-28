using System;
using System.Net;
using System.Net.Http;

namespace RestSharp.Portable
{
    /// <summary>
    /// The authenticator interface
    /// </summary>
    public interface IRestAuthenticationModuleSync : IRestAuthenticationModule
    {
        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        void PreAuthenticate(IRestClient client, IRestRequest request, AuthHeader header, NetworkCredential credential);

        /// <summary>
        /// Will be called in response to an authentication request.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        /// <returns>true when the authentication succeeded</returns>
        bool Authenticate(IRestClient client, IRestRequest request, HttpResponseMessage response, AuthHeader header, NetworkCredential credential);
    }
}
