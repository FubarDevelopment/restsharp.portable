using System;
using System.Net;
using System.Threading.Tasks;

namespace RestSharp.Portable.Authenticators.OAuth2.Infrastructure
{
    /// <summary>
    /// REST client extensions
    /// </summary>
    public static class RestClientExtensions
    {
        /// <summary>
        /// Execute a request and test if the status code is OK
        /// </summary>
        /// <param name="client">
        /// The client that executes the request
        /// </param>
        /// <param name="request">
        /// The request to be executed
        /// </param>
        /// <returns>
        /// The response of the request
        /// </returns>
        public static async Task<IRestResponse> ExecuteAndVerify(this IRestClient client, IRestRequest request)
        {
            var response = await client.Execute(request);
            if ((response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                || response.IsEmpty())
            {
                throw new UnexpectedResponseException(response);
            }

            return response;
        }
    }
}
