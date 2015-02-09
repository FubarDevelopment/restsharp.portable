using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// The default REST response
    /// </summary>
    public class RestResponse : IRestResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestResponse" /> class.
        /// </summary>
        /// <param name="client">REST client</param>
        /// <param name="request">REST request</param>
        public RestResponse(IRestClient client, IRestRequest request)
        {
            Client = client;
            Request = request;
        }

        /// <summary>
        /// Gets the Request that resulted in this response
        /// </summary>
        public IRestRequest Request { get; private set; }

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

        /// <summary>
        /// Gets the response headers (without content headers)
        /// </summary>
        public HttpHeaders Headers { get; private set; }

        /// <summary>
        /// Gets the HTTP status code
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets the description for the HTTP status code
        /// </summary>
        public string StatusDescription { get; private set; }

        /// <summary>
        /// Gets the REST client that created this response
        /// </summary>
        protected IRestClient Client { get; private set; }

        /// <summary>
        /// Utility function that really initializes this response object from
        /// a HttpResponseMessage
        /// </summary>
        /// <param name="response">Response that will be used to initialize this response.</param>
        /// <returns>Task, because this function runs asynchronously</returns>
        protected internal async virtual Task LoadResponse(HttpResponseMessage response)
        {
            Headers = response.Headers;

            StatusCode = response.StatusCode;
            StatusDescription = response.ReasonPhrase;

            ResponseUri = response.Headers.Location ?? Client.BuildUri(Request, false);
            var data = await response.Content.ReadAsByteArrayAsync();

            var contentType = response.Content.Headers.ContentType;
            var mediaType = contentType == null ? string.Empty : contentType.MediaType;
            ContentType = mediaType;

            var encoding = Client.GetEncoding(response.Content.Headers.ContentEncoding);
            if (encoding != null)
                data = encoding.Decode(data);

            RawBytes = data;
        }
    }
}
