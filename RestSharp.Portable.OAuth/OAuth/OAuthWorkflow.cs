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
using System.Collections.Generic;

using JetBrains.Annotations;

using RestSharp.Portable.Authenticators.OAuth.SignatureProviders;

namespace RestSharp.Portable.Authenticators.OAuth
{
    using Extensions;

    /// <summary>
    /// A class to encapsulate OAuth authentication flow. (<a href="http://oauth.net/core/1.0#anchor9"/>)
    /// </summary>
    internal class OAuthWorkflow
    {
        [NotNull]
        public OAuthCreateTimestampDelegate CreateTimestampFunc { get; set; } = OAuthTools.GetTimestamp;
        public string Version { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public string CallbackUrl { get; set; }
        public string Verifier { get; set; }
        public string SessionHandle { get; set; }
        public ISignatureProvider SignatureProvider { get; set; }
        public OAuthSignatureTreatment SignatureTreatment { get; set; }
        public OAuthParameterHandling ParameterHandling { get; set; }
        [NotNull]
        public IRandom RandomNumberGenerator { get; set; }
        public string ClientUsername { get; set; }
        public string ClientPassword { get; set; }

        /// <summary>
        /// The URL to query the request token (<a href="http://oauth.net/core/1.0#request_urls"/>
        /// </summary>
        public string RequestTokenUrl { get; set; }

        /// <a href="http://oauth.net/core/1.0#request_urls"/>
        /// <summary>
        /// The URL to query the access token (<a href="http://oauth.net/core/1.0#request_urls"/>
        /// </summary>
        public string AccessTokenUrl { get; set; }

        /// <summary>
        /// The URL where the user has to authorize the app
        /// </summary>
        public string AuthorizationUrl { get; set; }

        /// <summary>
        /// Generates a <see cref="OAuthWebQueryInfo"/> instance to pass to an
        /// <see cref="IAuthenticator" /> for the purpose of requesting an
        /// unauthorized request token.
        /// </summary>
        /// <param name="method">The HTTP method for the intended request (<a href="http://oauth.net/core/1.0#anchor9" />)</param>
        /// <returns></returns>
        public OAuthWebQueryInfo BuildRequestTokenInfo(string method)
        {
            return BuildRequestTokenInfo(method, null);
        }

        /// <summary>
        /// Generates a <see cref="OAuthWebQueryInfo"/> instance to pass to an
        /// <see cref="IAuthenticator" /> for the purpose of requesting an
        /// unauthorized request token.
        /// </summary>
        /// <param name="method">The HTTP method for the intended request</param>
        /// <param name="parameters">Any existing, non-OAuth query parameters desired in the request</param>
        /// <a href="http://oauth.net/core/1.0#anchor9"/>
        /// <returns></returns>
        public OAuthWebQueryInfo BuildRequestTokenInfo(string method, WebParameterCollection parameters)
        {
            ValidateTokenRequestState();
            if (parameters == null)
            {
                parameters = new WebParameterCollection();
            }
            var timestamp = CreateTimestampFunc();
            var nonce = OAuthTools.GetNonce(RandomNumberGenerator);
            AddAuthParameters(parameters, timestamp, nonce);
            var signatureBase = OAuthTools.ConcatenateRequestElements(method, RequestTokenUrl, parameters);
            var signature = OAuthTools.GetSignature(SignatureProvider, SignatureTreatment, signatureBase, ConsumerSecret);
            var info = new OAuthWebQueryInfo
            {
                WebMethod = method,
                ParameterHandling = ParameterHandling,
                ConsumerKey = ConsumerKey,
                SignatureMethod = SignatureProvider.Id,
                SignatureTreatment = SignatureTreatment,
                Signature = signature,
                Timestamp = timestamp,
                Nonce = nonce,
                Version = Version ?? "1.0",
                Callback = OAuthTools.UrlEncodeRelaxed(CallbackUrl ?? string.Empty),
                TokenSecret = TokenSecret,
                ConsumerSecret = ConsumerSecret
            };
            return info;
        }

        /// <summary>
        /// Generates a <see cref="OAuthWebQueryInfo"/> instance to pass to an
        /// <see cref="IAuthenticator" /> for the purpose of exchanging a request token
        /// for an access token authorized by the user at the Service Provider site.
        /// </summary>
        /// <param name="method">The HTTP method for the intended request</param>
        /// <a href="http://oauth.net/core/1.0#anchor9"/>
        public OAuthWebQueryInfo BuildAccessTokenInfo(string method)
        {
            return BuildAccessTokenInfo(method, null);
        }

        /// <summary>
        /// Generates a <see cref="OAuthWebQueryInfo"/> instance to pass to an
        /// <see cref="IAuthenticator" /> for the purpose of exchanging a request token
        /// for an access token authorized by the user at the Service Provider site.
        /// </summary>
        /// <param name="method">The HTTP method for the intended request</param>
        /// <a href="http://oauth.net/core/1.0#anchor9"/>
        /// <param name="parameters">Any existing, non-OAuth query parameters desired in the request</param>
        public OAuthWebQueryInfo BuildAccessTokenInfo(string method, WebParameterCollection parameters)
        {
            ValidateAccessRequestState();
            if (parameters == null)
            {
                parameters = new WebParameterCollection();
            }
            var uri = new Uri(AccessTokenUrl);
            var timestamp = CreateTimestampFunc();
            var nonce = OAuthTools.GetNonce(RandomNumberGenerator);
            AddAuthParameters(parameters, timestamp, nonce);
            var signatureBase = OAuthTools.ConcatenateRequestElements(method, uri.ToString(), parameters);
            var signature = OAuthTools.GetSignature(SignatureProvider, SignatureTreatment, signatureBase, ConsumerSecret, TokenSecret);
            var info = new OAuthWebQueryInfo
            {
                WebMethod = method,
                ParameterHandling = ParameterHandling,
                ConsumerKey = ConsumerKey,
                Token = Token,
                SignatureMethod = SignatureProvider.Id,
                SignatureTreatment = SignatureTreatment,
                Signature = signature,
                Timestamp = timestamp,
                Nonce = nonce,
                Version = Version ?? "1.0",
                Verifier = Verifier,
                Callback = CallbackUrl,
                TokenSecret = TokenSecret,
                ConsumerSecret = ConsumerSecret,
            };
            return info;
        }

        /// <summary>
        /// Generates a <see cref="OAuthWebQueryInfo"/> instance to pass to an
        /// <see cref="IAuthenticator" /> for the purpose of exchanging user credentials
        /// for an access token authorized by the user at the Service Provider site.
        /// </summary>
        /// <param name="method">The HTTP method for the intended request</param>
        /// <a href="http://tools.ietf.org/html/draft-dehora-farrell-oauth-accesstoken-creds-00#section-4"/>
        /// <param name="parameters">Any existing, non-OAuth query parameters desired in the request</param>
        public OAuthWebQueryInfo BuildClientAuthAccessTokenInfo(string method, WebParameterCollection parameters)
        {
            ValidateClientAuthAccessRequestState();
            if (parameters == null)
            {
                parameters = new WebParameterCollection();
            }
            var uri = new Uri(AccessTokenUrl);
            var timestamp = CreateTimestampFunc();
            var nonce = OAuthTools.GetNonce(RandomNumberGenerator);
            AddXAuthParameters(parameters, timestamp, nonce);
            var signatureBase = OAuthTools.ConcatenateRequestElements(method, uri.ToString(), parameters);
            var signature = OAuthTools.GetSignature(SignatureProvider, SignatureTreatment, signatureBase, ConsumerSecret);
            var info = new OAuthWebQueryInfo
            {
                WebMethod = method,
                ParameterHandling = ParameterHandling,
                ClientMode = "client_auth",
                ClientUsername = ClientUsername,
                ClientPassword = ClientPassword,
                ConsumerKey = ConsumerKey,
                SignatureMethod = SignatureProvider.Id,
                SignatureTreatment = SignatureTreatment,
                Signature = signature,
                Timestamp = timestamp,
                Nonce = nonce,
                Version = Version ?? "1.0",
                TokenSecret = TokenSecret,
                ConsumerSecret = ConsumerSecret
            };
            return info;
        }
        public OAuthWebQueryInfo BuildProtectedResourceInfo(string method, WebParameterCollection parameters, string url)
        {
            ValidateProtectedResourceState();
            if (parameters == null)
            {
                parameters = new WebParameterCollection();
            }

            // Include url parameters in query pool
            var uri = new Uri(url);
            var urlParameters = uri.Query.ParseQueryString();
            foreach (var parameter in urlParameters.Keys)
            {
                parameters.Add(new WebParameter(parameter, urlParameters[parameter], WebParameterType.Query));
            }
            var timestamp = CreateTimestampFunc();
            var nonce = OAuthTools.GetNonce(RandomNumberGenerator);
            AddAuthParameters(parameters, timestamp, nonce);
            var signatureBase = OAuthTools.ConcatenateRequestElements(method, url, parameters);
            var signature = OAuthTools.GetSignature(
            SignatureProvider, SignatureTreatment, signatureBase, ConsumerSecret, TokenSecret);
            var info = new OAuthWebQueryInfo
            {
                WebMethod = method,
                ParameterHandling = ParameterHandling,
                ConsumerKey = ConsumerKey,
                Token = Token,
                SignatureMethod = SignatureProvider.Id,
                SignatureTreatment = SignatureTreatment,
                Signature = signature,
                Timestamp = timestamp,
                Nonce = nonce,
                Version = Version ?? "1.0",
                Callback = CallbackUrl,
                ConsumerSecret = ConsumerSecret,
                TokenSecret = TokenSecret
            };
            return info;
        }
        private void ValidateTokenRequestState()
        {
            if (string.IsNullOrEmpty(RequestTokenUrl))
            {
                throw new ArgumentException("You must specify a request token URL");
            }
            if (string.IsNullOrEmpty(ConsumerKey))
            {
                throw new ArgumentException("You must specify a consumer key");
            }
            if (string.IsNullOrEmpty(ConsumerSecret))
            {
                throw new ArgumentException("You must specify a consumer secret");
            }
        }
        private void ValidateAccessRequestState()
        {
            if (string.IsNullOrEmpty(AccessTokenUrl))
            {
                throw new ArgumentException("You must specify an access token URL");
            }
            if (string.IsNullOrEmpty(ConsumerKey))
            {
                throw new ArgumentException("You must specify a consumer key");
            }
            if (string.IsNullOrEmpty(ConsumerSecret))
            {
                throw new ArgumentException("You must specify a consumer secret");
            }
            if (string.IsNullOrEmpty(Token))
            {
                throw new ArgumentException("You must specify a token");
            }
        }
        private void ValidateClientAuthAccessRequestState()
        {
            if (string.IsNullOrEmpty(AccessTokenUrl))
            {
                throw new ArgumentException("You must specify an access token URL");
            }
            if (string.IsNullOrEmpty(ConsumerKey))
            {
                throw new ArgumentException("You must specify a consumer key");
            }
            if (string.IsNullOrEmpty(ConsumerSecret))
            {
                throw new ArgumentException("You must specify a consumer secret");
            }
            if (string.IsNullOrEmpty(ClientUsername) || string.IsNullOrEmpty(ClientPassword))
            {
                throw new ArgumentException("You must specify user credentials");
            }
        }
        private void ValidateProtectedResourceState()
        {
            if (string.IsNullOrEmpty(ConsumerKey))
            {
                throw new ArgumentException("You must specify a consumer key");
            }
            if (string.IsNullOrEmpty(ConsumerSecret))
            {
                throw new ArgumentException("You must specify a consumer secret");
            }
        }
        private void AddAuthParameters(ICollection<WebParameter> parameters, string timestamp, string nonce)
        {
            var authParameters = new WebParameterCollection
            {
                new WebParameter("oauth_consumer_key", ConsumerKey, WebParameterType.Internal),
                new WebParameter("oauth_nonce", nonce, WebParameterType.Internal),
                new WebParameter("oauth_signature_method", SignatureProvider.Id, WebParameterType.Internal),
                new WebParameter("oauth_timestamp", timestamp, WebParameterType.Internal),
                new WebParameter("oauth_version", Version ?? "1.0", WebParameterType.Internal)
            };
            if (!string.IsNullOrEmpty(Token))
            {
                authParameters.Add(new WebParameter("oauth_token", Token, WebParameterType.Internal));
            }
            if (!string.IsNullOrEmpty(CallbackUrl))
            {
                authParameters.Add(new WebParameter("oauth_callback", CallbackUrl, WebParameterType.Internal));
            }
            if (!string.IsNullOrEmpty(Verifier))
            {
                authParameters.Add(new WebParameter("oauth_verifier", Verifier, WebParameterType.Internal));
            }
            if (!string.IsNullOrEmpty(SessionHandle))
            {
                authParameters.Add(new WebParameter("oauth_session_handle", SessionHandle, WebParameterType.Internal));
            }
            foreach (var authParameter in authParameters)
            {
                parameters.Add(authParameter);
            }
        }
        private void AddXAuthParameters(ICollection<WebParameter> parameters, string timestamp, string nonce)
        {
            var authParameters = new WebParameterCollection
            {
                new WebParameter("x_auth_username", ClientUsername, WebParameterType.Internal),
                new WebParameter("x_auth_password", ClientPassword, WebParameterType.Internal),
                new WebParameter("x_auth_mode", "client_auth", WebParameterType.Internal),
                new WebParameter("oauth_consumer_key", ConsumerKey, WebParameterType.Internal),
                new WebParameter("oauth_signature_method", SignatureProvider.Id, WebParameterType.Internal),
                new WebParameter("oauth_timestamp", timestamp, WebParameterType.Internal),
                new WebParameter("oauth_nonce", nonce, WebParameterType.Internal),
                new WebParameter("oauth_version", Version ?? "1.0", WebParameterType.Internal)
            };
            foreach (var authParameter in authParameters)
            {
                parameters.Add(authParameter);
            }
        }
    }
}
