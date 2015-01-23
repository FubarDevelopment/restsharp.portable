using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    class PostParametersContent : HttpContent
    {
        private class EncodedParameter
        {
            private readonly Parameter _parameter;

            public EncodedParameter(Parameter parameter)
            {
                Name = UrlUtility.Escape(parameter.Name);
                _parameter = parameter;
            }

            public string Name { get; private set; }

            private long GetDataLength()
            {
                var v = _parameter.Value;
                if (v == null)
                    return 0;

                var s = v as string;
                if (s != null)
                    return UrlUtility.ComputeLength(s, _parameter.Encoding ?? ParameterExtensions.DefaultEncoding);
                
                var bytes = v as byte[];
                if (bytes != null)
                    return UrlUtility.ComputeLength(bytes);

                s = string.Format("{0}", v);
                return UrlUtility.ComputeLength(s, _parameter.Encoding ?? ParameterExtensions.DefaultEncoding);
            }

            public byte[] GetData()
            {
                var v = _parameter.Value;
                if (v == null)
                    return new byte[0];

                var s = v as string;
                if (s != null)
                    return UrlUtility.EscapeToBytes(s, _parameter.Encoding ?? ParameterExtensions.DefaultEncoding);

                var bytes = v as byte[];
                if (bytes != null)
                    return UrlUtility.EscapeToBytes(bytes);

                s = string.Format("{0}", v);
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
        }

        private class EncodedParameterStream : Stream
        {
            class DataPart
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
                    _data = new[] {b};
                }

                public long Position { get; private set; }

                public long Length
                {
                    get
                    {
                        if (_parameter != null)
                            return _parameter.GetFullDataLength();
                        return _data.Length;
                    }
                }

                public byte[] Data
                {
                    get { return _data ?? (_data = _parameter.GetFullData()); }
                }

                public void ReleaseData()
                {
                    if (_parameter == null)
                        return;
                    _data = null;
                }

                public bool Contains(long position)
                {
                    return position >= Position && position < (Position + Length);
                }
            }

            private readonly List<DataPart> _parts;
            private readonly long _length;
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
                        _parts.Add(new DataPart(position++, (byte)'&'));
                    isFirst = false;
                    _parts.Add(new DataPart(position, parameter));
                    position += parameter.GetFullDataLength();
                }
                _length = position;
                _position = 0;
            }

            private bool IsEOF
            {
                get { return _activePart == _parts.Count; }
            }

            private int FindActivePartForPosition(long position)
            {
                for (var i = 0; i != _parts.Count; ++i)
                {
                    if (_parts[i].Contains(position))
                        return i;
                }
                return _parts.Count;
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override void Flush()
            {
            }

            public override long Length
            {
                get { return _length; }
            }

            public override long Position
            {
                get { return _position; }
                set
                {
                    _position = value;
                    var newActivePart = FindActivePartForPosition(_position);
                    if (newActivePart == _activePart)
                        return;
                    if (!IsEOF)
                        _parts[_activePart].ReleaseData();
                    _activePart = newActivePart;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (IsEOF)
                    return 0;

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
        }

        private readonly List<EncodedParameter> _postParameters;

        public PostParametersContent(IEnumerable<Parameter> postParameters)
        {
            _postParameters = postParameters.Select(x => new EncodedParameter(x)).ToList();
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return new Task<Stream>(() => new EncodedParameterStream(_postParameters));
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var isFirst = true;
            foreach (var parameter in _postParameters)
            {
                if (!isFirst)
                    stream.WriteByte((byte)'&');
                isFirst = false;
                var tmp = ParameterExtensions.DefaultEncoding.GetBytes(parameter.Name);
                await stream.WriteAsync(tmp, 0, tmp.Length);
                stream.WriteByte((byte)'=');
                tmp = parameter.GetData();
                await stream.WriteAsync(tmp, 0, tmp.Length);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            if (_postParameters.Count == 0)
                length = 0;
            else
                length = _postParameters.Sum(x => x.GetFullDataLength()) + _postParameters.Count - 1;
            return true;
        }
    }
}
