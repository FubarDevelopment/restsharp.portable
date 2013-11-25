using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// The default REST response
    /// </summary>
    public class RestResponse : IRestResponse
    {
        /// <summary>
        /// The REST client that created this response
        /// </summary>
        protected IRestClient Client { get; private set; }

        /// <summary>
        /// Request that resulted in this response
        /// </summary>
        public IRestRequest Request { get; private set; }

        /// <summary>
        /// The full response URL
        /// </summary>
        public Uri ResponseUri { get; private set; }

        /// <summary>
        /// Constructor that initializes the REST client and request
        /// </summary>
        /// <param name="client">REST client</param>
        /// <param name="request">REST request</param>
        public RestResponse(IRestClient client, IRestRequest request)
        {
            Client = client;
            Request = request;
        }

        /// <summary>
        /// Utility function that really initializes this response object from
        /// a HttpResponseMessage
        /// </summary>
        /// <param name="response">Response that will be used to initialize this response.</param>
        /// <returns>Task, because this function runs asynchronously</returns>
        protected internal async virtual Task LoadResponse(HttpResponseMessage response)
        {
            ResponseUri = response.Headers.Location ?? Client.BuildUrl(Request, false);
            var data = await response.Content.ReadAsByteArrayAsync();
            
            var contentType = response.Content.Headers.ContentType;
            var mediaType = contentType == null ? string.Empty : contentType.MediaType;
            ContentType = mediaType;

            var encoding = Client.GetEncoding(response.Content.Headers.ContentEncoding);
            if (encoding != null)
                data = encoding.Decode(data);

            RawBytes = data;
        }

        /// <summary>
        /// The raw data
        /// </summary>
        public byte[] RawBytes { get; private set; }

        /// <summary>
        /// Content type of the raw data
        /// </summary>
        public string ContentType { get; private set; }
    }

    /// <summary>
    /// The default deserializing REST response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RestResponse<T> : RestResponse, IRestResponse<T>
    {
        /// <summary>
        /// Constructor that initializes the REST client and request
        /// </summary>
        /// <param name="client">REST client</param>
        /// <param name="request">REST request</param>
        public RestResponse(IRestClient client, IRestRequest request)
            : base(client, request)
        {
        }

        /// <summary>
        /// Utility function that really initializes this response object from
        /// a HttpResponseMessage
        /// </summary>
        /// <param name="response">Response that will be used to initialize this response.</param>
        /// <returns>Task, because this function runs asynchronously</returns>
        /// <remarks>
        /// This override also deserializes the response
        /// </remarks>
        protected internal override async Task LoadResponse(HttpResponseMessage response)
        {
            await base.LoadResponse(response);
            var handler = Client.GetHandler(ContentType);
            Data = handler.Deserialize<T>(this);
        }

        /// <summary>
        /// Deserialized object of type T
        /// </summary>
        public T Data { get; private set; }
    }
}
