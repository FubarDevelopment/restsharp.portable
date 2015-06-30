using System;
using System.Net.Http;
using System.Net.Http.Headers;

using RestSharp.Portable.Serializers;

namespace RestSharp.Portable
{
    /// <summary>
    /// Extension functions for REST requests
    /// </summary>
    public static class RestRequestExtensions
    {
        private static readonly JsonSerializer _defaultJsonSerializer = new JsonSerializer();
        private static readonly XmlDataContractSerializer _defaultXmlDataContractSerializer = new XmlDataContractSerializer();

        /// <summary>
        /// Body to add to the parameters using a default <see cref="RestSharp.Portable.Serializers.JsonSerializer"/>.
        /// </summary>
        /// <param name="request">
        /// The REST request to add this parameter to
        /// </param>
        /// <param name="obj">
        /// Object to serialize to the request body
        /// </param>
        /// <returns>
        /// The request object to allow call chains
        /// </returns>
        public static IRestRequest AddJsonBody(this IRestRequest request, object obj)
        {
            var serializer = _defaultJsonSerializer;
            var data = serializer.Serialize(obj);
            return request.AddParameter(new Parameter { Value = data, Type = ParameterType.RequestBody, ContentType = serializer.ContentType });
        }

        /// <summary>
        /// Body to add to the parameters using a default <see cref="RestSharp.Portable.Serializers.XmlDataContractSerializer"/>.
        /// </summary>
        /// <param name="request">
        /// The REST request to add this parameter to
        /// </param>
        /// <param name="obj">
        /// Object to serialize to the request body
        /// </param>
        /// <returns>
        /// The request object to allow call chains
        /// </returns>
        public static IRestRequest AddXmlBody(this IRestRequest request, object obj)
        {
            var serializer = _defaultXmlDataContractSerializer;
            var data = serializer.Serialize(obj);
            return request.AddParameter(new Parameter { Value = data, Type = ParameterType.RequestBody, ContentType = serializer.ContentType });
        }

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
