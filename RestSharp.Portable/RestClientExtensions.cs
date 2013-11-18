using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RestSharp.Portable
{
    public static class RestClientExtensions
    {
        public static IRestClient AddDefaultParameter(this IRestClient client, string name, object value)
        {
            return client.AddDefaultParameter(new Parameter { Name = name, Value = value, Type = ParameterType.GetOrPost });
        }

        public static IRestClient AddDefaultParameter(this IRestClient client, string name, object value, ParameterType type)
        {
            return client.AddDefaultParameter(new Parameter { Name = name, Value = value, Type = type });
        }

        public static IRestClient AddDefaultParameter(this IRestClient client, Parameter parameter)
        {
            if (parameter.Type == ParameterType.RequestBody)
                throw new NotSupportedException("Cannot set request body from default headers. Use Request.AddBody() instead.");
            client.DefaultParameters.Add(parameter);
            return client;
        }

        public static IRestClient RemoveDefaultParameter(this IRestClient client, string name)
        {
            var parameter = client.DefaultParameters.SingleOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (parameter != null)
                client.DefaultParameters.Remove(parameter);
            return client;
        }

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
                    foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.GetOrPost))
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
