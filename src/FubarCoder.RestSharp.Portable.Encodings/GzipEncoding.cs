using System.IO;
using System.IO.Compression;

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
            var output = new MemoryStream();
            var input = new MemoryStream(data);
            using (var stream = new GZipStream(input, CompressionMode.Decompress))
                stream.CopyTo(output);
            return output.ToArray();
        }
    }
}
