using System;

namespace RestSharp.Portable.HttpClient
{
    /// <summary>
    /// The default REST client
    /// </summary>
    public sealed class RestClient : RestClientBase
    {
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

        /// <inheritdoc/>
        protected override IHttpContent GetContent(IRestRequest request, RequestParameters parameters)
        {
            return RestClientExtensions.GetContent(this, request, parameters);
        }
    }
}
