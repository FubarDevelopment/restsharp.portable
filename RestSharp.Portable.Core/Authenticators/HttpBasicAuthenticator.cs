using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The default HTTP Basic authenticator
    /// </summary>
    public class HttpBasicAuthenticator : IAuthenticator
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
        /// Gets a value indicating whether the authenticator already as an authorization token available for pre-authentication.
        /// </summary>
        protected bool HasAuthorizationToken => _authToken != null;

        /// <summary>
        /// Does the authentication module supports pre-authentication?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        public bool CanPreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials)
        {
            return false;
        }

        /// <summary>
        /// Does the authentication module supports pre-authentication for the given <see cref="IHttpRequestMessage" />?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        public bool CanPreAuthenticate(IHttpClient client, IHttpRequestMessage request, ICredentials credentials)
        {
            return HasAuthorizationToken;
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        public Task PreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        public Task PreAuthenticate(IHttpClient client, IHttpRequestMessage request, ICredentials credentials)
        {
            return Task.Factory.StartNew(() =>
            {
                if (!CanPreAuthenticate(client, request, credentials))
                {
                    throw new InvalidOperationException();
                }

                var authHeaderValue = $"{AuthenticationMethod} {_authToken}";
                request.SetAuthorizationHeader(_authHeader, authHeaderValue);
            });
        }

        /// <summary>
        /// Determines if the authentication module can handle the challenge sent with the response.
        /// </summary>
        /// <param name="client">The REST client the response is assigned to</param>
        /// <param name="request">The REST request the response is assigned to</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <param name="response">The response that returned the authentication challenge</param>
        /// <returns>true when the authenticator can handle the sent challenge</returns>
        public virtual bool CanHandleChallenge(IHttpClient client, IHttpRequestMessage request, ICredentials credentials, IHttpResponseMessage response)
        {
            // No credentials defined?
            if (credentials == null)
            {
                return false;
            }

            // No challenge header found?
            var authModeInfo = response.GetAuthenticationMethodValue(_authHeader, AuthenticationMethod);
            if (authModeInfo == null)
            {
                return false;
            }

            // Search for credential for request URI
            var responseUri = client.GetRequestUri(request, response);
            var credential = credentials.GetCredential(responseUri, AuthenticationMethod);
            if (credential == null)
            {
                return false;
            }

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
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <param name="response">Response of the failed request</param>
        /// <returns>Task where the handler for a failed authentication gets executed</returns>
        public Task HandleChallenge(IHttpClient client, IHttpRequestMessage request, ICredentials credentials, IHttpResponseMessage response)
        {
            return Task.Factory.StartNew(() =>
            {
                if (!CanHandleChallenge(client, request, credentials, response))
                {
                    throw new InvalidOperationException();
                }

                var responseUri = client.GetRequestUri(request, response);
                _authCredential = credentials.GetCredential(responseUri, AuthenticationMethod);
                _authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_authCredential.UserName}:{_authCredential.Password}"));
            });
        }
    }
}
