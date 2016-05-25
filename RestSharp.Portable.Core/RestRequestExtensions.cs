using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#if !NO_TYPEINFO
using System.Reflection;
#endif

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
        /// Automatically create parameters from object properties
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="obj">Object to serialize to the request body</param>
        /// <param name="objProperties">The object properties</param>
        /// <param name="filterMode">Include or exclude the properties?</param>
        /// <returns>The request object to allow call chains</returns>
        public static IRestRequest AddObject(this IRestRequest request, object obj, IEnumerable<string> objProperties, PropertyFilterMode filterMode)
        {
            var type = obj.GetType();

            var objectProperties = objProperties == null ? null : new HashSet<string>(objProperties);
            var addedProperties = new HashSet<string>();

            while (type != typeof(object))
            {
#if NO_TYPEINFO
                var typeInfo = type;
                var props = typeInfo.GetProperties();
#else
                var typeInfo = type.GetTypeInfo();
                var props = typeInfo.DeclaredProperties;
#endif

                foreach (var prop in props.Where(x => !addedProperties.Contains(x.Name)))
                {
                    bool isAllowed;

                    if (objectProperties == null)
                    {
                        isAllowed = true;
                    }
                    else
                    {
                        if (filterMode == PropertyFilterMode.Include)
                        {
                            isAllowed = objectProperties.Contains(prop.Name);
                        }
                        else
                        {
                            isAllowed = !objectProperties.Contains(prop.Name);
                        }
                    }

                    if (!isAllowed)
                    {
                        continue;
                    }

                    addedProperties.Add(prop.Name);

                    var propType = prop.PropertyType;
                    var val = prop.GetValue(obj, null);

                    if (val != null)
                    {
                        if (propType.IsArray)
                        {
                            var elementType = propType.GetElementType();
#if NO_TYPEINFO
                            var elementTypeInfo = elementType;
#else
                            var elementTypeInfo = elementType.GetTypeInfo();
#endif

                            if (((Array)val).Length > 0 &&
                                (elementTypeInfo.IsPrimitive || elementTypeInfo.IsValueType || elementType == typeof(string)))
                            {
                                // convert the array to an array of strings
                                var values = (from object item in (Array)val select item.ToString()).ToArray();
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

                type = typeInfo.BaseType;
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
            if (parameter.Type == ParameterType.UrlSegment || parameter.Type == ParameterType.RequestBody || parameter is FileParameter)
            {
                request.Parameters.AddOrUpdate(parameter);
            }
            else
            {
                request.Parameters.Add(parameter);
            }
            return request;
        }

        /// <summary>
        /// Generic add or update parameters function
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="parameter">Parameter to add</param>
        /// <returns>The request object to allow call chains</returns>
        public static IRestRequest AddOrUpdateParameter(this IRestRequest request, Parameter parameter)
        {
            request.Parameters.AddOrUpdate(parameter);
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
        /// Add or update a query parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddOrUpdateQueryParameter(this IRestRequest request, string name, object value)
        {
            return request.AddOrUpdateParameter(new Parameter { Name = name, Value = value, Type = ParameterType.QueryString });
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
            request.Parameters.AddOrUpdate(new Parameter { Name = name, Value = value, Type = ParameterType.UrlSegment });
            return request;
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
        /// Add or update a HTTP HEADER parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddOrUpdateHeader(this IRestRequest request, string name, object value)
        {
            return request.AddOrUpdateParameter(new Parameter { Name = name, Value = value, Type = ParameterType.HttpHeader });
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
        /// Add or updates a parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add or update this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddOrUpdateParameter(this IRestRequest request, string name, object value)
        {
            return request.AddOrUpdateParameter(new Parameter { Name = name, Value = value, Type = ParameterType.GetOrPost });
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
            {
                return request.AddParameter(null, value, ParameterType.RequestBody, name);
            }

            return request.AddParameter(new Parameter { Name = name, Value = value, Type = type });
        }

        /// <summary>
        /// Add or update a parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddOrUpdateParameter(this IRestRequest request, string name, object value, ParameterType type)
        {
            if (type == ParameterType.RequestBody && !string.IsNullOrEmpty(name))
            {
                return request.AddOrUpdateParameter(null, value, ParameterType.RequestBody, name);
            }

            return request.AddOrUpdateParameter(new Parameter { Name = name, Value = value, Type = type });
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
        public static IRestRequest AddParameter(this IRestRequest request, string name, object value, ParameterType type, string contentType)
        {
            return request.AddParameter(new Parameter { Name = name, Value = value, Type = type, ContentType = contentType });
        }

        /// <summary>
        /// Add or update a parameter to a REST request
        /// </summary>
        /// <param name="request">The REST request to add this parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <param name="contentType">Content type for the parameter (only applicable to a Body parameter)</param>
        /// <returns>The REST request to allow call chains</returns>
        public static IRestRequest AddOrUpdateParameter(this IRestRequest request, string name, object value, ParameterType type, string contentType)
        {
            return request.AddOrUpdateParameter(new Parameter { Name = name, Value = value, Type = type, ContentType = contentType });
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
            return request.AddOrUpdateParameter(FileParameter.Create(name, bytes, fileName));
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
        public static IRestRequest AddFile(this IRestRequest request, string name, byte[] bytes, string fileName, string contentType)
        {
            return request.AddOrUpdateParameter(FileParameter.Create(name, bytes, fileName, contentType));
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
            return request.AddOrUpdateParameter(FileParameter.Create(name, input, fileName));
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
        public static IRestRequest AddFile(this IRestRequest request, string name, Stream input, string fileName, string contentType)
        {
            return request.AddOrUpdateParameter(FileParameter.Create(name, input, fileName, contentType));
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
            return request.AddOrUpdateParameter(parameter);
        }
    }
}
