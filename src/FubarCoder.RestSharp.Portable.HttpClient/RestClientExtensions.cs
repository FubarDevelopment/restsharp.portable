using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using JetBrains.Annotations;

using RestSharp.Portable.HttpClient.Impl.Http;

namespace RestSharp.Portable.HttpClient
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
        /// <param name="parameters">The request parameters for the REST request (read-only)</param>
        /// <returns>The HTTP content to be sent</returns>
        internal static IHttpContent GetContent([CanBeNull] this IRestClient client, IRestRequest request, RequestParameters parameters)
        {
            HttpContent content;
            var collectionMode = request?.ContentCollectionMode ?? ContentCollectionMode.MultiPartForFileParameters;
            if (collectionMode != ContentCollectionMode.BasicContent)
            {
                var fileParameters = parameters.OtherParameters.GetFileParameters().ToList();
                if (collectionMode == ContentCollectionMode.MultiPart || fileParameters.Count != 0)
                {
                    content = client.GetMultiPartContent(request, parameters);
                }
                else
                {
                    content = client.GetBasicContent(request, parameters);
                }
            }
            else
            {
                content = client.GetBasicContent(request, parameters);
            }

            if (content == null)
            {
                return null;
            }

            foreach (var param in parameters.ContentHeaderParameters)
            {
                if (content.Headers.Contains(param.Name))
                {
                    content.Headers.Remove(param.Name);
                }

                if (param.ValidateOnAdd)
                {
                    content.Headers.Add(param.Name, param.ToRequestString());
                }
                else
                {
                    content.Headers.TryAddWithoutValidation(param.Name, param.ToRequestString());
                }
            }

            return new DefaultHttpContent(content);
        }

        /// <summary>
        /// Gets the basic content (without files) for a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">REST request to get the content for</param>
        /// <param name="parameters">The request parameters for the REST request (read-only)</param>
        /// <returns>The HTTP content to be sent</returns>
        private static HttpContent GetBasicContent([CanBeNull] this IRestClient client, IRestRequest request, RequestParameters parameters)
        {
            HttpContent content;
            var body = parameters.OtherParameters.FirstOrDefault(x => x.Type == ParameterType.RequestBody);
            if (body != null)
            {
                content = request.GetBodyContent(body);
            }
            else
            {
                var effectiveMethod = client.GetEffectiveHttpMethod(request);
                if (effectiveMethod != Method.GET)
                {
                    var getOrPostParameters = parameters.OtherParameters.GetGetOrPostParameters().ToList();
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
        /// <param name="parameters">The request parameters for the REST request (read-only)</param>
        /// <returns>The HTTP content to be sent</returns>
        private static HttpContent GetMultiPartContent([CanBeNull] this IRestClient client, IRestRequest request, RequestParameters parameters)
        {
            var isPostMethod = client.GetEffectiveHttpMethod(request) == Method.POST;
            var multipartContent = new MultipartFormDataContent();
            foreach (var parameter in parameters.OtherParameters)
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
                        {
                            data.Headers.ContentType = MediaTypeHeaderValue.Parse(parameter.ContentType);
                        }

                        multipartContent.Add(data, parameter.Name);
                    }
                }
                else if (parameter.Type == ParameterType.RequestBody)
                {
                    var data = request.GetBodyContent(parameter);
                    var parameterName = parameter.Name ?? data.Headers.ContentType.MediaType;
                    if (string.IsNullOrEmpty(parameterName))
                        throw new InvalidOperationException("You must specify a name for a body parameter.");
                    multipartContent.Add(data, parameterName);
                }
            }

            return multipartContent;
        }
    }
}
