namespace RestSharp.Portable.Encodings
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
    }
}
