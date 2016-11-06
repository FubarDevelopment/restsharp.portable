using System.IO;

namespace RestSharp.Portable
{
    /// <summary>
    /// Content encoding handler interface
    /// </summary>
    public interface IEncoding
    {
        /// <summary>
        /// Decode the content
        /// </summary>
        /// <param name="data">Content to decode</param>
        /// <returns>Decoded content</returns>
        byte[] Decode(byte[] data);

        /// <summary>
        /// Decode the response stream
        /// </summary>
        /// <param name="data">Response stream to decode</param>
        /// <returns>Stream returning the decoded data</returns>
        Stream DecodeStream(Stream data);
    }
}
