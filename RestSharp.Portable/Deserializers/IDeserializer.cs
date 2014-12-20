namespace RestSharp.Portable.Deserializers
{
    /// <summary>
    /// Deserialize for a content type
    /// </summary>
    public interface IDeserializer
    {
        /// <summary>
        /// Deserialize the response
        /// </summary>
        /// <typeparam name="T">Object type to deserialize the result to</typeparam>
        /// <param name="response">The response to deserialize the result from</param>
        /// <returns>The deserialized object</returns>
        T Deserialize<T>(IRestResponse response);
    }
}
