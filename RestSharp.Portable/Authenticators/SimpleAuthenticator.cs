namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Simple authenticator that adds the authentication information as GetOrPost parameter
    /// </summary>
    public class SimpleAuthenticator : IAuthenticator
    {
        private readonly string _usernameKey;
        private readonly string _username;
        private readonly string _passwordKey;
        private readonly string _password;

        /// <summary>
        /// Constructor for the simple authenticator
        /// </summary>
        /// <param name="usernameKey">GetOrPost parameter name for the user name</param>
        /// <param name="username">User name</param>
        /// <param name="passwordKey">GetOrPost parameter name for the password</param>
        /// <param name="password">Password</param>
        public SimpleAuthenticator(string usernameKey, string username, string passwordKey, string password)
        {
            _usernameKey = usernameKey;
            _username = username;
            _passwordKey = passwordKey;
            _password = password;
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddParameter(_usernameKey, _username);
            request.AddParameter(_passwordKey, _password);
        }
    }
}
