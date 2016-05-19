using System.IO;
#if !NETSTANDARD1_0 && !PROFILE328
using System.IO.Compression;
#endif

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
#if NETSTANDARD1_0 || PROFILE328
            return Zlib.Portable.GZipStream.UncompressBuffer(data);
#else
            var output = new MemoryStream();
            var input = new MemoryStream(data);
            using (var stream = new GZipStream(input, CompressionMode.Decompress))
                stream.CopyTo(output);
            return output.ToArray();
#endif
        }
    }
}
