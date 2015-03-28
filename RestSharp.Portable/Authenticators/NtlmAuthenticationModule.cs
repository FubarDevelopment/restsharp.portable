using System;
using System.Net;
using System.Net.Http;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Tries to Authenticate with the credentials of the currently logged in user, or impersonate a user
    /// </summary>
    public class NtlmAuthenticationModule : IRestAuthenticationModule
    {
        /// <summary>
        /// Gets the authentication type provided by this authentication module.
        /// </summary>
        public string AuthenticationType
        {
            get { return "NTLM"; }
        }

        /// <summary>
        /// Gets a value indicating whether the authentication module supports pre-authentication.
        /// </summary>
        public bool CanPreAuthenticate
        {
            get { return true; }
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        public void PreAuthenticate(
            IRestClient client,
            IRestRequest request,
            AuthHeader header,
            NetworkCredential credential)
        {
            request.Credentials = credential;
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
        public bool Authenticate(
            IRestClient client,
            IRestRequest request,
            HttpResponseMessage response,
            AuthHeader header,
            NetworkCredential credential)
        {
            if (ReferenceEquals(request.Credentials, credential))
                return false;
            request.Credentials = credential;
            return true;
        }
    }
}
