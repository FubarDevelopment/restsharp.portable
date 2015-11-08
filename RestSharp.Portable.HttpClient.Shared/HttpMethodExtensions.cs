using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace RestSharp.Portable.HttpClient
{
    /// <summary>
    /// Helper functions to convert to/from <see cref="HttpMethod"/>
    /// </summary>
    internal static class HttpMethodExtensions
    {
        private static readonly IDictionary<Method, HttpMethod> _methodsToHttpMethods = new Dictionary<Method, HttpMethod>
            {
                { Method.DELETE, HttpMethod.Delete },
                { Method.GET, HttpMethod.Get },
                { Method.HEAD, HttpMethod.Head },
                { Method.MERGE, new HttpMethod("MERGE") },
                { Method.OPTIONS, HttpMethod.Options },
                { Method.PATCH, new HttpMethod("PATCH") },
                { Method.POST, HttpMethod.Post },
                { Method.PUT, HttpMethod.Put },
            };

        private static readonly IDictionary<string, Method> _httpMethodsToMethods = _methodsToHttpMethods
            .ToDictionary(x => x.Value.Method, x => x.Key, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Converts a <see cref="Method"/> to a <see cref="HttpMethod"/>
        /// </summary>
        /// <param name="method">The <see cref="Method"/> to convert from</param>
        /// <returns>The converted <paramref name="method"/></returns>
        public static HttpMethod ToHttpMethod(this Method method)
        {
            HttpMethod result;
            if (_methodsToHttpMethods.TryGetValue(method, out result))
            {
                return result;
            }

            throw new NotSupportedException($"Unsupported HTTP method {method}");
        }

        /// <summary>
        /// Converts a <see cref="HttpMethod"/> to a <see cref="Method"/>
        /// </summary>
        /// <param name="method">The <see cref="HttpMethod"/> to convert from</param>
        /// <returns>The converted <paramref name="method"/></returns>
        public static Method ToMethod(this HttpMethod method)
        {
            Method result;
            if (_httpMethodsToMethods.TryGetValue(method.Method, out result))
            {
                return result;
            }

            throw new NotSupportedException($"Unsupported HTTP method {method.Method}");
        }
    }
}
