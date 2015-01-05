using RestSharp.Portable.Authenticators.OAuth2.Configuration;
using System.Linq;

namespace RestSharp.Portable.Authenticators.OAuth2
{
    /// <summary>
    /// Event arguments used before and after a request.
    /// </summary>
    public class BeforeAfterRequestArgs
    {
        /// <summary>
        /// Client instance.
        /// </summary>
        public IRestClient Client { get; set; }

        /// <summary>
        /// Request instance.
        /// </summary>
        public IRestRequest Request { get; set; }

        /// <summary>
        /// Response instance.
        /// </summary>
        public IRestResponse Response { get; set; }

        /// <summary>
        /// Values received from service.
        /// </summary>
        public ILookup<string, string> Parameters { get; set; }

        /// <summary>
        /// Client configuration.
        /// </summary>
        public IClientConfiguration Configuration { get; set; }
    }
}