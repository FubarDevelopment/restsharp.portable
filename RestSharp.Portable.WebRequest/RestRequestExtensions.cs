using System;

using RestSharp.Portable.Content;

namespace RestSharp.Portable.WebRequest
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
        internal static IHttpContent GetBodyContent(this IRestRequest request, Parameter body)
        {
            if (body == null)
                return null;

            string contentType;
            var buffer = body.Value as byte[];
            if (buffer != null)
            {
                contentType = body.ContentType;
            }
            else
            {
                buffer = request.Serializer.Serialize(body.Value);
                contentType = request.Serializer.ContentType;
            }

            var content = new ByteArrayContent(buffer);
            content.Headers.ReplaceWithoutValidation("Content-Type", contentType);
            content.Headers.ReplaceWithoutValidation("Content-Length", buffer.Length.ToString());
            return content;
        }
    }
}
