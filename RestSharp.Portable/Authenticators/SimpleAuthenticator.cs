using System;
using System.Net.Http;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Simple authenticator that adds the authentication information as GetOrPost parameter
    /// </summary>
    public class SimpleAuthenticator : IAuthenticator
    {
        private readonly string _usernameKey;
        private readonly string _username;
        private readonly string _passwordKey;
        private readonly string _password;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAuthenticator" /> class.
        /// </summary>
        /// <param name="usernameKey">GetOrPost parameter name for the user name</param>
        /// <param name="username">User name</param>
        /// <param name="passwordKey">GetOrPost parameter name for the password</param>
        /// <param name="password">The users password</param>
        public SimpleAuthenticator(string usernameKey, string username, string passwordKey, string password)
        {
            _usernameKey = usernameKey;
            _username = username;
            _passwordKey = passwordKey;
            _password = password;
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
            request.AddParameter(_usernameKey, _username);
            request.AddParameter(_passwordKey, _password);
        }

        /// <summary>
        /// Gets a value indicating whether the authentication module can handle the challenge sent with the response.
        /// </summary>
        public virtual bool CanHandleChallenge(HttpResponseMessage response)
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
