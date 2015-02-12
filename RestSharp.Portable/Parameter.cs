using System;
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
        /// Initializes a new instance of the <see cref="Parameter" /> class.
        /// </summary>
        public Parameter()
        {
            ValidateOnAdd = true;
        }

        /// <summary>
        /// Gets or sets the name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the parameter
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter
        /// </summary>
        public ParameterType Type { get; set; }

        /// <summary>
        /// Gets or sets the content type of the parameter
        /// </summary>
        /// <remarks>
        /// Applies to the following parameter types:
        /// - RequestBody
        /// </remarks>
        public MediaTypeHeaderValue ContentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this parameter should be validated?
        /// </summary>
        /// <remarks>
        /// Applies to the following parameter types:
        /// - HttpHeader
        /// </remarks>
        public bool ValidateOnAdd { get; set; }

        /// <summary>
        /// Gets or sets the encoding of this parameters value
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
