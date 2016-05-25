using System;
using System.Collections.Generic;
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
        /// <returns>A new HttpClient object</returns>
        /// <remarks>
        /// The DefaultHttpClientFactory contains some helpful protected methods that helps gathering
        /// the data required for a proper configuration of the HttpClient.
        /// </remarks>
        public virtual IHttpClient CreateClient(IRestClient client)
        {
            var headers = new GenericHttpHeaders();
            var httpClient = new DefaultHttpClient(this, headers)
            {
                BaseAddress = GetBaseAddress(client)
            };
            if (client.Timeout.HasValue)
            {
                httpClient.Timeout = client.Timeout.Value;
            }

#if !NO_PROXY
            if (client.Proxy != null)
            {
                httpClient.Proxy = client.Proxy;
            }
#endif

            if (client.CookieContainer != null)
            {
                httpClient.CookieContainer = client.CookieContainer;
            }

            if (_setCredentials)
            {
                var credentials = client.Credentials;
                if (credentials != null)
                {
                    httpClient.Credentials = credentials;
                }
            }

            return httpClient;
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
            var method = GetHttpMethod(client, request);
            var headers = new GenericHttpHeaders();
            AddHttpHeaderParameters(headers, request, parameters);
            return new DefaultHttpRequestMessage(method, address, headers, null);
        }

        /// <summary>
        /// Creates a <see cref="HttpWebRequest"/> using the given <paramref name="url"/>
        /// </summary>
        /// <param name="url">The <see cref="Uri"/> to initialize the <see cref="HttpWebRequest"/> with</param>
        /// <returns>The new <see cref="HttpWebRequest"/></returns>
        protected internal virtual HttpWebRequest CreateWebRequest(Uri url)
        {
            var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url);
#if NET40
            webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
#endif
            return webRequest;
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
        /// <param name="httpHeaders">HTTP headers</param>
        /// <param name="request">REST request</param>
        /// <param name="parameters">The request parameters for the REST request except the content header parameters (read-only)</param>
        protected virtual void AddHttpHeaderParameters(IHttpHeaders httpHeaders, IRestRequest request, IList<Parameter> parameters)
        {
            foreach (var param in parameters.Where(x => x.Type == ParameterType.HttpHeader))
            {
                if (httpHeaders.Contains(param.Name))
                {
                    httpHeaders.Remove(param.Name);
                }

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
