using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using RestSharp.Portable.Authenticators;
using RestSharp.Portable.Deserializers;
using RestSharp.Portable.Encodings;

namespace RestSharp.Portable
{
    /// <summary>
    /// The default REST client
    /// </summary>
    public class RestClient : IRestClient
    {
        private readonly IDictionary<string, IDeserializer> _contentHandlers = new Dictionary<string, IDeserializer>(StringComparer.OrdinalIgnoreCase);
        private readonly IList<string> _acceptTypes = new List<string>();

        private readonly IDictionary<string, IEncoding> _encodingHandlers = new Dictionary<string, IEncoding>(StringComparer.OrdinalIgnoreCase);
        private readonly IList<string> _acceptEncodings = new List<string>();

        private readonly List<Parameter> _defaultParameters = new List<Parameter>();
        private HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// </summary>
        public RestClient()
        {
            HttpClientFactory = new HttpClientImpl.DefaultHttpClientFactory();

            var jsonDeserializer = new JsonDeserializer();

            // register default handlers
            AddHandler("application/json", jsonDeserializer);
            AddHandler("text/json", jsonDeserializer);
            AddHandler("text/x-json", jsonDeserializer);
            AddHandler("text/javascript", jsonDeserializer);

            var xmlDataContractDeserializer = new XmlDataContractDeserializer();
            AddHandler("application/xml", xmlDataContractDeserializer);
            AddHandler("text/xml", xmlDataContractDeserializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// </summary>
        /// <param name="baseUrl">Base URL</param>
        public RestClient(Uri baseUrl)
            : this()
        {
            BaseUrl = baseUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// </summary>
        /// <param name="baseUrl">Base URL</param>
        public RestClient(string baseUrl)
            : this(new Uri(baseUrl))
        {
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
        /// Gets or sets the default <see cref="StringComparer"/> to be used for the requests.
        /// </summary>
        /// <remarks>
        /// If this property is null, the <see cref="StringComparer.Ordinal"/> is used.
        /// </remarks>
        public StringComparer DefaultParameterNameComparer { get; set; }

        /// <summary>
        /// Gets the collection of the default parameters for all requests
        /// </summary>
        public IList<Parameter> DefaultParameters
        {
            get { return _defaultParameters; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the response status code should be ignored by default.
        /// </summary>
        public bool IgnoreResponseStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the proxy to use for the requests
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned</returns>
        public async Task<IRestResponse> Execute(IRestRequest request)
        {
            using (var cts = new CancellationTokenSource())
            {
                using (var response = await ExecuteRequest(request, cts.Token))
                {
                    var restResponse = new RestResponse(this, request);
                    await restResponse.LoadResponse(response);
                    return restResponse;
                }
            }
        }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <typeparam name="T">
        /// The type to deserialize the response to.
        /// </typeparam>
        /// <param name="request">
        /// Request to execute
        /// </param>
        /// <returns>
        /// Response returned, with a deserialized object
        /// </returns>
        public async Task<IRestResponse<T>> Execute<T>(IRestRequest request)
        {
            using (var cts = new CancellationTokenSource())
            {
                using (var response = await ExecuteRequest(request, cts.Token))
                {
                    var restResponse = new RestResponse<T>(this, request);
                    await restResponse.LoadResponse(response);
                    return restResponse;
                }
            }
        }

        /// <summary>
        /// Cancellable request execution
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>Response returned</returns>
        public async Task<IRestResponse> Execute(IRestRequest request, CancellationToken ct)
        {
            using (var response = await ExecuteRequest(request, ct))
            {
                var restResponse = new RestResponse(this, request);
                await restResponse.LoadResponse(response);
                return restResponse;
            }
        }

        /// <summary>
        /// Cancellable request execution
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="request">Request to execute</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>Response returned, with a deserialized object</returns>
        public async Task<IRestResponse<T>> Execute<T>(IRestRequest request, CancellationToken ct)
        {
            using (var response = await ExecuteRequest(request, ct))
            {
                var restResponse = new RestResponse<T>(this, request);
                await restResponse.LoadResponse(response);
                return restResponse;
            }
        }

        /// <summary>
        /// Add a new content type handler
        /// </summary>
        /// <param name="contentType">The Accept header value</param>
        /// <param name="deserializer">The deserializer to decode the content</param>
        /// <returns>The client itself, to allow call chains</returns>
        public IRestClient AddHandler(string contentType, IDeserializer deserializer)
        {
            _contentHandlers[contentType] = deserializer;
            if (contentType != "*")
            {
                _acceptTypes.Add(contentType);
                UpdateAcceptsHeader();
            }

            return this;
        }

        /// <summary>
        /// Remove a previously added content type handler
        /// </summary>
        /// <param name="contentType">The Accept header value that identifies the handler</param>
        /// <returns>The client itself, to allow call chains</returns>
        public IRestClient RemoveHandler(string contentType)
        {
            _contentHandlers.Remove(contentType);
            if (contentType != "*")
            {
                _acceptTypes.Remove(contentType);
                UpdateAcceptsHeader();
            }

            return this;
        }

        /// <summary>
        /// Remove all previously added content type handlers
        /// </summary>
        /// <returns>The client itself, to allow call chains</returns>
        public IRestClient ClearHandlers()
        {
            _contentHandlers.Clear();
            _acceptTypes.Clear();
            UpdateAcceptsHeader();
            return this;
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
                return _contentHandlers["*"];
            if (contentType == null)
                contentType = string.Empty;
            var semicolonIndex = contentType.IndexOf(';');
            if (semicolonIndex != -1)
                contentType = contentType.Substring(0, semicolonIndex).TrimEnd();
            if (_contentHandlers.ContainsKey(contentType))
                return _contentHandlers[contentType];
            if (_contentHandlers.ContainsKey("*"))
                return _contentHandlers["*"];
            return null;
        }

        /// <summary>
        /// Replace all handlers of a given type with a new deserializer
        /// </summary>
        /// <param name="oldType">The type of the old deserializer</param>
        /// <param name="deserializer">The new deserializer</param>
        /// <returns>The client itself, to allow call chains</returns>
        public IRestClient ReplaceHandler(Type oldType, IDeserializer deserializer)
        {
            var contentHandlersToReplace = _contentHandlers.Where(x => oldType.IsInstanceOfType(x.Value)).ToList();
            foreach (var contentHandlerToReplace in contentHandlersToReplace)
            {
                _contentHandlers.Remove(contentHandlerToReplace.Key);
                _contentHandlers.Add(contentHandlerToReplace.Key, deserializer);
            }

            UpdateAcceptsHeader();
            return this;
        }

        /// <summary>
        /// Add a new content encoding handler
        /// </summary>
        /// <param name="encodingId">The Accept-Encoding header value</param>
        /// <param name="encoding">The encoding engine to decode the content</param>
        /// <returns>The client itself, to allow call chains</returns>
        public IRestClient AddEncoding(string encodingId, IEncoding encoding)
        {
            _encodingHandlers[encodingId] = encoding;
            _acceptEncodings.Add(encodingId);
            UpdateAcceptsEncodingHeader();
            return this;
        }

        /// <summary>
        /// Remove a previously added content encoding handler
        /// </summary>
        /// <param name="encodingId">The Accept-Encoding header value that identifies the handler</param>
        /// <returns>The client itself, to allow call chains</returns>
        public IRestClient RemoveEncoding(string encodingId)
        {
            _encodingHandlers.Remove(encodingId);
            _acceptEncodings.Remove(encodingId);
            UpdateAcceptsEncodingHeader();
            return this;
        }

        /// <summary>
        /// Remove all previously added content encoding handlers
        /// </summary>
        /// <returns>The client itself, to allow call chains</returns>
        public IRestClient ClearEncodings()
        {
            _encodingHandlers.Clear();
            _acceptEncodings.Clear();
            UpdateAcceptsEncodingHeader();
            return this;
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
                        return _encodingHandlers[encodingId];
                }
            }

            if (_encodingHandlers.ContainsKey("*"))
                return _encodingHandlers["*"];
            return null;
        }

        /// <summary>
        /// Close the used HTTP client
        /// </summary>
        public void Dispose()
        {
            if (_httpClient == null)
                return;
            _httpClient.Dispose();
            _httpClient = null;
        }

        private void UpdateAcceptsHeader()
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
                    ValidateOnAdd = !EnvironmentUtilities.IsMono,
                });
            }
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
        /// Add overridable default parameters to the request
        /// </summary>
        /// <param name="request">The requests to add the default parameters to.</param>
        private void AddDefaultParameters(IRestRequest request)
        {
            var comparer = new ParameterComparer(this, request);

            var startIndex = 0;
            foreach (var parameter in DefaultParameters.Where(x => x.Type != ParameterType.HttpHeader))
            {
                if (request.Parameters.Contains(parameter, comparer))
                    continue;
                request.Parameters.Insert(startIndex++, parameter);
            }
        }

        private async Task AuthenticateRequest(IRestRequest request)
        {
            if (Authenticator == null)
                return;

            var asyncAuth = Authenticator as IAsyncAuthenticator;
            if (asyncAuth != null)
            {
                await asyncAuth.Authenticate(this, request);
            }
            else
            {
                Authenticator.Authenticate(this, request);
            }
        }

        /// <summary>
        /// Notify the authenticator about a failed request to be able to retry the request
        /// with updated authentication information.
        /// </summary>
        /// <param name="request">The failed request</param>
        /// <param name="response">The response of the failed request</param>
        /// <returns>true == Authenticator notified</returns>
        private async Task<bool> NotifyAuthenticatorAboutFailedRequest(IRestRequest request, HttpResponseMessage response)
        {
            var asyncRoundTripAuthenticator = Authenticator as IAsyncRoundTripAuthenticator;
            if (asyncRoundTripAuthenticator != null && asyncRoundTripAuthenticator.StatusCodes.Contains(response.StatusCode))
            {
                var restResponse = new RestResponse(this, request);
                await restResponse.LoadResponse(response);
                await asyncRoundTripAuthenticator.AuthenticationFailed(this, request, restResponse);
                return true;
            }

            var roundTripAuthenticator = Authenticator as IRoundTripAuthenticator;
            if (roundTripAuthenticator != null && roundTripAuthenticator.StatusCodes.Contains(response.StatusCode))
            {
                var restResponse = new RestResponse(this, request);
                await restResponse.LoadResponse(response);
                roundTripAuthenticator.AuthenticationFailed(this, request, restResponse);
                return true;
            }

            return false;
        }

        private async Task<HttpResponseMessage> ExecuteRequest(IRestRequest request, CancellationToken ct)
        {
            var retryWithAuthentication = true;
            AddDefaultParameters(request);
            while (true)
            {
                await AuthenticateRequest(request);
                if (_httpClient == null)
                    _httpClient = HttpClientFactory.CreateClient(this, request);
                using (var message = HttpClientFactory.CreateRequestMessage(this, request))
                {
                    var bodyData = this.GetContent(request);
                    if (bodyData != null)
                        message.Content = bodyData;

                    if (EnvironmentUtilities.IsSilverlight && message.Method == HttpMethod.Get)
                        _httpClient.DefaultRequestHeaders.Accept.Clear();

                    bool failed = true;
                    var response = await _httpClient.SendAsync(message, ct);
                    try
                    {
                        if (retryWithAuthentication)
                        {
                            retryWithAuthentication = false;
                            var retry = await NotifyAuthenticatorAboutFailedRequest(request, response);
                            if (retry)
                            {
                                failed = false;
                                continue;
                            }
                        }

                        if (!IgnoreResponseStatusCode)
                            response.EnsureSuccessStatusCode();
                        failed = false;
                    }
                    finally
                    {
                        if (failed && response != null)
                            response.Dispose();
                    }

                    return response;
                }
            }
        }
    }
}
