using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RestSharp.Portable
{
    public class RestResponse : IRestResponse
    {
        public IRestRequest Request { get; private set; }

        public Uri ResponseUri { get; private set; }

        public RestResponse(IRestRequest request, HttpWebResponse response)
        {
            ResponseUri = response.ResponseUri;
            Request = request;
            var temp = new MemoryStream();
            using (var stream = response.GetResponseStream())
                stream.CopyTo(temp);
            RawBytes = temp.ToArray();
        }

        public byte[] RawBytes { get; private set; }
    }

    public class RestResponse<T> : RestResponse, IRestResponse<T>
    {
        public RestResponse(IRestClient client, IRestRequest request, HttpWebResponse response)
            : base(request, response)
        {
            var handler = client.GetHandler(response.ContentType);
            Data = handler.Deserialize<T>(this);
        }

        public T Data { get; private set; }
    }
}
