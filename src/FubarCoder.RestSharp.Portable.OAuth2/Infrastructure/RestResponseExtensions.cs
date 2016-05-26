using System.Linq;

namespace RestSharp.Portable.OAuth2.Infrastructure
{
    /// <summary>
    /// REST response extensions
    /// </summary>
    public static class RestResponseExtensions
    {
        /// <summary>
        /// IsEmpty for RestSharp.Portable
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool IsEmpty(this IRestResponse response)
        {
            var data = response.RawBytes;
            if (data == null)
                return true;
            if (data.All(x => x == 0 || x == 9 || x == 32))
                return true;
            return false;
        }
    }
}
