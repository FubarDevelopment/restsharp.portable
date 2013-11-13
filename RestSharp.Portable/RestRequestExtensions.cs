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
        public static IRestRequest AddUrlSegment(this IRestRequest request, string name, object value)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = ParameterType.UrlSegment });
        }

        public static IRestRequest AddHeader(this IRestRequest request, string name, object value)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = ParameterType.HttpHeader });
        }

        public static IRestRequest AddParameter(this IRestRequest request, string name, object value)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = ParameterType.GetOrPost });
        }

        public static IRestRequest AddParameter(this IRestRequest request, string name, object value, ParameterType type)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = type });
        }

        public static IRestRequest AddParameter(this IRestRequest request, string name, object value, ParameterType type, MediaTypeHeaderValue contentType)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = type, ContentType = contentType });
        }

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
