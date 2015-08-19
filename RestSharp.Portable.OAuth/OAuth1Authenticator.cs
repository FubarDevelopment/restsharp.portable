#region License

//// <copyright file="OAuth1Authenticator.cs" company="John Sheehan">2010</copyright>
////
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// http://www.apache.org/licenses/LICENSE-2.0
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using RestSharp.Portable.Authenticators.OAuth;
using RestSharp.Portable.Authenticators.OAuth.SignatureProviders;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// OAuth 1.0a authenticator
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class OAuth1Authenticator : IAuthenticator
    {
        /// <summary>
        /// The authentication method ID used in HTTP authentication challenge
        /// </summary>
        public const string AuthenticationMethod = "OAuth";

        /// <summary>
        /// Prevents a default instance of the <see cref="OAuth1Authenticator" /> class from being created.
        /// </summary>
        /// <remarks>
        /// Sets the default CreateTimestamp function. Creation is only allowed by
        /// using the static functions like <see cref="ForRequestToken(string, string)"/>.
        /// </remarks>
        private OAuth1Authenticator()
        {
            CreateTimestampFunc = OAuthTools.GetTimestamp;
        }

        /// <summary>
        /// Gets or sets the realm to be authenticated for.
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets the OAuth parameter handling
        /// </summary>
        public OAuthParameterHandling ParameterHandling { get; set; }

        /// <summary>
        /// Gets or sets the OAuth signature method
        /// </summary>
        public ISignatureProvider SignatureProvider { get; set; }

        /// <summary>
        /// Gets or sets the OAuth signature treatment
        /// </summary>
        public OAuthSignatureTreatment SignatureTreatment { get; set; }

        /// <summary>
        /// Gets or sets the function to create a timestamp
        /// </summary>
        public OAuthCreateTimestampDelegate CreateTimestampFunc { get; set; }

        internal OAuthType Type { get; set; }

        internal string ConsumerKey { get; set; }

        internal string ConsumerSecret { get; set; }

        internal string Token { get; set; }

        internal string TokenSecret { get; set; }

        internal string Verifier { get; set; }

        internal string Version { get; set; }

        internal string CallbackUrl { get; set; }

        internal string SessionHandle { get; set; }

        internal string ClientUsername { get; set; }

        internal string ClientPassword { get; set; }

        /// <summary>
        /// Create an authenticator to gather a request token
        /// </summary>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <returns>The new authenticator</returns>
        public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret)
        {
            var authenticator = new OAuth1Authenticator
                {
                    ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                    SignatureProvider = new HmacSha1SignatureProvider(),
                    SignatureTreatment = OAuthSignatureTreatment.Escaped,
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    Type = OAuthType.RequestToken
                };
            return authenticator;
        }

        /// <summary>
        /// Create an authenticator to gather a request token
        /// </summary>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="callbackUrl">The callback URL</param>
        /// <returns>The new authenticator</returns>
        public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret, string callbackUrl)
        {
            var authenticator = ForRequestToken(consumerKey, consumerSecret);
            authenticator.CallbackUrl = callbackUrl;
            return authenticator;
        }

        /// <summary>
        /// Create an authenticator to gather an access token
        /// </summary>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="token">The access token</param>
        /// <param name="tokenSecret">The access token secret</param>
        /// <returns>The new authenticator</returns>
        public static OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            var authenticator = new OAuth1Authenticator
                {
                    ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                    SignatureProvider = new HmacSha1SignatureProvider(),
                    SignatureTreatment = OAuthSignatureTreatment.Escaped,
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    Token = token,
                    TokenSecret = tokenSecret,
                    Type = OAuthType.AccessToken
                };
            return authenticator;
        }

        /// <summary>
        /// Create an authenticator to gather an access token
        /// </summary>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="token">The access token</param>
        /// <param name="tokenSecret">The access token secret</param>
        /// <param name="verifier">The verifier</param>
        /// <returns>The new authenticator</returns>
        public static OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token, string tokenSecret, string verifier)
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);
            authenticator.Verifier = verifier;
            return authenticator;
        }

        /// <summary>
        /// Create an authenticator to refresh an access token
        /// </summary>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="token">The access token</param>
        /// <param name="tokenSecret">The access token secret</param>
        /// <param name="sessionHandle">The session handle used to refresh the access token</param>
        /// <returns>The new authenticator</returns>
        public static OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token, string tokenSecret, string sessionHandle)
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);
            authenticator.SessionHandle = sessionHandle;
            return authenticator;
        }

        /// <summary>
        /// Create an authenticator to refresh an access token
        /// </summary>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="token">The access token</param>
        /// <param name="tokenSecret">The access token secret</param>
        /// <param name="verifier">The verifier</param>
        /// <param name="sessionHandle">The session handle used to refresh the access token</param>
        /// <returns>The new authenticator</returns>
        public static OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token, string tokenSecret, string verifier, string sessionHandle)
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);
            authenticator.SessionHandle = sessionHandle;
            authenticator.Verifier = verifier;
            return authenticator;
        }

        /// <summary>
        /// Create an authenticator to authenticate the client
        /// </summary>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="username">The client user name</param>
        /// <param name="password">The client password</param>
        /// <returns>The new authenticator</returns>
        public static OAuth1Authenticator ForClientAuthentication(string consumerKey, string consumerSecret, string username, string password)
        {
            var authenticator = new OAuth1Authenticator
                {
                    ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                    SignatureProvider = new HmacSha1SignatureProvider(),
                    SignatureTreatment = OAuthSignatureTreatment.Escaped,
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    ClientUsername = username,
                    ClientPassword = password,
                    Type = OAuthType.ClientAuthentication
                };
            return authenticator;
        }

        /// <summary>
        /// Create an authenticator to access a protected resource
        /// </summary>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="accessToken">The access token</param>
        /// <param name="accessTokenSecret">The access token secret</param>
        /// <returns>The new authenticator</returns>
        public static OAuth1Authenticator ForProtectedResource(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            var authenticator = new OAuth1Authenticator
                {
                    Type = OAuthType.ProtectedResource,
                    ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                    SignatureProvider = new HmacSha1SignatureProvider(),
                    SignatureTreatment = OAuthSignatureTreatment.Escaped,
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    Token = accessToken,
                    TokenSecret = accessTokenSecret
                };
            return authenticator;
        }

        /// <summary>
        /// Does the authentication module supports pre-authentication?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        public bool CanPreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials)
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
        public bool CanPreAuthenticate(IHttpClient client, IHttpRequestMessage request, ICredentials credentials)
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
        public Task PreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials)
        {
            return Task.Factory.StartNew(() =>
            {
                var workflow = new OAuthWorkflow
                    {
                        ConsumerKey = ConsumerKey,
                        ConsumerSecret = ConsumerSecret,
                        ParameterHandling = ParameterHandling,
                        SignatureProvider = SignatureProvider,
                        SignatureTreatment = SignatureTreatment,
                        Verifier = Verifier,
                        Version = Version,
                        CallbackUrl = CallbackUrl,
                        SessionHandle = SessionHandle,
                        Token = Token,
                        TokenSecret = TokenSecret,
                        ClientUsername = ClientUsername,
                        ClientPassword = ClientPassword,
                        CreateTimestampFunc = CreateTimestampFunc ?? OAuthTools.GetTimestamp
                    };
                AddOAuthData(client, request, workflow);
            });
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
            throw new NotSupportedException();
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
            return false;
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <param name="response">Response of the failed request</param>
        /// <returns>Task where the handler for a failed authentication gets executed</returns>
        public virtual Task HandleChallenge(IHttpClient client, IHttpRequestMessage request, ICredentials credentials, IHttpResponseMessage response)
        {
            throw new NotSupportedException();
        }

        private void AddOAuthData(IRestClient client, IRestRequest request, OAuthWorkflow workflow)
        {
            var url = client.BuildUri(request, false).ToString();
            OAuthWebQueryInfo oauth;
            var method = request.Method.ToString();
            var parameters = new WebParameterCollection();

            // include all GET and POST parameters before generating the signature
            // according to the RFC 5849 - The OAuth 1.0 Protocol
            // http://tools.ietf.org/html/rfc5849#section-3.4.1
            // if this change causes trouble we need to introduce a flag indicating the specific OAuth implementation level,
            // or implement a seperate class for each OAuth version
            var useMultiPart = request.ContentCollectionMode == ContentCollectionMode.MultiPart
                               || (request.ContentCollectionMode == ContentCollectionMode.MultiPartForFileParameters
                                   && (client.DefaultParameters.GetFileParameters().Any() || request.Parameters.GetFileParameters().Any()));

            var requestParameters = client.MergeParameters(request).Where(x => x.Type == ParameterType.GetOrPost || x.Type == ParameterType.QueryString);
            if (!useMultiPart)
            {
                foreach (var p in requestParameters)
                {
                    parameters.Add(new WebPair(p.Name, p.Value.ToString()));
                }
            }
            else
            {
                // if we are sending a multipart request, only the "oauth_" parameters should be included in the signature
                foreach (var p in requestParameters.Where(p => p.Name.StartsWith("oauth_", StringComparison.Ordinal)))
                {
                    parameters.Add(new WebPair(p.Name, p.Value.ToString()));
                }
            }

            switch (Type)
            {
                case OAuthType.RequestToken:
                    workflow.RequestTokenUrl = url;
                    oauth = workflow.BuildRequestTokenInfo(method, parameters);
                    break;
                case OAuthType.AccessToken:
                    workflow.AccessTokenUrl = url;
                    oauth = workflow.BuildAccessTokenInfo(method, parameters);
                    break;
                case OAuthType.ClientAuthentication:
                    workflow.AccessTokenUrl = url;
                    oauth = workflow.BuildClientAuthAccessTokenInfo(method, parameters);
                    break;
                case OAuthType.ProtectedResource:
                    oauth = workflow.BuildProtectedResourceInfo(method, parameters, url);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (ParameterHandling)
            {
                case OAuthParameterHandling.HttpAuthorizationHeader:
                    parameters.Add("oauth_signature", oauth.Signature);
                    request.AddHeader("Authorization", GetAuthorizationHeader(parameters));
                    break;
                case OAuthParameterHandling.UrlOrPostParameters:
                    parameters.Add("oauth_signature", oauth.Signature);
                    foreach (var parameter in parameters.Where(
                        parameter => !string.IsNullOrEmpty(parameter.Name)
                                     && (parameter.Name.StartsWith("oauth_") || parameter.Name.StartsWith("x_auth_"))))
                    {
                        var v = parameter.Value;
                        v = Uri.UnescapeDataString(v.Replace('+', ' '));
                        request.AddParameter(parameter.Name, v);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetAuthorizationHeader(WebPairCollection parameters)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(Realm))
            {
                sb.Append(string.Format("realm=\"{0}\",", OAuthTools.UrlEncodeRelaxed(Realm)));
            }

            parameters.Sort((l, r) => string.Compare(l.Name, r.Name, StringComparison.Ordinal));
            var parameterCount = 0;
            var oathParameters = parameters.Where(
                parameter => !string.IsNullOrEmpty(parameter.Name)
                             && !string.IsNullOrEmpty(parameter.Value)
                             && (parameter.Name.StartsWith("oauth_") || parameter.Name.StartsWith("x_auth_"))).ToList();
            foreach (var parameter in oathParameters)
            {
                parameterCount++;
                var format = parameterCount < oathParameters.Count ? "{0}=\"{1}\"," : "{0}=\"{1}\"";
                sb.Append(string.Format(format, parameter.Name, parameter.Value));
            }

            var authorization = sb.ToString();
            return string.Format("{0} {1}", AuthenticationMethod, authorization);
        }
    }
}
