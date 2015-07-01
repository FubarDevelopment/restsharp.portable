using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RestSharp.Portable.WebRequest.Impl.Http
{
    public class HttpWebResponseContent : IHttpContent
    {
        private readonly HttpWebResponse _response;

        private readonly List<byte[]> _buffers = new List<byte[]>();

        private bool _isDisposed;

        private long _bufferSize;

        private Stream _responseStream;

        private bool _endOfStreamReached;

        private long? _contentLength;

        public HttpWebResponseContent(IHttpHeaders headers, HttpWebResponse response)
        {
            Headers = headers;
            _response = response;
            IEnumerable<string> contentLength;
            if (headers.TryGetValues("content-length", out contentLength))
            {
                var headerValue = contentLength.FirstOrDefault();
                if (headerValue != null)
                    _contentLength = long.Parse(headerValue);
            }
        }

        /// <summary>
        /// Gets the HTTP headers for the content.
        /// </summary>
        public IHttpHeaders Headers { get; private set; }

        /// <summary>
        /// Asynchronously copy the data to the given stream.
        /// </summary>
        /// <param name="stream">The stream to copy to</param>
        /// <returns>The task that copies the data to the stream</returns>
        public async Task CopyToAsync(Stream stream)
        {
            LoadIntoBuffer(null, 4000);
            foreach (var buffer in _buffers)
                await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Gets the raw content as byte array.
        /// </summary>
        /// <param name="maxBufferSize">The maximum buffer size</param>
        /// <returns>The task that loads the data into an internal buffer</returns>
        public Task LoadIntoBufferAsync(long maxBufferSize)
        {
            return TaskEx.Run(() => LoadIntoBuffer(maxBufferSize, 4000));
        }

        /// <summary>
        /// Returns the data as a stream
        /// </summary>
        /// <returns>The task that returns the stream</returns>
        public async Task<Stream> ReadAsStreamAsync()
        {
            return new MemoryStream(await ReadAsByteArrayAsync());
        }

        /// <summary>
        /// Returns the data as byte array
        /// </summary>
        /// <returns>The task that returns the data as byte array</returns>
        public Task<byte[]> ReadAsByteArrayAsync()
        {
            LoadIntoBuffer(null, 4000);

            var result = new byte[_bufferSize];
            var offset = 0;
            foreach (var buffer in _buffers)
            {
                Array.Copy(buffer, 0, result, offset, buffer.Length);
                offset += buffer.Length;
            }

            return TaskEx.FromResult(result);
        }

        /// <summary>
        /// Returns the data as string
        /// </summary>
        /// <returns>The task that returns the data as string</returns>
        public async Task<string> ReadAsStringAsync()
        {
            return await new StreamReader(new MemoryStream(await ReadAsByteArrayAsync()), true).ReadToEndAsync();
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
            if (_contentLength != null)
            {
                length = _contentLength.Value;
            }
            else
            {
                LoadIntoBuffer(null, 4000);
                length = _bufferSize;
            }

            return true;
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
                return;
            if (_isDisposed)
                return;
            _isDisposed = true;
            _response.Dispose();
        }

        private void LoadIntoBuffer(long? size, int readBlockSize)
        {
            if (_endOfStreamReached)
                return;
            if (_bufferSize >= size)
                return;
            if (_responseStream == null)
                _responseStream = _response.GetResponseStream();
            while (!_endOfStreamReached && (size == null || _bufferSize < size))
            {
                var blockSize = size == null ? readBlockSize : (int)Math.Min(readBlockSize, size.Value - _bufferSize);
                var buffer = new byte[blockSize];
                var readSize = _responseStream.Read(buffer, 0, blockSize);
                if (readSize == 0)
                {
                    _endOfStreamReached = true;
                }
                else if (readSize < blockSize)
                {
                    var temp = new byte[readSize];
                    Array.Copy(buffer, temp, readSize);
                    _buffers.Add(temp);
                }
                else
                {
                    _buffers.Add(buffer);
                }

                _bufferSize += readSize;
            }

            if (_endOfStreamReached)
            {
                _responseStream.Dispose();
                _responseStream = null;
            }
        }
    }
}
