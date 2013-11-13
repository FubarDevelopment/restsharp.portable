using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    public class RestRequest : IRestRequest
    {
        private List<Parameter> _parameters = new List<Parameter>();

        public RestRequest(string resource, HttpMethod method)
        {
            Method = method;
            Resource = resource;
            Serializer = new Serializers.JsonSerializer();
        }

        public HttpMethod Method { get; set; }

        public string Resource { get; private set; }

        public IList<Parameter> Parameters { get { return _parameters; } }

        public IRestRequest AddBody(object obj)
        {
            var data = Serializer.Serialize(obj);
            return AddParameter(new Parameter { Value = data, Type = ParameterType.RequestBody, ContentType = Serializer.ContentType });
        }

        public IRestRequest AddParameter(Parameter parameter)
        {
            _parameters.Add(parameter);
            return this;
        }

        public Serializers.ISerializer Serializer { get; set; }
    }
}
