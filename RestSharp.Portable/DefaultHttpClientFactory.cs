using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace RestSharp.Portable.HttpClientImpl
{
    /// <summary>
    /// The default HTTP client factory
    /// </summary>
    /// <remarks>
    /// Any other implementation should derive from this class, because it contains serveral
    /// useful utility functions for the creation of a HTTP client and request message.
    /// </remarks>
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
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
            return client.BaseUrl;
        }

        /// <summary>
        /// Get the REST requests relative address (for HTTP request message)
        /// </summary>
        /// <param name="client">REST client</param>
        /// <param name="request">REST request</param>
        /// <returns>The relative request message URL</returns>
        protected virtual Uri GetMessageAddress(IRestClient client, IRestRequest request)
        {
            var fullUrl = client.BuildUrl(request);
            var url = client.BaseUrl.MakeRelativeUri(fullUrl);
            return url;
        }

        /// <summary>
        /// The proxy to be used by the HTTP client
        /// </summary>
        /// <param name="client">REST client</param>
        /// <returns>Proxy object or null</returns>
        protected virtual IWebProxy GetProxy(IRestClient client)
        {
            return client.Proxy;
        }

        /// <summary>
        /// Get the credentials required for the REST request
        /// </summary>
        /// <param name="request">REST request</param>
        /// <returns>Credentials for the HTTP client or null</returns>
        protected virtual ICredentials GetCredentials(IRestRequest request)
        {
            return request.Credentials;
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
            var newCookies = client.CookieContainer = (client.CookieContainer ?? new CookieContainer());
            var oldCookies = client.CookieContainer.GetCookies(baseUrl)
                .Cast<Cookie>().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var cookieParameter in request.Parameters.Where(x => x.Type == ParameterType.Cookie && !oldCookies.ContainsKey(x.Name)))
                newCookies.Add(baseUrl, new Cookie(cookieParameter.Name, string.Format("{0}", cookieParameter.Value)));
            return newCookies;
        }

        /// <summary>
        /// Returns the HTTP method for the request message.
        /// </summary>
        /// <param name="request">REST request</param>
        /// <returns>HTTP method</returns>
        protected virtual HttpMethod GetHttpMethod(IRestRequest request)
        {
            return request.GetEffectiveHttpMethod();
        }

        /// <summary>
        /// Returns a modified HTTP request message object with HTTP header parameters
        /// </summary>
        /// <param name="message">HTTP request message</param>
        /// <param name="request">REST request</param>
        /// <returns>The modified HTTP request message</returns>
        protected virtual HttpRequestMessage AddHttpHeaderParameters(HttpRequestMessage message, IRestRequest request)
        {
            foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.HttpHeader))
            {
                if (message.Headers.Contains(param.Name))
                    message.Headers.Remove(param.Name);
                var paramValue = string.Format("{0}", param.Value);
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
        /// Create the client handler
        /// </summary>
        /// <param name="client">The REST client that wants to create the HTTP client</param>
        /// <param name="request">The REST request for which the HTTP client is created</param>
        /// <returns>A new HttpClientHandler object</returns>
        protected virtual HttpClientHandler CreateClientHandler(IRestClient client, IRestRequest request)
        {
            var handler = new HttpClientHandler();

            var proxy = GetProxy(client);
            if (handler.SupportsProxy && proxy != null)
                handler.Proxy = proxy;

            var cookieContainer = GetCookies(client, request);
            if (cookieContainer != null)
            {
                handler.UseCookies = true;
                handler.CookieContainer = cookieContainer;
            }

            var credentials = GetCredentials(request);
            if (credentials != null)
                handler.Credentials = credentials;

            //if (handler.SupportsAutomaticDecompression)
            //    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            return handler;
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
        public virtual HttpClient CreateClient(IRestClient client, IRestRequest request)
        {
            HttpClient httpClient;

            var handler = CreateClientHandler(client, request);

            httpClient = new HttpClient(handler, true);
            httpClient.BaseAddress = GetBaseAddress(client);
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
        public virtual HttpRequestMessage CreateRequestMessage(IRestClient client, IRestRequest request)
        {
            var address = GetMessageAddress(client, request);
            var method = GetHttpMethod(request);
            var message = new HttpRequestMessage(method, address);
            message = AddHttpHeaderParameters(message, request);
            return message;
        }
    }
}
