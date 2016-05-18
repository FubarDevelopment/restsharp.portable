using System.Net;

namespace RestSharp.Portable.WebRequest.Impl.Http
{
    internal static class PlatformSupportExtensions
    {
        public static bool HasHeaderSupport(this HttpWebResponse response)
        {
#if NET40
            return true;
#else
            return response.SupportsHeaders;
#endif
        }

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
