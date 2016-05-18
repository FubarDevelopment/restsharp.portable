using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace RestSharp.Portable.Content
{
    /// <summary>
    /// A <see cref="IHttpContent"/> implementation of <code>multipart/form-data</code>
    /// </summary>
    public class MultipartFormDataContent : IHttpContent, IEnumerable<IHttpContent>
    {
        private readonly List<IHttpContent> _contents = new List<IHttpContent>();

        private byte[] _buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFormDataContent"/> class.
        /// </summary>
        /// <param name="headers">The HTTP headers for this content element</param>
        public MultipartFormDataContent(IHttpHeaders headers)
        {
            Headers = headers;
            Boundary = Guid.NewGuid().ToString("N");
            Headers.TryAddWithoutValidation("Content-Type", ContentType);
        }

        /// <summary>
        /// Gets the boundary tag
        /// </summary>
        public string Boundary { get; }

        /// <summary>
        /// Gets the HTTP headers for the content.
        /// </summary>
        public IHttpHeaders Headers { get; }

        /// <summary>
        /// Gets the content type of the <code>multipart/form-data</code>
        /// </summary>
        public string ContentType => string.Format("multipart/form-data; boundary={0}", Boundary);

        /// <summary>
        /// Adds a content element to this <code>multipart/form-data</code>
        /// </summary>
        /// <param name="content">The content to add</param>
        /// <param name="name">The name of the content to add</param>
        public void Add([NotNull] IHttpContent content, [NotNull] string name)
        {
            Add(content, name, null);
        }

        /// <summary>
        /// Adds a content element to this <code>multipart/form-data</code>
        /// </summary>
        /// <param name="content">The content to add</param>
        /// <param name="name">The name of the content to add</param>
        /// <param name="fileName">The optional file name of this content element</param>
        public void Add([NotNull] IHttpContent content, [NotNull] string name, [CanBeNull] string fileName)
        {
            if (!content.Headers.Contains("Content-Disposition"))
            {
                content.Headers.TryAddWithoutValidation("Content-Disposition", new[] { BuildContentDisposition(name, fileName) });
            }

            _contents.Add(content);
        }

        /// <summary>
        /// Disposes the content
        /// </summary>
        public void Dispose()
        {
            foreach (var content in _contents)
            {
                content.Dispose();
            }
        }

        /// <summary>
        /// Asynchronously copy the data to the given stream.
        /// </summary>
        /// <param name="stream">The stream to copy to</param>
        /// <returns>The task that copies the data to the stream</returns>
        public async Task CopyToAsync(Stream stream)
        {
            if (_buffer != null)
            {
                await stream.WriteAsync(_buffer, 0, _buffer.Length);
            }
            else
            {
                await WriteTo(stream, false);
            }
        }

        /// <summary>
        /// Gets the raw content as byte array.
        /// </summary>
        /// <param name="maxBufferSize">The maximum buffer size</param>
        /// <returns>The task that loads the data into an internal buffer</returns>
        public async Task LoadIntoBufferAsync(long maxBufferSize)
        {
            var temp = new MemoryStream();
            await WriteTo(temp, false);
            _buffer = temp.ToArray();
        }

        /// <summary>
        /// Returns the data as a stream
        /// </summary>
        /// <returns>The task that returns the stream</returns>
        public async Task<Stream> ReadAsStreamAsync()
        {
            if (_buffer != null)
            {
                return new MemoryStream(_buffer);
            }

            var temp = new MemoryStream();
            await WriteTo(temp, false);
            return temp;
        }

        /// <summary>
        /// Returns the data as byte array
        /// </summary>
        /// <returns>The task that returns the data as byte array</returns>
        public async Task<byte[]> ReadAsByteArrayAsync()
        {
            if (_buffer != null)
            {
                return _buffer;
            }

            var temp = new MemoryStream();
            await WriteTo(temp, false);
            return temp.ToArray();
        }

        /// <summary>
        /// Returns the data as string
        /// </summary>
        /// <returns>The task that returns the data as string</returns>
        public async Task<string> ReadAsStringAsync()
        {
            var data = await ReadAsByteArrayAsync();
            return Encoding.UTF8.GetString(data, 0, data.Length);
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
            long result = 0;
            foreach (var content in _contents)
            {
                result += Boundary.Length + 4;
                result += HttpHeaderContent.ComputeLength(content.Headers);
                long contentLength;
                if (!content.TryComputeLength(out contentLength))
                {
                    length = 0;
                    return false;
                }

                result += contentLength;
                result += 2;
            }

            result += Boundary.Length + 6;
            length = result;
            return true;
        }

        /// <summary>
        /// Returns the enumeration of sub-contents
        /// </summary>
        /// <returns>the enumeration of sub-contents</returns>
        public IEnumerator<IHttpContent> GetEnumerator()
        {
            return _contents.GetEnumerator();
        }

        /// <summary>
        /// Returns the enumeration of sub-contents
        /// </summary>
        /// <returns>the enumeration of sub-contents</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private string BuildContentDisposition([CanBeNull] string name, [CanBeNull] string fileName)
        {
            var result = new StringBuilder("form-data");
            if (name != null)
            {
                result.AppendFormat("; name=\"{0}\"", name);
            }

            if (fileName != null)
            {
                result.AppendFormat("; filename=\"{0}\"", fileName);
            }

            return result.ToString();
        }

        private async Task WriteTo(Stream stream, bool withHeaders)
        {
            if (withHeaders)
            {
                await HttpHeaderContent.WriteTo(Headers, stream);
            }

            var boundaryStart = Encoding.UTF8.GetBytes($"--{Boundary}\r\n");
            var boundaryEnd = Encoding.UTF8.GetBytes($"--{Boundary}--\r\n");
            var lineBreak = Encoding.UTF8.GetBytes("\r\n");
            foreach (var content in _contents)
            {
                await stream.WriteAsync(boundaryStart, 0, boundaryStart.Length);
                await HttpHeaderContent.WriteTo(content.Headers, stream);
                await content.CopyToAsync(stream);

                // ensure line break
                await stream.WriteAsync(lineBreak, 0, lineBreak.Length);
            }

            await stream.WriteAsync(boundaryEnd, 0, boundaryEnd.Length);
        }
    }
}
