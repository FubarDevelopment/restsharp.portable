using System.IO;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// Abstraction of HTTP content used to write a request or read from a (buffered) response or buffered request.
    /// </summary>
    public interface IHttpContent
    {
        /// <summary>
        /// Asynchronously copy the data to the given stream.
        /// </summary>
        /// <param name="stream">The stream to copy to</param>
        /// <returns>The task that copies the data to the stream</returns>
        Task CopyToAsync(Stream stream);

        /// <summary>
        /// Gets the raw content as byte array.
        /// </summary>
        /// <param name="maxBufferSize">The maximum buffer size</param>
        /// <returns>The task that loads the data into an internal buffer</returns>
        Task LoadIntoBufferAsync(long maxBufferSize);

        /// <summary>
        /// Returns the data as a stream
        /// </summary>
        /// <returns>The task that returns the stream</returns>
        Task<Stream> ReadAsStreamAsync();

        /// <summary>
        /// Returns the data as byte array
        /// </summary>
        /// <returns>The task that returns the data as byte array</returns>
        Task<byte[]> ReadAsByteArrayAsync();
    }
}
