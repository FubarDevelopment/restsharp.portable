using System;
using System.Net;

using JetBrains.Annotations;

using RestSharp.Portable.Impl;

namespace RestSharp.Portable.WebRequest.Impl.Http
{
    /// <summary>
    /// Wraps a <see cref="WebResponse"/> as <see cref="IHttpResponseMessage"/>.
    /// </summary>
    public class DefaultHttpResponseMessage : IHttpResponseMessage
    {
        private readonly IHttpRequestMessage _requestMessage;

        private readonly IHttpHeaders _responseHttpHeaders;

        private readonly IHttpContent _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpResponseMessage"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message for this response</param>
        /// <param name="responseMessage">The response message to wrap</param>
        public DefaultHttpResponseMessage([NotNull] IHttpRequestMessage requestMessage, [NotNull] HttpWebResponse responseMessage)
        {
            ResponseMessage = responseMessage;
            _requestMessage = requestMessage;

            var responseHeaders = new GenericHttpHeaders();
            var contentHeaders = new GenericHttpHeaders();
            if (responseMessage.SupportsHeaders)
            {
                foreach (var headerName in responseMessage.Headers.AllKeys)
                {
                    if (headerName.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                    {
                        contentHeaders.TryAddWithoutValidation(headerName, responseMessage.Headers[headerName]);
                    }
                    else
                    {
                        responseHeaders.TryAddWithoutValidation(headerName, responseMessage.Headers[headerName]);
                    }
                }
            }

            _content = new HttpWebResponseContent(contentHeaders, responseMessage);
            _responseHttpHeaders = responseHeaders;
        }

        /// <summary>
        /// Gets the wrapper <see cref="HttpWebResponse"/> instance.
        /// </summary>
        public HttpWebResponse ResponseMessage { get; private set; }

        /// <summary>
        /// Gets the HTTP headers returned by the response
        /// </summary>
        public IHttpHeaders Headers
        {
            get { return _responseHttpHeaders; }
        }

        /// <summary>
        /// Gets a value indicating whether the request was successful
        /// </summary>
        public bool IsSuccessStatusCode
        {
            get { return (int)ResponseMessage.StatusCode < 300; }
        }

        /// <summary>
        /// Gets the reason phrase returned together with the status code
        /// </summary>
        public string ReasonPhrase
        {
            get { return ResponseMessage.StatusDescription; }
        }

        /// <summary>
        /// Gets the request message this response was returned for
        /// </summary>
        public IHttpRequestMessage RequestMessage
        {
            get { return _requestMessage; }
        }

        /// <summary>
        /// Gets the status code
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return ResponseMessage.StatusCode; }
        }

        /// <summary>
        /// Gets the content of the response
        /// </summary>
        public IHttpContent Content
        {
            get { return _content; }
        }

        /// <summary>
        /// Throws an exception when the status doesn't indicate success.
        /// </summary>
        public void EnsureSuccessStatusCode()
        {
            ResponseMessage.EnsureSuccessStatusCode();
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
                return;
            ResponseMessage.Dispose();
            _requestMessage.Dispose();
        }
    }
}
