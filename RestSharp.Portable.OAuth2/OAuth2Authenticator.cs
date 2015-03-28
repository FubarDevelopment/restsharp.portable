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
using System.Net;
using System.Net.Http;
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
    /// Any other OAuth2 authenticators must derive from this
    /// abstract class.
    /// </remarks>
    public abstract class OAuth2Authenticator : AsyncAuthenticationModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2Authenticator"/> class.
        /// </summary>
        /// <param name="client">The OAuth2 client</param>
        protected OAuth2Authenticator(OAuth2.OAuth2Client client)
        {
            Client = client;
        }

        /// <summary>
        /// The OAuth client that is used by this authenticator
        /// </summary>
        protected OAuth2.OAuth2Client Client { get; private set; }

        /// <summary>
        /// Gets the authentication type provided by this authentication module.
        /// </summary>
        public override string AuthenticationType
        {
            get { return "OAuth 2.0"; }
        }

        /// <summary>
        /// Gets a value indicating whether the authentication module supports pre-authentication.
        /// </summary>
        public override bool CanPreAuthenticate
        {
            get { return true; }
        }

        /// <summary>
        /// Will be called in response to an authentication request.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        /// <returns>The task the authentication is performed on, which returns true when the authentication succeeded</returns>
        public override async Task<bool> Authenticate(
            IRestClient client,
            IRestRequest request,
            HttpResponseMessage response,
            AuthHeader header,
            NetworkCredential credential)
        {
            if (string.IsNullOrEmpty(Client.RefreshToken))
                return false;
            await Client.GetCurrentToken(forceUpdate: true);
            return true;
        }
    }
}
