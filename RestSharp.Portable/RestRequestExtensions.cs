using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace RestSharp.Portable
{
    public static class RestRequestExtensions
    {
        public static HttpContent GetContent(this IRestRequest request)
        {
            HttpContent content;
            var body = request.Parameters.FirstOrDefault(x => x.Type == ParameterType.RequestBody);
            if (body != null)
            {
                MediaTypeHeaderValue contentType;
                byte[] buffer;
                if (body.Value is byte[])
                {
                    buffer = (byte[])body.Value;
                    contentType = body.ContentType;
                }
                else
                {
                    buffer = request.Serializer.Serialize(body.Value);
                    contentType = request.Serializer.ContentType;
                }
                content = new ByteArrayContent(buffer);
                content.Headers.ContentType = contentType;
                content.Headers.ContentLength = buffer.Length;
            }
            else if (request.Method == HttpMethod.Post)
            {
                var postData = request.Parameters.Where(x => x.Type == ParameterType.GetOrPost)
                    .Select(x => new KeyValuePair<string, string>(x.Name, string.Format("{0}", x.Value)))
                    .ToList();
                content = new FormUrlEncodedContent(postData);
            }
            else
            {
                content = null;
            }
            return content;
        }
    }
}
