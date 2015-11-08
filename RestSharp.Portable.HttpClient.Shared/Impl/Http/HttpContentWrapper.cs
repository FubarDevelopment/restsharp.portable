using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace RestSharp.Portable.HttpClient.Impl.Http
{
    /// <summary>
    /// Wraps a <see cref="IHttpContent"/> as <see cref="HttpContent"/>.
    /// </summary>
    public class HttpContentWrapper : HttpContent
    {
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContentWrapper"/> class.
        /// </summary>
        /// <param name="content">The content to wrap</param>
        public HttpContentWrapper([NotNull] IHttpContent content)
        {
            Content = content;
            content.Headers.CopyTo(Headers);
        }

        /// <summary>
        /// Gets the <see cref="IHttpContent"/> to wrap.
        /// </summary>
        public IHttpContent Content { get; }

        /// <summary>
        /// Serialize the HTTP content to a stream as an asynchronous operation.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task"/>.The task object representing the asynchronous operation.
        /// </returns>
        /// <param name="stream">The target stream.</param><param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Content.CopyToAsync(stream);
        }

        /// <summary>
        /// Determines whether the HTTP content has a valid length in bytes.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.Boolean"/>.true if <paramref name="length"/> is a valid length; otherwise, false.
        /// </returns>
        /// <param name="length">The length in bytes of the HHTP content.</param>
        protected override bool TryComputeLength(out long length)
        {
            return Content.TryComputeLength(out length);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Net.Http.HttpContent"/> and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;
            _isDisposed = false;
            base.Dispose(disposing);
            Content.Dispose();
        }
    }
}
