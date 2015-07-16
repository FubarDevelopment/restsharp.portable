using System;
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
        private readonly IHttpHeaders _defaultHeaders;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpClient"/> class.
        /// </summary>
        /// <param name="defaultHttpHeaders">The default HTTP headers</param>
        public DefaultHttpClient([NotNull] IHttpHeaders defaultHttpHeaders)
        {
            Timeout = TimeSpan.FromSeconds(100);
            _defaultHeaders = defaultHttpHeaders;
        }

        /// <summary>
        /// Gets or sets the base address of the HTTP client
        /// </summary>
        public Uri BaseAddress { get; set; }

        /// <summary>
        /// Gets the default request headers
        /// </summary>
        public IHttpHeaders DefaultRequestHeaders
        {
            get { return _defaultHeaders; }
        }

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
            var wr = System.Net.WebRequest.CreateHttp(uri);
            if (wr.SupportsCookieContainer && CookieContainer != null)
                wr.CookieContainer = CookieContainer;
            if (Credentials != null)
                wr.Credentials = Credentials;
            wr.Method = request.Method.ToString();
#if !PCL
            wr.Proxy = Proxy ?? System.Net.WebRequest.DefaultWebProxy;
#endif

            // Combine all headers into one header collection
            var headers = new GenericHttpHeaders();

            if (request.Content != null && request.Content.Headers != null)
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
                var value = string.Join(", ", header.Value);
                if (string.Equals(header.Key, "content-type", StringComparison.OrdinalIgnoreCase))
                {
                    wr.ContentType = value;
                }
                else if (string.Equals(header.Key, "accept", StringComparison.OrdinalIgnoreCase))
                {
                    wr.Accept = value;
                }
                else
                {
                    if (string.Equals(header.Key, "content-length", StringComparison.OrdinalIgnoreCase))
                    {
                        hasContentLength = true;
#if PCL
                        wr.Headers[HttpRequestHeader.ContentLength] = value;
#endif
                    }
                    else
                    {
                        wr.Headers[header.Key] = value;
                    }
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
#if PCL
                        wr.Headers[HttpRequestHeader.ContentLength] = contentLength.ToString();
#else
                        wr.ContentLength = contentLength;
#endif
                    }
                }

                try
                {
                    var requestStream = await wr.GetRequestStreamAsync().HandleCancellation(cancellationToken);
                    var temp = new System.IO.MemoryStream();
                    await request.Content.CopyToAsync(temp);
                    temp.Position = 0;
                    await temp.CopyToAsync(requestStream);
                }
                catch (OperationCanceledException)
                {
                    wr.Abort();
                    throw;
                }
            }

            try
            {
                var response = await wr.GetResponseAsync().HandleCancellation(cancellationToken);
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
    }
}
