using System;
using System.Net;
using System.Net.Http;

using JetBrains.Annotations;

namespace RestSharp.Portable.HttpClient.Impl.Http
{
    /// <summary>
    /// Wraps a <see cref="HttpResponseMessage"/> as <see cref="IHttpResponseMessage"/>.
    /// </summary>
    public class DefaultHttpResponseMessage : IHttpResponseMessage
    {
        private readonly IHttpRequestMessage _requestMessage;

        private readonly DefaultHttpHeaders _responseHttpHeaders;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpResponseMessage"/> class.
        /// </summary>
        /// <param name="responseMessage">The response message to wrap</param>
        public DefaultHttpResponseMessage([NotNull] HttpResponseMessage responseMessage)
        {
            ResponseMessage = responseMessage;
            if (responseMessage.RequestMessage != null)
            {
                _requestMessage = new DefaultHttpRequestMessage(responseMessage.RequestMessage);
            }

            Content = responseMessage.Content.AsRestHttpContent();
            _responseHttpHeaders = new DefaultHttpHeaders(responseMessage.Headers);
        }

        /// <summary>
        /// Gets the wrapper <see cref="HttpResponseMessage"/> instance.
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; }

        /// <summary>
        /// Gets the HTTP headers returned by the response
        /// </summary>
        public IHttpHeaders Headers => _responseHttpHeaders;

        /// <summary>
        /// Gets a value indicating whether the request was successful
        /// </summary>
        public bool IsSuccessStatusCode => ResponseMessage.IsSuccessStatusCode;

        /// <summary>
        /// Gets the reason phrase returned together with the status code
        /// </summary>
        public string ReasonPhrase => ResponseMessage.ReasonPhrase;

        /// <summary>
        /// Gets the request message this response was returned for
        /// </summary>
        public IHttpRequestMessage RequestMessage => _requestMessage;

        /// <summary>
        /// Gets the status code
        /// </summary>
        public HttpStatusCode StatusCode => ResponseMessage.StatusCode;

        /// <summary>
        /// Gets the content of the response
        /// </summary>
        public IHttpContent Content { get; }

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
            {
                return;
            }

            ResponseMessage.Dispose();
            _requestMessage.Dispose();
        }
    }
}
