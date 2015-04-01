using System;
using System.Net;
using System.Net.Http;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Tries to Authenticate with the credentials of the currently logged in user, or impersonate a user
    /// </summary>
    public class NtlmAuthenticator : ISyncAuthenticator
    {
        private readonly ICredentials _credentials;

        /// <summary>
        /// Initializes a new instance of the <see cref="NtlmAuthenticator" /> class.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="password">The users password</param>
        public NtlmAuthenticator(string username, string password)
            : this(new NetworkCredential(username, password))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NtlmAuthenticator" /> class.
        /// </summary>
        /// <param name="credentials">The credentials to use</param>
        public NtlmAuthenticator(ICredentials credentials)
        {
            if (credentials == null)
                throw new ArgumentNullException("credentials");
            _credentials = credentials;
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
        public void PreAuthenticate(IRestClient client, IRestRequest request)
        {
            request.Credentials = _credentials;
        }

        /// <summary>
        /// Determines if the authentication module can handle the challenge sent with the response.
        /// </summary>
        /// <param name="response">The response that returned the authentication challenge</param>
        /// <returns>true when the authenticator can handle the sent challenge</returns>
        public bool CanHandleChallenge(HttpResponseMessage response)
        {
            return false;
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        public void HandleChallenge(IRestClient client, IRestRequest request, HttpResponseMessage response)
        {
            throw new NotSupportedException();
        }
    }
}
