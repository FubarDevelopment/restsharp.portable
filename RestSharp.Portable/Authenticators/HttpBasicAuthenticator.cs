using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The default HTTP Basic authenticator
    /// </summary>
    public class HttpBasicAuthenticator : ISyncAuthenticator
    {
        /// <summary>
        /// The authentication method ID used in HTTP authentication challenge
        /// </summary>
        public const string AuthenticationMethod = "Basic";

        private readonly AuthHeader _authHeader;

        private string _authToken;

        private NetworkCredential _authCredential;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpBasicAuthenticator" /> class.
        /// </summary>
        public HttpBasicAuthenticator()
            : this(AuthHeader.Www)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpBasicAuthenticator" /> class.
        /// </summary>
        /// <param name="authHeader">Authentication/Authorization header type</param>
        public HttpBasicAuthenticator(AuthHeader authHeader)
        {
            _authHeader = authHeader;
        }

        /// <summary>
        /// Gets a value indicating whether the authentication module supports pre-authentication.
        /// </summary>
        public bool CanPreAuthenticate
        {
            get { return HasAuthorizationToken; }
        }

        /// <summary>
        /// Gets a value indicating whether the authenticator already as an authorization token available for pre-authentication.
        /// </summary>
        protected bool HasAuthorizationToken
        {
            get { return _authToken != null; }
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        public void PreAuthenticate(IRestClient client, IRestRequest request)
        {
            if (!CanPreAuthenticate)
                throw new InvalidOperationException();
            AuthHeaderUtilities.TrySetAuthorizationHeader(client, request, _authHeader, _authToken);
        }

        /// <summary>
        /// Determines if the authentication module can handle the challenge sent with the response.
        /// </summary>
        /// <param name="client">The REST client the response is assigned to</param>
        /// <param name="request">The REST request the response is assigned to</param>
        /// <param name="response">The response that returned the authentication challenge</param>
        /// <returns>true when the authenticator can handle the sent challenge</returns>
        public virtual bool CanHandleChallenge(IRestClient client, IRestRequest request, HttpResponseMessage response)
        {
            // No credentials defined?
            if (client.Credentials == null)
                return false;

            // No challenge header found?
            var authModeInfo = response.GetAuthenticationMethodValue(_authHeader, AuthenticationMethod);
            if (authModeInfo == null)
                return false;

            // Search for credential for request URI
            var responseUri = response.Headers.Location ?? client.BuildUri(request, false);
            var credential = client.Credentials.GetCredential(responseUri, AuthenticationMethod);
            if (credential == null)
                return false;

            // Did we already try to use the found credentials?
            if (ReferenceEquals(credential, _authCredential))
            {
                // Yes, so we don't retry the authentication.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        public void HandleChallenge(IRestClient client, IRestRequest request, HttpResponseMessage response)
        {
            if (!CanHandleChallenge(client, request, response))
                throw new InvalidOperationException();

            var responseUri = response.Headers.Location ?? client.BuildUri(request, false);
            _authCredential = client.Credentials.GetCredential(responseUri, AuthenticationMethod);
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", _authCredential.UserName, _authCredential.Password)));
            _authToken = string.Format("{0} {1}", AuthenticationMethod, token);
        }
    }
}
