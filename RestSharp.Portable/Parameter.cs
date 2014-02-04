using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// Parameter type
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// Parameter will be stored in the URL query for a GET request, or in the body for a POST request
        /// </summary>
        GetOrPost,
        /// <summary>
        /// The parameter is part of the IRestResponse.Resource
        /// </summary>
        UrlSegment,
        /// <summary>
        /// The parameter is part of the resulting URL query
        /// </summary>
        QueryString,
        /// <summary>
        /// The parameter will be sent as HTTP header
        /// </summary>
        HttpHeader,
        /// <summary>
        /// The parameter will be sent in the HTTP POST body
        /// </summary>
        RequestBody,
        /// <summary>
        /// The parameter will be sent as cookie value.
        /// </summary>
        Cookie,
    }

    /// <summary>
    /// Parameter container for REST requests
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Parameter()
        {
            ValidateOnAdd = true;
        }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of the parameter
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public ParameterType Type { get; set; }

        /// <summary>
        /// Content type of the parameter
        /// </summary>
        /// <remarks>
        /// Only applicable when Type is Body.
        /// </remarks>
        public MediaTypeHeaderValue ContentType { get; set; }

        /// <summary>
        /// Should this parameter be validated?
        /// </summary>
        /// <remarks>
        /// Applies only HTTP header parameters
        /// </remarks>
        public bool ValidateOnAdd { get; set; }
    }
}
