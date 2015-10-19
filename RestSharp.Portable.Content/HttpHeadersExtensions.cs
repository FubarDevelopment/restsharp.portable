using System;
using System.Collections.Generic;

namespace RestSharp.Portable.Content
{
    internal static class HttpHeadersExtensions
    {
        public static void ReplaceWithoutValidation(this IHttpHeaders headers, string key, string value)
        {
            if (headers.Contains(key))
                headers.Remove(key);
            headers.TryAddWithoutValidation(key, value);
        }

        public static void ReplaceWithoutValidation(this IHttpHeaders headers, string key, IEnumerable<string> values)
        {
            if (headers.Contains(key))
                headers.Remove(key);
            headers.TryAddWithoutValidation(key, values);
        }

        public static void Replace(this IHttpHeaders headers, string key, string value)
        {
            if (headers.Contains(key))
                headers.Remove(key);
            headers.Add(key, value);
        }

        public static void Replace(this IHttpHeaders headers, string key, IEnumerable<string> values)
        {
            if (headers.Contains(key))
                headers.Remove(key);
            headers.Add(key, values);
        }
    }
}
