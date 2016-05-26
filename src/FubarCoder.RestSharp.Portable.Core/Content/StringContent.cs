using System.IO;
using System.Text;
using System.Threading.Tasks;

using JetBrains.Annotations;

using RestSharp.Portable.Impl;

namespace RestSharp.Portable.Content
{
    /// <summary>
    /// A <see cref="IHttpContent"/> implementation for a string.
    /// </summary>
    public class StringContent : IHttpContent
    {
        private readonly string _value;

        private readonly Encoding _encoding;

        private byte[] _buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringContent"/> class.
        /// </summary>
        /// <param name="value">The value of this HTTP content element</param>
        /// <param name="encoding">The encoding when the data has to be serializes into bytes</param>
        public StringContent([NotNull] string value, [NotNull] Encoding encoding)
        {
            _value = value;
            _encoding = encoding;
            Headers = new GenericHttpHeaders();
            Headers.TryAddWithoutValidation("Content-Type", $"text/plain; charset={encoding.WebName}");
        }

        /// <summary>
        /// Gets the HTTP headers for the content.
        /// </summary>
        public IHttpHeaders Headers { get; }

        /// <inheritdoc/>
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
            var data = _buffer ?? _encoding.GetBytes(_value);
            await stream.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Gets the raw content as byte array.
        /// </summary>
        /// <param name="maxBufferSize">The maximum buffer size</param>
        /// <returns>The task that loads the data into an internal buffer</returns>
        public Task LoadIntoBufferAsync(long maxBufferSize)
        {
            _buffer = _encoding.GetBytes(_value);
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
            var data = _buffer ?? _encoding.GetBytes(_value);
#if USE_TASKEX
            return TaskEx.FromResult<Stream>(new MemoryStream(data));
#else
            return Task.FromResult<Stream>(new MemoryStream(data));
#endif
        }

        /// <summary>
        /// Returns the data as byte array
        /// </summary>
        /// <returns>The task that returns the data as byte array</returns>
        public Task<byte[]> ReadAsByteArrayAsync()
        {
            var data = _buffer ?? _encoding.GetBytes(_value);
#if USE_TASKEX
            return TaskEx.FromResult(data);
#else
            return Task.FromResult(data);
#endif
        }

        /// <summary>
        /// Returns the data as string
        /// </summary>
        /// <returns>The task that returns the data as string</returns>
        public Task<string> ReadAsStringAsync()
        {
#if USE_TASKEX
            return TaskEx.FromResult(_value);
#else
            return Task.FromResult(_value);
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
            length = _buffer?.Length ?? _encoding.GetByteCount(_value);

            return true;
        }
    }
}
