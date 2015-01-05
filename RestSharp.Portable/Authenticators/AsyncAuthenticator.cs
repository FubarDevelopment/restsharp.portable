using System.Threading.Tasks;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// Abstract base class for asynchronous authenticators
    /// </summary>
    public abstract class AsyncAuthenticator : IAuthenticator, IAsyncAuthenticator
    {
        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <returns></returns>
        public abstract Task Authenticate(IRestClient client, IRestRequest request);

        /// <summary>
        /// This is for API compatibility with the current IRestClient
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        void IAuthenticator.Authenticate(IRestClient client, IRestRequest request)
        {
            Authenticate(client, request).Wait();
        }
    }
}
