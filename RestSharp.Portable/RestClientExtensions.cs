using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

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
                throw new NotSupportedException("Cannot set request body from default headers. Use Request.AddBody() instead.");
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
                client.DefaultParameters.Remove(parameter);
            return client;
        }

        /// <summary>
        /// Merge parameters from client and request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">The REST request</param>
        /// <returns>A list of merged parameters</returns>
        public static IList<Parameter> MergeParameters(this IRestClient client, IRestRequest request)
        {
            var parameters = new List<Parameter>();

            // Add default parameters first
            if (client != null)
            {
                parameters.AddRange(client.DefaultParameters);
            }

            // Now the client parameters
            if (request != null)
            {
                parameters.AddRange(request.Parameters);
            }

            var comparer = new ParameterComparer(client, request);

            var result = parameters
                .Select((p, i) => new { Parameter = p, Index = i })
                // Group by parameter type/name
                .GroupBy(x => x.Parameter, comparer)
                // Select only the last of all duplicate parameters
                .Select(x => new { Parameter = x.Last().Parameter, Index = x.First().Index })
                // Sort by appearance
                .OrderBy(x => x.Index)
                .Select(x => x.Parameter)
                .ToList();

            return result;
        }

        private static string ReplaceUrlSegments(string url, IEnumerable<Parameter> parameters)
        {
            foreach (var param in parameters.Where(x => x.Type == ParameterType.UrlSegment))
            {
                var searchText = string.Format("{{{0}}}", param.Name);
                var replaceText = param.ToEncodedString();
                url = url.Replace(searchText, replaceText);
            }
            return url;
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
        [Obsolete("Use BuildUri instead (to be compatible with the original RestSharp)")]
        public static Uri BuildUrl(this IRestClient client, IRestRequest request, bool withQuery = true)
        {
            return BuildUri(client, request, withQuery);
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
        public static Uri BuildUri(this IRestClient client, IRestRequest request, bool withQuery = true)
        {
            var parameters = client.MergeParameters(request);
            UriBuilder urlBuilder;
            if (client.BaseUrl == null)
            {
                var resource = ReplaceUrlSegments(request.Resource, parameters);
                urlBuilder = new UriBuilder(resource);
            }
            else if (request == null || string.IsNullOrEmpty(request.Resource))
            {
                var baseUrl = ReplaceUrlSegments(client.BaseUrl.OriginalString, parameters);
                urlBuilder = new UriBuilder(baseUrl);
            }
            else
            {
                var baseUrl = ReplaceUrlSegments(client.BaseUrl.OriginalString, parameters);
                var resource = ReplaceUrlSegments(request.Resource, parameters);
                if (string.IsNullOrEmpty(resource))
                {
                    urlBuilder = new UriBuilder(baseUrl);
                }
                else if (string.IsNullOrEmpty(baseUrl))
                {
                    urlBuilder = new UriBuilder(resource);
                }
                else
                {
                    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
                        baseUrl += "/";
                    urlBuilder = new UriBuilder(new Uri(new Uri(baseUrl), new Uri(resource, UriKind.RelativeOrAbsolute)));
                }
            }
            if (withQuery)
            {
                var queryString = new StringBuilder(urlBuilder.Query ?? string.Empty);
                var startsWithQuestionmark = queryString.ToString().StartsWith("?");
                foreach (var param in parameters.Where(x => x.Type == ParameterType.QueryString))
                {
                    if (queryString.Length > (startsWithQuestionmark ? 1 : 0))
                        queryString.Append("&");
                    queryString.AppendFormat("{0}={1}", Uri.EscapeDataString(param.Name), param.ToEncodedString());
                }
                if (client.GetEffectiveHttpMethod(request) == HttpMethod.Get)
                {
                    var getOrPostParameters = parameters.GetGetOrPostParameters().ToList();
                    foreach (var param in getOrPostParameters)
                    {
                        if (queryString.Length > (startsWithQuestionmark ? 1 : 0))
                            queryString.Append("&");
                        queryString.AppendFormat("{0}={1}", Uri.EscapeDataString(param.Name), param.ToEncodedString());
                    }
                }
                urlBuilder.Query = queryString.ToString();
            }
            else
            {
                urlBuilder.Query = string.Empty;
            }
            return urlBuilder.Uri;
        }

        /// <summary>
        /// Gets the basic content (without files) for a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">REST request to get the content for</param>
        /// <returns>The HTTP content to be sent</returns>
        internal static HttpContent GetBasicContent(this IRestClient client, IRestRequest request)
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
                var getOrPostParameters = parameters.GetGetOrPostParameters().ToList();
                if (client.GetEffectiveHttpMethod(request) == HttpMethod.Post && getOrPostParameters.Count != 0)
                {
                    var hasEncoding = getOrPostParameters.Any(x => x.Encoding != null && x.Encoding != ParameterExtensions.DefaultEncoding);
                    if (hasEncoding)
                    {
                        var postData = string.Join("&", getOrPostParameters
                            .Select(x => string.Format("{0}={1}", Uri.EscapeDataString(x.Name), x.ToEncodedString())));
                        var bytes = ParameterExtensions.DefaultEncoding.GetBytes(postData);
                        content = new ByteArrayContent(bytes);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    }
                    else
                    {
                        var postData = getOrPostParameters
                            .Select(x => new KeyValuePair<string, string>(x.Name, string.Format("{0}", x.Value)))
                            .ToList();
                        content = new FormUrlEncodedContent(postData);
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
        /// Gets the content for a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">REST request to get the content for</param>
        /// <returns>The HTTP content to be sent</returns>
        public static HttpContent GetContent(this IRestClient client, IRestRequest request)
        {
            HttpContent content;
            var parameters = client.MergeParameters(request);
            var collectionMode = request.ContentCollectionMode;
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
            return content;
        }

        /// <summary>
        /// Gets the multi-part content (with files) for a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">REST request to get the content for</param>
        /// <returns>The HTTP content to be sent</returns>
        internal static HttpContent GetMultiPartContent(this IRestClient client, IRestRequest request)
        {
            var isPostMethod = client.GetEffectiveHttpMethod(request) == HttpMethod.Post;
            var multipartContent = new MultipartFormDataContent();
            var parameters = client.MergeParameters(request);
            foreach (var parameter in parameters)
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
                        data = new StringContent(value, parameter.Encoding ?? ParameterExtensions.DefaultEncoding);
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
        /// Returns the HTTP method GET or POST - depending on the parameters
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">The request to determine the HTTP method for</param>
        /// <returns>GET or POST</returns>
        internal static HttpMethod GetDefaultMethod(this IRestClient client, IRestRequest request)
        {
            var parameters = (client == null ? new List<Parameter>() : client.DefaultParameters)
                .Union(request == null ? new List<Parameter>() : request.Parameters);
            if (parameters.Any(x => x.Type == ParameterType.RequestBody || (x is FileParameter)))
                return HttpMethod.Post;
            return HttpMethod.Get;
        }

        /// <summary>
        /// Returns the real HTTP method that must be used to execute a request
        /// </summary>
        /// <param name="client">The REST client that will execute the request</param>
        /// <param name="request">The request to determine the HTTP method for</param>
        /// <returns>The real HTTP method that must be used</returns>
        public static HttpMethod GetEffectiveHttpMethod(this IRestClient client, IRestRequest request)
        {
            if (request.Method == null || request.Method == HttpMethod.Get)
                return client.GetDefaultMethod(request);
            return request.Method;
        }
    }
}
