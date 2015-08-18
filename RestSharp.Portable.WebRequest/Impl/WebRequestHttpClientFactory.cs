using System;
using System.Linq;
using System.Net;

using RestSharp.Portable.Impl;
using RestSharp.Portable.WebRequest.Impl.Http;

namespace RestSharp.Portable.WebRequest.Impl
{
    /// <summary>
    /// The default HTTP client factory
    /// </summary>
    /// <remarks>
    /// Any other implementation should derive from this class, because it contains several
    /// useful utility functions for the creation of a HTTP client and request message.
    /// </remarks>
    public class WebRequestHttpClientFactory : IHttpClientFactory
    {
        private readonly bool _setCredentials;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequestHttpClientFactory" /> class.
        /// </summary>
        public WebRequestHttpClientFactory()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequestHttpClientFactory"/> class.
        /// </summary>
        /// <param name="setCredentials">Set the credentials for the native <see cref="System.Net.WebRequest"/>?</param>
        public WebRequestHttpClientFactory(bool setCredentials)
        {
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
            var headers = new GenericHttpHeaders();
            AddHttpHeaderParameters(headers, client);
            var httpClient = new DefaultHttpClient(headers)
                {
                    BaseAddress = GetBaseAddress(client),
                };
            if (client.Timeout.HasValue)
                httpClient.Timeout = client.Timeout.Value;

            var proxy = GetProxy(client);
            if (proxy != null)
                httpClient.Proxy = new RequestProxyWrapper(proxy);

            var cookieContainer = GetCookies(client, request);
            if (cookieContainer != null)
                httpClient.CookieContainer = cookieContainer;

            if (_setCredentials)
            {
                var credentials = client.Credentials;
                if (credentials != null)
                    httpClient.Credentials = credentials;
            }

            return httpClient;
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
            var method = GetHttpMethod(client, request);
            var headers = new GenericHttpHeaders();
            AddHttpHeaderParameters(headers, request);
            return new DefaultHttpRequestMessage(method, address, headers, null);
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
                return null;
            var baseUrl = GetBaseAddress(client);
            var newCookies = client.CookieContainer = client.CookieContainer ?? new CookieContainer();
            var oldCookies = client.CookieContainer.GetCookies(baseUrl)
                .Cast<Cookie>().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var cookieParameter in request.Parameters.Where(x => x.Type == ParameterType.Cookie && !oldCookies.ContainsKey(x.Name)))
                newCookies.Add(baseUrl, new Cookie(cookieParameter.Name, cookieParameter.ToRequestString()));
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
        /// <param name="httpHeaders">HTTP headers</param>
        /// <param name="request">REST request</param>
        protected virtual void AddHttpHeaderParameters(IHttpHeaders httpHeaders, IRestRequest request)
        {
            foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.HttpHeader))
            {
                if (httpHeaders.Contains(param.Name))
                    httpHeaders.Remove(param.Name);
                var paramValue = param.ToRequestString();
                if (param.ValidateOnAdd)
                {
                    httpHeaders.Add(param.Name, paramValue);
                }
                else
                {
                    httpHeaders.TryAddWithoutValidation(param.Name, paramValue);
                }
            }
        }

        /// <summary>
        /// Returns a modified HTTP client object with the default HTTP header parameters
        /// </summary>
        /// <param name="httpHeaders">HTTP headers</param>
        /// <param name="restClient">REST client</param>
        protected virtual void AddHttpHeaderParameters(IHttpHeaders httpHeaders, IRestClient restClient)
        {
            foreach (var param in restClient.DefaultParameters.Where(x => x.Type == ParameterType.HttpHeader))
            {
                if (httpHeaders.Contains(param.Name))
                    httpHeaders.Remove(param.Name);
                var paramValue = param.ToRequestString();
                if (param.ValidateOnAdd)
                {
                    httpHeaders.Add(param.Name, paramValue);
                }
                else
                {
                    httpHeaders.TryAddWithoutValidation(param.Name, paramValue);
                }
            }
        }
    }
}
