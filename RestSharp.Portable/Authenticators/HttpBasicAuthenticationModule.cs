using System;
using System.Net;
using System.Text;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The default HTTP Basic authenticator
    /// </summary>
    public class HttpBasicAuthenticationModule : IRestAuthenticationModule
    {
        /// <summary>
        /// Gets the authentication type provided by this authentication module.
        /// </summary>
        public string AuthenticationType
        {
            get { return "Basic"; }
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
            if (credential == null)
                return;
            var token = CreateToken(credential);
            var authHeader = string.Format("{0} {1}", AuthenticationType, token);
            AuthHeaderUtilities.TrySetAuthorizationHeader(client, request, header, authHeader);
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
            IRestResponse response,
            AuthHeader header,
            NetworkCredential credential)
        {
            if (credential == null)
                return false;
            var token = CreateToken(credential);
            var authHeader = string.Format("{0} {1}", AuthenticationType, token);
            return AuthHeaderUtilities.TrySetAuthorizationHeader(client, request, header, authHeader);
        }

        private string CreateToken(NetworkCredential credential)
        {
            var userName = string.IsNullOrEmpty(credential.Domain)
                ? credential.UserName
                : string.Format("{0}\\{1}", credential.Domain, credential.UserName);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", userName, credential.Password)));
        }
    }
}
