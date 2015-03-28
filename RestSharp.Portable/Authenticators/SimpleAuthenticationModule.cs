using System.Net;
using System.Net.Http;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Simple authenticator that adds the authentication information as GetOrPost parameter
    /// </summary>
    public class SimpleAuthenticationModule : IRestAuthenticationModule
    {
        private readonly string _usernameKey;
        private readonly string _passwordKey;
        private readonly ParameterType _parameterType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAuthenticationModule" /> class.
        /// </summary>
        /// <param name="usernameKey">Parameter name for the user name</param>
        /// <param name="passwordKey">Parameter name for the password</param>
        public SimpleAuthenticationModule(string usernameKey, string passwordKey)
            : this(usernameKey, passwordKey, ParameterType.GetOrPost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAuthenticationModule" /> class.
        /// </summary>
        /// <param name="usernameKey">Parameter name for the user name</param>
        /// <param name="passwordKey">Parameter name for the password</param>
        /// <param name="parameterType">Type of the parameter used for user name and password</param>
        public SimpleAuthenticationModule(string usernameKey, string passwordKey, ParameterType parameterType)
        {
            _usernameKey = usernameKey;
            _passwordKey = passwordKey;
            _parameterType = parameterType;
        }

        /// <summary>
        /// Gets the authentication type provided by this authentication module.
        /// </summary>
        public string AuthenticationType
        {
            get { return "Simple"; }
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
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        public void PreAuthenticate(
            IRestClient client,
            IRestRequest request,
            AuthHeader header,
            NetworkCredential credential)
        {
            if (credential == null)
                return;
            TryAuthenticate(client, request, credential);
        }

        /// <summary>
        /// Will be called in response to an authentication request.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        /// <param name="header">Authentication/Authorization header</param>
        /// <param name="credential">Credential to be used for the authentication</param>
        /// <returns>true when the authentication succeeded</returns>
        public bool Authenticate(
            IRestClient client,
            IRestRequest request,
            HttpResponseMessage response,
            AuthHeader header,
            NetworkCredential credential)
        {
            if (credential == null)
                return false;
            return TryAuthenticate(client, request, credential);
        }

        private bool TryAuthenticate(IRestClient client, IRestRequest request, NetworkCredential credential)
        {
            if (credential == null)
                return false;

            var userName = string.IsNullOrEmpty(credential.Domain)
                ? credential.UserName
                : string.Format("{0}\\{1}", credential.Domain, credential.UserName);

            if (client.HasDefaultParameterWithValue(_usernameKey, userName))
                return false;
            if (client.HasRequestParameterWithValue(request, _usernameKey, userName))
                return false;

            var password = credential.Password;

            if (client.HasDefaultParameterWithValue(_passwordKey, password))
                return false;
            if (client.HasRequestParameterWithValue(request, _passwordKey, password))
                return false;

            client.RemoveRequestParameter(request, _usernameKey);
            client.RemoveRequestParameter(request, _passwordKey);

            request.AddParameter(_usernameKey, credential.UserName, _parameterType);
            request.AddParameter(_passwordKey, credential.Password, _parameterType);

            return true;
        }
    }
}
