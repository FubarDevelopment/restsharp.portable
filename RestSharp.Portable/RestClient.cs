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
            var jsonDeserializer = new JsonDeserializer();
            // register default handlers
            AddHandler("application/json", jsonDeserializer);
            AddHandler("text/json", jsonDeserializer);
            AddHandler("text/x-json", jsonDeserializer);
            AddHandler("text/javascript", jsonDeserializer);
        }

        /// <summary>
        /// Constructor that initializes the base URL and some default content handlers
        /// </summary>
        public RestClient(Uri baseUrl)
            : this()
        {
            BaseUrl = baseUrl;
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

        private Uri BuildUri(IRestRequest request)
        {
            var fullUrl = this.BuildUrl(request);
            var url = BaseUrl.MakeRelativeUri(fullUrl);
            return url;
        }

        private HttpMethod GetDefaultMethod(IRestRequest request)
        {
            if (request.GetFileParameters().Any())
                return HttpMethod.Post;
            return HttpMethod.Get;
        }

        private HttpMethod GetHttpMethod(IRestRequest request)
        {
            if (request.Method == null || request.Method == HttpMethod.Get)
                return GetDefaultMethod(request);
            return request.Method;
        }

        private HttpRequestMessage CreateHttpRequestMessage(IRestRequest request)
        {
            var message = new HttpRequestMessage(GetHttpMethod(request), BuildUri(request));
            foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.HttpHeader))
            {
                if (message.Headers.Contains(param.Name))
                    message.Headers.Remove(param.Name);
                message.Headers.Add(param.Name, string.Format("{0}", param.Value));
            }
            return message;
        }

        private bool HasCookies(IRestRequest request)
        {
            return CookieContainer != null || request.Parameters.Any(x => x.Type == ParameterType.Cookie);
        }

        private bool HasProxy
        {
            get
            {
                return Proxy != null;
            }
        }

        private HttpClient CreateHttpClient(IRestRequest request)
        {
            HttpClient httpClient;
            var hasCookies = HasCookies(request);
            if (HasProxy || hasCookies || request.Credentials != null)
            {
                var handler = new HttpClientHandler();
                if (handler.SupportsProxy && HasProxy)
                    handler.Proxy = Proxy;
                if (hasCookies)
                {
                    CookieContainer = handler.CookieContainer = CookieContainer ?? new CookieContainer();
                    handler.UseCookies = true;
                    var cookies = handler.CookieContainer.GetCookies(BaseUrl)
                        .Cast<Cookie>().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
                    foreach (var cookieParameter in request.Parameters.Where(x => x.Type == ParameterType.Cookie && !cookies.ContainsKey(x.Name)))
                        handler.CookieContainer.Add(BaseUrl, new Cookie(cookieParameter.Name, string.Format("{0}", cookieParameter.Value)));
                }
                if (request.Credentials != null)
                    handler.Credentials = request.Credentials;
                //if (handler.SupportsAutomaticDecompression)
                //    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                httpClient = new HttpClient(handler, true);
            }
            else
            {
                httpClient = new HttpClient();
            }
            httpClient.BaseAddress = BaseUrl;
            return httpClient;
        }

        private void AuthenticateRequest(IRestRequest request)
        {
            if (Authenticator != null)
                Authenticator.Authenticate(this, request);
        }

        private async Task<HttpResponseMessage> ExecuteRequest(IRestRequest request)
        {
            ConfigureRequest(request);
            AuthenticateRequest(request);
            var httpClient = CreateHttpClient(request);
            var message = CreateHttpRequestMessage(request);
            
            var bodyData = request.GetContent();
            if (bodyData != null)
                message.Content = bodyData;

            var response = await httpClient.SendAsync(message);
            if (!IgnoreResponseStatusCode)
                response.EnsureSuccessStatusCode();
            return response;
        }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned</returns>
        public async Task<IRestResponse> Execute(IRestRequest request)
        {
            var response = await ExecuteRequest(request);
            var restResponse = new RestResponse(this, request);
            await restResponse.LoadResponse(response);
            return restResponse;
        }

        /// <summary>
        /// Execute the given request
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response returned, with a deserialized object</returns>
        public async Task<IRestResponse<T>> Execute<T>(IRestRequest request)
        {
            var response = await ExecuteRequest(request);
            var restResponse = new RestResponse<T>(this, request);
            await restResponse.LoadResponse(response);
            return restResponse;
        }

        private void UpdateAcceptsHeader()
        {
            this.RemoveDefaultParameter("Accept");
            if (_acceptTypes.Count != 0)
            {
                var accepts = string.Join(", ", _acceptTypes);
                this.AddDefaultParameter("Accept", accepts, ParameterType.HttpHeader);
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
