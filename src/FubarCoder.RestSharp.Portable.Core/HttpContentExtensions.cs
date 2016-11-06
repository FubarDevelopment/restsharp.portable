using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// Extension methods for <see cref="IHttpContent"/>.
    /// </summary>
    public static class HttpContentExtensions
    {
        /// <summary>
        /// Loads the data into a buffer with a maximum size of <see cref="int.MaxValue"/>.
        /// </summary>
        /// <param name="content">The content to load the data from</param>
        /// <returns>The task that loads the content</returns>
        public static Task LoadIntoBufferAsync(this IHttpContent content)
        {
            return content.LoadIntoBufferAsync(int.MaxValue);
        }

        /// <summary>
        /// Tries to get the encoding handler for compressed responses
        /// </summary>
        /// <param name="content">The content to decode</param>
        /// <param name="restClient">The REST client containing the encoding handlers</param>
        /// <returns>The found encoding</returns>
        internal static IEncoding GetEncoding(this IHttpContent content, IRestClient restClient)
        {
            IEnumerable<string> contentEncodings;
            if (content.Headers.TryGetValues("Content-Encoding", out contentEncodings))
            {
                return restClient.GetEncoding(contentEncodings);
            }
            return null;
        }
    }
}
