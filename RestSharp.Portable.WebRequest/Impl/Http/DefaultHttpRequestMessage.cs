using System;
using System.Net.Http;

using JetBrains.Annotations;

namespace RestSharp.Portable.WebRequest.Impl.Http
{
    /// <summary>
    /// Wraps an instance of the <see cref="HttpRequestMessage"/> as <see cref="IHttpRequestMessage"/>.
    /// </summary>
    public class DefaultHttpRequestMessage : IHttpRequestMessage
    {
        private readonly DefaultHttpHeaders _requestHttpHeaders;

        private IHttpContent _content;

        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpRequestMessage"/> class.
        /// </summary>
        /// <param name="request">The request message to wrap</param>
        public DefaultHttpRequestMessage([NotNull] System.Net.WebRequest request)
        {
            Request = request;
            _requestHttpHeaders = new DefaultHttpHeaders(request.Headers);
            _content = request.Content.AsRestHttpContent();
        }

        /// <summary>
        /// Gets the HTTP request message to wrap
        /// </summary>
        public System.Net.WebRequest Request { get; private set; }

        /// <summary>
        /// Gets the HTTP headers for the request message
        /// </summary>
        public IHttpHeaders Headers
        {
            get { return _requestHttpHeaders; }
        }

        /// <summary>
        /// Gets or sets the HTTP request method
        /// </summary>
        public Method Method
        {
            get { return Request.Method.ToMethod(); }
            set { Request.Method = value.ToHttpMethod(); }
        }

        /// <summary>
        /// Gets or sets the request URI
        /// </summary>
        public Uri RequestUri
        {
            get { return Request.RequestUri; }
            set { Request.RequestUri = value; }
        }

        /// <summary>
        /// Gets or sets the HTTP protocol version
        /// </summary>
        public Version Version
        {
            get { return Request.Version; }
            set { Request.Version = value; }
        }

        /// <summary>
        /// Gets or sets the content of the request message
        /// </summary>
        public IHttpContent Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
                Request.Content = value.AsHttpContent();
            }
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
            Request.Dispose();
        }

        /// <summary>
        /// Disposes the underlying HTTP request message
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
