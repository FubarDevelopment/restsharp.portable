using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// The default REST response
    /// </summary>
    public class RestResponse : IRestResponse
    {
        private readonly Lazy<string> _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestResponse" /> class.
        /// </summary>
        /// <param name="client">REST client</param>
        /// <param name="request">REST request</param>
        protected RestResponse(IRestClient client, IRestRequest request)
        {
            Client = client;
            Request = request;
            _content = new Lazy<string>(this.GetStringContent);
        }

        /// <summary>
        /// Gets the Request that resulted in this response
        /// </summary>
        public IRestRequest Request { get; }

        /// <summary>
        /// Gets the full response URL
        /// </summary>
        public Uri ResponseUri { get; private set; }

        /// <summary>
        /// Gets the raw data
        /// </summary>
        public byte[] RawBytes { get; private set; }

        /// <summary>
        /// Gets the content type of the raw data
        /// </summary>
        public string ContentType { get; private set; }

        /// <inheritdoc/>
        public CookieCollection Cookies { get; private set; }

        /// <summary>
        /// Gets the response headers (without content headers)
        /// </summary>
        public IHttpHeaders Headers { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the request was successful.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Gets the HTTP status code
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets the description for the HTTP status code
        /// </summary>
        public string StatusDescription { get; private set; }

        /// <inheritdoc />
        public string Content => _content.Value;

        /// <summary>
        /// Gets the REST client that created this response
        /// </summary>
        protected IRestClient Client { get; }

        /// <summary>
        /// Create a <see cref="IRestResponse"/> from a <see cref="IRestClient"/>, <see cref="IRestRequest"/> and a <see cref="IHttpResponseMessage"/>.
        /// </summary>
        /// <param name="client">The <see cref="IRestClient"/> used to create a <see cref="IRestResponse"/></param>
        /// <param name="request">The <see cref="IRestRequest"/> used to create a <see cref="IRestResponse"/></param>
        /// <param name="responseMessage">The <see cref="IHttpResponseMessage"/> used to create a <see cref="IRestResponse"/></param>
        /// <returns>The new <see cref="IRestResponse"/></returns>
        public static async Task<IRestResponse> CreateResponse(IRestClient client, IRestRequest request, IHttpResponseMessage responseMessage)
        {
            var response = new RestResponse(client, request);
            await response.LoadResponse(responseMessage);
            return response;
        }

        /// <summary>
        /// Create a <see cref="IRestResponse"/> from a <see cref="IRestClient"/>, <see cref="IRestRequest"/> and a <see cref="IHttpResponseMessage"/>.
        /// </summary>
        /// <typeparam name="T">The type to instantiate the response for.</typeparam>
        /// <param name="client">The <see cref="IRestClient"/> used to create a <see cref="IRestResponse"/></param>
        /// <param name="request">The <see cref="IRestRequest"/> used to create a <see cref="IRestResponse"/></param>
        /// <param name="responseMessage">The <see cref="IHttpResponseMessage"/> used to create a <see cref="IRestResponse"/></param>
        /// <returns>The new <see cref="IRestResponse"/></returns>
        public static async Task<IRestResponse<T>> CreateResponse<T>(IRestClient client, IRestRequest request, IHttpResponseMessage responseMessage)
        {
            var response = new RestResponse<T>(client, request);
            await response.LoadResponse(responseMessage);
            return response;
        }

        /// <summary>
        /// Utility function that really initializes this response object from
        /// a HttpResponseMessage
        /// </summary>
        /// <param name="response">Response that will be used to initialize this response.</param>
        /// <returns>Task, because this function runs asynchronously</returns>
        protected virtual async Task LoadResponse(IHttpResponseMessage response)
        {
            Headers = response.Headers;

            IsSuccess = response.IsSuccessStatusCode;
            StatusCode = response.StatusCode;
            StatusDescription = response.ReasonPhrase;

            var requestUri = Client.BuildUri(Request, false);
            ResponseUri = new Uri(requestUri, response.Headers.GetValue("Location", requestUri.ToString()));

            Cookies = response.Cookies?.GetCookies(ResponseUri);

            var content = response.Content;

            if (content == null)
            {
                RawBytes = new byte[0];
            }
            else
            {
                var contentType = content.Headers.GetValue("Content-Type");
                if (string.IsNullOrEmpty(contentType))
                {
                    ContentType = string.Empty;
                }
                else
                {
                    var semicolonPos = contentType.IndexOf(';');
                    if (semicolonPos != -1)
                    {
                        contentType = contentType.Substring(0, semicolonPos);
                    }

                    ContentType = contentType.Trim();
                }

                var data = await content.ReadAsByteArrayAsync();

                IEnumerable<string> contentEncodings;
                if (content.Headers.TryGetValues("Content-Encoding", out contentEncodings))
                {
                    var encoding = Client.GetEncoding(contentEncodings);
                    if (encoding != null)
                    {
                        data = encoding.Decode(data);
                    }
                }

                RawBytes = data;
            }
        }
    }
}
