using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Encodings
{
    public class GzipEncoding : IEncoding
    {
        public byte[] Encode(byte[] data)
        {
            var output = new MemoryStream();
            using (var stream = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress))
                stream.Write(data, 0, data.Length);
            return output.ToArray();
        }

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
