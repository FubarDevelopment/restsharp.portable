using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// Interface that's an abstraction from the default HttpClient provided by the .NET framework
    /// </summary>
    public interface IHttpClient : IDisposable
    {
        /// <summary>
        /// Gets or sets the base address of the HTTP client
        /// </summary>
        Uri BaseAddress { get; set; }

        /// <summary>
        /// Gets the default request headers
        /// </summary>
        IHttpHeaders DefaultRequestHeaders { get; }

        /// <summary>
        /// Gets or sets the timeout
        /// </summary>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// Asynchronously send a request
        /// </summary>
        /// <param name="request">The request do send</param>
        /// <param name="cancellationToken">The cancellation token used to signal an abortion</param>
        /// <returns>The task to query the response</returns>
        Task<IHttpResponseMessage> SendAsync(IHttpRequestMessage request, CancellationToken cancellationToken);
    }
}
