using Org.BouncyCastle.Security;

namespace RestSharp.Portable.Authenticators.OAuth.Crypto
{
    /// <summary>
    /// Hash calculation functions
    /// </summary>
    public static class Hash
    {
        /// <summary>
        /// Calculates <code>MD5</code> hash
        /// </summary>
        /// <param name="data">The data to calculate the hash for</param>
        /// <returns>The calculated hash</returns>
        public static byte[] MD5(byte[] data)
        {
            return DigestUtilities.CalculateDigest("MD5", data);
        }
    }
}
