using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Portable.HttpClient
{
    /// <summary>
    /// The default REST client
    /// </summary>
    public sealed class RestClient : RestClientBase
    {
        private readonly AsyncLock _requestGuard = new AsyncLock();

        private bool _disposedValue; // For the detection of multiple calls

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// </summary>
        public RestClient()
            : base(new Impl.DefaultHttpClientFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// </summary>
        /// <param name="baseUrl">Base URL</param>
        public RestClient(Uri baseUrl)
            : this()
        {
            BaseUrl = baseUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// </summary>
        /// <param name="baseUrl">Base URL</param>
        public RestClient(string baseUrl)
            : this(new Uri(baseUrl))
        {
        }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned</returns>
        public override async Task<IRestResponse> Execute(IRestRequest request)
        {
            using (await _requestGuard.LockAsync(CancellationToken.None))
            {
                using (var response = await ExecuteRequest(request, CancellationToken.None))
                {
                    return await RestResponse.CreateResponse(this, request, response);
                }
            }
        }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <typeparam name="T">
        /// The type to deserialize the response to.
        /// </typeparam>
        /// <param name="request">
        /// Request to execute
        /// </param>
        /// <returns>
        /// Response returned, with a deserialized object
        /// </returns>
        public override async Task<IRestResponse<T>> Execute<T>(IRestRequest request)
        {
            using (await _requestGuard.LockAsync(CancellationToken.None))
            {
                using (var response = await ExecuteRequest(request, CancellationToken.None))
                {
                    return await RestResponse.CreateResponse<T>(this, request, response);
                }
            }
        }

        /// <summary>
        /// Cancellable request execution
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>Response returned</returns>
        public override async Task<IRestResponse> Execute(IRestRequest request, CancellationToken ct)
        {
            using (await _requestGuard.LockAsync(ct))
            {
                using (var response = await ExecuteRequest(request, ct))
                {
                    return await RestResponse.CreateResponse(this, request, response);
                }
            }
        }

        /// <summary>
        /// Cancellable request execution
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="request">Request to execute</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>Response returned, with a deserialized object</returns>
        public override async Task<IRestResponse<T>> Execute<T>(IRestRequest request, CancellationToken ct)
        {
            using (await _requestGuard.LockAsync(ct))
            {
                using (var response = await ExecuteRequest(request, ct))
                {
                    return await RestResponse.CreateResponse<T>(this, request, response);
                }
            }
        }

        /// <summary>
        /// Dispose the <see cref="IHttpClient"/>.
        /// </summary>
        /// <param name="disposing"><code>true</code> when called from <see cref="RestClientBase.Dispose()"/></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _requestGuard.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Updates the <code>Accepts</code> default header parameter
        /// </summary>
        protected override void UpdateAcceptsHeader()
        {
            UpdateAcceptsHeader(!EnvironmentUtilities.IsMono);
        }

        /// <summary>
        /// Allows the implementor to modify the <paramref name="httpClient"/> and the <paramref name="requestMessage"/>
        /// before the request gets authenticated and sent.
        /// </summary>
        /// <param name="httpClient">The <see cref="IHttpClient"/> used to send the <paramref name="requestMessage"/></param>
        /// <param name="requestMessage">The <see cref="IHttpRequestMessage"/> to send</param>
        protected override void ModifyRequestBeforeAuthentication(IHttpClient httpClient, IHttpRequestMessage requestMessage)
        {
            base.ModifyRequestBeforeAuthentication(httpClient, requestMessage);

            if (EnvironmentUtilities.IsSilverlight && requestMessage.Method == Method.GET)
            {
                httpClient.DefaultRequestHeaders.Remove("Accept");
            }
        }

        /// <inheritdoc/>
        protected override IHttpContent GetContent(IRestRequest request, RequestParameters requestParameters)
        {
            return RestClientExtensions.GetContent(this, request, requestParameters);
        }
    }
}
