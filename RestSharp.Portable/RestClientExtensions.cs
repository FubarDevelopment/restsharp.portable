using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using JetBrains.Annotations;

using RestSharp.Portable.Impl.Http;

namespace RestSharp.Portable
{
    /// <summary>
    /// Extension functions for REST clients
    /// </summary>
    public static class RestClientExtensions
    {
        /// <summary>
        /// Gets the content for a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">REST request to get the content for</param>
        /// <returns>The HTTP content to be sent</returns>
        internal static IHttpContent GetContent([CanBeNull] this IRestClient client, IRestRequest request)
        {
            HttpContent content;
            var parameters = client.MergeParameters(request);
            var collectionMode = request == null ? ContentCollectionMode.MultiPartForFileParameters : request.ContentCollectionMode;
            if (collectionMode != ContentCollectionMode.BasicContent)
            {
                var fileParameters = parameters.GetFileParameters().ToList();
                if (collectionMode == ContentCollectionMode.MultiPart || fileParameters.Count != 0)
                {
                    content = client.GetMultiPartContent(request);
                }
                else
                {
                    content = client.GetBasicContent(request);
                }
            }
            else
            {
                content = client.GetBasicContent(request);
            }

            if (content == null)
                return null;

            return new DefaultHttpContent(content);
        }

        /// <summary>
        /// Gets the basic content (without files) for a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">REST request to get the content for</param>
        /// <returns>The HTTP content to be sent</returns>
        internal static HttpContent GetBasicContent([CanBeNull] this IRestClient client, IRestRequest request)
        {
            HttpContent content;
            var parameters = client.MergeParameters(request);
            var body = parameters.FirstOrDefault(x => x.Type == ParameterType.RequestBody);
            if (body != null)
            {
                content = request.GetBodyContent(body);
            }
            else
            {
                if (client.GetEffectiveHttpMethod(request) == Method.POST)
                {
                    var getOrPostParameters = parameters.GetGetOrPostParameters().ToList();
                    if (getOrPostParameters.Count != 0)
                    {
                        content = new PostParametersContent(getOrPostParameters).AsHttpContent();
                    }
                    else
                    {
                        content = null;
                    }
                }
                else
                {
                    content = null;
                }
            }

            return content;
        }

        /// <summary>
        /// Gets the multi-part content (with files) for a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">REST request to get the content for</param>
        /// <returns>The HTTP content to be sent</returns>
        internal static HttpContent GetMultiPartContent([CanBeNull] this IRestClient client, IRestRequest request)
        {
            var isPostMethod = client.GetEffectiveHttpMethod(request) == Method.POST;
            var multipartContent = new MultipartFormDataContent();
            var parameters = client.MergeParameters(request);
            foreach (var parameter in parameters)
            {
                var fileParameter = parameter as FileParameter;
                if (fileParameter != null)
                {
                    var file = fileParameter;
                    var data = new ByteArrayContent((byte[])file.Value);
                    data.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                    data.Headers.ContentLength = file.ContentLength;
                    multipartContent.Add(data, file.Name, file.FileName);
                }
                else if (isPostMethod && parameter.Type == ParameterType.GetOrPost)
                {
                    HttpContent data;
                    var bytes = parameter.Value as byte[];
                    if (bytes != null)
                    {
                        var rawData = bytes;
                        data = new ByteArrayContent(rawData);
                        data.Headers.ContentType = string.IsNullOrEmpty(parameter.ContentType)
                                                       ? new MediaTypeHeaderValue("application/octet-stream")
                                                       : MediaTypeHeaderValue.Parse(parameter.ContentType);
                        data.Headers.ContentLength = rawData.Length;
                        multipartContent.Add(data, parameter.Name);
                    }
                    else
                    {
                        var value = parameter.ToRequestString();
                        data = new StringContent(value, parameter.Encoding ?? ParameterExtensions.DefaultEncoding);
                        if (!string.IsNullOrEmpty(parameter.ContentType))
                            data.Headers.ContentType = MediaTypeHeaderValue.Parse(parameter.ContentType);
                        multipartContent.Add(data, parameter.Name);
                    }
                }
                else if (parameter.Type == ParameterType.RequestBody)
                {
                    var data = request.GetBodyContent(parameter);
                    multipartContent.Add(data, parameter.Name);
                }
            }

            return multipartContent;
        }
    }
}
