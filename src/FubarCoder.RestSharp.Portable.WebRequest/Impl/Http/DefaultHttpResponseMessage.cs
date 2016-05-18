using System;
using System.Net;

using JetBrains.Annotations;

using RestSharp.Portable.Impl;

namespace RestSharp.Portable.WebRequest.Impl.Http
{
    /// <summary>
    /// Wraps a <see cref="WebResponse"/> as <see cref="IHttpResponseMessage"/>.
    /// </summary>
    internal class DefaultHttpResponseMessage : IHttpResponseMessage
    {
        private readonly IHttpRequestMessage _requestMessage;

        private readonly WebException _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpResponseMessage"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message for this response</param>
        /// <param name="responseMessage">The response message to wrap</param>
        /// <param name="exception">The exception that occurred during the request</param>
        public DefaultHttpResponseMessage([NotNull] IHttpRequestMessage requestMessage, [CanBeNull] HttpWebResponse responseMessage, [CanBeNull] WebException exception = null)
        {
            ResponseMessage = responseMessage;
            _exception = exception;
            _requestMessage = requestMessage;

            var responseHeaders = new GenericHttpHeaders();
            var contentHeaders = new GenericHttpHeaders();
            if (responseMessage != null && responseMessage.HasHeaderSupport())
            {
                foreach (var headerName in responseMessage.Headers.AllKeys)
                {
                    IHttpHeaders headers;
                    if (headerName.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                    {
                        headers = contentHeaders;
                    }
                    else
                    {
                        headers = responseHeaders;
                    }

                    headers.TryAddWithoutValidation(headerName, responseMessage.Headers[headerName]);
                }
            }

            Content = new HttpWebResponseContent(contentHeaders, responseMessage);
            Headers = responseHeaders;
        }

        /// <summary>
        /// Gets the wrapper <see cref="HttpWebResponse"/> instance.
        /// </summary>
        [CanBeNull]
        public HttpWebResponse ResponseMessage { get; }

        /// <summary>
        /// Gets the HTTP headers returned by the response
        /// </summary>
        [NotNull]
        public IHttpHeaders Headers { get; }

        /// <summary>
        /// Gets a value indicating whether the request was successful
        /// </summary>
        public bool IsSuccessStatusCode => (int)StatusCode < 300;

        /// <summary>
        /// Gets the reason phrase returned together with the status code
        /// </summary>
        [CanBeNull]
        public string ReasonPhrase => ResponseMessage?.StatusDescription;

        /// <summary>
        /// Gets the request message this response was returned for
        /// </summary>
        public IHttpRequestMessage RequestMessage => _requestMessage;

        /// <summary>
        /// Gets the status code
        /// </summary>
        public HttpStatusCode StatusCode => ResponseMessage?.StatusCode ?? HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets the content of the response
        /// </summary>
        public IHttpContent Content { get; }

        /// <summary>
        /// Throws an exception when the status doesn't indicate success.
        /// </summary>
        public void EnsureSuccessStatusCode()
        {
            if (_exception != null)
            {
                throw new WebException(_exception.Message, _exception.InnerException, _exception.Status, _exception.Response);
            }
        }

        /// <summary>
        /// Disposes the underlying HTTP response message
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes the underlying HTTP response message when disposing is set to true
        /// </summary>
        /// <param name="disposing">true, when called from <see cref="Dispose()"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

#if !NET40
            ResponseMessage?.Dispose();
#endif
            _requestMessage.Dispose();
        }
    }
}
