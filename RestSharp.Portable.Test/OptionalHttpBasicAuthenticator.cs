using System;
using System.Collections.Generic;
using System.Net;

// ReSharper disable once CheckNamespace
namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Same as HttpBasicAuthenticator, but it only applies the authentication information only when
    /// a request failed before with 401 or 404.
    /// </summary>
    public class OptionalHttpBasicAuthenticator : IRoundTripAuthenticator
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
        /// Gets all the status codes where a round trip is allowed
        /// </summary>
        public IEnumerable<HttpStatusCode> StatusCodes
        {
            get { return _statusCodes; }
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        public void AuthenticationFailed(IRestClient client, IRestRequest request, IRestResponse response)
        {
            _authRequired = true;
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            if (!_authRequired)
                return;
            _basicAuth.Authenticate(client, request);
        }
    }
}
