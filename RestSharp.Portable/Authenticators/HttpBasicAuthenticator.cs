using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The default HTTP Basic authenticator
    /// </summary>
    /// <remarks>
    /// Code was taken from http://www.ifjeffcandoit.com/2013/05/16/digest-authentication-with-restsharp/
    /// </remarks>
    public class HttpBasicAuthenticator : IAuthenticator
    {
        private readonly string _username;
        private readonly string _password;

        /// <summary>
        /// Constructor taking the user name and password for the HTTP Basic authenticator
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="password">Password</param>
        public HttpBasicAuthenticator(string username, string password)
        {
            _password = password;
            _username = username;
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            // only add the Authorization parameter if it hasn't been added by a previous Execute
            if (request.Parameters.Any(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
                return;
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", _username, _password)));
            var authHeader = string.Format("Basic {0}", token);
            request.AddParameter("Authorization", authHeader, ParameterType.HttpHeader);
        }
    }
}
