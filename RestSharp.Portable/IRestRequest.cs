using RestSharp.Portable.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace RestSharp.Portable
{
    /// <summary>
    /// Defines a REST request
    /// </summary>
    public interface IRestRequest
    {
        /// <summary>
        /// The serializer that should serialize the body
        /// </summary>
        ISerializer Serializer { get; set; }
        /// <summary>
        /// HTTP request method (GET, POST, etc...)
        /// </summary>
        HttpMethod Method { get; set; }
        /// <summary>
        /// The resource relative to the REST clients base URL
        /// </summary>
        string Resource { get; }
        /// <summary>
        /// REST request parameters
        /// </summary>
        IList<Parameter> Parameters { get; }
        /// <summary>
        /// The credentials used for the request (e.g. NTLM authentication)
        /// </summary>
        ICredentials Credentials { get; set; }
        /// <summary>
        /// Controls if we use basic content or multi part content by default.
        /// </summary>
        ContentCollectionMode ContentCollectionMode { get; set; }
    }
}
