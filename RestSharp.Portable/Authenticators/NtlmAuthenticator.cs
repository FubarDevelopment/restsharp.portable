using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Tries to Authenticate with the credentials of the currently logged in user, or impersonate a user
    /// </summary>
    public class NtlmAuthenticator : IAuthenticator
    {
        private readonly ICredentials credentials;

        /// <summary>
        /// Authenticate by impersonation
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="password">Password</param>
        public NtlmAuthenticator(string username, string password)
            : this(new NetworkCredential(username, password))
        {
        }

        /// <summary>
        /// Authenticate by impersonation, using an existing <c>ICredentials</c> instance
        /// </summary>
        /// <param name="credentials">The credentials to use</param>
        public NtlmAuthenticator(ICredentials credentials)
        {
            if (credentials == null)
                throw new ArgumentNullException("credentials");
            this.credentials = credentials;
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.Credentials = credentials;
        }
    }
}
