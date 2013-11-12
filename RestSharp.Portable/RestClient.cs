using RestSharp.Portable.Deserializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    public class RestClient : IRestClient
    {
        private readonly IDictionary<string, IDeserializer> _contentHandlers = new Dictionary<string, IDeserializer>(StringComparer.OrdinalIgnoreCase);
        private readonly IList<string> _acceptTypes = new List<string>();
        private readonly List<Parameter> _defaultParameters = new List<Parameter>();

        public Uri BaseUrl { get; set; }

        public IAuthenticator Authenticator { get; set; }

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
            var urlBuilder = new UriBuilder(new Uri(BaseUrl, new Uri(request.Resource, UriKind.RelativeOrAbsolute)));
            var queryString = new StringBuilder(urlBuilder.Query ?? string.Empty);
            var startsWithQuestionmark = queryString.ToString().StartsWith("?");
            foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.QueryString))
            {
                if (queryString.Length > (startsWithQuestionmark ? 1 : 0))
                    queryString.Append("&");
                queryString.AppendFormat("{0}={1}", Uri.EscapeUriString(param.Name), Uri.EscapeUriString(string.Format("{0}", param.Value)));
            }
            if (request.Method == HttpMethod.Get)
            {
                foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.GetOrPost))
                {
                    if (queryString.Length > (startsWithQuestionmark ? 1 : 0))
                        queryString.Append("&");
                    queryString.AppendFormat("{0}={1}", Uri.EscapeUriString(param.Name), Uri.EscapeUriString(string.Format("{0}", param.Value)));
                }
            }
            urlBuilder.Query = queryString.ToString();
            var url = urlBuilder.Uri;
            return url;
        }

        private void AddHttpHeaders(HttpWebRequest http, IRestRequest request)
        {
            foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.HttpHeader))
            {
                if (string.Equals(param.Name, "accept", StringComparison.OrdinalIgnoreCase))
                {
                    http.Accept = (string)param.Value;
                }
                else
                {
                    http.Headers[param.Name] = string.Format("{0}", param.Value);
                }
            }
        }

        private byte[] GetBodyData(HttpWebRequest http, IRestRequest request)
        {
            var bodyData = new MemoryStream();
            var body = request.Parameters.FirstOrDefault(x => x.Type == ParameterType.RequestBody);
            if (body != null)
            {
                byte[] buffer;
                if (body.Value is byte[])
                {
                    buffer = (byte[])body.Value;
                    http.ContentType = body.ContentType;
                }
                else
                {
                    buffer = request.Serializer.Serialize(body.Value);
                    http.ContentType = request.Serializer.ContentType;
                }
                bodyData.Write(buffer, 0, buffer.Length);
            }
            else if (request.Method == HttpMethod.Post)
            {
                var bodyDataWriter = new StreamWriter(bodyData);
                bodyDataWriter.NewLine = "&";
                foreach (var param in request.Parameters.Where(x => x.Type == ParameterType.GetOrPost))
                {
                    var data = Uri.EscapeDataString(string.Format("{0}", param.Value));
                    bodyDataWriter.WriteLine("{0}={1}", param.Name, data);
                    System.Diagnostics.Debug.WriteLine("{0}={1}", param.Name, data);
                }
                bodyDataWriter.Flush();
            }
            return bodyData.ToArray();
        }

        private HttpWebRequest CreateHttpRequest(IRestRequest request)
        {
            var url = BuildUri(request);
            var http = HttpWebRequest.CreateHttp(url);
            if (request.Method != null)
                http.Method = request.Method.Method;
            AddHttpHeaders(http, request);
            return http;
        }

        private async Task<HttpWebResponse> ExecuteRequest(IRestRequest request)
        {
            ConfigureRequest(request);
            var http = CreateHttpRequest(request);
            var bodyData = GetBodyData(http, request);
            if (bodyData.Length != 0)
            {
                var bodyBuffer = bodyData.ToArray();
                using (var requestStream = await Task.Factory.FromAsync(http.BeginGetRequestStream, new Func<IAsyncResult, Stream>(http.EndGetRequestStream), null))
                {
                    requestStream.Write(bodyBuffer, 0, bodyBuffer.Length);
                }
            }
            var response = await Task.Factory.FromAsync(http.BeginGetResponse, new Func<IAsyncResult, WebResponse>(http.EndGetResponse), null) as HttpWebResponse;
            return response;
        }

        public async Task<IRestResponse> Execute(IRestRequest request)
        {
            var response = await ExecuteRequest(request);
            return new RestResponse(request, response);
        }

        public async Task<IRestResponse<T>> Execute<T>(IRestRequest request)
        {
            var response = await ExecuteRequest(request);
            return new RestResponse<T>(this, request, response);
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
    }
}
