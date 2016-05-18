using System;

using RestSharp.Portable.Content;
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
#if NETSTANDARD1_1
            EncodingHandlers.Add("gzip", new Encodings.GzipEncoding());
            EncodingHandlers.Add("deflate", new Encodings.GzipEncoding());
#endif            
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
        /// Gets the content for a request.
        /// </summary>
        /// <param name="request">The <see cref="IRestRequest"/> to get the content for.</param>
        /// <returns>The <see cref="IHttpContent"/> for the <paramref name="request"/></returns>
        protected override IHttpContent GetContent(IRestRequest request)
        {
            return GenericContentCollector.GetContent(this, request);
        }
    }
}
