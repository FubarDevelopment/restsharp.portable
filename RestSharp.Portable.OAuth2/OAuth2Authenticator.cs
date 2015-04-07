#region License

//// <copyright file="OAuth2Authenticator.cs" company="John Sheehan">2010</copyright>
////
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
////
////   http://www.apache.org/licenses/LICENSE-2.0
////
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.

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
    /// <para>
    /// Since there are many ways to authenticate in OAuth2,
    /// this is used as a base class to differentiate between
    /// other authenticators.
    /// </para>
    /// <para>
    /// Any other OAuth2 authenticators must derive from this
    /// abstract class.
    /// </para>
    /// </remarks>
    public abstract class OAuth2Authenticator : IAuthenticator
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
        /// Gets the OAuth client that is used by this authenticator
        /// </summary>
        protected OAuth2.OAuth2Client Client { get; private set; }

        /// <summary>
        /// Does the authentication module supports pre-authentication?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        public abstract bool CanPreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials);

        /// <summary>
        /// Does the authentication module supports pre-authentication for the given <see cref="HttpRequestMessage" />?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        public abstract bool CanPreAuthenticate(HttpClient client, HttpRequestMessage request, ICredentials credentials);

        /// <summary>
        /// Determines if the authentication module can handle the challenge sent with the response.
        /// </summary>
        /// <param name="client">The REST client the response is assigned to</param>
        /// <param name="request">The REST request the response is assigned to</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <param name="response">The response that returned the authentication challenge</param>
        /// <returns>true when the authenticator can handle the sent challenge</returns>
        public virtual bool CanHandleChallenge(HttpClient client, HttpRequestMessage request, ICredentials credentials, HttpResponseMessage response)
        {
            return !string.IsNullOrEmpty(Client.RefreshToken);
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        public abstract Task PreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials);

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        public abstract Task PreAuthenticate(HttpClient client, HttpRequestMessage request, ICredentials credentials);

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <param name="response">Response of the failed request</param>
        /// <returns>Task where the handler for a failed authentication gets executed</returns>
        public virtual async Task HandleChallenge(HttpClient client, HttpRequestMessage request, ICredentials credentials, HttpResponseMessage response)
        {
            if (!CanHandleChallenge(client, request, credentials, response))
                throw new InvalidOperationException();
            await Client.GetCurrentToken(forceUpdate: true);
        }
    }
}
