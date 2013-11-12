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
            return AddParameter(null, data, ParameterType.RequestBody, Serializer.ContentType);
        }

        public IRestRequest AddParameter(string name, object value)
        {
            return AddParameter(name, value, ParameterType.GetOrPost);
        }

        public IRestRequest AddParameter(string name, object value, ParameterType type)
        {
            return AddParameter(new Parameter { Name = name, Value = value, Type = type });
        }

        public IRestRequest AddParameter(string name, object value, ParameterType type, MediaTypeHeaderValue contentType)
        {
            return AddParameter(new Parameter { Name = name, Value = value, Type = type, ContentType = contentType });
        }

        public IRestRequest AddParameter(Parameter parameter)
        {
            _parameters.Add(parameter);
            return this;
        }

        public Serializers.ISerializer Serializer { get; set; }
    }
}
