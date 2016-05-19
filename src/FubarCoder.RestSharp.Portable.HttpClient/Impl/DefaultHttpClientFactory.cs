using System;
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

            httpClient = AddHttpHeaderParameters(httpClient, client);

            var timeout = client.Timeout;
            if (timeout.HasValue)
            {
                httpClient.Timeout = timeout.Value;
            }

            return new DefaultHttpClient(httpClient);
        }

        /// <summary>
        /// Create the request message
        /// </summary>
        /// <param name="client">The REST client that wants to create the HTTP request message</param>
        /// <param name="request">The REST request for which the HTTP request message is created</param>
        /// <returns>A new HttpRequestMessage object</returns>
        /// <remarks>
        /// The DefaultHttpClientFactory contains some helpful protected methods that helps gathering
        /// the data required for a proper configuration of the HttpClient.
        /// </remarks>
        public virtual IHttpRequestMessage CreateRequestMessage(IRestClient client, IRestRequest request)
        {
            var address = GetMessageAddress(client, request);
            var method = GetHttpMethod(client, request).ToHttpMethod();
            var message = new HttpRequestMessage(method, address);
            message = AddHttpHeaderParameters(message, request);
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
        /// <returns>The modified HTTP request message</returns>
        protected virtual HttpRequestMessage AddHttpHeaderParameters(HttpRequestMessage message, IRestRequest request)
        {
            foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.HttpHeader && !x.IsContentParameter()))
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
        /// Returns a modified HTTP client object with the default HTTP header parameters
        /// </summary>
        /// <param name="httpClient">HTTP client</param>
        /// <param name="restClient">REST client</param>
        /// <returns>The modified HTTP request message</returns>
        protected virtual System.Net.Http.HttpClient AddHttpHeaderParameters(System.Net.Http.HttpClient httpClient, IRestClient restClient)
        {
            foreach (var param in restClient.DefaultParameters.Where(x => x.Type == ParameterType.HttpHeader))
            {
                if (httpClient.DefaultRequestHeaders.Contains(param.Name))
                {
                    httpClient.DefaultRequestHeaders.Remove(param.Name);
                }

                var paramValue = param.ToRequestString();
                if (param.ValidateOnAdd)
                {
                    httpClient.DefaultRequestHeaders.Add(param.Name, paramValue);
                }
                else
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(param.Name, paramValue);
                }
            }

            return httpClient;
        }

        /// <summary>
        /// Create the message handler
        /// </summary>
        /// <param name="client">The REST client that wants to create the HTTP client</param>
        /// <returns>A new HttpMessageHandler object</returns>
        protected virtual HttpMessageHandler CreateMessageHandler(IRestClient client)
        {
            var handler = new HttpClientHandler();

#if !PROFILE259
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
