using System;
using System.Threading;
using System.Threading.Tasks;

using RestSharp.Portable.WebRequest.Impl;

namespace RestSharp.Portable.WebRequest
{
    /// <summary>
    /// The default REST client
    /// </summary>
    public class RestClient : RestClientBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// </summary>
        public RestClient()
            : base(new WebRequestHttpClientFactory())
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
            using (var response = await ExecuteRequest(request, CancellationToken.None))
            {
                return await RestResponse.CreateResponse(this, request, response);
            }
        }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned, with a deserialized object</returns>
        public override async Task<IRestResponse<T>> Execute<T>(IRestRequest request)
        {
            using (var response = await ExecuteRequest(request, CancellationToken.None))
            {
                return await RestResponse.CreateResponse<T>(this, request, response);
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
            using (var response = await ExecuteRequest(request, ct))
            {
                return await RestResponse.CreateResponse(this, request, response);
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
            using (var response = await ExecuteRequest(request, ct))
            {
                return await RestResponse.CreateResponse<T>(this, request, response);
            }
        }

        /// <summary>
        /// Gets the content for a request.
        /// </summary>
        /// <param name="request">The <see cref="IRestRequest"/> to get the content for.</param>
        /// <returns>The <see cref="IHttpContent"/> for the <paramref name="request"/></returns>
        protected override IHttpContent GetContent(IRestRequest request)
        {
            return RestClientExtensions.GetContent(this, request);
        }
    }
}
