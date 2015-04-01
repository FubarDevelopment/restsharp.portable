using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

using RestSharp.Portable.Serializers;

namespace RestSharp.Portable
{
    /// <summary>
    /// Defines a REST request
    /// </summary>
    public interface IRestRequest
    {
        /// <summary>
        /// Gets or sets the serializer that should serialize the body
        /// </summary>
        ISerializer Serializer { get; set; }

        /// <summary>
        /// Gets or sets the HTTP request method (GET, POST, etc...)
        /// </summary>
        HttpMethod Method { get; set; }

        /// <summary>
        /// Gets the resource relative to the REST clients base URL
        /// </summary>
        string Resource { get; }

        /// <summary>
        /// Gets the REST request parameters
        /// </summary>
        IList<Parameter> Parameters { get; }

        /// <summary>
        /// Gets or sets the content collection mode which controls if we use basic content or multi part content by default.
        /// </summary>
        ContentCollectionMode ContentCollectionMode { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="StringComparer"/> to be used for this request.
        /// </summary>
        /// <remarks>
        /// If this property is null, the <see cref="IRestClient.DefaultParameterNameComparer"/> is used.
        /// </remarks>
        StringComparer ParameterNameComparer { get; set; }
    }
}
