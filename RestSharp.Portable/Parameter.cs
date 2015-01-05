using System.Net.Http.Headers;
using System.Text;

namespace RestSharp.Portable
{
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
        /// Applies to the following parameter types:
        /// - RequestBody
        /// </remarks>
        public MediaTypeHeaderValue ContentType { get; set; }

        /// <summary>
        /// Should this parameter be validated?
        /// </summary>
        /// <remarks>
        /// Applies to the following parameter types:
        /// - HttpHeader
        /// </remarks>
        public bool ValidateOnAdd { get; set; }

        /// <summary>
        /// Encoding of this parameters value
        /// </summary>
        /// <remarks>
        /// Applies to the following parameter types:
        /// - GetOrPost
        /// - QueryString
        /// - UrlSegment
        /// </remarks>
        public Encoding Encoding { get; set; }
    }
}
