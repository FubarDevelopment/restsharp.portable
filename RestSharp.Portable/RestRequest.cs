using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;

using JetBrains.Annotations;

namespace RestSharp.Portable
{
    /// <summary>
    /// The default REST request
    /// </summary>
    public class RestRequest : IRestRequest
    {
        private readonly List<Parameter> _parameters = new List<Parameter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest" /> class.
        /// </summary>
        public RestRequest()
            : this((string)null, HttpMethod.Get)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest" /> class.
        /// </summary>
        /// <param name="method">The HTTP request method to use</param>
        public RestRequest(HttpMethod method)
            : this((string)null, method)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest" /> class.
        /// </summary>
        /// <param name="resource">The resource this request is targeting</param>
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification = "resource")]
        public RestRequest(string resource)
            : this(resource, HttpMethod.Get)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest" /> class.
        /// </summary>
        /// <param name="resource">The resource this request is targeting</param>
        /// <param name="method">The HTTP request method</param>
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification = "resource might be null and the resource URI must never be null")]
        public RestRequest(string resource, HttpMethod method)
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
            : this(resource, HttpMethod.Get)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest" /> class.
        /// </summary>
        /// <param name="resource">The resource this request is targeting</param>
        /// <param name="method">The HTTP request method</param>
        public RestRequest([NotNull] Uri resource, HttpMethod method)
            : this(resource.IsAbsoluteUri ? resource.AbsolutePath + resource.Query : resource.OriginalString, method)
        {
        }

        /// <summary>
        /// Gets or sets the HTTP request method (GET, POST, etc...)
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the resource relative to the REST clients base URL
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Gets the REST request parameters
        /// </summary>
        public IList<Parameter> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Gets or sets the serializer that should serialize the body
        /// </summary>
        public Serializers.ISerializer Serializer { get; set; }

        /// <summary>
        /// Gets or sets the content collection mode which controls if we use basic content or multi part content by default.
        /// </summary>
        public ContentCollectionMode ContentCollectionMode { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="StringComparer"/> to be used for this request.
        /// </summary>
        /// <remarks>
        /// If this property is null, the <see cref="IRestClient.DefaultParameterNameComparer"/> is used.
        /// </remarks>
        public StringComparer ParameterNameComparer { get; set; }
    }
}
