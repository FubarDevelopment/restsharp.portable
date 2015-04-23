using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Simple authenticator that adds the authentication information as GetOrPost parameter
    /// </summary>
    public class SimpleAuthenticator : IAuthenticator
    {
        /// <summary>
        /// The authentication method ID used to search for the credentials.
        /// </summary>
        public const string AuthenticationMethod = "Simple";

        private readonly string _usernameKey;

        private readonly string _passwordKey;

        private readonly ParameterType _parameterType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAuthenticator" /> class.
        /// </summary>
        /// <param name="usernameKey">GetOrPost parameter name for the user name</param>
        /// <param name="passwordKey">GetOrPost parameter name for the password</param>
        public SimpleAuthenticator(string usernameKey, string passwordKey)
            : this(usernameKey, passwordKey, ParameterType.GetOrPost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAuthenticator" /> class.
        /// </summary>
        /// <param name="usernameKey">GetOrPost parameter name for the user name</param>
        /// <param name="passwordKey">GetOrPost parameter name for the password</param>
        /// <param name="parameterType">The type of the request parameter</param>
        public SimpleAuthenticator(string usernameKey, string passwordKey, ParameterType parameterType)
        {
            _usernameKey = usernameKey;
            _passwordKey = passwordKey;
            _parameterType = parameterType;
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
            if (credentials == null)
                return false;
            var cred = credentials.GetCredential(client.BuildUri(request, false), AuthenticationMethod);
            if (cred == null)
                return false;
            return true;
        }

        /// <summary>
        /// Does the authentication module supports pre-authentication for the given <see cref="HttpRequestMessage" />?
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
                if (credentials == null)
                    throw new InvalidOperationException("The credentials must be set using the IRestClient.Credential property.");
                var cred = credentials.GetCredential(client.BuildUri(request, false), AuthenticationMethod);
                if (cred == null)
                    throw new InvalidOperationException(string.Format("No credentials provided for the {0} authentication type.", AuthenticationMethod));
                request.AddParameter(_usernameKey, cred.UserName, _parameterType);
                request.AddParameter(_passwordKey, cred.Password, _parameterType);
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
        /// <param name="client">The HTTP client the response is assigned to</param>
        /// <param name="request">The HTTP request the response is assigned to</param>
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
        public Task HandleChallenge(IHttpClient client, IHttpRequestMessage request, ICredentials credentials, IHttpResponseMessage response)
        {
            throw new NotSupportedException();
        }
    }
}
