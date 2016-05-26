namespace RestSharp.Portable
{
    /// <summary>
    /// Typed response
    /// </summary>
    /// <typeparam name="T">
    /// Type of the object to deserialize from the raw data
    /// </typeparam>
    public interface IRestResponse<out T> : IRestResponse
    {
        /// <summary>
        /// Gets the deserialized object of type T
        /// </summary>
        T Data { get; }
    }
}
