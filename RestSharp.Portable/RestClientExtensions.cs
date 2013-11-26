using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public static Uri BuildUrl(this IRestClient client, IRestRequest request, bool withQuery = true)
        {
            var resource = request.Resource;
            foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.UrlSegment))
            {
                var searchText = string.Format("{{{0}}}", param.Name);
                var replaceText = string.Format("{0}", param.Value);
                resource = resource.Replace(searchText, replaceText);
            }
            var urlBuilder = new UriBuilder(new Uri(client.BaseUrl, new Uri(resource, UriKind.RelativeOrAbsolute)));
            if (withQuery)
            {
                var queryString = new StringBuilder(urlBuilder.Query ?? string.Empty);
                var startsWithQuestionmark = queryString.ToString().StartsWith("?");
                foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.QueryString))
                {
                    if (queryString.Length > (startsWithQuestionmark ? 1 : 0))
                        queryString.Append("&");
                    queryString.AppendFormat("{0}={1}", Uri.EscapeUriString(param.Name), Uri.EscapeUriString(string.Format("{0}", param.Value)));
                }
                if (request.Method == HttpMethod.Get)
                {
                    var getOrPostParameters = request.GetGetOrPostParameters().ToList();
                    foreach (var param in getOrPostParameters)
                    {
                        if (queryString.Length > (startsWithQuestionmark ? 1 : 0))
                            queryString.Append("&");
                        queryString.AppendFormat("{0}={1}", Uri.EscapeUriString(param.Name), Uri.EscapeUriString(string.Format("{0}", param.Value)));
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
    }
}
