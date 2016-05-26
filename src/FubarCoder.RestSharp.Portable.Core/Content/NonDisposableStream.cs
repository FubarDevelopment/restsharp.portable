using System;
using System.IO;

namespace RestSharp.Portable.Content
{
    /// <summary>
    /// A stream that encapsulates another and avoids calling the <see cref="IDisposable.Dispose"/>
    /// </summary>
    internal class NonDisposableStream : Stream
    {
        private readonly Stream _baseStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonDisposableStream"/> class.
        /// </summary>
        /// <param name="baseStream">The underlying stream</param>
        public NonDisposableStream(Stream baseStream)
        {
            _baseStream = baseStream;
        }

        /// <inheritdoc/>
        public override bool CanRead => _baseStream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => _baseStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => _baseStream.CanWrite;

        /// <inheritdoc/>
        public override long Length => _baseStream.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get { return _baseStream.Position; }
            set { _baseStream.Position = value; }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            _baseStream.Flush();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
        }
    }
}
