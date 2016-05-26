using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET40ALIKE
using System.Reflection;
#endif

using JetBrains.Annotations;

namespace RestSharp.Portable
{
    /// <summary>
    /// Extension functions for REST clients
    /// </summary>
    public static class RestClientExtensions
    {
        /// <summary>
        /// Add a default parameter to a REST client
        /// </summary>
        /// <param name="client">REST client to add the new parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>The REST client to allow call chains</returns>
        public static IRestClient AddDefaultParameter(this IRestClient client, string name, object value)
        {
            return client.AddDefaultParameter(new Parameter { Name = name, Value = value, Type = ParameterType.GetOrPost });
        }

        /// <summary>
        /// Add a default parameter to a REST client
        /// </summary>
        /// <param name="client">REST client to add the new parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <returns>The REST client to allow call chains</returns>
        public static IRestClient AddDefaultParameter(this IRestClient client, string name, object value, ParameterType type)
        {
            return client.AddDefaultParameter(new Parameter { Name = name, Value = value, Type = type });
        }

        /// <summary>
        /// Add a default parameter to a REST client
        /// </summary>
        /// <param name="client">REST client to add the new parameter to</param>
        /// <param name="parameter">The parameter to add</param>
        /// <returns>The REST client to allow call chains</returns>
        public static IRestClient AddDefaultParameter(this IRestClient client, Parameter parameter)
        {
            if (parameter.Type == ParameterType.RequestBody)
            {
                throw new NotSupportedException("Cannot set request body from default headers. Use Request.AddBody() instead.");
            }

            client.DefaultParameters.Add(parameter);
            return client;
        }

        /// <summary>
        /// Remove a default parameter from the REST client
        /// </summary>
        /// <param name="client">REST client to remove the parameter from</param>
        /// <param name="name">Name of the parameter</param>
        /// <returns>The REST client to allow call chains</returns>
        public static IRestClient RemoveDefaultParameter(this IRestClient client, string name)
        {
            var parameter = client.DefaultParameters.SingleOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (parameter != null)
            {
                client.DefaultParameters.Remove(parameter);
            }

            return client;
        }
        
        /// <summary>
        /// Build the full URL for a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">The REST request</param>
        /// <returns>Resulting URL</returns>
        /// <remarks>
        /// The resulting URL is a combination of the REST client's BaseUrl and the REST requests
        /// Resource, where all URL segments are replaced and - optionally - the query parameters
        /// added.
        /// </remarks>
        public static Uri BuildUri([CanBeNull] this IRestClient client, IRestRequest request)
        {
            return BuildUri(client, request, true);
        }

        /// <summary>
        /// Build the full URL for a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">The REST request</param>
        /// <param name="withQuery">Should the resulting URL contain the query?</param>
        /// <returns>Resulting URL</returns>
        /// <remarks>
        /// The resulting URL is a combination of the REST client's BaseUrl and the REST requests
        /// Resource, where all URL segments are replaced and - optionally - the query parameters
        /// added.
        /// </remarks>
        [NotNull]
        public static Uri BuildUri([CanBeNull] this IRestClient client, IRestRequest request, bool withQuery)
        {
            var parameters = client.MergeParameters(request).OtherParameters;
            UriBuilder urlBuilder;
            if (client?.BaseUrl == null)
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                if (string.IsNullOrEmpty(request.Resource))
                {
                    throw new ArgumentOutOfRangeException(nameof(request), "The resource must be specified and not be empty");
                }

                var resource = ReplaceUrlSegments(request.Resource, parameters);
                urlBuilder = new UriBuilder(new Uri(resource, UriKind.RelativeOrAbsolute));
            }
            else if (string.IsNullOrEmpty(request?.Resource))
            {
                if (client.BaseUrl == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(client), "The BaseUrl must be specified");
                }

                var baseUrl = ReplaceUrlSegments(client.BaseUrl.OriginalString, parameters);
                urlBuilder = new UriBuilder(new Uri(baseUrl, UriKind.RelativeOrAbsolute));
            }
            else
            {
                var baseUrl = ReplaceUrlSegments(client.BaseUrl.OriginalString, parameters);
                var resource = ReplaceUrlSegments(request.Resource, parameters);
                if (string.IsNullOrEmpty(resource))
                {
                    urlBuilder = new UriBuilder(new Uri(baseUrl, UriKind.RelativeOrAbsolute));
                }
                else if (string.IsNullOrEmpty(baseUrl))
                {
                    urlBuilder = new UriBuilder(new Uri(resource, UriKind.RelativeOrAbsolute));
                }
                else
                {
                    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
                    {
                        baseUrl += "/";
                    }

                    urlBuilder = new UriBuilder(new Uri(new Uri(baseUrl), new Uri(resource, UriKind.RelativeOrAbsolute)));
                }
            }

