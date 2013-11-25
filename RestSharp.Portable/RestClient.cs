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
    public class RestClient : IRestClient
    {
        private readonly IDictionary<string, IDeserializer> _contentHandlers = new Dictionary<string, IDeserializer>(StringComparer.OrdinalIgnoreCase);
        private readonly IList<string> _acceptTypes = new List<string>();

        private readonly IDictionary<string, IEncoding> _encodingHandlers = new Dictionary<string, IEncoding>(StringComparer.OrdinalIgnoreCase);
        private readonly IList<string> _acceptEncodings = new List<string>();
        
        private readonly List<Parameter> _defaultParameters = new List<Parameter>();

        public Uri BaseUrl { get; set; }

        public IAuthenticator Authenticator { get; set; }

        public CookieContainer CookieContainer { get; set; }

        public IList<Parameter> DefaultParameters { get { return _defaultParameters; } }

        public RestClient()
        {
            // register default handlers
            AddHandler("application/json", new JsonDeserializer());
            AddHandler("text/json", new JsonDeserializer());
            AddHandler("text/x-json", new JsonDeserializer());
            AddHandler("text/javascript", new JsonDeserializer());
        }

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
            if (Authenticator != null)
                Authenticator.Authenticate(this, request);
        }

        private Uri BuildUri(IRestRequest request)
        {
            var fullUrl = this.BuildUrl(request);
            var url = BaseUrl.MakeRelativeUri(fullUrl);
            return url;
        }

        private HttpRequestMessage CreateHttpRequestMessage(IRestRequest request)
        {
            var message = new HttpRequestMessage(request.Method ?? HttpMethod.Get, BuildUri(request));
            foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.HttpHeader))
            {
                if (message.Headers.Contains(param.Name))
                    message.Headers.Remove(param.Name);
                message.Headers.Add(param.Name, string.Format("{0}", param.Value));
            }
            return message;
        }

        private bool HasCookies
        {
            get
            {
                return CookieContainer != null;
            }
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
            var hasCookies = HasCookies || request.Parameters.Any(x => x.Type == ParameterType.Cookie);
            if (HasProxy || hasCookies)
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

        private async Task<HttpResponseMessage> ExecuteRequest(IRestRequest request)
        {
            ConfigureRequest(request);
            var httpClient = CreateHttpClient(request);
            var message = CreateHttpRequestMessage(request);
            
            var bodyData = request.GetContent();
            if (bodyData != null)
                message.Content = bodyData;

            var response = await httpClient.SendAsync(message);
            return response;
        }

        public async Task<IRestResponse> Execute(IRestRequest request)
        {
            var response = await ExecuteRequest(request);
            var restResponse = new RestResponse(this, request);
            await restResponse.LoadResponse(response);
            return restResponse;
        }

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

        public IRestClient ClearHandlers()
        {
            _contentHandlers.Clear();
            _acceptTypes.Clear();
            UpdateAcceptsHeader();
            return this;
        }

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

        public IWebProxy Proxy { get; set; }

        public IRestClient AddEncoding(string encodingId, Encodings.IEncoding encoding)
        {
            _encodingHandlers[encodingId] = encoding;
            _acceptEncodings.Add(encodingId);
            UpdateAcceptsEncodingHeader();
            return this;
        }

        public IRestClient RemoveEncoding(string encodingId)
        {
            _encodingHandlers.Remove(encodingId);
            _acceptEncodings.Remove(encodingId);
            UpdateAcceptsEncodingHeader();
            return this;
        }

        public IRestClient ClearEncodings()
        {
            _encodingHandlers.Clear();
            _acceptEncodings.Clear();
            UpdateAcceptsEncodingHeader();
            return this;
        }

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
