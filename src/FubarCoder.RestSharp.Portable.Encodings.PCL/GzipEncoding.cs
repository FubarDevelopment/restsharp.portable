using Zlib.Portable;

namespace RestSharp.Portable.Encodings
{
    /// <summary>
    /// GZIP content encoding handler
    /// </summary>
    public class GzipEncoding : IEncoding
    {
        /// <summary>
        /// Decode the content
        /// </summary>
        /// <param name="data">Content to decode</param>
        /// <returns>Decoded content</returns>
        public byte[] Decode(byte[] data)
        {
            return GZipStream.UncompressBuffer(data);
        }
    }
}
