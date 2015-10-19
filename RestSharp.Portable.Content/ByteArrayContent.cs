using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using RestSharp.Portable.Impl;

namespace RestSharp.Portable.Content
{
    internal class ByteArrayContent : IHttpContent
    {
        private readonly byte[] _data;

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

        public int Length => _data.Length;

        public IEnumerable<byte> Data => _data;

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
            await stream.WriteAsync(_data, 0, _data.Length);
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
        public Task<Stream> ReadAsStreamAsync()
        {
#if PCL && !ASYNC_PCL
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
#if PCL && !ASYNC_PCL
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
#if PCL && !ASYNC_PCL
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
