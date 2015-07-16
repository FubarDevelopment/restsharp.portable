using System;
using System.Net.Http;
using System.Net.Http.Headers;

using JetBrains.Annotations;

using RestSharp.Portable.HttpClient.Impl.Http;

namespace RestSharp.Portable.HttpClient
{
    internal static class DefaultHttpExtensions
    {
        public static IHttpHeaders AsRestHeaders([CanBeNull] this HttpHeaders headers)
        {
            if (headers == null)
                return null;
            return new DefaultHttpHeaders(headers);
        }

        public static HttpHeaders AsHttpHeaders([CanBeNull] this IHttpHeaders headers)
        {
            if (headers == null)
                return null;
            var defaultHeaders = headers as DefaultHttpHeaders;
            if (defaultHeaders != null)
                return defaultHeaders.Headers;
            throw new InvalidOperationException();
        }

        public static IHttpContent AsRestHttpContent([CanBeNull] this HttpContent content)
        {
            if (content == null)
                return null;
            var wrapper = content as HttpContentWrapper;
            if (wrapper == null)
                return new DefaultHttpContent(content);
            return wrapper.Content;
        }

        public static HttpContent AsHttpContent([CanBeNull] this IHttpContent content)
        {
            if (content == null)
                return null;
            var defaultHttpContent = content as DefaultHttpContent;
            if (defaultHttpContent != null)
                return defaultHttpContent.Content;
            return new HttpContentWrapper(content);
        }

        public static void CopyTo([NotNull] this IHttpHeaders headers, [NotNull] HttpHeaders destination)
        {
            foreach (var httpHeader in headers)
            {
                destination.TryAddWithoutValidation(httpHeader.Key, httpHeader.Value);
            }
        }

        public static HttpRequestMessage AsHttpRequestMessage([NotNull] this IHttpRequestMessage message)
        {
            var req = message as DefaultHttpRequestMessage;
            if (req != null)
                return req.RequestMessage;
            var result = new HttpRequestMessage(message.Method.ToHttpMethod(), message.RequestUri);
            if (message.Version != null)
                result.Version = message.Version;
            message.Headers.CopyTo(result.Headers);
            if (message.Content != null)
                result.Content = message.Content.AsHttpContent();
            return result;
        }
    }
}
