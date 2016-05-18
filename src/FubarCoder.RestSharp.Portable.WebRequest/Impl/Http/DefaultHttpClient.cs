using System;
using System.Collections.Generic;
#if !PCL && !NETFX_CORE && !WINDOWS_STORE
using System.Globalization;
#endif
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using RestSharp.Portable.Impl;

namespace RestSharp.Portable.WebRequest.Impl.Http
{
    /// <summary>
    /// Wraps a <see cref="System.Net.WebRequest"/> instance as <see cref="IHttpClient"/>.
    /// </summary>
    internal class DefaultHttpClient : IHttpClient
    {
        private static readonly IDictionary<string, Action<HttpWebRequest, string>> _valueToWebRequest =
            new Dictionary<string, Action<HttpWebRequest, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["content-type"] = (wr, value) => wr.ContentType = value,
                ["accept"] = (wr, value) => wr.Accept = value,
#if !PCL && !NETFX_CORE && !WINDOWS_STORE
                ["content-length"] = (wr, value) => wr.ContentLength = Convert.ToInt64(value, 10),
                ["user-agent"] = (wr, value) => wr.UserAgent = value,
                ["date"] = (wr, value) => wr.Date = DateTime.ParseExact(value, "R", CultureInfo.InvariantCulture),
                ["expect"] = (wr, value) => wr.Expect = value,
                ["If-Modified-Since"] = (wr, value) => wr.IfModifiedSince = DateTime.ParseExact(value, "R", CultureInfo.InvariantCulture),
                ["Referer"] = (wr, value) => wr.Referer = value,
                ["Transfer-Encoding"] = (wr, value) => wr.TransferEncoding = value,
#endif
            };

        private readonly WebRequestHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpClient"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create this <see cref="IHttpClient"/> implementation</param>
        /// <param name="defaultHttpHeaders">The default HTTP headers</param>
        public DefaultHttpClient([NotNull] WebRequestHttpClientFactory httpClientFactory, [NotNull] IHttpHeaders defaultHttpHeaders)
        {
            _httpClientFactory = httpClientFactory;
            Timeout = TimeSpan.FromSeconds(100);
            DefaultRequestHeaders = defaultHttpHeaders;
        }

        /// <summary>
        /// Gets or sets the base address of the HTTP client
        /// </summary>
        public Uri BaseAddress { get; set; }

        /// <summary>
        /// Gets the default request headers
        /// </summary>
        public IHttpHeaders DefaultRequestHeaders { get; }

        /// <summary>
        /// Gets or sets the timeout
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets the proxy to use for the client
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Gets or sets the cookie container that will hold all cookies
        /// </summary>
        public CookieContainer CookieContainer { get; set; }

        /// <summary>
        /// Gets or sets the credentials to be used for the proxy and/or the requested web site
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Asynchronously send a request
        /// </summary>
        /// <param name="request">The request do send</param>
        /// <param name="cancellationToken">The cancellation token used to signal an abortion</param>
        /// <returns>The task to query the response</returns>
        public async Task<IHttpResponseMessage> SendAsync(IHttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = new Uri(BaseAddress, request.RequestUri);
            var wr = _httpClientFactory.CreateWebRequest(uri);
            if (wr.SupportsCookieContainer && CookieContainer != null)
            {
                wr.CookieContainer = CookieContainer;
            }

            if (Credentials != null)
            {
                wr.Credentials = Credentials;
            }

            wr.Method = request.Method.ToString();
#if !PCL
            wr.Proxy = Proxy ?? System.Net.WebRequest.DefaultWebProxy;
#endif

            // Combine all headers into one header collection
            var headers = new GenericHttpHeaders();

            if (request.Content?.Headers != null)
            {
                foreach (var header in request.Content.Headers.Where(x => !headers.Contains(x.Key)))
                {
                    headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (request.Headers != null)
            {
                foreach (var header in request.Headers.Where(x => !headers.Contains(x.Key)))
                {
                    headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (DefaultRequestHeaders != null)
            {
                foreach (var header in DefaultRequestHeaders.Where(x => !headers.Contains(x.Key)))
                {
                    headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            bool hasContentLength = false;
            foreach (var header in headers)
            {
                var value = string.Join(",", header.Value);
                SetWebRequestHeaderValue(wr, header.Key, value);
                if (!hasContentLength && string.Equals(header.Key, "content-length", StringComparison.OrdinalIgnoreCase))
                {
                    hasContentLength = true;
                }
            }

            if (request.Content != null)
            {
                // Add content length if not provided by the user.
                if (!hasContentLength)
                {
                    long contentLength;
                    if (request.Content.TryComputeLength(out contentLength))
                    {
#if PCL || NETFX_CORE || WINDOWS_STORE
                        wr.Headers[HttpRequestHeader.ContentLength] = contentLength.ToString();
#else
                        wr.ContentLength = contentLength;
#endif
                    }
                }

                try
                {
#if PCL && ASYNC_PCL
                    var getRequestStreamAsync = Task.Factory.FromAsync<Stream>(wr.BeginGetRequestStream, wr.EndGetRequestStream, null);
                    var requestStream = await getRequestStreamAsync.HandleCancellation(cancellationToken);
#else
                    var requestStream = await wr.GetRequestStreamAsync().HandleCancellation(cancellationToken);
#endif
                    using (requestStream)
                    {
                        var temp = new MemoryStream();
                        await request.Content.CopyToAsync(temp);
                        var buffer = temp.ToArray();
                        await requestStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                        await requestStream.FlushAsync(cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    wr.Abort();
                    throw;
                }
            }

            try
            {
#if PCL && ASYNC_PCL
                var getResponseAsync = Task.Factory.FromAsync<WebResponse>(wr.BeginGetResponse, wr.EndGetResponse, null);
                var response = await getResponseAsync.HandleCancellation(cancellationToken);
#else
                var response = await wr.GetResponseAsync().HandleCancellation(cancellationToken);
#endif
                var httpWebResponse = response as HttpWebResponse;
                if (httpWebResponse == null)
                {
                    response.Dispose();
                    throw new ProtocolViolationException("No HTTP request")
                        {
                            Data =
                                {
                                    { "URI", wr.RequestUri },
                                },
                        };
                }

                return new DefaultHttpResponseMessage(request, httpWebResponse);
            }
            catch (WebException ex)
            {
                var httpWebResponse = (HttpWebResponse)ex.Response;
                return new DefaultHttpResponseMessage(request, httpWebResponse, ex);
            }
            catch (OperationCanceledException)
            {
                wr.Abort();
                throw;
            }
        }

        /// <summary>
        /// Disposes the underlying HTTP client
        /// </summary>
        void IDisposable.Dispose()
        {
        }

        private static void SetWebRequestHeaderValue(HttpWebRequest wr, string key, string value)
        {
            Action<HttpWebRequest, string> setAction;
            if (_valueToWebRequest.TryGetValue(key, out setAction))
            {
                setAction(wr, value);
            }
            else
            {
                wr.Headers[key] = value;
            }
        }
    }
}
