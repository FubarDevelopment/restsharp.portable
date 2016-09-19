using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using RestSharp.Portable.Collections;
using RestSharp.Portable.Deserializers;

namespace RestSharp.Portable
{
    /// <summary>
    /// The default REST client
    /// </summary>
    public abstract class RestClientBase : IRestClient
    {
        private static readonly string s_defaultUserAgent = GetDefaultVersion();

        private static readonly Lazy<bool> s_isSilverlight = new Lazy<bool>(() => PlatformSupport.IsSilverlight);

        private static readonly Lazy<bool> s_isMono = new Lazy<bool>(() => PlatformSupport.IsMono);

        private readonly ObservableDictionary<string, IDeserializer> _contentHandlers = new ObservableDictionary<string, IDeserializer>(StringComparer.OrdinalIgnoreCase);

        private readonly IList<string> _acceptTypes = new List<string>();

        private readonly ObservableDictionary<string, IEncoding> _encodingHandlers = new ObservableDictionary<string, IEncoding>(StringComparer.OrdinalIgnoreCase);

        private readonly IList<string> _acceptEncodings = new List<string>();

        private readonly Lazy<IHttpClient> _httpClient;

        private bool _disposedValue; // For the detection of multiple calls

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClientBase" /> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory to use</param>
        protected RestClientBase(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;

            _httpClient = new Lazy<IHttpClient>(() => this.HttpClientFactory.CreateClient(this));

            // register default handlers
            var jsonDeserializer = new JsonDeserializer();
            _contentHandlers.Add("application/json", jsonDeserializer);
            _contentHandlers.Add("text/json", jsonDeserializer);
            _contentHandlers.Add("text/x-json", jsonDeserializer);
            _contentHandlers.Add("text/javascript", jsonDeserializer);

            var xmlDataContractDeserializer = new XmlDataContractDeserializer();
            _contentHandlers.Add("application/xml", xmlDataContractDeserializer);
            _contentHandlers.Add("text/xml", xmlDataContractDeserializer);

            // Don't automatically update the Accept header, when we're running on Mono (is this still needed?)
            if (!s_isMono.Value)
            {
                _contentHandlers.CollectionChanged += ContentHandlersOnCollectionChanged;
                UpdateAcceptHeader();
            }

            _encodingHandlers.CollectionChanged += EncodingHandlersOnCollectionChanged;

            UserAgent = s_defaultUserAgent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClientBase" /> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory to use</param>
        /// <param name="baseUrl">Base URL</param>
        protected RestClientBase(IHttpClientFactory httpClientFactory, Uri baseUrl)
            : this(httpClientFactory)
        {
            BaseUrl = baseUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClientBase" /> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory to use</param>
        /// <param name="baseUrl">Base URL</param>
        protected RestClientBase(IHttpClientFactory httpClientFactory, string baseUrl)
            : this(httpClientFactory, new Uri(baseUrl))
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RestClientBase"/> class.
        /// </summary>
        /// <remarks>
        /// Calls <see cref="Dispose(bool)"/> with <code>false</code>
        /// </remarks>
        ~RestClientBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets or sets the HTTP client factory used to create IHttpClient implementations
        /// </summary>
        public IHttpClientFactory HttpClientFactory { get; set; }

        /// <summary>
        /// Gets or sets the base URL for all requests
        /// </summary>
        public Uri BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the Authenticator to use for all requests
        /// </summary>
        public IAuthenticator Authenticator { get; set; }

        /// <summary>
        /// Gets or sets the Cookies for all requests
        /// </summary>
        /// <remarks>
        /// Cookies set by the server will be collected here.
        /// </remarks>
        public CookieContainer CookieContainer { get; set; }

        /// <summary>
        /// Gets or sets the timeout to be used for requests.
        /// </summary>
        /// <remarks>
        /// When the value isn't set, it uses the default timeout of whatever
        /// is used to implement the <see cref="IHttpClientFactory"/>.
        /// </remarks>
        public TimeSpan? Timeout { get; set; }

        /// <inheritdoc />
        public IDictionary<string, IDeserializer> ContentHandlers => _contentHandlers;

        /// <inheritdoc />
        public IDictionary<string, IEncoding> EncodingHandlers => _encodingHandlers;

        /// <summary>
        /// Gets or sets the user agent for the REST client
        /// </summary>
        /// <remarks>
        /// The default value is "RestSharp/{version}"
        /// </remarks>
        public string UserAgent
        {
            get
            {
                var userAgentParameter = DefaultParameters.Find(ParameterType.HttpHeader, "User-Agent").FirstOrDefault();
                return (string)userAgentParameter?.Value;
            }
            set
            {
                DefaultParameters.AddOrUpdate(new Parameter { Type = ParameterType.HttpHeader, Name = "User-Agent", Value = value, ValidateOnAdd = false });
            }
        }

        /// <summary>
        /// Gets the collection of the default parameters for all requests
        /// </summary>
        public IParameterCollection DefaultParameters { get; } = new ParameterCollection();

        /// <summary>
        /// Gets or sets the credentials used for the request (e.g. NTLM authentication)
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the response status code should be ignored by default.
        /// </summary>
        public bool IgnoreResponseStatusCode { get; set; }

#if !NO_PROXY
        /// <summary>
        /// Gets or sets the proxy to use for the requests
        /// </summary>
        public IWebProxy Proxy { get; set; }
#endif

        /// <summary>
        /// Disposes the used HTTP client
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned</returns>
        public virtual async Task<IRestResponse> Execute(IRestRequest request)
        {
            using (var response = await ExecuteRequest(request, CancellationToken.None))
            {
                return await RestResponse.CreateResponse(this, request, response);
            }
        }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned, with a deserialized object</returns>
        public virtual async Task<IRestResponse<T>> Execute<T>(IRestRequest request)
        {
            using (var response = await ExecuteRequest(request, CancellationToken.None))
            {
                return await RestResponse.CreateResponse<T>(this, request, response);
            }
        }

        /// <summary>
        /// Cancellable request execution
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>Response returned</returns>
        public virtual async Task<IRestResponse> Execute(IRestRequest request, CancellationToken ct)
        {
            using (var response = await ExecuteRequest(request, ct))
            {
                return await RestResponse.CreateResponse(this, request, response);
            }
        }

        /// <summary>
        /// Cancellable request execution
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="request">Request to execute</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>Response returned, with a deserialized object</returns>
        public virtual async Task<IRestResponse<T>> Execute<T>(IRestRequest request, CancellationToken ct)
        {
            using (var response = await ExecuteRequest(request, ct))
            {
                return await RestResponse.CreateResponse<T>(this, request, response);
            }
        }

        /// <summary>
        /// Get a previously added content type handler
        /// </summary>
        /// <param name="contentType">The Accept header value that identifies the handler</param>
        /// <returns>The deserializer that can handle the given content type.</returns>
        /// <remarks>
        /// This function returns NULL if the handler for the given content type cannot be found.
        /// </remarks>
        public IDeserializer GetHandler(string contentType)
        {
            if (string.IsNullOrEmpty(contentType) && _contentHandlers.ContainsKey("*"))
            {
                return _contentHandlers["*"];
            }

            if (contentType == null)
            {
                contentType = string.Empty;
            }

            var semicolonIndex = contentType.IndexOf(';');
            if (semicolonIndex != -1)
            {
                contentType = contentType.Substring(0, semicolonIndex).TrimEnd();
            }

            if (_contentHandlers.ContainsKey(contentType))
            {
                return _contentHandlers[contentType];
            }

            if (_contentHandlers.ContainsKey("*"))
            {
                return _contentHandlers["*"];
            }

            return null;
        }

        /// <summary>
        /// Get a previously added content encoding handler
        /// </summary>
        /// <param name="encodingIds">The Accept-Encoding header value that identifies the handler</param>
        /// <returns>The handler that can decode the given content encoding.</returns>
        /// <remarks>
        /// This function returns NULL if the handler for the given content encoding cannot be found.
        /// </remarks>
        public IEncoding GetEncoding(IEnumerable<string> encodingIds)
        {
            if (encodingIds != null)
            {
                foreach (var encodingId in encodingIds)
                {
                    if (_encodingHandlers.ContainsKey(encodingId))
                    {
                        return _encodingHandlers[encodingId];
                    }
                }
            }

            if (_encodingHandlers.ContainsKey("*"))
            {
                return _encodingHandlers["*"];
            }

            return null;
        }

        /// <summary>
        /// Add a new content type handler
        /// </summary>
        /// <param name="contentType">The Accept header value</param>
        /// <param name="deserializer">The deserializer to decode the content</param>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the ContentHandlers property instead")]
        public IRestClient AddHandler(string contentType, IDeserializer deserializer)
        {
            ContentHandlers.Add(contentType, deserializer);
            return this;
        }

        /// <summary>
        /// Remove a previously added content type handler
        /// </summary>
        /// <param name="contentType">The Accept header value that identifies the handler</param>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the ContentHandlers property instead")]
        public IRestClient RemoveHandler(string contentType)
        {
            ContentHandlers.Remove(contentType);
            return this;
        }

        /// <summary>
        /// Remove all previously added content type handlers
        /// </summary>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the ContentHandlers property instead")]
        public IRestClient ClearHandlers()
        {
            ContentHandlers.Clear();
            return this;
        }

        /// <summary>
        /// Add a new content encoding handler
        /// </summary>
        /// <param name="encodingId">The Accept-Encoding header value</param>
        /// <param name="encoding">The encoding engine to decode the content</param>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the EncodingHandlers property instead")]
        public IRestClient AddEncoding(string encodingId, IEncoding encoding)
        {
            EncodingHandlers.Add(encodingId, encoding);
            return this;
        }

        /// <summary>
        /// Remove a previously added content encoding handler
        /// </summary>
        /// <param name="encodingId">The Accept-Encoding header value that identifies the handler</param>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the EncodingHandlers property instead")]
        public IRestClient RemoveEncoding(string encodingId)
        {
            EncodingHandlers.Remove(encodingId);
            return this;
        }

        /// <summary>
        /// Remove all previously added content encoding handlers
        /// </summary>
        /// <returns>The client itself, to allow call chains</returns>
        [Obsolete("Use the EncodingHandlers property instead")]
        public IRestClient ClearEncodings()
        {
            EncodingHandlers.Clear();
            return this;
        }

        /// <summary>
        /// Updates the <code>Accept</code> default header parameter
        /// </summary>
        protected void UpdateAcceptHeader()
        {
            this.RemoveDefaultParameter("Accept");
            if (_acceptTypes.Count != 0)
            {
                var accepts = string.Join(", ", _acceptTypes);
                this.AddDefaultParameter(new Parameter
                {
                    Name = "Accept",
                    Value = accepts,
                    Type = ParameterType.HttpHeader,
                    ValidateOnAdd = false,
                });
            }
        }

        /// <summary>
        /// Gets the content for a request.
        /// </summary>
        /// <param name="request">The <see cref="IRestRequest"/> to get the content for.</param>
        /// <param name="parameters">The request parameters for the REST request (read-only)</param>
        /// <returns>The <see cref="IHttpContent"/> for the <paramref name="request"/></returns>
        protected abstract IHttpContent GetContent(IRestRequest request, RequestParameters parameters);

        /// <summary>
        /// Allows the implementor to modify the <paramref name="httpClient"/> and the <paramref name="requestMessage"/>
        /// before the request gets authenticated and sent.
        /// </summary>
        /// <param name="httpClient">The <see cref="IHttpClient"/> used to send the <paramref name="requestMessage"/></param>
        /// <param name="requestMessage">The <see cref="IHttpRequestMessage"/> to send</param>
        protected virtual void ModifyRequestBeforeAuthentication(IHttpClient httpClient, IHttpRequestMessage requestMessage)
        {
            // Remove the Accept header for GET requests on Silverlight platforms
            if (s_isSilverlight.Value && requestMessage.Method == Method.GET)
            {
                httpClient.DefaultRequestHeaders.Remove("Accept");
            }
        }

        /// <summary>
        /// Execute the request (which is unguarded)
        /// </summary>
        /// <param name="request">The request to execute</param>
        /// <param name="ct">The cancellation token to use</param>
        /// <returns>The <see cref="IHttpResponseMessage"/> for the request</returns>
        protected async Task<IHttpResponseMessage> ExecuteRequest(IRestRequest request, CancellationToken ct)
        {
            while (true)
            {
                if (Authenticator != null && Authenticator.CanPreAuthenticate(this, request, Credentials))
                {
                    await Authenticator.PreAuthenticate(this, request, Credentials);
                }

                var requestParameters = this.MergeParameters(request);
                bool failed = true;
                var httpClient = _httpClient.Value;
                var message = HttpClientFactory.CreateRequestMessage(this, request, requestParameters.OtherParameters);
                try
                {
                    var bodyData = GetContent(request, requestParameters);
                    if (bodyData != null)
                    {
                        message.Content = bodyData;
                    }

                    ModifyRequestBeforeAuthentication(httpClient, message);

                    if (Authenticator != null && Authenticator.CanPreAuthenticate(httpClient, message, Credentials))
                    {
                        await Authenticator.PreAuthenticate(httpClient, message, Credentials);
                    }

                    var response = await httpClient.SendAsync(message, ct);

                    try
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (Authenticator != null && Authenticator.CanHandleChallenge(httpClient, message, Credentials, response))
                            {
                                await Authenticator.HandleChallenge(httpClient, message, Credentials, response);
                                continue;
                            }

                            if (!IgnoreResponseStatusCode)
                            {
                                response.EnsureSuccessStatusCode();
                            }
                        }

                        failed = false;
                    }
                    finally
                    {
                        if (failed)
                        {
                            if (response != null)
                            {
                                response.Dispose();
                            }
                            else
                            {
                                message.Dispose();
                            }

                            message = null;
                        }
                    }

                    return response;
                }
                finally
                {
                    if (failed)
                    {
                        message?.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Dispose the <see cref="IHttpClient"/>.
        /// </summary>
        /// <param name="disposing"><code>true</code> when called from <see cref="Dispose()"/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_httpClient.IsValueCreated)
                    {
                        _httpClient.Value.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        private static string GetDefaultVersion()
        {
#if NET40ALIKE
            var assembly = typeof(RestClientBase).Assembly;
            var version = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).Cast<AssemblyFileVersionAttribute>().Single();
#else
            var assembly = typeof(RestClientBase).GetTypeInfo().Assembly;
            var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
#endif
            return $"RestSharp/{version.Version}";
        }

        private void UpdateAcceptsEncodingHeader()
        {
            this.RemoveDefaultParameter("Accept-Encoding");
            if (_acceptEncodings.Count != 0)
            {
                var accepts = string.Join(", ", _acceptEncodings);
                this.AddDefaultParameter("Accept-Encoding", accepts, ParameterType.HttpHeader);
            }
        }

        /// <summary>
        /// Updates the <code>Accepts</code> header whenever the list of content handlers changes
        /// </summary>
        /// <param name="sender">The observable dictionary</param>
        /// <param name="args">The changes</param>
        private void ContentHandlersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var contentType in args.NewItems.Cast<KeyValuePair<string, IDeserializer>>().Where(x => x.Key != "*").Select(x => x.Key))
                    {
                        _acceptTypes.Add(contentType);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _acceptTypes.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var contentType in args.OldItems.Cast<KeyValuePair<string, IDeserializer>>().Where(x => x.Key != "*").Select(x => x.Key))
                    {
                        _acceptTypes.Remove(contentType);
                    }
                    break;
            }

            UpdateAcceptHeader();
        }

        /// <summary>
        /// Updates the <code>Accept-Encoding</code> header whenever the list of encoding handlers changes
        /// </summary>
        /// <param name="sender">The observable dictionary</param>
        /// <param name="args">The changes</param>
        private void EncodingHandlersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var contentType in args.NewItems.Cast<KeyValuePair<string, IEncoding>>().Select(x => x.Key))
                    {
                        _acceptEncodings.Add(contentType);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _acceptEncodings.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var contentType in args.OldItems.Cast<KeyValuePair<string, IEncoding>>().Select(x => x.Key))
                    {
                        _acceptEncodings.Remove(contentType);
                    }
                    break;
            }

            UpdateAcceptsEncodingHeader();
        }
    }
}
