using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Encodings
{
    /// <summary>
    /// GZIP content encoding handler
    /// </summary>
    public class GzipEncoding : IEncoding
    {
        /// <summary>
        /// Encode the content
        /// </summary>
        /// <param name="data">Content to encode</param>
        /// <returns>Encoded content</returns>
        public byte[] Encode(byte[] data)
        {
            var output = new MemoryStream();
            using (var stream = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress))
                stream.Write(data, 0, data.Length);
            return output.ToArray();
        }

        /// <summary>
        /// Decode the content
        /// </summary>
        /// <param name="data">Content to decode</param>
        /// <returns>Decoded content</returns>
        public byte[] Decode(byte[] data)
        {
            var output = new MemoryStream();
            var input = new MemoryStream(data);
            using (var stream = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress))
                stream.CopyTo(output);
            return output.ToArray();
        }
    }
}
