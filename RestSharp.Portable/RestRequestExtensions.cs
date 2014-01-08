using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace RestSharp.Portable
{
    /// <summary>
    /// Extension functions for REST requests
    /// </summary>
    public static class RestRequestExtensions
    {
        /// <summary>
        /// Body to add to the parameters
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="obj">Object to serialize to the request body</param>
        /// <returns>The request object to allow call chains</returns>
        public static IRestRequest AddBody(this IRestRequest request, object obj)
        {
            var data = request.Serializer.Serialize(obj);
            return request.AddParameter(new Parameter { Value = data, Type = ParameterType.RequestBody, ContentType = request.Serializer.ContentType });
        }

        /// <summary>
        /// Generic add parameters function
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="parameter">Parameter to add</param>
        /// <returns>The request object to allow call chains</returns>
        public static IRestRequest AddParameter(this IRestRequest request, Parameter parameter)
        {
            request.Parameters.Add(parameter);
            return request;
        }

        /// <summary>
        /// Add an URL segment parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddUrlSegment(this IRestRequest request, string name, object value)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = ParameterType.UrlSegment });
        }

        /// <summary>
        /// Add a HTTP HEADER parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddHeader(this IRestRequest request, string name, object value)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = ParameterType.HttpHeader });
        }

        /// <summary>
        /// Add a parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddParameter(this IRestRequest request, string name, object value)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = ParameterType.GetOrPost });
        }

        /// <summary>
        /// Add a parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddParameter(this IRestRequest request, string name, object value, ParameterType type)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = type });
        }

        /// <summary>
        /// Add a parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <param name="contentType">Content type for the parameter (only applicable to a Body parameter)</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddParameter(this IRestRequest request, string name, object value, ParameterType type, MediaTypeHeaderValue contentType)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = type, ContentType = contentType });
        }

        /// <summary>
        /// Add a file parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="bytes">File content</param>
        /// <param name="fileName">File name</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddFile(this IRestRequest request, string name, byte[] bytes, string fileName)
        {
            return request.AddParameter(FileParameter.Create(name, bytes, fileName));
        }

        /// <summary>
        /// Add a file parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="bytes">File content</param>
        /// <param name="fileName">File name</param>
        /// <param name="contentType">Content type for the parameter (only applicable to a Body parameter)</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddFile(this IRestRequest request, string name, byte[] bytes, string fileName, MediaTypeHeaderValue contentType)
        {
            return request.AddParameter(FileParameter.Create(name, bytes, fileName, contentType));
        }

        /// <summary>
        /// Add a file parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="input">File content</param>
        /// <param name="fileName">File name</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddFile(this IRestRequest request, string name, Stream input, string fileName)
        {
            return request.AddParameter(FileParameter.Create(name, input, fileName));
        }

        /// <summary>
        /// Add a file parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="input">File content</param>
        /// <param name="fileName">File name</param>
        /// <param name="contentType">Content type for the parameter (only applicable to a Body parameter)</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddFile(this IRestRequest request, string name, Stream input, string fileName, MediaTypeHeaderValue contentType)
        {
            return request.AddParameter(FileParameter.Create(name, input, fileName, contentType));
        }

        /// <summary>
        /// Add a file parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="parameter">The new file parameter</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddFile(this IRestRequest request, FileParameter parameter)
        {
            return request.AddParameter(parameter);
        }

        /// <summary>
        /// Get the GetOrPost parameters (by default without file parameters, which are POST-only)
        /// </summary>
        /// <param name="request">The request to get the parameters from</param>
        /// <param name="withFile">true == with file parameters, but those are POST-only!</param>
        /// <returns>The list of GET or POST parameters</returns>
        public static IEnumerable<Parameter> GetGetOrPostParameters(this IRestRequest request, bool withFile = false)
        {
            return request.Parameters.Where(x => x.Type == ParameterType.GetOrPost && (withFile || !(x is FileParameter)));
        }

        /// <summary>
        /// Get the file parameters
        /// </summary>
        /// <param name="request">The request to get the parameters from</param>
        /// <returns>The list of POST file parameters</returns>
        public static IEnumerable<FileParameter> GetFileParameters(this IRestRequest request)
        {
            return request.Parameters.OfType<FileParameter>();
        }

        /// <summary>
        /// Returns the HTTP method GET or POST - depending on the parameters
        /// </summary>
        /// <param name="request">The request to determine the HTTP method for</param>
        /// <returns>GET or POST</returns>
        internal static HttpMethod GetDefaultMethod(this IRestRequest request)
        {
            if (request.GetFileParameters().Any() || request.Parameters.Any(x => x.Type == ParameterType.RequestBody))
                return HttpMethod.Post;
            return HttpMethod.Get;
        }

        /// <summary>
        /// Returns the real HTTP method that must be used
        /// </summary>
        /// <param name="request">The request to determine the HTTP method for</param>
        /// <returns>The real HTTP method that must be used</returns>
        internal static HttpMethod GetHttpMethod(this IRestRequest request)
        {
            if (request.Method == null || request.Method == HttpMethod.Get)
                return GetDefaultMethod(request);
            return request.Method;
        }

        /// <summary>
        /// Returns the HttpContent for the body parameter
        /// </summary>
        /// <param name="request">The request the body parameter belongs to</param>
        /// <param name="body">The body parameter</param>
        /// <returns>The resulting HttpContent</returns>
        private static HttpContent GetBodyContent(this IRestRequest request, Parameter body)
        {
            if (body == null)
                return null;

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
            var content = new ByteArrayContent(buffer);
            content.Headers.ContentType = contentType;
            content.Headers.ContentLength = buffer.Length;
            return content;
        }

        /// <summary>
        /// Gets the basic content (without files) for a request
        /// </summary>
        /// <param name="request">REST request to get the content for</param>
        /// <returns>The HTTP content to be sent</returns>
        private static HttpContent GetBasicContent(this IRestRequest request)
        {
            HttpContent content;
            var body = request.Parameters.FirstOrDefault(x => x.Type == ParameterType.RequestBody);
            if (body != null)
            {
                content = request.GetBodyContent(body);
            }
            else
            {
                var getOrPostParameters = request.GetGetOrPostParameters().ToList();
                if (request.GetHttpMethod() == HttpMethod.Post && getOrPostParameters.Count != 0)
                {
                    var postData = getOrPostParameters
                        .Select(x => new KeyValuePair<string, string>(x.Name, string.Format("{0}", x.Value)))
                        .ToList();
                    content = new FormUrlEncodedContent(postData);
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
        /// <param name="request">REST request to get the content for</param>
        /// <returns>The HTTP content to be sent</returns>
        private static HttpContent GetMultiPartContent(this IRestRequest request)
        {
            var isPostMethod = request.GetHttpMethod() == HttpMethod.Post;
            var multipartContent = new MultipartFormDataContent();
            foreach (var parameter in request.Parameters)
            {
                if (parameter is FileParameter)
                {
                    var file = (FileParameter)parameter;
                    var data = new ByteArrayContent((byte[])file.Value);
                    data.Headers.ContentType = file.ContentType;
                    data.Headers.ContentLength = file.ContentLength;
                    multipartContent.Add(data, file.Name, file.FileName);
                }
                else if (isPostMethod && parameter.Type == ParameterType.GetOrPost)
                {
                    HttpContent data;
                    if (parameter.Value is byte[])
                    {
                        var rawData = (byte[])parameter.Value;
                        data = new ByteArrayContent(rawData);
                        data.Headers.ContentType = parameter.ContentType ?? new MediaTypeHeaderValue("application/octet-stream");
                        data.Headers.ContentLength = rawData.Length;
                        multipartContent.Add(data, parameter.Name);
                    }
                    else
                    {
                        var value = string.Format("{0}", parameter.Value);
                        data = new StringContent(value, Encoding.UTF8);
                        if (parameter.ContentType != null)
                            data.Headers.ContentType = parameter.ContentType;
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

        /// <summary>
        /// Gets the content for a request
        /// </summary>
        /// <param name="request">REST request to get the content for</param>
        /// <returns>The HTTP content to be sent</returns>
        public static HttpContent GetContent(this IRestRequest request)
        {
            HttpContent content;
            var collectionMode = request.ContentCollectionMode;
            if (collectionMode != ContentCollectionMode.BasicContent)
            {
                var fileParameters = request.GetFileParameters().ToList();
                if (collectionMode == ContentCollectionMode.MultiPart || fileParameters.Count != 0)
                {
                    content = request.GetMultiPartContent();
                }
                else
                {
                    content = request.GetBasicContent();
                }
            }
            else
            {
                content = request.GetBasicContent();
            }
            return content;
        }
    }
}
