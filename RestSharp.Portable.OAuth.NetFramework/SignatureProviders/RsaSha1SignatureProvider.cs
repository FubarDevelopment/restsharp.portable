using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace RestSharp.Portable.Authenticators.OAuth.SignatureProviders
{
    /// <summary>
    /// Calculates a <code>RSA-SHA1</code> signature
    /// </summary>
    public class RsaSha1SignatureProvider : ISignatureProvider
    {
        private readonly RSA _privateKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaSha1SignatureProvider"/> class.
        /// </summary>
        /// <param name="privateKey">The private key to use to calculate the signature</param>
        public RsaSha1SignatureProvider(RSA privateKey)
        {
            _privateKey = privateKey;
        }

        /// <summary>
        /// The ID (also used as signature method name for requests) of the signature provider
        /// </summary>
        public string Id
        {
            get { return "RSA-SHA1"; }
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
            var signatureFormatter = new RSAPKCS1SignatureFormatter(_privateKey);
            signatureFormatter.SetHashAlgorithm("SHA1");
            using (var hasher = HashAlgorithm.Create("SHA1"))
            {
                Debug.Assert(hasher != null, "hasher != null");
                var hash = hasher.ComputeHash(data);
                var signature = signatureFormatter.CreateSignature(hash);
                return Convert.ToBase64String(signature);
            }
        }
    }
}
