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
            else
            {
                var getOrPostParameters = request.GetGetOrPostParameters().ToList();
                if (request.Method == HttpMethod.Post && getOrPostParameters.Count != 0)
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
        /// Gets the content for a request
        /// </summary>
        /// <param name="request">REST request to get the content for</param>
        /// <param name="withFiles">true = Creates a multipart form data content containing the file parameters (if any)</param>
        /// <returns>The HTTP content to be sent</returns>
        public static HttpContent GetContent(this IRestRequest request, bool withFiles = true)
        {
            var content = request.GetBasicContent();
            if (withFiles)
            {
                var fileParameters = request.GetFileParameters().ToList();
                if (fileParameters.Count != 0)
                {
                    var fileContent = new MultipartFormDataContent();
                    if (content != null)
                        fileContent.Add(content);
                    foreach (var file in fileParameters)
                    {
                        var data = new ByteArrayContent((byte[])file.Value);
                        data.Headers.ContentType = file.ContentType;
                        data.Headers.ContentLength = file.ContentLength;
                        fileContent.Add(data, file.Name, file.FileName);
                    }
                    content = fileContent;
                }
            }
            return content;
        }
    }
}
