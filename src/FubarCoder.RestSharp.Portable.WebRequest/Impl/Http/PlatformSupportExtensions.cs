using System.Net;

namespace RestSharp.Portable.WebRequest.Impl.Http
{
    /// <summary>
    /// Platform specific values
    /// </summary>
    internal static class PlatformSupportExtensions
    {
        /// <summary>
        /// Does the <see cref="HttpWebResponse"/> support access to the headers?
        /// </summary>
        /// <param name="response">The <see cref="HttpWebResponse"/> to test</param>
        /// <returns><code>true</code> when the <see cref="HttpWebResponse"/> supports access to the response headers</returns>
        public static bool HasHeaderSupport(this HttpWebResponse response)
        {
#if NET40
            return true;
#else
            return response.SupportsHeaders;
#endif
        }

        /// <summary>
        /// Does the <see cref="HttpWebRequest"/> support setting the cookie container?
        /// </summary>
        /// <param name="request">The <see cref="HttpWebRequest"/> to test</param>
        /// <returns><code>true</code> when the <see cref="HttpWebRequest"/> supports setting the cookie container</returns>
        public static bool HasCookieContainerSupport(this HttpWebRequest request)
        {
#if NET40
            return true;
#else
            return request.SupportsCookieContainer;
#endif
        }
    }
}
