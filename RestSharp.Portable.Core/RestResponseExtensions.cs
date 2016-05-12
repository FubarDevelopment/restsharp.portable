using System.Text;
using System.IO;

namespace RestSharp.Portable
{
    /// <summary>
    /// Extensions for <see cref="RestResponse"/>
    /// </summary>
    public static class RestResponseExtensions
    {
        /// <summary>
        /// Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
        /// http://www.shrinkrays.net/code-snippets/csharp/an-extension-method-for-converting-a-byte-array-to-a-string.aspx
        /// </summary>
        /// <param name="buffer">An array of bytes to convert</param>
        /// <returns>The byte as a string.</returns>
        public static string AsString(this byte[] buffer)
        {
            if (buffer == null)
            {
                return "";
            }

            // Ansi as default
            Encoding encoding = Encoding.UTF8;

#if !PCL
            return encoding.GetString(buffer, 0, buffer.Length);
#else
            if (buffer.Length == 0)
                return "";

            /*
                EF BB BF            UTF-8 
                FF FE UTF-16        little endian 
                FE FF UTF-16        big endian 
                FF FE 00 00         UTF-32, little endian 
                00 00 FE FF         UTF-32, big-endian 
            */

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
            {
                encoding = Encoding.UTF8;
            }
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
            {
                encoding = Encoding.Unicode;
            }
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
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
            #endif
        }
    }
}

