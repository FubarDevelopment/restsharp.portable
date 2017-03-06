using System;
using System.Net;
using System.Threading.Tasks;

namespace RestSharp.Portable.OAuth2
{
    /// <summary>
    /// The OAuth 2 authenticator using URI query parameter.
    /// </summary>
    /// <remarks>
    /// Based on http://tools.ietf.org/html/draft-ietf-oauth-v2-10#section-5.1.2
    /// </remarks>
    public class OAuth2UriQueryParameterAuthenticator : OAuth2Authenticator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2UriQueryParameterAuthenticator"/> class.
        /// </summary>
        /// <param name="client">The OAuth2 client</param>
        public OAuth2UriQueryParameterAuthenticator(OAuth2.OAuth2Client client)
            : base(client)
        {
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
            return true;
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
            return false;
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        public override async Task PreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials)
        {
            request.AddOrUpdateParameter("oauth_token", await Client.GetCurrentToken().ConfigureAwait(false), ParameterType.GetOrPost);
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        public override Task PreAuthenticate(IHttpClient client, IHttpRequestMessage request, ICredentials credentials)
        {
            throw new NotSupportedException();
        }
    }
}
