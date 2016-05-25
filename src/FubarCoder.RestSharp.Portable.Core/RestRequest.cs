using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace RestSharp.Portable
{
    /// <summary>
    /// The default REST request
    /// </summary>
    public class RestRequest : IRestRequest
    {
        private readonly IParameterCollection _parameters = new ParameterCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest" /> class.
        /// </summary>
        public RestRequest()
            : this((string)null, Method.GET)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest" /> class.
        /// </summary>
        /// <param name="method">The HTTP request method to use</param>
        public RestRequest(Method method)
            : this((string)null, method)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest" /> class.
        /// </summary>
        /// <param name="resource">The resource this request is targeting</param>
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification = "resource")]
        public RestRequest(string resource)
            : this(resource, Method.GET)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest" /> class.
        /// </summary>
        /// <param name="resource">The resource this request is targeting</param>
        /// <param name="method">The HTTP request method</param>
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification = "resource might be null and the resource URI must never be null")]
        public RestRequest(string resource, Method method)
        {
            ContentCollectionMode = ContentCollectionMode.MultiPartForFileParameters;
            Method = method;
            Resource = resource;
            Serializer = new Serializers.JsonSerializer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest" /> class.
        /// </summary>
        /// <param name="resource">The resource this request is targeting</param>
        public RestRequest([NotNull] Uri resource)
            : this(resource, Method.GET)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest" /> class.
        /// </summary>
        /// <param name="resource">The resource this request is targeting</param>
        /// <param name="method">The HTTP request method</param>
        public RestRequest([NotNull] Uri resource, Method method)
            : this(resource.IsAbsoluteUri ? resource.ToString() : resource.OriginalString, method)
        {
        }

        /// <summary>
        /// Gets or sets the HTTP request method (GET, POST, etc...)
        /// </summary>
        public Method Method { get; set; }

        /// <summary>
        /// Gets or sets the resource relative to the REST clients base URL
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Gets the REST request parameters
        /// </summary>
        public IParameterCollection Parameters => _parameters;

        /// <summary>
        /// Gets or sets the serializer that should serialize the body
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// Gets or sets the content collection mode which controls if we use basic content or multi part content by default.
        /// </summary>
        public ContentCollectionMode ContentCollectionMode { get; set; }
    }
}
