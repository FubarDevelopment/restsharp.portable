using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using RestSharp.Portable.Impl;

namespace RestSharp.Portable.Content
{
    /// <summary>
    /// A <see cref="IHttpContent"/> implementation for a byte array
    /// </summary>
    public class ByteArrayContent : IHttpContent
    {
        private readonly byte[] _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayContent"/> class.
        /// </summary>
        /// <param name="data">The underlying binary data</param>
        public ByteArrayContent(byte[] data)
        {
            _data = data;
            Headers = new GenericHttpHeaders();
            Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
        }

        /// <summary>
        /// Gets the HTTP headers for the content.
        /// </summary>
        public IHttpHeaders Headers { get; }

        /// <summary>
        /// Gets the content length
        /// </summary>
        public int Length => _data.Length;

        /// <summary>
        /// Gets the content
        /// </summary>
        public IEnumerable<byte> Data => _data;

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>
        /// Asynchronously copy the data to the given stream.
        /// </summary>
        /// <param name="stream">The stream to copy to</param>
        /// <returns>The task that copies the data to the stream</returns>
        public Task CopyToAsync(Stream stream)
        {
            return stream.WriteAsync(_data, 0, _data.Length);
        }

        /// <summary>
        /// Gets the raw content as byte array.
        /// </summary>
        /// <param name="maxBufferSize">The maximum buffer size</param>
        /// <returns>The task that loads the data into an internal buffer</returns>
        public Task LoadIntoBufferAsync(long maxBufferSize)
        {
#if USE_TASKEX
            return TaskEx.FromResult(0);
#else
            return Task.FromResult(0);
#endif
        }

        /// <summary>
        /// Returns the data as a stream
        /// </summary>
        /// <returns>The task that returns the stream</returns>
        public Task<Stream> ReadAsStreamAsync()
        {
#if USE_TASKEX
            return TaskEx.FromResult<Stream>(new MemoryStream(_data));
#else
            return Task.FromResult<Stream>(new MemoryStream(_data));
#endif
        }

        /// <summary>
        /// Returns the data as byte array
        /// </summary>
        /// <returns>The task that returns the data as byte array</returns>
        public Task<byte[]> ReadAsByteArrayAsync()
        {
#if USE_TASKEX
            return TaskEx.FromResult(_data);
#else
            return Task.FromResult(_data);
#endif
        }

        /// <summary>
        /// Returns the data as string
        /// </summary>
        /// <returns>The task that returns the data as string</returns>
        public Task<string> ReadAsStringAsync()
        {
#if USE_TASKEX
            return TaskEx.FromResult(Encoding.UTF8.GetString(_data, 0, _data.Length));
#else
            return Task.FromResult(Encoding.UTF8.GetString(_data, 0, _data.Length));
#endif
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
            length = _data.Length;
            return true;
        }
    }
}