            if (withQuery)
            {
                var queryString = new StringBuilder(urlBuilder.Query);
                var startsWithQuestionmark = queryString.ToString().StartsWith("?");
                var effectiveMethod = client.GetEffectiveHttpMethod(request, parameters);
                var queryParams = effectiveMethod != Method.POST
                    ? parameters.Where(x => x.Type == ParameterType.QueryString || x.Type == ParameterType.GetOrPost)
                    : parameters.Where(x => x.Type == ParameterType.QueryString);
                foreach (var param in queryParams)
                {
                    if (queryString.Length > (startsWithQuestionmark ? 1 : 0))
                    {
                        queryString.Append("&");
                    }

                    queryString.AppendFormat("{0}={1}", UrlUtility.Escape(param.Name), param.ToEncodedString());
                }

                urlBuilder.Query = queryString.ToString().Substring(startsWithQuestionmark ? 1 : 0);
            }
            else
            {
                urlBuilder.Query = string.Empty;
            }

            return urlBuilder.Uri;
        }

        /// <summary>
        /// Returns the real HTTP method that must be used to execute a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">The request to determine the HTTP method for</param>
        /// <returns>The real HTTP method that must be used</returns>
        public static Method GetEffectiveHttpMethod([CanBeNull] this IRestClient client, IRestRequest request)
        {
            if (request == null || request.Method == Method.GET)
            {
                var parameters = client.MergeParameters(request).OtherParameters;
                return client.GetDefaultMethod(request, parameters);
            }

            return request.Method;
        }

        /// <summary>
        /// Returns the real HTTP method that must be used to execute a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">The request to determine the HTTP method for</param>
        /// <param name="parameters">The request parameters for the REST request except the content header parameters (read-only)</param>
        /// <returns>The real HTTP method that must be used</returns>
        public static Method GetEffectiveHttpMethod([CanBeNull] this IRestClient client, IRestRequest request, IList<Parameter> parameters)
        {
            if (request == null || request.Method == Method.GET)
            {
                return client.GetDefaultMethod(request, parameters);
            }

            return request.Method;
        }

        /// <summary>
        /// Replace all handlers of a given type with a new deserializer
        /// </summary>
        /// <param name="client">The REST client</param>
        /// <param name="oldType">The type of the old deserializer</param>
        /// <param name="deserializer">The new deserializer</param>
        /// <returns>The client itself, to allow call chains</returns>
        public static IRestClient ReplaceHandler(this IRestClient client, Type oldType, IDeserializer deserializer)
        {
#if NET40ALIKE
            var contentHandlersToReplace = client.ContentHandlers.Where(x => x.Value.GetType().IsAssignableFrom(oldType)).ToList();
#else
            var contentHandlersToReplace = client.ContentHandlers.Where(x => x.Value.GetType().GetTypeInfo().IsAssignableFrom(oldType.GetTypeInfo())).ToList();
#endif
            foreach (var contentHandlerToReplace in contentHandlersToReplace)
            {
                client.ContentHandlers[contentHandlerToReplace.Key] = deserializer;
            }

            return client;
        }

        /// <summary>
        /// Merge parameters from client and request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">The REST request</param>
        /// <returns>A <see cref="RequestParameters"/> object that contains the merged parameters</returns>
        public static RequestParameters MergeParameters(this IRestClient client, IRestRequest request)
        {
            return new RequestParameters(client, request);
        }

        /// <summary>
        /// Returns the HTTP method GET or POST - depending on the parameters
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">The request to determine the HTTP method for</param>
        /// <param name="parameters">The request parameters for the REST request except the content header parameters (read-only)</param>
        /// <returns>GET or POST</returns>
        internal static Method GetDefaultMethod([CanBeNull] this IRestClient client, IRestRequest request, IList<Parameter> parameters)
        {
            if (parameters.Any(x => x.Type == ParameterType.RequestBody || (x is FileParameter)))
            {
                return Method.POST;
            }

            return Method.GET;
        }

        private static string ReplaceUrlSegments([NotNull] string url, [NotNull] IEnumerable<Parameter> parameters)
        {
            foreach (var param in parameters.Where(x => x.Type == ParameterType.UrlSegment))
            {
                var searchText = $"{{{param.Name}}}";
                var replaceText = param.ToEncodedString();
                url = url.Replace(searchText, replaceText);
            }

            return url;
        }
    }
}
