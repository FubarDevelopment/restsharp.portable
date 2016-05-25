using System;
using System.Collections.Generic;

namespace RestSharp.Portable
{
    /// <summary>
    /// Interface to allow custom creation of HttpClient and HttpRequestMessage objects
    /// </summary>
    /// <remarks>
    /// The DefaultHttpClientFactory contains some helpful protected methods that helps gathering
    /// the data required for a proper configuration of the HttpClient.
    /// </remarks>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Create the client
        /// </summary>
        /// <param name="client">The REST client that wants to create the HTTP client</param>
        /// <returns>A new HttpClient object</returns>
        /// <remarks>
        /// The DefaultHttpClientFactory contains some helpful protected methods that helps gathering
        /// the data required for a proper configuration of the HttpClient.
        /// </remarks>
        IHttpClient CreateClient(IRestClient client);

        /// <summary>
        /// Create the request message
        /// </summary>
        /// <param name="client">The REST client that wants to create the HTTP request message</param>
        /// <param name="request">The REST request for which the HTTP request message is created</param>
        /// <param name="parameters">The parameters for the REST request (read-only)</param>
        /// <returns>A new HttpRequestMessage object</returns>
        /// <remarks>
        /// The DefaultHttpClientFactory contains some helpful protected methods that helps gathering
        /// the data required for a proper configuration of the HttpClient.
        /// </remarks>
        IHttpRequestMessage CreateRequestMessage(IRestClient client, IRestRequest request, IList<Parameter> parameters);
    }
}
