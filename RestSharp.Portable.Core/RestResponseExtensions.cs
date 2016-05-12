using System.IO;
using System.Text;

namespace RestSharp.Portable
{
    /// <summary>
    /// Extensions for <see cref="RestResponse"/>
    /// </summary>
    internal static class RestResponseExtensions
    {
        /// <summary>
        /// The default encoding, when none could be detected
        /// </summary>
        private static readonly Encoding _defaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
        /// http://www.shrinkrays.net/code-snippets/csharp/an-extension-method-for-converting-a-byte-array-to-a-string.aspx
        /// </summary>
        /// <param name="response">The REST response</param>
        /// <returns><see cref="IRestResponse.RawBytes"/> as a string</returns>
        public static string GetStringContent(this IRestResponse response)
        {
            var buffer = response.RawBytes;

            if (buffer == null)
                return string.Empty;

            // UTF-8 as default
            var encoding = _defaultEncoding;

            if (buffer.Length == 0)
                return string.Empty;

            /*
                EF BB BF            UTF-8
                FF FE UTF-16        little endian
                FE FF UTF-16        big endian
                FF FE 00 00         UTF-32, little endian
                00 00 FE FF         UTF-32, big-endian
            */

            if (buffer.Length > 1 && buffer[0] == 0xfe && buffer[1] == 0xff)
            {
                encoding = Encoding.Unicode;
            }
            else if (buffer.Length > 1 && buffer[0] == 0xfe && buffer[1] == 0xff)
            {
                encoding = Encoding.BigEndianUnicode; // utf-16be
            }

            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(buffer, 0, buffer.Length);
                stream.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
