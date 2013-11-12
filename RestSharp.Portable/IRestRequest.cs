using RestSharp.Portable.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RestSharp.Portable
{
    public interface IRestRequest
    {
        ISerializer Serializer { get; set; }
        HttpMethod Method { get; set; }
        string Resource { get; }
        IList<Parameter> Parameters { get; }
        IRestRequest AddBody(object obj);
        IRestRequest AddParameter(string name, object value);
        IRestRequest AddParameter(string name, object value, ParameterType type);
        IRestRequest AddParameter(string name, object value, ParameterType type, string contentType);
        IRestRequest AddParameter(Parameter parameter);
    }
}
