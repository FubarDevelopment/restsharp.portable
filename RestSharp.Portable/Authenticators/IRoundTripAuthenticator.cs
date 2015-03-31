using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

using JetBrains.Annotations;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// This authenticator can handle 401 responses and modify the Authentication behavior/result.
    /// </summary>
    public interface IRoundTripAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Gets all the status codes where a round trip is allowed
        /// </summary>
        IEnumerable<HttpStatusCode> StatusCodes { get; }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        void Authenticate([NotNull] IRestClient client, [NotNull] IRestRequest request, [NotNull] HttpResponseMessage response);
    }
}
