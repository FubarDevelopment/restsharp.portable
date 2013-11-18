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
    public class RestResponse : IRestResponse
    {
        protected IRestClient Client { get; private set; }

        public IRestRequest Request { get; private set; }

        public Uri ResponseUri { get; private set; }

        public RestResponse(IRestClient client, IRestRequest request)
        {
            Client = client;
            Request = request;
        }

        protected internal async virtual Task LoadResponse(HttpResponseMessage response)
        {
            ResponseUri = response.Headers.Location ?? Client.BuildUrl(Request, false);
            RawBytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType;
            var mediaType = contentType == null ? string.Empty : contentType.MediaType;
            ContentType = mediaType;
        }

        public byte[] RawBytes { get; private set; }

        public string ContentType { get; private set; }
    }

    public class RestResponse<T> : RestResponse, IRestResponse<T>
    {
        public RestResponse(IRestClient client, IRestRequest request)
            : base(client, request)
        {
        }

        protected internal override async Task LoadResponse(HttpResponseMessage response)
        {
            await base.LoadResponse(response);
            var handler = Client.GetHandler(ContentType);
            Data = handler.Deserialize<T>(this);
        }

        public T Data { get; private set; }
    }
}
