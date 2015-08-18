using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RestSharp.Portable.HttpClient
{
    /// <summary>
    /// Extension functions for REST requests
    /// </summary>
    public static class RestRequestExtensions
    {
        /// <summary>
        /// Returns the HttpContent for the body parameter
        /// </summary>
        /// <param name="request">
        /// The request the body parameter belongs to
        /// </param>
        /// <param name="body">
        /// The body parameter
        /// </param>
        /// <returns>
        /// The resulting HttpContent
        /// </returns>
        internal static HttpContent GetBodyContent(this IRestRequest request, Parameter body)
        {
            if (body == null)
                return null;

            MediaTypeHeaderValue contentType;
            var buffer = body.Value as byte[];
            if (buffer != null)
            {
                contentType = MediaTypeHeaderValue.Parse(body.ContentType);
            }
            else
            {
                buffer = request.Serializer.Serialize(body.Value);
                contentType = MediaTypeHeaderValue.Parse(request.Serializer.ContentType);
            }

            var content = new ByteArrayContent(buffer);
            content.Headers.ContentType = contentType;
            content.Headers.ContentLength = buffer.Length;
            return content;
        }
    }
}
