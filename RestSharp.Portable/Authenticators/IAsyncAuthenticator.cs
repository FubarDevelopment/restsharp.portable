using System.Threading.Tasks;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The async authenticator interface
    /// </summary>
    public interface IAsyncAuthenticator
    {
        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <returns>The task the authentication is performed on</returns>
        Task Authenticate(IRestClient client, IRestRequest request);
    }
}
