#if NETSTANDARD1_0
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Security;
#endif

namespace RestSharp.Portable.OAuth1.Crypto
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
#if NETSTANDARD1_0
            var digest = new MD5Digest();
            return DigestUtilities.DoFinal(digest, data);
#else
            using (var digest = System.Security.Cryptography.MD5.Create())
                return digest.ComputeHash(data);
#endif
        }
    }
}
