using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The OAuth 2 authenticator using the authorization request header field.
    /// </summary>
    /// <remarks>
    /// Based on <a href="http://tools.ietf.org/html/draft-ietf-oauth-v2-10#section-5.1.1" />
    /// </remarks>
    public class OAuth2AuthorizationRequestHeaderAuthenticator : OAuth2Authenticator
    {
        private readonly string _tokenType;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2AuthorizationRequestHeaderAuthenticator"/> class.
        /// </summary>
        /// <param name="client">The OAuth2 client</param>
        public OAuth2AuthorizationRequestHeaderAuthenticator(OAuth2.OAuth2Client client)
            : this(client, string.IsNullOrEmpty(client.TokenType) ? "OAuth" : client.TokenType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2AuthorizationRequestHeaderAuthenticator"/> class.
        /// </summary>
        /// <param name="client">The OAuth2 client</param>
        /// <param name="tokenType">The token type.</param>
        public OAuth2AuthorizationRequestHeaderAuthenticator(OAuth2.OAuth2Client client, string tokenType)
            : base(client)
        {
            _tokenType = tokenType;
        }

        /// <summary>
        /// Does the authentication module supports pre-authentication?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        public override bool CanPreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials)
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
        public override bool CanPreAuthenticate(IHttpClient client, IHttpRequestMessage request, ICredentials credentials)
        {
            return true;
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        public override Task PreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials)
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
        public override async Task PreAuthenticate(IHttpClient client, IHttpRequestMessage request, ICredentials credentials)
        {
            // When the authorization failed or when the Authorization header is missing, we're just adding it (again) with the
            // new AccessToken.
            var authHeader = $"{_tokenType} {await Client.GetCurrentToken()}";
            request.SetAuthorizationHeader(AuthHeader.Www, authHeader);
        }

        /// <inheritdoc/>
        public override bool CanHandleChallenge(IHttpClient client, IHttpRequestMessage request, ICredentials credentials, IHttpResponseMessage response)
        {
            // Get authentication header using the method specified by <code>_tokenType</code> or <code>OAuth</code>
            var authHeaderInfo = response
                .GetAuthenticationHeaderInfo(AuthHeader.Www)
                .FirstOrDefault(
                    x => string.Equals(x.Name, _tokenType, StringComparison.OrdinalIgnoreCase)
                         || string.Equals(x.Name, "OAuth", StringComparison.OrdinalIgnoreCase));
            if (authHeaderInfo == null)
                return false;

            // Check for WWW-Authenticate when status code is 401 (Unauthorized)
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return authHeaderInfo.Values["error"].Any(x => string.Equals(x, "invalid_token"));

            // Not a 400 (Bad Request)? Cannot handle challenge.
            if (response.StatusCode != HttpStatusCode.BadRequest)
                return false;

            // Check for Facebooks broken WWW-Authenticate
            var isFacebook = authHeaderInfo.Values[string.Empty].Any(x => x.Contains("Facebook"));
            if (!isFacebook)
                return false;
            if (!authHeaderInfo.Values[string.Empty].Any(x => x.Equals("invalid_token", StringComparison.OrdinalIgnoreCase)))
                return false;

            if (!base.CanHandleChallenge(client, request, credentials, response))
                return false;

            return true;
        }
    }
}
