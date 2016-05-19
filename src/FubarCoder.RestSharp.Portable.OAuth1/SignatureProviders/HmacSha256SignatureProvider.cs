#if !NETSTANDARD1_0
using System.Security.Cryptography;
#endif
using System.Text;

#if NETSTANDARD1_0
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
#endif

namespace RestSharp.Portable.OAuth1.SignatureProviders
{
    /// <summary>
    /// Calculates a <code>HMAC-SHA256</code> signature
    /// </summary>
    public class HmacSha256SignatureProvider : ISignatureProvider
    {
        /// <summary>
        /// All text parameters are UTF-8 encoded (per section 5.1).
        /// </summary>
        /// <a href="http://www.hueniverse.com/hueniverse/2008/10/beginners-gui-1.html"/>
        private static readonly Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// The ID (also used as signature method name for requests) of the signature provider
        /// </summary>
        public string Id => "HMAC-SHA256";

        /// <summary>
        /// Calculate the signature of <paramref name="data"/>
        /// </summary>
        /// <param name="data">The data to sign</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="tokenSecret">The token secret</param>
        /// <returns>The signature</returns>
        public string CalculateSignature(byte[] data, string consumerSecret, string tokenSecret)
        {
            var key = $"{consumerSecret}&{tokenSecret}";
            var keyData = _encoding.GetBytes(key);
#if NETSTANDARD1_0
            var digest = new Sha256Digest();
            var crypto = new HMac(digest);
            crypto.Init(new KeyParameter(keyData));
            crypto.BlockUpdate(data, 0, data.Length);
            var hash = MacUtilities.DoFinal(crypto);
            return System.Convert.ToBase64String(hash);
#else
            using (var digest = new HMACSHA256(keyData))
            {
                var hash = digest.ComputeHash(data);
                return System.Convert.ToBase64String(hash);
            }
#endif
        }
    }
}
