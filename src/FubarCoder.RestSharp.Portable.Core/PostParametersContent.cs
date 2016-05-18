using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp.Portable.Impl;

namespace RestSharp.Portable
{
    /// <summary>
    /// Provides a <see cref="IHttpContent"/> implementation for POST parameters.
    /// </summary>
    public class PostParametersContent : IHttpContent
    {
        private readonly List<EncodedParameter> _postParameters;

        private readonly IHttpHeaders _headers = new GenericHttpHeaders();

        private byte[] _buffer;

        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostParametersContent"/> class.
        /// </summary>
        /// <param name="postParameters">The post parameters to provide as content.</param>
        public PostParametersContent(IEnumerable<Parameter> postParameters)
        {
            _postParameters = postParameters.Select(x => new EncodedParameter(x)).ToList();
            _headers.Add("Content-Type", "application/x-www-form-urlencoded");
        }

        /// <summary>
        /// Gets the HTTP headers for the content.
        /// </summary>
        public IHttpHeaders Headers => _headers;

        /// <summary>
        /// Asynchronously copy the data to the given stream.
        /// </summary>
        /// <param name="stream">The stream to copy to</param>
        /// <returns>The task that copies the data to the stream</returns>
        public Task CopyToAsync(Stream stream)
        {
            return Task.Factory.StartNew(
                () =>
                    {
                        if (_buffer != null)
                        {
                            stream.Write(_buffer, 0, _buffer.Length);
                        }
                        else
                        {
                            WriteTo(stream);
                        }
                    });
        }

        /// <summary>
        /// Gets the raw content as byte array.
        /// </summary>
        /// <param name="maxBufferSize">The maximum buffer size</param>
        /// <returns>The task that loads the data into an internal buffer</returns>
        public Task LoadIntoBufferAsync(long maxBufferSize)
        {
            return Task.Factory.StartNew(
                () =>
                    {
                        if (_buffer != null)
                        {
                            return;
                        }

                        using (var temp = new MemoryStream())
                        {
                            WriteTo(temp);
                            _buffer = temp.ToArray();
                        }
                    });
        }

        /// <summary>
        /// Returns the data as a stream
        /// </summary>
        /// <returns>The task that returns the stream</returns>
        public Task<Stream> ReadAsStreamAsync()
        {
            return Task.Factory.StartNew<Stream>(
                () =>
                    {
                        if (_buffer != null)
                        {
                            return new MemoryStream(_buffer, false);
                        }

                        return new EncodedParameterStream(_postParameters);
                    });
        }

        /// <summary>
        /// Returns the data as byte array
        /// </summary>
        /// <returns>The task that returns the data as byte array</returns>
        public Task<byte[]> ReadAsByteArrayAsync()
        {
            return Task.Factory.StartNew(
                () =>
                    {
                        if (_buffer == null)
                        {
                            using (var tempStream = new MemoryStream())
                            {
                                WriteTo(tempStream);
                                _buffer = tempStream.ToArray();
                            }
                        }

                        var temp = new byte[_buffer.Length];
                        Array.Copy(_buffer, temp, _buffer.Length);
                        return temp;
                    });
        }

        /// <summary>
        /// Returns the data as string
        /// </summary>
        /// <returns>The task that returns the data as string</returns>
        public Task<string> ReadAsStringAsync()
        {
            return Task.Factory.StartNew(
                () =>
                {
                    if (_buffer == null)
                    {
                        using (var temp = new MemoryStream())
                        {
                            WriteTo(temp);
                            _buffer = temp.ToArray();
                        }
                    }

                    return Encoding.UTF8.GetString(_buffer, 0, _buffer.Length);
                });
        }

