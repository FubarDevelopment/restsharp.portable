using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RestSharp.Portable
{
    public class RestResponse
    {
        public RestRequest Request { get; private set; }

        public Uri ResponseUri { get; private set; }

        public RestResponse(RestRequest request, HttpWebResponse response)
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

    public class RestResponse<T> : RestResponse
    {
        public RestResponse(RestRequest request, HttpWebResponse response)
            : base(request, response)
        {
            var input = new MemoryStream(RawBytes);
            using (var reader = new StreamReader(input))
            {
                var serializer = new JsonSerializer();
                Data = serializer.Deserialize<T>(new JsonTextReader(reader));
            }
        }

        public T Data { get; private set; }
    }
}
