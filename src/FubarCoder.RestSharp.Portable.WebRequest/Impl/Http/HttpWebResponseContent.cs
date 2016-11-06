using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace RestSharp.Portable.WebRequest.Impl.Http
{
    /// <summary>
    /// A <see cref="IHttpContent"/> implementation for a <see cref="HttpWebResponse"/>
    /// </summary>
    internal class HttpWebResponseContent : IHttpContent
    {
        private delegate Task BufferWriteAsyncDelegate(byte[] buffer, int length);

        private readonly HttpWebResponse _response;

        private readonly List<byte[]> _buffers = new List<byte[]>();

        private bool? _storedIntoBuffer;

        private bool _isDisposed;

        private long _bufferSize;

        private Stream _responseStream;

        private bool _endOfStreamReached;

        private long? _contentLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebResponseContent"/> class.
        /// </summary>
        /// <param name="headers">The HTTP headers for the response content</param>
        /// <param name="response">The response</param>
        public HttpWebResponseContent([NotNull] IHttpHeaders headers, [CanBeNull] HttpWebResponse response)
        {
            Headers = headers;
            _response = response;
            IEnumerable<string> contentLength;
            if (headers.TryGetValues("content-length", out contentLength))
            {
                var headerValue = contentLength.FirstOrDefault();
                if (headerValue != null)
                {
                    _contentLength = long.Parse(headerValue);
                }
            }
        }

        /// <summary>
        /// Gets the HTTP headers for the content.
        /// </summary>
        public IHttpHeaders Headers { get; }

        /// <summary>
        /// Asynchronously copy the data to the given stream.
        /// </summary>
        /// <param name="stream">The stream to copy to</param>
        /// <returns>The task that copies the data to the stream</returns>
        public async Task CopyToAsync(Stream stream)
        {
            if (_storedIntoBuffer == null)
            {
                _storedIntoBuffer = false;
                await LoadData(null, 4000, (data, length) =>
                {
                    _bufferSize += length;
                    return stream.WriteAsync(data, 0, length);
                });
            }
            else if (_storedIntoBuffer.Value)
            {
                foreach (var buffer in _buffers)
                {
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot store data into buffer when a different load operation was executed.");
            }
        }

        /// <summary>
        /// Gets the raw content as byte array.
        /// </summary>
        /// <param name="maxBufferSize">The maximum buffer size</param>
        /// <returns>The task that loads the data into an internal buffer</returns>
        public Task LoadIntoBufferAsync(long maxBufferSize)
        {
            if (!_storedIntoBuffer.GetValueOrDefault(true))
                throw new InvalidOperationException("Cannot store data into buffer when a different load operation was executed.");
            _storedIntoBuffer = true;
            return LoadData(maxBufferSize, 4000, StoreIntoBuffer);
        }

        /// <summary>
        /// Returns the data as a stream
        /// </summary>
        /// <returns>The task that returns the stream</returns>
        public Task<Stream> ReadAsStreamAsync()
        {
            if (_storedIntoBuffer != null)
                throw new InvalidOperationException("Cannot store data into buffer when a different load operation was executed.");
            _storedIntoBuffer = false;
#if USE_TASKEX
            return TaskEx.FromResult(_response.GetResponseStream());
#else
            return Task.FromResult(_response.GetResponseStream());
#endif
        }

        /// <summary>
        /// Returns the data as byte array
        /// </summary>
        /// <returns>The task that returns the data as byte array</returns>
        public async Task<byte[]> ReadAsByteArrayAsync()
        {
            if (!_storedIntoBuffer.GetValueOrDefault(true))
                throw new InvalidOperationException("Cannot store data into buffer when a different load operation was executed.");

            _storedIntoBuffer = true;
            await LoadData(null, 4000, StoreIntoBuffer);

            var result = new byte[_bufferSize];
            var offset = 0;
            foreach (var buffer in _buffers)
            {
                Array.Copy(buffer, 0, result, offset, buffer.Length);
                offset += buffer.Length;
            }

            return result;
        }

        /// <summary>
        /// Returns the data as string
        /// </summary>
        /// <returns>The task that returns the data as string</returns>
        public async Task<string> ReadAsStringAsync()
        {
            if (_storedIntoBuffer != null)
                throw new InvalidOperationException("Cannot store data into buffer when a different load operation was executed.");
            _storedIntoBuffer = false;
            var responseStream = _response?.GetResponseStream();
            if (responseStream == null)
                return null;
            return await new StreamReader(responseStream).ReadToEndAsync();
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
                return true;
            }

            length = 0;
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
#if !NET40
            _response?.Dispose();
#endif
        }

        private async Task LoadData(long? size, int readBlockSize, BufferWriteAsyncDelegate writeFunc)
        {
            if (_endOfStreamReached)
            {
                return;
            }

            if (_bufferSize >= size)
            {
                return;
            }

            if (_response == null)
            {
                return;
            }

            if (_responseStream == null)
            {
                _responseStream = _response.GetResponseStream();
            }

            if (_responseStream == null)
            {
                return;
            }

            var buffer = new byte[readBlockSize];
            while (!_endOfStreamReached && (size == null || _bufferSize < size))
            {
                var blockSize = size == null ? readBlockSize : (int)Math.Min(readBlockSize, size.Value - _bufferSize);
                var readSize = await _responseStream.ReadAsync(buffer, 0, blockSize);
                if (readSize == 0)
                {
                    _endOfStreamReached = true;
                }
                else
                {
                    await writeFunc(buffer, readSize);
                }

                _bufferSize += readSize;
            }

            if (_endOfStreamReached)
            {
                _responseStream.Dispose();
                _responseStream = null;
            }
        }

        private Task StoreIntoBuffer(byte[] buffer, int length)
        {
            var temp = new byte[length];
            Array.Copy(buffer, temp, length);
            _buffers.Add(temp);
            _storedIntoBuffer = true;
#if USE_TASKEX
            return TaskEx.FromResult(0);
#else
            return Task.FromResult(0);
#endif
        }
    }
}
