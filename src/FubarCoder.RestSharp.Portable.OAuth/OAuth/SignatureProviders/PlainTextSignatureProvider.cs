using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Portable.Authenticators.OAuth.SignatureProviders
{
    /// <summary>
    /// The <code>PLAINTEXT</code> signature provider
    /// </summary>
    public class PlainTextSignatureProvider : ISignatureProvider
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
            get { return "PLAINTEXT"; }
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
            return key;
        }
    }
}
