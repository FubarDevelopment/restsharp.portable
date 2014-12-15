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
