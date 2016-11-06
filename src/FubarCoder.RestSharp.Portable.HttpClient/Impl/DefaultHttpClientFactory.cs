using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

using RestSharp.Portable.HttpClient.Impl.Http;

namespace RestSharp.Portable.HttpClient.Impl
{
    /// <summary>
    /// The default HTTP client factory
    /// </summary>
    /// <remarks>
    /// Any other implementation should derive from this class, because it contains several
    /// useful utility functions for the creation of a HTTP client and request message.
    /// </remarks>
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
        private readonly bool _setCredentials;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpClientFactory" /> class.
        /// </summary>
        public DefaultHttpClientFactory()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpClientFactory"/> class.
        /// </summary>
        /// <param name="setCredentials">Set the credentials for the native <see cref="HttpMessageHandler"/> (<see cref="HttpClientHandler"/>)?</param>
        public DefaultHttpClientFactory(bool setCredentials)
        {
            _setCredentials = setCredentials;
        }

        /// <summary>
        /// Create the client
        /// </summary>
        /// <param name="client">The REST client that wants to create the HTTP client</param>
        /// <returns>A new HttpClient object</returns>
        /// <remarks>
        /// The DefaultHttpClientFactory contains some helpful protected methods that helps gathering
        /// the data required for a proper configuration of the HttpClient.
        /// </remarks>
        public virtual IHttpClient CreateClient(IRestClient client)
        {
            var handler = CreateMessageHandler(client);

            var httpClient = new System.Net.Http.HttpClient(handler, true)
            {
                BaseAddress = GetBaseAddress(client)
            };

            var timeout = client.Timeout;
            if (timeout.HasValue)
            {
                httpClient.Timeout = timeout.Value;
            }

            return new DefaultHttpClient(httpClient, client.CookieContainer);
        }

        /// <summary>
        /// Create the request message
        /// </summary>
        /// <param name="client">The REST client that wants to create the HTTP request message</param>
        /// <param name="request">The REST request for which the HTTP request message is created</param>
        /// <param name="parameters">The request parameters for the REST request except the content header parameters (read-only)</param>
        /// <returns>A new HttpRequestMessage object</returns>
        /// <remarks>
        /// The DefaultHttpClientFactory contains some helpful protected methods that helps gathering
        /// the data required for a proper configuration of the HttpClient.
        /// </remarks>
        public virtual IHttpRequestMessage CreateRequestMessage(IRestClient client, IRestRequest request, IList<Parameter> parameters)
        {
            var address = GetMessageAddress(client, request);
            var method = GetHttpMethod(client, request).ToHttpMethod();
            var message = new HttpRequestMessage(method, address);
            message = AddHttpHeaderParameters(message, request, parameters);
            return new DefaultHttpRequestMessage(message);
        }

        /// <summary>
        /// Get the REST requests base address (for HTTP client)
        /// </summary>
        /// <param name="client">REST client</param>
        /// <returns>The base URL</returns>
        protected virtual Uri GetBaseAddress(IRestClient client)
        {
            return client.BuildUri(null, false);
        }

        /// <summary>
        /// Get the REST requests relative address (for HTTP request message)
        /// </summary>
        /// <param name="client">REST client</param>
        /// <param name="request">REST request</param>
        /// <returns>The relative request message URL</returns>
        protected virtual Uri GetMessageAddress(IRestClient client, IRestRequest request)
        {
            var fullUrl = client.BuildUri(request);
            var url = client.BuildUri(null, false).MakeRelativeUri(fullUrl);
            return url;
        }

        /// <summary>
        /// Returns the HTTP method for the request message.
        /// </summary>
        /// <param name="client">The REST client that wants to create the HTTP client</param>
        /// <param name="request">REST request</param>
        /// <returns>HTTP method</returns>
        protected virtual Method GetHttpMethod(IRestClient client, IRestRequest request)
        {
            return client.GetEffectiveHttpMethod(request);
        }

        /// <summary>
        /// Returns a modified HTTP request message object with HTTP header parameters
        /// </summary>
        /// <param name="message">HTTP request message</param>
        /// <param name="request">REST request</param>
        /// <param name="parameters">The request parameters for the REST request except the content header parameters (read-only)</param>
        /// <returns>The modified HTTP request message</returns>
        protected virtual HttpRequestMessage AddHttpHeaderParameters(HttpRequestMessage message, IRestRequest request, IList<Parameter> parameters)
        {
            foreach (var param in parameters.Where(x => x.Type == ParameterType.HttpHeader))
            {
                if (message.Headers.Contains(param.Name))
                {
                    message.Headers.Remove(param.Name);
                }

                var paramValue = param.ToRequestString();
                if (param.ValidateOnAdd)
                {
                    message.Headers.Add(param.Name, paramValue);
                }
                else
                {
                    message.Headers.TryAddWithoutValidation(param.Name, paramValue);
                }
            }

            return message;
        }

        /// <summary>
        /// Create the message handler
        /// </summary>
        /// <param name="client">The REST client that wants to create the HTTP client</param>
        /// <returns>A new HttpMessageHandler object</returns>
        protected virtual HttpMessageHandler CreateMessageHandler(IRestClient client)
        {
            var handler = new HttpClientHandler();

#if !NO_PROXY
            if (handler.SupportsProxy && client.Proxy != null)
            {
                handler.Proxy = client.Proxy;
            }
#endif

            if (client.CookieContainer != null)
            {
                handler.UseCookies = true;
                handler.CookieContainer = client.CookieContainer;
            }

            if (_setCredentials)
            {
                var credentials = client.Credentials;
                if (credentials != null)
                {
                    handler.Credentials = credentials;
                }
            }

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }

            return handler;
        }
    }
}
