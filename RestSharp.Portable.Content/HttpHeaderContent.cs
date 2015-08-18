using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable.Content
{
    internal class HttpHeaderContent : IHttpContent
    {
        public HttpHeaderContent(IHttpHeaders headers)
        {
            Headers = headers;
        }

        /// <summary>
        /// Gets the HTTP headers for the content.
        /// </summary>
        public IHttpHeaders Headers { get; private set; }

        public static async Task WriteTo(IHttpHeaders headers, Stream stream)
        {
#if PCL && !ASYNC_PCL
            var writer = new StreamWriter(new NonDisposableStream(stream), Encoding.UTF8, 128);
#else
            var writer = new StreamWriter(stream, Encoding.UTF8, 128, true);
#endif
            try
            {
                writer.NewLine = "\r\n";

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        await
                            writer.WriteLineAsync(string.Format("{0}: {1}", header.Key, string.Join(", ", header.Value)));
                    }
                }

                await writer.WriteLineAsync();
            }
            finally
            {
                writer.Dispose();
            }
        }

        public static long ComputeLength(IHttpHeaders headers)
        {
            long result = 2;
            foreach (var header in headers)
            {
                result += header.Key.Length + header.Value.Sum(x => x.Length + 2) + 2;
            }

            return result;
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Asynchronously copy the data to the given stream.
        /// </summary>
        /// <param name="stream">The stream to copy to</param>
        /// <returns>The task that copies the data to the stream</returns>
        public async Task CopyToAsync(Stream stream)
        {
            await WriteTo(stream);
        }

        /// <summary>
        /// Gets the raw content as byte array.
        /// </summary>
        /// <param name="maxBufferSize">The maximum buffer size</param>
        /// <returns>The task that loads the data into an internal buffer</returns>
        public async Task LoadIntoBufferAsync(long maxBufferSize)
        {
#if PCL && !ASYNC_PCL
            await TaskEx.Yield();
#else
            await Task.Yield();
#endif
        }

        /// <summary>
        /// Returns the data as a stream
        /// </summary>
        /// <returns>The task that returns the stream</returns>
        public async Task<Stream> ReadAsStreamAsync()
        {
            var stream = new MemoryStream();
            await WriteTo(stream);
            return stream;
        }

        /// <summary>
        /// Returns the data as byte array
        /// </summary>
        /// <returns>The task that returns the data as byte array</returns>
        public async Task<byte[]> ReadAsByteArrayAsync()
        {
            var stream = new MemoryStream();
            await WriteTo(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Returns the data as string
        /// </summary>
        /// <returns>The task that returns the data as string</returns>
        public async Task<string> ReadAsStringAsync()
        {
            var buffer = await ReadAsByteArrayAsync();
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Determines whether the HTTP content has a valid length in bytes.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.Boolean"/>.true if <paramref name="length"/> is a valid length; otherwise, false.
        /// </returns>
        /// <param name="length">The length in bytes of the HTTP content.</param>
        public bool TryComputeLength(out long length)
        {
            length = ComputeLength(Headers);
            return true;
        }

        private async Task WriteTo(Stream stream)
        {
            await WriteTo(Headers, stream);
        }
    }
}
