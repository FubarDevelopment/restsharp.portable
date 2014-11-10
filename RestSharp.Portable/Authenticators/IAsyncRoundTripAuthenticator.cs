using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// This authenticator can handle 401 responses and modify the Authentication behavior/result.
    /// </summary>
    public interface IAsyncRoundTripAuthenticator : IAsyncAuthenticator
    {
        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        Task AuthenticationFailed(IRestClient client, IRestRequest request, IRestResponse response);

        /// <summary>
        /// Returns all the status codes where a round trip is allowed
        /// </summary>
        IEnumerable<HttpStatusCode> StatusCodes { get; }
    }
}
