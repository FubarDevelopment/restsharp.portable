#region License
//   Copyright 2010 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Base class for OAuth 2 Authenticators.
    /// </summary>
    /// <remarks>
    /// Since there are many ways to authenticate in OAuth2,
    /// this is used as a base class to differentiate between 
    /// other authenticators.
    /// 
    /// Any other OAuth2 authenticators must derive from this
    /// abstract class.
    /// </remarks>
    public abstract class OAuth2Authenticator : AsyncAuthenticator, IAsyncRoundTripAuthenticator, IRoundTripAuthenticator
    {
        protected readonly OAuth2.OAuth2Client _client;

        private static readonly IEnumerable<HttpStatusCode> _statusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.Unauthorized,
        };
        private static readonly IEnumerable<HttpStatusCode> _noStatusCodes = new List<HttpStatusCode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2Authenticator"/> class.
        /// </summary>
        /// <param name="client">The OAuth2 client</param>
        protected OAuth2Authenticator(OAuth2.OAuth2Client client)
        {
            _client = client;
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        /// <returns>Task where the handler for a failed authentication gets executed</returns>
        public virtual async Task AuthenticationFailed(IRestClient client, IRestRequest request, IRestResponse response)
        {
            if (string.IsNullOrEmpty(_client.RefreshToken))
                return;
            await _client.GetCurrentToken(forceUpdate: true);
        }

        /// <summary>
        /// Returns all the status codes where a round trip is allowed
        /// </summary>
        public virtual IEnumerable<System.Net.HttpStatusCode> StatusCodes
        {
            get
            {
                if (string.IsNullOrEmpty(_client.RefreshToken))
                    return _noStatusCodes;
                return _statusCodes;
            }
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        void IRoundTripAuthenticator.AuthenticationFailed(IRestClient client, IRestRequest request, IRestResponse response)
        {
            AuthenticationFailed(client, request, response).Wait();
        }
    }

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
            : base(client) { }

        public override async Task Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddParameter("oauth_token", await _client.GetCurrentToken(), ParameterType.GetOrPost);
        }
    }

    /// <summary>
    /// The OAuth 2 authenticator using the authorization request header field.
    /// </summary>
    /// <remarks>
    /// Based on http://tools.ietf.org/html/draft-ietf-oauth-v2-10#section-5.1.1
    /// </remarks>
    public class OAuth2AuthorizationRequestHeaderAuthenticator : OAuth2Authenticator
    {
        private readonly string _tokenType;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2AuthorizationRequestHeaderAuthenticator"/> class.
        /// </summary>
        /// <param name="client">The OAuth2 client</param>
        public OAuth2AuthorizationRequestHeaderAuthenticator(OAuth2.OAuth2Client client)
            : this(client, (string.IsNullOrEmpty(client.TokenType) ? "OAuth" : client.TokenType)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2AuthorizationRequestHeaderAuthenticator"/> class.
        /// </summary>
        /// <param name="client">The OAuth2 client</param>
        /// <param name="tokenType">
        /// The token type.
        /// </param>
        public OAuth2AuthorizationRequestHeaderAuthenticator(OAuth2.OAuth2Client client, string tokenType)
            : base(client)
        {
            _tokenType = tokenType;
        }

        public override async Task Authenticate(IRestClient client, IRestRequest request)
        {
            // only add the Authorization parameter if it hasn't been added.
            if (request.Parameters.Any(p => p.Type == ParameterType.HttpHeader && p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
                return;

            var authValue = string.Format("{0} {1}", _tokenType, await _client.GetCurrentToken());
            request.AddParameter("Authorization", authValue, ParameterType.HttpHeader);
        }
    }
}
