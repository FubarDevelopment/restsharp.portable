using System.IO;
using System.Net.Http.Headers;

namespace RestSharp.Portable
{
    /// <summary>
    /// Container for files to be uploaded with requests
    /// </summary>
    public class FileParameter : Parameter
    {
        /// <summary>
        /// Gets or sets the length of data to be sent
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// Gets or sets the name of the file to use when uploading
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///  Creates a file parameter from an array of bytes.
        /// </summary>
        /// <param name="name">The parameter name to use in the request.</param>
        /// <param name="data">The data to use as the file's contents.</param>
        /// <param name="filename">The filename to use in the request.</param>
        /// <param name="contentType">The content type to use in the request.</param>
        /// <returns>The <see cref="FileParameter"/></returns>
        public static FileParameter Create(string name, byte[] data, string filename, MediaTypeHeaderValue contentType)
        {
            var length = (long)data.Length;
            return new FileParameter
            {
                Value = data,
                FileName = filename,
                ContentType = contentType,
                ContentLength = length,
                Name = name
            };
        }

        /// <summary>
        ///  Creates a file parameter from an array of bytes.
        /// </summary>
        /// <param name="name">The parameter name to use in the request.</param>
        /// <param name="data">The data to use as the file's contents.</param>
        /// <param name="filename">The filename to use in the request.</param>
        /// <returns>The <see cref="FileParameter"/> using the default content type.</returns>
        public static FileParameter Create(string name, byte[] data, string filename)
        {
            return Create(name, data, filename, new MediaTypeHeaderValue("application/octet-stream"));
        }

        /// <summary>
        ///  Creates a file parameter from an array of bytes.
        /// </summary>
        /// <param name="name">The parameter name to use in the request.</param>
        /// <param name="input">The input stream for the file's contents.</param>
        /// <param name="filename">The filename to use in the request.</param>
        /// <param name="contentType">The content type to use in the request.</param>
        /// <returns>The <see cref="FileParameter"/></returns>
        public static FileParameter Create(string name, Stream input, string filename, MediaTypeHeaderValue contentType)
        {
            var temp = new MemoryStream();
            input.CopyTo(temp);
            var data = temp.ToArray();
            return Create(name, data, filename, contentType);
        }

        /// <summary>
        ///  Creates a file parameter from an array of bytes.
        /// </summary>
        /// <param name="name">The parameter name to use in the request.</param>
        /// <param name="input">The input stream for the file's contents.</param>
        /// <param name="filename">The filename to use in the request.</param>
        /// <returns>The <see cref="FileParameter"/> using the default content type.</returns>
        public static FileParameter Create(string name, Stream input, string filename)
        {
            return Create(name, input, filename, new MediaTypeHeaderValue("application/octet-stream"));
        }
    }
}
