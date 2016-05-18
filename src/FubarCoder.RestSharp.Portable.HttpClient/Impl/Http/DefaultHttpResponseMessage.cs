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
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpResponseMessage"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message</param>
        /// <param name="responseMessage">The response message to wrap</param>
        public DefaultHttpResponseMessage([NotNull] HttpRequestMessage requestMessage, [CanBeNull] HttpResponseMessage responseMessage)
        {
            ResponseMessage = responseMessage;
            RequestMessage = new DefaultHttpRequestMessage(requestMessage);

            if (responseMessage != null)
            {
                Content = responseMessage.Content.AsRestHttpContent();
                Headers = new DefaultHttpHeaders(responseMessage.Headers);
            }
            else
            {
                Content = null;
                Headers = new Portable.Impl.GenericHttpHeaders();
            }
        }

        /// <summary>
        /// Gets the wrapper <see cref="HttpResponseMessage"/> instance.
        /// </summary>
        [CanBeNull]
        public HttpResponseMessage ResponseMessage { get; }

        /// <summary>
        /// Gets the HTTP headers returned by the response
        /// </summary>
        public IHttpHeaders Headers { get; }

        /// <summary>
        /// Gets a value indicating whether the request was successful
        /// </summary>
        public bool IsSuccessStatusCode => ResponseMessage?.IsSuccessStatusCode ?? false;

        /// <summary>
        /// Gets the reason phrase returned together with the status code
        /// </summary>
        public string ReasonPhrase => ResponseMessage?.ReasonPhrase ?? "Unknown error";

        /// <summary>
        /// Gets the request message this response was returned for
        /// </summary>
        public IHttpRequestMessage RequestMessage { get; }

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
        /// <exception cref="WebException">Thrown when no response message was returned for a request.</exception>
        public void EnsureSuccessStatusCode()
        {
            if (ResponseMessage == null)
                throw new WebException(ReasonPhrase, WebExceptionStatus.UnknownError);

            ResponseMessage?.EnsureSuccessStatusCode();
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

            ResponseMessage?.Dispose();
            RequestMessage.Dispose();
        }
    }
}
