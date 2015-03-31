using System;
using System.Net;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Tries to Authenticate with the credentials of the currently logged in user, or impersonate a user
    /// </summary>
    public class NtlmAuthenticator : IAuthenticator
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
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        public void PreAuthenticate(IRestClient client, IRestRequest request)
        {
            request.Credentials = _credentials;
        }
    }
}
