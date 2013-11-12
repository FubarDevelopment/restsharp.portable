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
    public class RestClient
    {
        public Uri BaseUrl { get; set; }

        public IAuthenticator Authenticator { get; set; }

        public RestClient(Uri baseUrl)
        {
            BaseUrl = baseUrl;
        }

        private async Task<HttpWebResponse> ExecuteRequest(RestRequest request)
        {
            if (Authenticator != null)
                Authenticator.Authenticate(this, request);
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
            var http = WebRequest.CreateHttp(url);
            if (request.Method != null)
                http.Method = request.Method.Method;
            var bodyData = new MemoryStream();
            var bodyParameter = request.Parameters.FirstOrDefault(x => x.Type == ParameterType.Body);
            if (bodyParameter != null)
            {
                var buffer = (byte[])bodyParameter.Value;
                bodyData.Write(buffer, 0, buffer.Length);
            }
            if (request.Method == HttpMethod.Post)
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

        public async Task<RestResponse> Execute(RestRequest request)
        {
            var response = await ExecuteRequest(request);
            return new RestResponse(request, response);
        }

        public async Task<RestResponse<T>> Execute<T>(RestRequest request)
        {
            var response = await ExecuteRequest(request);
            return new RestResponse<T>(request, response);
        }
    }
}
