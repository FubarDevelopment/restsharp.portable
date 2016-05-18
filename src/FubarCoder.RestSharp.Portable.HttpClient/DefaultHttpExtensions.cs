using System.Net.Http;
using System.Net.Http.Headers;

using JetBrains.Annotations;

using RestSharp.Portable.HttpClient.Impl.Http;

namespace RestSharp.Portable.HttpClient
{
    /// <summary>
    /// Extension methods useful for supporting conversion between the objects used by <see cref="System.Net.Http.HttpClient"/> and <see cref="RestSharp.Portable.HttpClient.RestClient"/>.
    /// </summary>
    internal static class DefaultHttpExtensions
    {
        /// <summary>
        /// Converts <see cref="HttpHeaders"/> to <see cref="IHttpHeaders"/>
        /// </summary>
        /// <param name="headers">The <see cref="HttpHeaders"/> to convert to <see cref="IHttpHeaders"/></param>
        /// <returns>The converted <see cref="IHttpHeaders"/></returns>
        public static IHttpHeaders AsRestHeaders([CanBeNull] this HttpHeaders headers)
        {
            if (headers == null)
            {
                return null;
            }

            return new DefaultHttpHeaders(headers);
        }

        /// <summary>
        /// Converts the <see cref="IHttpHeaders"/> to <see cref="HttpHeaders"/>
        /// </summary>
        /// <param name="headers">The <see cref="IHttpHeaders"/> to convert</param>
        /// <returns>The converted <see cref="IHttpHeaders"/></returns>
        [CanBeNull]
        public static HttpHeaders AsHttpHeaders([CanBeNull] this IHttpHeaders headers)
        {
            var defaultHeaders = headers as DefaultHttpHeaders;
            return defaultHeaders?.Headers;
        }

        /// <summary>
        /// Converts the <see cref="HttpContent"/> to a <see cref="IHttpContent"/>
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> to convert</param>
        /// <returns>The converted <see cref="HttpContent"/></returns>
        public static IHttpContent AsRestHttpContent([CanBeNull] this HttpContent content)
        {
            if (content == null)
            {
                return null;
            }

            var wrapper = content as HttpContentWrapper;
            if (wrapper == null)
            {
                return new DefaultHttpContent(content);
            }

            return wrapper.Content;
        }

        /// <summary>
        /// Converts the <see cref="IHttpContent"/> to a <see cref="HttpContent"/>
        /// </summary>
        /// <param name="content">The <see cref="IHttpContent"/> to convert</param>
        /// <returns>The converted <see cref="IHttpContent"/></returns>
        [CanBeNull]
        public static HttpContent AsHttpContent([CanBeNull] this IHttpContent content)
        {
            if (content == null)
            {
                return null;
            }

            var defaultHttpContent = content as DefaultHttpContent;
            if (defaultHttpContent != null)
            {
                return defaultHttpContent.Content;
            }

            return new HttpContentWrapper(content);
        }

        /// <summary>
        /// Copy the <see cref="IHttpHeaders"/> into the <see cref="HttpHeaders"/>
        /// </summary>
        /// <param name="headers">The <see cref="IHttpHeaders"/> to copy from</param>
        /// <param name="destination">The <see cref="HttpHeaders"/> to copy to</param>
        public static void CopyTo([NotNull] this IHttpHeaders headers, [NotNull] HttpHeaders destination)
        {
            foreach (var httpHeader in headers)
            {
                destination.TryAddWithoutValidation(httpHeader.Key, httpHeader.Value);
            }
        }

        /// <summary>
        /// Creates a <see cref="HttpRequestMessage"/> from a <see cref="IHttpRequestMessage"/>
        /// </summary>
        /// <param name="message">The <see cref="IHttpRequestMessage"/> used to create the <see cref="HttpRequestMessage"/> from</param>
        /// <returns>The <see cref="HttpRequestMessage"/> created from <paramref name="message"/></returns>
        public static HttpRequestMessage AsHttpRequestMessage([NotNull] this IHttpRequestMessage message)
        {
            var req = message as DefaultHttpRequestMessage;
            if (req != null)
            {
                return req.RequestMessage;
            }

            var result = new HttpRequestMessage(message.Method.ToHttpMethod(), message.RequestUri);
            if (message.Version != null)
            {
                result.Version = message.Version;
            }

            message.Headers.CopyTo(result.Headers);
            if (message.Content != null)
            {
                result.Content = message.Content.AsHttpContent();
            }

            return result;
        }
    }
}
