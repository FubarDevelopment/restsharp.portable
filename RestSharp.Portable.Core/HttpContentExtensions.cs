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
    }
}
