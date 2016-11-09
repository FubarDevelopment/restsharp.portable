using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
        Method Method { get; set; }

        /// <summary>
        /// Gets the resource relative to the REST clients base URL
        /// </summary>
        string Resource { get; }

        /// <summary>
        /// Gets the REST request parameters
        /// </summary>
        IParameterCollection Parameters { get; }

        /// <summary>
        /// Gets or sets the content collection mode which controls if we use basic content or multi part content by default.
        /// </summary>
        ContentCollectionMode ContentCollectionMode { get; set; }

        /// <summary>
        /// Gets or sets the method to use to write the response instead of reading into RawBytes
        /// </summary>
        Func<Stream, CancellationToken, Task> ResponseWriterAsync { get; set; }
    }
}
