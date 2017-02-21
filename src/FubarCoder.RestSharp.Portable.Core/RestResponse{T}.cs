using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// The REST response which contains the deserialized value.
    /// </summary>
    /// <typeparam name="T">
    /// The type to deserialize
    /// </typeparam>
    public class RestResponse<T> : RestResponse, IRestResponse<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestResponse{T}"/> class.
        /// </summary>
        /// <param name="client">
        /// REST client
        /// </param>
        /// <param name="request">
        /// REST request
        /// </param>
        protected internal RestResponse(IRestClient client, IRestRequest request)
            : base(client, request)
        {
        }

        /// <summary>
        /// Gets the deserialized object of type T
        /// </summary>
        /// <remarks>
        /// When the object cannot be deserialized, this property
        /// contains the value of default(T).
        /// </remarks>
        public T Data { get; private set; }

        /// <summary>
        /// Utility function that really initializes this response object from
        /// a HttpResponseMessage
        /// </summary>
        /// <param name="response">
        /// Response that will be used to initialize this response.
        /// </param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>
        /// Task, because this function runs asynchronously
        /// </returns>
        /// <remarks>
        /// This override also deserializes the response
        /// </remarks>
        protected override async Task LoadResponse(IHttpResponseMessage response, CancellationToken ct)
        {
            await base.LoadResponse(response, ct).ConfigureAwait(false);
            if (RawBytes != null)
            {
                var handler = Client.GetHandler(ContentType);
                Data = (handler != null) ? handler.Deserialize<T>(this) : default(T);
            }
            else
            {
                Data = default(T);
            }
        }
    }
}
