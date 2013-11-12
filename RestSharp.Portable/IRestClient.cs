using RestSharp.Portable.Deserializers;
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

        Task<IRestResponse> Execute(IRestRequest request);
        Task<IRestResponse<T>> Execute<T>(IRestRequest request);

        IRestClient AddHandler(string contentType, IDeserializer deserializer);
        IRestClient RemoveHandler(string contentType);
        IRestClient ClearHandlers();
        IDeserializer GetHandler(string contentType);

        IWebProxy Proxy { get; set; }
    }
}
