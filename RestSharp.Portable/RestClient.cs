using RestSharp.Portable.Authenticators;
using RestSharp.Portable.Deserializers;
using RestSharp.Portable.Encodings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        /// <summary>
        /// HTTP client factory used to create IHttpClient implementations
        /// </summary>
        public IHttpClientFactory HttpClientFactory { get; set; }

        /// <summary>
        /// Base URL for all requests
        /// </summary>
        public Uri BaseUrl { get; set; }

        /// <summary>
        /// Authenticator to use for all requests
        /// </summary>
        public IAuthenticator Authenticator { get; set; }

        /// <summary>
        /// Cookies for all requests
        /// </summary>
        /// <remarks>
        /// Cookies set by the server will be collected here.
        /// </remarks>
        public CookieContainer CookieContainer { get; set; }

        /// <summary>
        /// Default parameters for all requests
        /// </summary>
        public IList<Parameter> DefaultParameters { get { return _defaultParameters; } }

        /// <summary>
        /// Ignore the response status code?
        /// </summary>
        public bool IgnoreResponseStatusCode { get; set; }

        /// <summary>
        /// Constructor that initializes some default content handlers
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

            var xmlDataContractDeserializer = new Deserializers.XmlDataContractDeserializer();
            AddHandler("application/xml", xmlDataContractDeserializer);
            AddHandler("text/xml", xmlDataContractDeserializer);
        }

        /// <summary>
        /// Constructor that initializes the base URL and some default content handlers
        /// </summary>
        /// <param name="baseUrl">Base URL</param>
        public RestClient(Uri baseUrl)
            : this()
        {
            BaseUrl = baseUrl;
        }

        /// <summary>
        /// Constructor that initializes the base URL and some default content handlers
        /// </summary>
        /// <param name="baseUrl">Base URL</param>
        public RestClient(string baseUrl)
            : this(new Uri(baseUrl))
        {

        }

        private void ConfigureRequest(IRestRequest request)
        {
            foreach (var parameter in DefaultParameters)
            {
                if (request.Parameters.Contains(parameter, ParameterNameComparer.Default))
                    continue;
                request.Parameters.Add(parameter);
            }
        }

        private async Task AuthenticateRequest(IRestRequest request)
        {
            if (Authenticator == null)
                return;

            var asyncAuth = this.Authenticator as IAsyncAuthenticator;
            if (asyncAuth != null)
            {
                await asyncAuth.Authenticate(this, request);
            }
            else
            {
                Authenticator.Authenticate(this, request);
            }
        }

        private async Task<HttpResponseMessage> ExecuteRequest(IRestRequest request, CancellationToken ct)
        {
            var retryWithAuthentication = true;
            ConfigureRequest(request);
            for (; ; )
            {
                await AuthenticateRequest(request);
                var httpClient = HttpClientFactory.CreateClient(this, request);
                var message = HttpClientFactory.CreateRequestMessage(this, request);

                var bodyData = request.GetContent();
                if (bodyData != null)
                    message.Content = bodyData;

                var response = await httpClient.SendAsync(message, ct);
                if (retryWithAuthentication)
                {
                    retryWithAuthentication = false;
                    var asyncRoundTripAuthenticator = Authenticator as IAsyncRoundTripAuthenticator;
                    if (asyncRoundTripAuthenticator != null && asyncRoundTripAuthenticator.StatusCodes.Contains(response.StatusCode))
                    {
                        var restResponse = new RestResponse(this, request);
                        await restResponse.LoadResponse(response);
                        await asyncRoundTripAuthenticator.AuthenticationFailed(this, request, restResponse);
                        continue;
                    }
                    else
                    {
                        var roundTripAuthenticator = Authenticator as IRoundTripAuthenticator;
                        if (roundTripAuthenticator != null && roundTripAuthenticator.StatusCodes.Contains(response.StatusCode))
                        {
                            var restResponse = new RestResponse(this, request);
                            await restResponse.LoadResponse(response);
                            roundTripAuthenticator.AuthenticationFailed(this, request, restResponse);
                            continue;
                        }
                    }
                }
                if (!IgnoreResponseStatusCode)
                    response.EnsureSuccessStatusCode();
                return response;
            }
        }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned</returns>
        public async Task<IRestResponse> Execute(IRestRequest request)
        {
            using (var cts = new CancellationTokenSource())
            {
                var response = await ExecuteRequest(request, cts.Token);
                var restResponse = new RestResponse(this, request);
                await restResponse.LoadResponse(response);
                return restResponse;
            }
        }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned, with a deserialized object</returns>
        public async Task<IRestResponse<T>> Execute<T>(IRestRequest request)
        {
            using (var cts = new CancellationTokenSource())
            {
                var response = await ExecuteRequest(request, cts.Token);
                var restResponse = new RestResponse<T>(this, request);
                await restResponse.LoadResponse(response);
                return restResponse;
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
            var response = await ExecuteRequest(request, ct);
            var restResponse = new RestResponse(this, request);
            await restResponse.LoadResponse(response);
            return restResponse;
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
            var response = await ExecuteRequest(request, ct);
            var restResponse = new RestResponse<T>(this, request);
            await restResponse.LoadResponse(response);
            return restResponse;
        }

        private static bool? _isMono;
        private static bool IsMono
        {
            get
            {
                if (_isMono == null)
                    _isMono = Type.GetType("Mono.Runtime") != null;
                return _isMono.Value;
            }
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
                    ValidateOnAdd = !IsMono,
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
        public Deserializers.IDeserializer GetHandler(string contentType)
        {
            if (string.IsNullOrEmpty(contentType) && _contentHandlers.ContainsKey("*"))
                return _contentHandlers["*"];
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
            var contentHandlersToReplace = _contentHandlers.Where(x => oldType.IsAssignableFrom(x.Value.GetType())).ToList();
            foreach (var contentHandlerToReplace in contentHandlersToReplace)
            {
                _contentHandlers.Remove(contentHandlerToReplace.Key);
                _contentHandlers.Add(contentHandlerToReplace.Key, deserializer);
            }
            UpdateAcceptsHeader();
            return this;
        }

        /// <summary>
        /// Proxy to use for the requests
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Add a new content encoding handler
        /// </summary>
        /// <param name="encodingId">The Accept-Encoding header value</param>
        /// <param name="encoding">The encoding engine to decode the content</param>
        /// <returns>The client itself, to allow call chains</returns>
        public IRestClient AddEncoding(string encodingId, Encodings.IEncoding encoding)
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
        public Encodings.IEncoding GetEncoding(IEnumerable<string> encodingIds)
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
    }
}
