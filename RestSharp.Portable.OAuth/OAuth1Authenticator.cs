#region License
// Copyright 2010 John Sheehan
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Authenticators
{
    using OAuth;
    using OAuth.Extensions;

    /// <summary>
    /// OAuth 1.0a authenticator
    /// </summary>
    public class OAuth1Authenticator : IAuthenticator
    {
        /// <summary>
        /// Realm
        /// </summary>
        public virtual string Realm { get; set; }
        /// <summary>
        /// OAuth parameter handling
        /// </summary>
        public virtual OAuthParameterHandling ParameterHandling { get; set; }
        /// <summary>
        /// OAuth signature method
        /// </summary>
        public virtual OAuthSignatureMethod SignatureMethod { get; set; }
        /// <summary>
        /// OAuth signature treatment
        /// </summary>
        public virtual OAuthSignatureTreatment SignatureTreatment { get; set; }
        internal virtual OAuthType Type { get; set; }
        internal virtual string ConsumerKey { get; set; }
        internal virtual string ConsumerSecret { get; set; }
        internal virtual string Token { get; set; }
        internal virtual string TokenSecret { get; set; }
        internal virtual string Verifier { get; set; }
        internal virtual string Version { get; set; }
        internal virtual string CallbackUrl { get; set; }
        internal virtual string SessionHandle { get; set; }
        internal virtual string ClientUsername { get; set; }
        internal virtual string ClientPassword { get; set; }

        /// <summary>
        /// Create an authenticator to gather a request token
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <returns></returns>
        public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret)
        {
            var authenticator = new OAuth1Authenticator
            {
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
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
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="callbackUrl"></param>
        /// <returns></returns>
        public static OAuth1Authenticator ForRequestToken(string consumerKey, string consumerSecret, string callbackUrl)
        {
            var authenticator = ForRequestToken(consumerKey, consumerSecret);
            authenticator.CallbackUrl = callbackUrl;
            return authenticator;
        }

        /// <summary>
        /// Create an authenticator to gather an access token
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="token"></param>
        /// <param name="tokenSecret"></param>
        /// <returns></returns>
        public static OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            var authenticator = new OAuth1Authenticator
            {
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
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
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="token"></param>
        /// <param name="tokenSecret"></param>
        /// <param name="verifier"></param>
        /// <returns></returns>
        public static OAuth1Authenticator ForAccessToken(string consumerKey, string consumerSecret, string token, string tokenSecret, string verifier)
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);
            authenticator.Verifier = verifier;
            return authenticator;
        }

        /// <summary>
        /// Create an authenticator to refresh an access token
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="token"></param>
        /// <param name="tokenSecret"></param>
        /// <param name="sessionHandle"></param>
        /// <returns></returns>
        public static OAuth1Authenticator ForAccessTokenRefresh(string consumerKey, string consumerSecret, string token, string tokenSecret, string sessionHandle)
        {
            var authenticator = ForAccessToken(consumerKey, consumerSecret, token, tokenSecret);
            authenticator.SessionHandle = sessionHandle;
            return authenticator;
        }

        /// <summary>
        /// Create an authenticator to refresh an access token
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="token"></param>
        /// <param name="tokenSecret"></param>
        /// <param name="verifier"></param>
        /// <param name="sessionHandle"></param>
        /// <returns></returns>
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
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static OAuth1Authenticator ForClientAuthentication(string consumerKey, string consumerSecret, string username, string password)
        {
            var authenticator = new OAuth1Authenticator
            {
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
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
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="accessToken"></param>
        /// <param name="accessTokenSecret"></param>
        /// <returns></returns>
        public static OAuth1Authenticator ForProtectedResource(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            var authenticator = new OAuth1Authenticator
            {
                Type = OAuthType.ProtectedResource,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Token = accessToken,
                TokenSecret = accessTokenSecret
            };
            return authenticator;
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            var workflow = new OAuthWorkflow
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                ParameterHandling = ParameterHandling,
                SignatureMethod = SignatureMethod,
                SignatureTreatment = SignatureTreatment,
                Verifier = Verifier,
                Version = Version,
                CallbackUrl = CallbackUrl,
                SessionHandle = SessionHandle,
                Token = Token,
                TokenSecret = TokenSecret,
                ClientUsername = ClientUsername,
                ClientPassword = ClientPassword
            };
            AddOAuthData(client, request, workflow);
        }
        private void AddOAuthData(IRestClient client, IRestRequest request, OAuthWorkflow workflow)
        {
            var url = client.BuildUri(request, false).ToString();
            OAuthWebQueryInfo oauth;
            var method = request.Method.Method;
            var parameters = new WebParameterCollection();
            // include all GET and POST parameters before generating the signature
            // according to the RFC 5849 - The OAuth 1.0 Protocol
            // http://tools.ietf.org/html/rfc5849#section-3.4.1
            // if this change causes trouble we need to introduce a flag indicating the specific OAuth implementation level,
            // or implement a seperate class for each OAuth version

            var useMultiPart = request.ContentCollectionMode == ContentCollectionMode.MultiPart
                || (request.ContentCollectionMode == ContentCollectionMode.MultiPartForFileParameters && (client.DefaultParameters.GetFileParameters().Any() || request.Parameters.GetFileParameters().Any()));

            if (!useMultiPart)
            {
                var getOrPostParameters = client.MergeParameters(request).GetGetOrPostParameters();
                foreach (var p in getOrPostParameters)
                    parameters.Add(new WebPair(p.Name, p.Value.ToString()));
            }
            else
            {
                // if we are sending a multipart request, only the "oauth_" parameters should be included in the signature
                foreach (var p in client.DefaultParameters.Where(
                p => p.Type == ParameterType.GetOrPost && p.Name.StartsWith("oauth_")))
                {
                    parameters.Add(new WebPair(p.Name, p.Value.ToString()));
                }
                foreach (var p in request.Parameters.Where(p => p.Type == ParameterType.GetOrPost && p.Name.StartsWith("oauth_")))
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
                    foreach (var parameter in parameters.Where(parameter =>
                    !parameter.Name.IsNullOrBlank() &&
                    (parameter.Name.StartsWith("oauth_") || parameter.Name.StartsWith("x_auth_"))))
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
            var sb = new StringBuilder("OAuth ");
            if (!Realm.IsNullOrBlank())
            {
                sb.Append("realm=\"{0}\",".FormatWith(OAuthTools.UrlEncodeRelaxed(Realm)));
            }
            parameters.Sort((l, r) => String.Compare(l.Name, r.Name, StringComparison.Ordinal));
            var parameterCount = 0;
            var oathParameters = parameters.Where(parameter =>
            !parameter.Name.IsNullOrBlank() &&
            !parameter.Value.IsNullOrBlank() &&
            (parameter.Name.StartsWith("oauth_") || parameter.Name.StartsWith("x_auth_"))
            ).ToList();
            foreach (var parameter in oathParameters)
            {
                parameterCount++;
                var format = parameterCount < oathParameters.Count ? "{0}=\"{1}\"," : "{0}=\"{1}\"";
                sb.Append(format.FormatWith(parameter.Name, parameter.Value));
            }
            var authorization = sb.ToString();
            return authorization;
        }
    }
}
