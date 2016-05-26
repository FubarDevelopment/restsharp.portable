using System.Collections.Generic;
using System.Linq;

namespace RestSharp.Portable
{
    /// <summary>
    /// Extension methods for <see cref="IHttpHeaders"/>.
    /// </summary>
    public static class HttpHeadersExtensions
    {
        /// <summary>
        /// Gets the HTTP header value or null
        /// </summary>
        /// <param name="headers">The headers to get the value from</param>
        /// <param name="name">The header name to get the value for</param>
        /// <returns>The first HTTP header value or null</returns>
        public static string GetValue(this IHttpHeaders headers, string name)
        {
            return GetValue(headers, name, null);
        }

        /// <summary>
        /// Gets the HTTP header value or the given <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="headers">The headers to get the value from</param>
        /// <param name="name">The header name to get the value for</param>
        /// <param name="defaultValue">The default value when the header value couldn't be found in the headers.</param>
        /// <returns>The first HTTP header value or the given <paramref name="defaultValue"/>.</returns>
        public static string GetValue(this IHttpHeaders headers, string name, string defaultValue)
        {
            IEnumerable<string> values;
            if (!headers.TryGetValues(name, out values))
            {
                return defaultValue;
            }

            return values.FirstOrDefault() ?? defaultValue;
        }
    }
}
