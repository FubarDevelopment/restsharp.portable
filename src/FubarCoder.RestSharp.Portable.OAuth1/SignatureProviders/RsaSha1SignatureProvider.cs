#if !USE_BOUNCYCASTLE
using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace RestSharp.Portable.OAuth1.SignatureProviders
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
        public string Id => "RSA-SHA1";

        /// <summary>
        /// Calculate the signature of <paramref name="data"/>
        /// </summary>
        /// <param name="data">The data to sign</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="tokenSecret">The token secret</param>
        /// <returns>The signature</returns>
        public string CalculateSignature(byte[] data, string consumerSecret, string tokenSecret)
        {
            var hash = CalculateHash(data);
#if NET40 || NET45
            var signatureFormatter = new RSAPKCS1SignatureFormatter(_privateKey);
            signatureFormatter.SetHashAlgorithm("SHA1");
            var signature = signatureFormatter.CreateSignature(hash);
#else
            var signature = _privateKey.SignHash(hash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
#endif
            return Convert.ToBase64String(signature);
        }

        private static byte[] CalculateHash(byte[] data)
        {
            using (var hasher = SHA1.Create())
            {
                Debug.Assert(hasher != null, "hasher != null");
                return hasher.ComputeHash(data);
            }
        }
    }
}
#endif
