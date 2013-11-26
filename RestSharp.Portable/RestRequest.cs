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
    /// <summary>
    /// The default REST request
    /// </summary>
    public class RestRequest : IRestRequest
    {
        private List<Parameter> _parameters = new List<Parameter>();

        /// <summary>
        /// Constructor that initializes the resource path, HTTP request method and the JSON serializer as default serializer
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="method"></param>
        public RestRequest(string resource, HttpMethod method)
        {
            Method = method;
            Resource = resource;
            Serializer = new Serializers.JsonSerializer();
        }

        /// <summary>
        /// HTTP request method (GET, POST, etc...)
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// The resource relative to the REST clients base URL
        /// </summary>
        public string Resource { get; private set; }

        /// <summary>
        /// REST request parameters
        /// </summary>
        public IList<Parameter> Parameters { get { return _parameters; } }

        /// <summary>
        /// The serializer that should serialize the body
        /// </summary>
        public Serializers.ISerializer Serializer { get; set; }
    }
}
