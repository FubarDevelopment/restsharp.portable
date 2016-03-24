using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

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
        private readonly PropertyInfo _proxyProperty;

        private readonly FieldInfo _proxyField;

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
#if USE_TYPEINFO
            _proxyProperty = typeof(HttpClientHandler).GetTypeInfo().GetDeclaredProperty("Proxy");
#else
            _proxyProperty = typeof(HttpClientHandler).GetProperty("Proxy");
#endif
            if (_proxyProperty == null)
            {
#if USE_TYPEINFO
                var proxyField = typeof(HttpClientHandler).GetTypeInfo().DeclaredFields.FirstOrDefault(x => x.Name == "proxy" && x.IsPrivate);
#else
                var proxyField = typeof(HttpClientHandler).GetField("proxy", BindingFlags.NonPublic | BindingFlags.Instance);
#endif
                if (proxyField != null && proxyField.FieldType == typeof(IWebProxy) && !proxyField.IsInitOnly)
                    _proxyField = proxyField;
            }
            _setCredentials = setCredentials;
        }

        /// <summary>
        /// Create the client
        /// </summary>
        /// <param name="client">The REST client that wants to create the HTTP client</param>
        /// <param name="request">The REST request for which the HTTP client is created</param>
        /// <returns>A new HttpClient object</returns>
        /// <remarks>
        /// The DefaultHttpClientFactory contains some helpful protected methods that helps gathering
        /// the data required for a proper configuration of the HttpClient.
        /// </remarks>
        public virtual IHttpClient CreateClient(IRestClient client, IRestRequest request)
        {
            var handler = CreateMessageHandler(client, request);

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
        /// Returns if the HTTP client should be aware of cookies
        /// </summary>
        /// <param name="client">REST client</param>
        /// <param name="request">REST request</param>
        /// <returns>true == HTTP client should use a cookie container</returns>
        protected virtual bool HasCookies(IRestClient client, IRestRequest request)
        {
            return client.CookieContainer != null || request.Parameters.Any(x => x.Type == ParameterType.Cookie);
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
        /// The proxy to be used by the HTTP client
        /// </summary>
        /// <param name="client">REST client</param>
        /// <returns>Proxy object or null</returns>
        protected virtual IRequestProxy GetProxy(IRestClient client)
        {
            return client.Proxy;
        }

        /// <summary>
        /// Get the cookies for the HTTP client
        /// </summary>
        /// <param name="client">REST client</param>
        /// <param name="request">REST request</param>
        /// <returns>The cookie container or null</returns>
        protected virtual CookieContainer GetCookies(IRestClient client, IRestRequest request)
        {
            if (!HasCookies(client, request))
            {
                return null;
            }

            var baseUrl = GetBaseAddress(client);
            var newCookies = client.CookieContainer = client.CookieContainer ?? new CookieContainer();
            var oldCookies = client.CookieContainer.GetCookies(baseUrl)
                .Cast<Cookie>().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var cookieParameter in request.Parameters.Where(x => x.Type == ParameterType.Cookie && !oldCookies.ContainsKey(x.Name)))
            {
                newCookies.Add(baseUrl, new Cookie(cookieParameter.Name, cookieParameter.ToRequestString()));
            }

            return newCookies;
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
        /// <param name="request">The REST request for which the HTTP client is created</param>
        /// <returns>A new HttpMessageHandler object</returns>
        protected virtual HttpMessageHandler CreateMessageHandler(IRestClient client, IRestRequest request)
        {
            var handler = new HttpClientHandler();

            var proxy = GetProxy(client);
            if (handler.SupportsProxy && proxy != null)
            {
                if (_proxyProperty != null)
                {
                    _proxyProperty.SetValue(handler, new RequestProxyWrapper(proxy), null);
                }
                else if (_proxyField != null)
                {
                    _proxyField.SetValue(handler, new RequestProxyWrapper(proxy));
                }
            }

            var cookieContainer = GetCookies(client, request);
            if (cookieContainer != null)
            {
                handler.UseCookies = true;
                handler.CookieContainer = cookieContainer;
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
