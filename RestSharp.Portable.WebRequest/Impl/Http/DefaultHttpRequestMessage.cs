using System;

using JetBrains.Annotations;

namespace RestSharp.Portable.WebRequest.Impl.Http
{
    /// <summary>
    /// Wraps an instance of the <see cref="System.Net.WebRequest"/> as <see cref="IHttpRequestMessage"/>.
    /// </summary>
    public class DefaultHttpRequestMessage : IHttpRequestMessage
    {
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpRequestMessage"/> class.
        /// </summary>
        /// <param name="method">The request method</param>
        /// <param name="requestUri">The request URI</param>
        /// <param name="headers">The request headers to be used for the request</param>
        /// <param name="content">The content of the request body or NULL if no content</param>
        public DefaultHttpRequestMessage(Method method, [NotNull] Uri requestUri, [NotNull] IHttpHeaders headers, [CanBeNull] IHttpContent content)
        {
            Method = method;
            RequestUri = requestUri;
            Headers = headers;
            Version = new Version(1, 1);
            Content = content;
        }

        /// <summary>
        /// Gets the HTTP headers for the request message
        /// </summary>
        public IHttpHeaders Headers { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP request method
        /// </summary>
        public Method Method { get; set; }

        /// <summary>
        /// Gets or sets the request URI
        /// </summary>
        public Uri RequestUri { get; set; }

        /// <summary>
        /// Gets or sets the HTTP protocol version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the content of the request message
        /// </summary>
        public IHttpContent Content { get; set; }

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
            Content.Dispose();
        }
    }
}
