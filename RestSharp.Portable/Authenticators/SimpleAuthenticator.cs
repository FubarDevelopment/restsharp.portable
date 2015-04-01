using System;
using System.Net.Http;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Simple authenticator that adds the authentication information as GetOrPost parameter
    /// </summary>
    public class SimpleAuthenticator : ISyncAuthenticator
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
        /// Gets a value indicating whether the authentication module supports pre-authentication.
        /// </summary>
        public bool CanPreAuthenticate
        {
            get { return true; }
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        public void PreAuthenticate(IRestClient client, IRestRequest request)
        {
            if (client.Credentials == null)
                throw new InvalidOperationException("The credentials must be set using the IRestClient.Credential property.");
            var cred = client.Credentials.GetCredential(client.BuildUri(request, false), AuthenticationMethod);
            if (cred == null)
                throw new InvalidOperationException(string.Format("No credentials provided for the {0} authentication type.", AuthenticationMethod));
            request.AddParameter(_usernameKey, cred.UserName, _parameterType);
            request.AddParameter(_passwordKey, cred.Password, _parameterType);
        }

        /// <summary>
        /// Determines if the authentication module can handle the challenge sent with the response.
        /// </summary>
        /// <param name="client">The REST client the response is assigned to</param>
        /// <param name="request">The REST request the response is assigned to</param>
        /// <param name="response">The response that returned the authentication challenge</param>
        /// <returns>true when the authenticator can handle the sent challenge</returns>
        public virtual bool CanHandleChallenge(IRestClient client, IRestRequest request, HttpResponseMessage response)
        {
            return false;
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        public void HandleChallenge(IRestClient client, IRestRequest request, HttpResponseMessage response)
        {
            throw new NotSupportedException();
        }
    }
}
