﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// REST client
    /// </summary>
    public interface IRestClient : IDisposable
    {
        /// <summary>
        /// Gets or sets the authenticator to use for all requests
        /// </summary>
        IAuthenticator Authenticator { get; set; }

        /// <summary>
        /// Gets or sets the base URL for all requests
        /// </summary>
        Uri BaseUrl { get; set; }

        /// <summary>
        /// Gets the default parameters for all requests
        /// </summary>
        IParameterCollection DefaultParameters { get; }

        /// <summary>
        /// Gets or sets the cookies for all requests
        /// </summary>
        /// <remarks>
        /// Cookies set by the server will be collected here.
        /// </remarks>
        CookieContainer CookieContainer { get; set; }

#if !NO_PROXY
        /// <summary>
        /// Gets or sets a proxy to use for the requests
        /// </summary>
        IWebProxy Proxy { get; set; }
#endif

        /// <summary>
        /// Gets or sets the credentials used for the request (e.g. NTLM authentication)
        /// </summary>
        ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the response status code should be ignored?
        /// </summary>
        bool IgnoreResponseStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the timeout to be used for requests.
        /// </summary>
        /// <remarks>
        /// When the value isn't set, it uses the default timeout of the underlying HTTP client (100 seconds) or whatever
        /// is used to execute the HTTP requests.
        /// </remarks>
        TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Gets or sets the user agent for the REST client
        /// </summary>
        /// <remarks>
        /// The default value is "RestSharp/{version}"
        /// </remarks>
        string UserAgent { get; set; }

        /// <summary>
        /// Gets the dictionary that maps the content type to its handler
        /// </summary>
        IDictionary<string, IDeserializer> ContentHandlers { get; }

        /// <summary>
        /// Gets the dictionary that maps the encoding to its handler
        /// </summary>
        IDictionary<string, IEncoding> EncodingHandlers { get; }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned</returns>
        Task<IRestResponse> Execute(IRestRequest request);

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned, with a deserialized object</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "No other solution for async/await API")]
        Task<IRestResponse<T>> Execute<T>(IRestRequest request);

        /// <summary>
        /// Cancellable request execution
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>Response returned</returns>
        Task<IRestResponse> Execute(IRestRequest request, CancellationToken ct);

        /// <summary>
        /// Cancellable request execution
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="request">Request to execute</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>Response returned, with a deserialized object</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "No other solution for async/await API")]
        Task<IRestResponse<T>> Execute<T>(IRestRequest request, CancellationToken ct);

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
        /// Get a previously added content encoding handler
        /// </summary>
        /// <param name="encodingIds">The Accept-Encoding header value that identifies the handler</param>
        /// <returns>The handler that can decode the given content encoding.</returns>
        /// <remarks>
        /// This function returns NULL if the handler for the given content encoding cannot be found.
        /// </remarks>
        IEncoding GetEncoding(IEnumerable<string> encodingIds);

        /// <summary>
        /// Add a new content type handler
        /// </summary>
        /// <param name="contentType">The Accept header value</param>
        /// <param name="deserializer">The deserializer to decode the content</param>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the ContentHandlers property instead")]
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "AddHandler", Justification = "Required for RestSharp compatibility")]
        IRestClient AddHandler(string contentType, IDeserializer deserializer);

        /// <summary>
        /// Remove a previously added content type handler
        /// </summary>
        /// <param name="contentType">The Accept header value that identifies the handler</param>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the ContentHandlers property instead")]
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "RemoveHandler", Justification = "Required for RestSharp compatibility")]
        IRestClient RemoveHandler(string contentType);

        /// <summary>
        /// Remove all previously added content type handlers
        /// </summary>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the ContentHandlers property instead")]
        IRestClient ClearHandlers();

        /// <summary>
        /// Add a new content encoding handler
        /// </summary>
        /// <param name="encodingId">The Accept-Encoding header value</param>
        /// <param name="encoding">The encoding engine to decode the content</param>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the EncodingHandlers property instead")]
        IRestClient AddEncoding(string encodingId, IEncoding encoding);

        /// <summary>
        /// Remove a previously added content encoding handler
        /// </summary>
        /// <param name="encodingId">The Accept-Encoding header value that identifies the handler</param>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the EncodingHandlers property instead")]
        IRestClient RemoveEncoding(string encodingId);

        /// <summary>
        /// Remove all previously added content encoding handlers
        /// </summary>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the EncodingHandlers property instead")]
        IRestClient ClearEncodings();
    }
}