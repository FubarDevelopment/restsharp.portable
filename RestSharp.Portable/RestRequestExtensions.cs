using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RestSharp.Portable
{
    /// <summary>
    /// Extension functions for REST requests
    /// </summary>
    public static class RestRequestExtensions
    {
        /// <summary>
        /// Body to add to the parameters using the <see cref="RestSharp.Portable.IRestRequest.Serializer" />
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="obj">Object to serialize to the request body</param>
        /// <returns>The request object to allow call chains</returns>
        public static IRestRequest AddBody(this IRestRequest request, object obj)
        {
            var serializer = request.Serializer;
            var data = serializer.Serialize(obj);
            return request.AddParameter(new Parameter { Value = data, Type = ParameterType.RequestBody, ContentType = serializer.ContentType });
        }

        /// <summary>
        /// Body to add to the parameters using a default <see cref="RestSharp.Portable.Serializers.JsonSerializer"/>.
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="obj">Object to serialize to the request body</param>
        /// <returns>The request object to allow call chains</returns>
        public static IRestRequest AddJsonBody(this IRestRequest request, object obj)
        {
            var serializer = Serializers.JsonSerializer.Default;
            var data = serializer.Serialize(obj);
            return request.AddParameter(new Parameter { Value = data, Type = ParameterType.RequestBody, ContentType = serializer.ContentType });
        }

        /// <summary>
        /// Body to add to the parameters using a default <see cref="RestSharp.Portable.Serializers.XmlDataContractSerializer"/>.
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="obj">Object to serialize to the request body</param>
        /// <returns>The request object to allow call chains</returns>
        public static IRestRequest AddXmlBody(this IRestRequest request, object obj)
        {
            var serializer = Serializers.XmlDataContractSerializer.Default;
            var data = serializer.Serialize(obj);
            return request.AddParameter(new Parameter { Value = data, Type = ParameterType.RequestBody, ContentType = serializer.ContentType });
        }

        /// <summary>
        /// Port of AddObject to RestSharp.Portable
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="obj">Object to serialize to the request body</param>
        /// <param name="includedProperties">Properties to include</param>
        /// <returns>The request object to allow call chains</returns>
        public static IRestRequest AddObject(this IRestRequest request, object obj, params string[] includedProperties)
        {
            return AddObject(request, obj, (includedProperties == null || includedProperties.Length == 0) ? null : includedProperties, PropertyFilterMode.Include);
        }

        /// <summary>
        /// Port of AddObject to RestSharp.Portable
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="obj">Object to serialize to the request body</param>
        /// <param name="objProperties">The object properties</param>
        /// <param name="filterMode">Include or exclude the properties?</param>
        /// <returns>The request object to allow call chains</returns>
        public static IRestRequest AddObject(this IRestRequest request, object obj, IEnumerable<string> objProperties, PropertyFilterMode filterMode)
        {
            // automatically create parameters from object props
            var type = obj.GetType();
            var props = type.GetProperties();

            var objProps = objProperties == null ? null : objProperties.Where(x => x != null).ToList();

            foreach (var prop in props)
            {
                bool isAllowed;

                if (objProps == null)
                {
                    isAllowed = true;
                }
                else
                {
                    if (filterMode == PropertyFilterMode.Include)
                    {
                        isAllowed = objProps.Contains(prop.Name);
                    }
                    else
                    {
                        isAllowed = !objProps.Contains(prop.Name);
                    }
                }

                if (!isAllowed)
                    continue;

                var propType = prop.PropertyType;
                var val = prop.GetValue(obj, null);

                if (val != null)
                {
                    if (propType.IsArray)
                    {
                        var elementType = propType.GetElementType();

                        if (((Array)val).Length > 0 &&
                            (elementType.IsPrimitive || elementType.IsValueType || elementType == typeof(string)))
                        {
                            // convert the array to an array of strings
                            var values =
                                (from object item in ((Array)val) select item.ToString()).ToArray<string>();
                            val = string.Join(",", values);
                        }
                        else
                        {
                            // try to cast it
                            val = string.Join(",", (string[])val);
                        }
                    }

                    request.AddParameter(prop.Name, val);
                }
            }

            return request;
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
        /// Add a query parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddQueryParameter(this IRestRequest request, string name, object value)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = ParameterType.QueryString });
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
            if (type == ParameterType.RequestBody && !string.IsNullOrEmpty(name))
                return request.AddParameter(null, value, ParameterType.RequestBody, new MediaTypeHeaderValue(name));
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
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Communicate to the caller that a file parameter is required.")]
        public static IRestRequest AddFile(this IRestRequest request, FileParameter parameter)
        {
            return request.AddParameter(parameter);
        }

        /// <summary>
        /// Returns the HttpContent for the body parameter
        /// </summary>
        /// <param name="request">The request the body parameter belongs to</param>
        /// <param name="body">The body parameter</param>
        /// <returns>The resulting HttpContent</returns>
        internal static HttpContent GetBodyContent(this IRestRequest request, Parameter body)
        {
            if (body == null)
                return null;

            MediaTypeHeaderValue contentType;
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
            content.Headers.ContentType = contentType;
            content.Headers.ContentLength = buffer.Length;
            return content;
        }
    }
}
