using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Same as HttpBasicAuthenticator, but it only applies the authentication information only when
    /// a request failed before with 401 or 404.
    /// </summary>
    public class OptionalHttpBasicAuthenticator : ISyncAuthenticator
    {
        private static readonly IEnumerable<HttpStatusCode> _statusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound,
        };

        private readonly HttpBasicAuthenticator _basicAuth;

        private bool _authRequired;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalHttpBasicAuthenticator" /> class.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="password">The users password</param>
        public OptionalHttpBasicAuthenticator(string username, string password)
        {
            _basicAuth = new HttpBasicAuthenticator(username, password);
        }

        /// <summary>
        /// Gets a value indicating whether the authentication module supports pre-authentication.
        /// </summary>
        public bool CanPreAuthenticate
        {
            get { return _authRequired; }
        }

        /// <summary>
        /// Gets a value indicating whether the authentication module can handle the challenge sent with the response.
        /// </summary>
        public bool CanHandleChallenge(HttpResponseMessage response)
        {
            return !_authRequired;
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        public void HandleChallenge(IRestClient client, IRestRequest request, HttpResponseMessage response)
        {
            _authRequired = true;
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        public void PreAuthenticate(IRestClient client, IRestRequest request)
        {
            if (!_authRequired)
                throw new InvalidOperationException();
            _basicAuth.PreAuthenticate(client, request);
        }
    }
}
