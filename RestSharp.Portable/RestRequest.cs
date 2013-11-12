using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    public class RestRequest
    {
        private List<Parameter> _parameters = new List<Parameter>();

        public RestRequest(string resource, HttpMethod method)
        {
            Method = method;
            Resource = resource;
        }

        public HttpMethod Method { get; set; }

        public string Resource { get; private set; }

        public List<Parameter> Parameters { get { return _parameters; } }

        public RestRequest AddBody(object obj)
        {
            var output = new MemoryStream();
            var serializer = new JsonSerializer();
            using (var writer = new StreamWriter(output))
            {
                serializer.Serialize(writer, obj);
            }
            return AddParameter(null, output.ToArray(), ParameterType.Body);
        }

        public RestRequest AddParameter(string name, object value)
        {
            return AddParameter(name, value, ParameterType.GetOrPost);
        }

        public RestRequest AddParameter(string name, object value, ParameterType type)
        {
            return AddParameter(new Parameter { Name = name, Value = value, Type = type });
        }

        public RestRequest AddParameter(Parameter parameter)
        {
            _parameters.Add(parameter);
            return this;
        }
    }
}
