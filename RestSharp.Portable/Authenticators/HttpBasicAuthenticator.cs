using System;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The default HTTP Basic authenticator
    /// </summary>
    public class HttpBasicAuthenticator : ISyncAuthenticator
    {
        private readonly string _authHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpBasicAuthenticator" /> class.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="password">The users password</param>
        public HttpBasicAuthenticator(string username, string password)
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password)));
            _authHeader = string.Format("Basic {0}", token);
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
            // only add the Authorization parameter if it hasn't been added by a previous Execute
            if (request.Parameters.Any(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
                return;
            request.AddParameter("Authorization", _authHeader, ParameterType.HttpHeader);
        }

        /// <summary>
        /// Gets a value indicating whether the authentication module can handle the challenge sent with the response.
        /// </summary>
        public virtual bool CanHandleChallenge(HttpResponseMessage response)
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
