namespace RestSharp.Portable.OAuth1.SignatureProviders
{
    /// <summary>
    /// Calculates the signature for a request
    /// </summary>
    public interface ISignatureProvider
    {
        /// <summary>
        /// The ID (also used as signature method name for requests) of the signature provider
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Calculate the signature of <paramref name="data"/>
        /// </summary>
        /// <param name="data">The data to sign</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="tokenSecret">The token secret</param>
        /// <returns>The signature</returns>
        string CalculateSignature(byte[] data, string consumerSecret, string tokenSecret);
    }
}