        /// <summary>
        /// Try to compute the resulting length of all POST parameters.
        /// </summary>
        /// <param name="length">The variable that will be set to the computed length</param>
        /// <returns>true, when the length could be computed</returns>
        public bool TryComputeLength(out long length)
        {
            if (_postParameters.Count == 0)
            {
                length = 0;
            }
            else
            {
                length = _postParameters.Sum(x => x.GetFullDataLength()) + _postParameters.Count - 1;
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
            {
                return;
            }

            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
        }

        private void WriteTo(Stream stream)
        {
            var isFirst = true;
            foreach (var parameter in _postParameters)
            {
                if (!isFirst)
                {
                    stream.WriteByte((byte)'&');
                }

                isFirst = false;
                var tmp = ParameterExtensions.DefaultEncoding.GetBytes(parameter.Name);
                stream.Write(tmp, 0, tmp.Length);
                stream.WriteByte((byte)'=');
                tmp = parameter.GetData();
                stream.Write(tmp, 0, tmp.Length);
            }
        }

        private class EncodedParameter
        {
            private readonly Parameter _parameter;

            public EncodedParameter(Parameter parameter)
            {
                Name = UrlUtility.Escape(parameter.Name);
                _parameter = parameter;
            }

            public string Name { get; }

            public byte[] GetData()
            {
                var v = _parameter.Value;
                if (v == null)
                {
                    return new byte[0];
                }

                var s = v as string;
                if (s != null)
                {
                    return UrlUtility.EscapeToBytes(s, _parameter.Encoding ?? ParameterExtensions.DefaultEncoding);
                }

                var bytes = v as byte[];
                if (bytes != null)
                {
                    return UrlUtility.EscapeToBytes(bytes);
                }

                s = _parameter.ToRequestString();
                return UrlUtility.EscapeToBytes(s, _parameter.Encoding ?? ParameterExtensions.DefaultEncoding);
            }

            public long GetFullDataLength()
            {
                return Name.Length + 1 + GetDataLength();
            }

            public byte[] GetFullData()
            {
                var result = new byte[GetFullDataLength()];
                var tempName = ParameterExtensions.DefaultEncoding.GetBytes(Name);
                Array.Copy(tempName, result, tempName.Length);
                var tempData = GetData();
                Array.Copy(tempData, 0, result, tempName.Length, tempData.Length);
                return result;
            }

            private long GetDataLength()
            {
                var v = _parameter.Value;
                if (v == null)
                {
                    return 0;
                }

                var s = v as string;
                if (s != null)
                {
                    return UrlUtility.ComputeLength(s, _parameter.Encoding ?? ParameterExtensions.DefaultEncoding);
                }

                var bytes = v as byte[];
                if (bytes != null)
                {
                    return UrlUtility.ComputeLength(bytes);
                }

                s = _parameter.ToRequestString();
                return UrlUtility.ComputeLength(s, _parameter.Encoding ?? ParameterExtensions.DefaultEncoding);
            }
        }

        private class EncodedParameterStream : Stream
        {
            private readonly List<DataPart> _parts;
            private long _position;
            private int _activePart;

            public EncodedParameterStream(IEnumerable<EncodedParameter> parameters)
            {
                _parts = new List<DataPart>();
                var isFirst = true;
                long position = 0;
                foreach (var parameter in parameters)
                {
                    if (!isFirst)
                    {
                        _parts.Add(new DataPart(position++, (byte)'&'));
                    }

                    isFirst = false;
                    _parts.Add(new DataPart(position, parameter));
                    position += parameter.GetFullDataLength();
                }

                Length = position;
                _position = 0;
            }

            public override bool CanRead => true;

            public override bool CanSeek => true;

            public override bool CanWrite => false;

            public override long Length { get; }

            public override long Position
            {
                get
                {
                    return _position;
                }
                set
                {
                    _position = value;
                    var newActivePart = FindActivePartForPosition(_position);
                    if (newActivePart == _activePart)
                    {
                        return;
                    }

                    if (!IsEOF)
                    {
                        _parts[_activePart].ReleaseData();
                    }

                    _activePart = newActivePart;
                }
            }

            private bool IsEOF => _activePart == _parts.Count;

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (IsEOF)
                {
                    return 0;
                }

                var part = _parts[_activePart];
                var partOffset = (int)(Position - part.Position);
                var remainingLength = (int)part.Length - partOffset;
                var copyLength = Math.Min(remainingLength, count);
                Array.Copy(part.Data, partOffset, buffer, offset, copyLength);
                if (copyLength == remainingLength)
                {
                    part.ReleaseData();
                    _activePart += 1;
                }

                return copyLength;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        Position = offset;
                        break;
                    case SeekOrigin.Current:
                        Position = Position + offset;
                        break;
                    case SeekOrigin.End:
                        Position = Length + offset;
                        break;
                }

                return Position;
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            private int FindActivePartForPosition(long position)
            {
                for (var i = 0; i != _parts.Count; ++i)
                {
                    if (_parts[i].Contains(position))
                    {
                        return i;
                    }
                }

                return _parts.Count;
            }

            private class DataPart
            {
                private readonly EncodedParameter _parameter;
                private byte[] _data;

                public DataPart(long position, EncodedParameter parameter)
                {
                    Position = position;
                    _parameter = parameter;
                }

                public DataPart(long position, byte b)
                {
                    Position = position;
                    _data = new[] { b };
                }

                public long Position { get; }

                public long Length
                {
                    get
                    {
                        if (_parameter != null)
                        {
                            return _parameter.GetFullDataLength();
                        }

                        return _data.Length;
                    }
                }

                public byte[] Data => _data ?? (_data = _parameter.GetFullData());

                public void ReleaseData()
                {
                    if (_parameter == null)
                    {
                        return;
                    }

                    _data = null;
                }

                public bool Contains(long position)
                {
                    return position >= Position && position < (Position + Length);
                }
            }
        }
    }
}
