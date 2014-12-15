using Ionic.Zlib;

namespace RestSharp.Portable.Encodings
{
    /// <summary>
    /// Deflate content encoding handler
    /// </summary>
    public class DeflateEncoding : IEncoding
    {
        /// <summary>
        /// Decode the content
        /// </summary>
        /// <param name="data">Content to decode</param>
        /// <returns>Decoded content</returns>
        public byte[] Decode(byte[] data)
        {
            return DeflateStream.UncompressBuffer(data);
        }
    }
}
