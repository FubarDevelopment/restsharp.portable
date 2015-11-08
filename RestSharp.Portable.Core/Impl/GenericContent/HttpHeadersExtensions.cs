using System.Collections.Generic;

namespace RestSharp.Portable.Content
{
    /// <summary>
    /// Extension functions for easier HTTP header value handling
    /// </summary>
    internal static class HttpHeadersExtensions
    {
        /// <summary>
        /// Removes the header with the given <paramref name="key"/> and adds it again without validation.
        /// </summary>
        /// <param name="headers">The header to replace the key/value for</param>
        /// <param name="key">The HTTP header name</param>
        /// <param name="value">The HTTP header value</param>
        public static void ReplaceWithoutValidation(this IHttpHeaders headers, string key, string value)
        {
            if (headers.Contains(key))
            {
                headers.Remove(key);
            }

            headers.TryAddWithoutValidation(key, value);
        }

        /// <summary>
        /// Removes the header with the given <paramref name="key"/> and adds it again without validation.
        /// </summary>
        /// <param name="headers">The header to replace the key/value for</param>
        /// <param name="key">The HTTP header name</param>
        /// <param name="values">The HTTP header values</param>
        public static void ReplaceWithoutValidation(this IHttpHeaders headers, string key, IEnumerable<string> values)
        {
            if (headers.Contains(key))
            {
                headers.Remove(key);
            }

            headers.TryAddWithoutValidation(key, values);
        }

        /// <summary>
        /// Removes the header with the given <paramref name="key"/> and adds it again.
        /// </summary>
        /// <param name="headers">The header to replace the key/value for</param>
        /// <param name="key">The HTTP header name</param>
        /// <param name="value">The HTTP header value</param>
        public static void Replace(this IHttpHeaders headers, string key, string value)
        {
            if (headers.Contains(key))
            {
                headers.Remove(key);
            }

            headers.Add(key, value);
        }

        /// <summary>
        /// Removes the header with the given <paramref name="key"/> and adds it again.
        /// </summary>
        /// <param name="headers">The header to replace the key/value for</param>
        /// <param name="key">The HTTP header name</param>
        /// <param name="values">The HTTP header values</param>
        public static void Replace(this IHttpHeaders headers, string key, IEnumerable<string> values)
        {
            if (headers.Contains(key))
            {
                headers.Remove(key);
            }

            headers.Add(key, values);
        }
    }
}
