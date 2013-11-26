using RestSharp.Portable.Authenticators;
using RestSharp.Portable.Deserializers;
using RestSharp.Portable.Encodings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// REST client
    /// </summary>
    public interface IRestClient
    {
        /// <summary>
        /// Authenticator to use for all requests
        /// </summary>
        IAuthenticator Authenticator { get; set; }

        /// <summary>
        /// Base URL for all requests
        /// </summary>
        Uri BaseUrl { get; set; }

        /// <summary>
        /// Default parameters for all requests
        /// </summary>
        IList<Parameter> DefaultParameters { get; }

        /// <summary>
        /// Cookies for all requests
        /// </summary>
        /// <remarks>
        /// Cookies set by the server will be collected here.
        /// </remarks>
        CookieContainer CookieContainer { get; set; }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned</returns>
        Task<IRestResponse> Execute(IRestRequest request);
        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned, with a deserialized object</returns>
        Task<IRestResponse<T>> Execute<T>(IRestRequest request);

        /// <summary>
        /// Add a new content type handler
        /// </summary>
        /// <param name="contentType">The Accept header value</param>
        /// <param name="deserializer">The deserializer to decode the content</param>
        /// <returns>The client itself, to allow call chains</returns>
        IRestClient AddHandler(string contentType, IDeserializer deserializer);
        /// <summary>
        /// Remove a previously added content type handler
        /// </summary>
        /// <param name="contentType">The Accept header value that identifies the handler</param>
        /// <returns>The client itself, to allow call chains</returns>
        IRestClient RemoveHandler(string contentType);
        /// <summary>
        /// Remove all previously added content type handlers
        /// </summary>
        /// <returns>The client itself, to allow call chains</returns>
        IRestClient ClearHandlers();
        /// <summary>
        /// Get a previously added content type handler
        /// </summary>
        /// <param name="contentType">The Accept header value that identifies the handler</param>
        /// <returns>The deserializer that can handle the given content type.</returns>
        /// <remarks>
        /// This function returns NULL if the handler for the given content type cannot be found.
        /// </remarks>
        IDeserializer GetHandler(string contentType);

        /// <summary>
        /// Add a new content encoding handler
        /// </summary>
        /// <param name="encodingId">The Accept-Encoding header value</param>
        /// <param name="encoding">The encoding engine to decode the content</param>
        /// <returns>The client itself, to allow call chains</returns>
        IRestClient AddEncoding(string encodingId, IEncoding encoding);
        /// <summary>
        /// Remove a previously added content encoding handler
        /// </summary>
        /// <param name="encodingId">The Accept-Encoding header value that identifies the handler</param>
        /// <returns>The client itself, to allow call chains</returns>
        IRestClient RemoveEncoding(string encodingId);
        /// <summary>
        /// Remove all previously added content encoding handlers
        /// </summary>
        /// <returns>The client itself, to allow call chains</returns>
        IRestClient ClearEncodings();
        /// <summary>
        /// Get a previously added content encoding handler
        /// </summary>
        /// <param name="encodingIds">The Accept-Encoding header value that identifies the handler</param>
        /// <returns>The handler that can decode the given content encoding.</returns>
        /// <remarks>
        /// This function returns NULL if the handler for the given content encoding cannot be found.
        /// </remarks>
        IEncoding GetEncoding(IEnumerable<string> encodingIds);

        /// <summary>
        /// Proxy to use for the requests
        /// </summary>
        IWebProxy Proxy { get; set; }

        /// <summary>
        /// Ignore the response status code?
        /// </summary>
        bool IgnoreResponseStatusCode { get; set; }
    }
}
