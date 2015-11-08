using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace RestSharp.Portable.HttpClient.Impl.Http
{
    /// <summary>
    /// A shallow wrapper around a <see cref="HttpContent"/> instance.
    /// </summary>
    public class DefaultHttpContent : IHttpContent
    {
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpContent"/> class.
        /// </summary>
        /// <param name="content">The content to perform the actions on</param>
        public DefaultHttpContent([NotNull] HttpContent content)
        {
            Content = content;
            Headers = content.Headers.AsRestHeaders();
        }

        /// <summary>
        /// Gets the content to perform the actions on.
        /// </summary>
        public HttpContent Content { get; }

        /// <summary>
        /// Gets the HTTP headers for the content.
        /// </summary>
        public IHttpHeaders Headers { get; }

        /// <summary>
        /// Asynchronously copy the data to the given stream.
        /// </summary>
        /// <param name="stream">
        /// The stream to copy to
        /// </param>
        /// <returns>
        /// The task that copies the data to the stream
        /// </returns>
        public Task CopyToAsync(Stream stream)
        {
            return Content.CopyToAsync(stream);
        }

        /// <summary>
        /// Gets the raw content as byte array.
        /// </summary>
        /// <param name="maxBufferSize">
        /// The maximum buffer size
        /// </param>
        /// <returns>
        /// The task that loads the data into an internal buffer
        /// </returns>
        public Task LoadIntoBufferAsync(long maxBufferSize)
        {
            return Content.LoadIntoBufferAsync(maxBufferSize);
        }

        /// <summary>
        /// Returns the data as a stream
        /// </summary>
        /// <returns>The task that returns the stream</returns>
        public Task<Stream> ReadAsStreamAsync()
        {
            return Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Returns the data as byte array
        /// </summary>
        /// <returns>The task that returns the data as byte array</returns>
        public Task<byte[]> ReadAsByteArrayAsync()
        {
            return Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Returns the data as string
        /// </summary>
        /// <returns>The task that returns the data as string</returns>
        public Task<string> ReadAsStringAsync()
        {
            return Content.ReadAsStringAsync();
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
            length = long.MaxValue;
            return false;
        }

        /// <summary>
        /// Disposes the underlying HTTP request message
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes the underlying HTTP request message when disposing is set to true
        /// </summary>
        /// <param name="disposing">true, when called from <see cref="Dispose()"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            Content.Dispose();
        }
    }
}
