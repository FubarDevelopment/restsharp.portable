using RestSharp.Portable.Deserializers;
using RestSharp.Portable.Encodings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    public interface IRestClient
    {
        IAuthenticator Authenticator { get; set; }

        Uri BaseUrl { get; set; }

        IList<Parameter> DefaultParameters { get; }

        CookieContainer CookieContainer { get; set; }

        Task<IRestResponse> Execute(IRestRequest request);
        Task<IRestResponse<T>> Execute<T>(IRestRequest request);

        IRestClient AddHandler(string contentType, IDeserializer deserializer);
        IRestClient RemoveHandler(string contentType);
        IRestClient ClearHandlers();
        IDeserializer GetHandler(string contentType);

        IRestClient AddEncoding(string encodingId, IEncoding encoding);
        IRestClient RemoveEncoding(string encodingId);
        IRestClient ClearEncodings();
        IEncoding GetEncoding(IEnumerable<string> encodingIds);

        IWebProxy Proxy { get; set; }

        bool IgnoreResponseStatusCode { get; set; }
    }
}
