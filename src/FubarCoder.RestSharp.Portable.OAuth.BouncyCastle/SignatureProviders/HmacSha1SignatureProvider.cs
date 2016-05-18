using System;
using System.Text;

using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace RestSharp.Portable.Authenticators.OAuth.SignatureProviders
{
    /// <summary>
    /// Calculates a <code>HMAC-SHA1</code> signature
    /// </summary>
    public class HmacSha1SignatureProvider : ISignatureProvider
    {
        /// <summary>
        /// All text parameters are UTF-8 encoded (per section 5.1).
        /// </summary>
        /// <a href="http://www.hueniverse.com/hueniverse/2008/10/beginners-gui-1.html"/>
        private static readonly Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// The ID (also used as signature method name for requests) of the signature provider
        /// </summary>
        public string Id
        {
            get { return "HMAC-SHA1"; }
        }

        /// <summary>
        /// Calculate the signature of <paramref name="data"/>
        /// </summary>
        /// <param name="data">The data to sign</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="tokenSecret">The token secret</param>
        /// <returns>The signature</returns>
        public string CalculateSignature(byte[] data, string consumerSecret, string tokenSecret)
        {
            var key = string.Format("{0}&{1}", consumerSecret, tokenSecret);
            var keyData = _encoding.GetBytes(key);
            var digest = new Sha1Digest();
            var crypto = new HMac(digest);
            crypto.Init(new KeyParameter(keyData));
            crypto.BlockUpdate(data, 0, data.Length);
            var hash = MacUtilities.DoFinal(crypto);
            return Convert.ToBase64String(hash);
        }
    }
}
