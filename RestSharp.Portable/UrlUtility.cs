using System;
using System.Linq;
using System.Text;

namespace RestSharp.Portable
{
    /// <summary>
    /// URL utility functions
    /// </summary>
    internal static class UrlUtility
    {
        private static readonly byte[] s_lowalpha = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz");
        private static readonly byte[] s_upalpha = Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        private static readonly byte[] s_alpha = s_lowalpha.Union(s_upalpha).ToArray();
        private static readonly byte[] s_digit = Encoding.UTF8.GetBytes("0123456789");
        private static readonly byte[] s_mark = Encoding.UTF8.GetBytes("-_.!~*'()");
        private static readonly UrlEscapeUtility s_defaultEscapeUtility = new UrlEscapeUtility();

        private static byte[] s_alphanum;

        private static byte[] s_unreserved;

        internal static byte[] AlphaNum
        {
            get { return s_alphanum ?? (s_alphanum = s_alpha.Union(s_digit).ToArray()); }
        }

        internal static byte[] Unreserved
        {
            get { return s_unreserved ?? (s_unreserved = AlphaNum.Union(s_mark).ToArray()); }
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escaped</param>
        /// <returns>Returns the escaped data</returns>
        public static string Escape(string data)
        {
            return s_defaultEscapeUtility.Escape(data);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="encoding">The encoding to use</param>
        /// <returns>Returns the escaped data</returns>
        public static string Escape(string data, Encoding encoding)
        {
            return s_defaultEscapeUtility.Escape(data, encoding);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <returns>Returns the escaped data</returns>
        public static string Escape(byte[] data)
        {
            return s_defaultEscapeUtility.Escape(data);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <returns>Returns the escaped data</returns>
        public static byte[] EscapeToBytes(string data)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="encoding">The encoding to use to convert the string into a byte array</param>
        /// <returns>Returns the escaped data</returns>
        public static byte[] EscapeToBytes(string data, Encoding encoding)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data, encoding);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <returns>Returns the escaped data</returns>
        public static byte[] EscapeToBytes(byte[] data)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the escaping behavior</param>
        /// <returns>Returns the escaped data</returns>
        public static string Escape(string data, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.Escape(data, flags);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="encoding">The encoding to use to convert the string into a byte array</param>
        /// <param name="flags">The flags to modify the escaping behavior</param>
        /// <returns>Returns the escaped data</returns>
        public static string Escape(string data, Encoding encoding, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.Escape(data, encoding, flags);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the escaping behavior</param>
        /// <returns>Returns the escaped data</returns>
        public static string Escape(byte[] data, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.Escape(data, flags);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the escaping behavior</param>
        /// <returns>Returns the escaped data</returns>
        public static byte[] EscapeToBytes(string data, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data, flags);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="encoding">The encoding to use to convert the string into a byte array</param>
        /// <param name="flags">The flags to modify the escaping behavior</param>
        /// <returns>Returns the escaped data</returns>
        public static byte[] EscapeToBytes(string data, Encoding encoding, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data, encoding, flags);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the escaping behavior</param>
        /// <returns>Returns the escaped data</returns>
        public static byte[] EscapeToBytes(byte[] data, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data, flags);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to compute the escaped length for</param>
        /// <returns>Returns the length of the escaped data</returns>
        public static long ComputeLength(string data)
        {
            return s_defaultEscapeUtility.ComputeLength(data);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to compute the escaped length for</param>
        /// <param name="encoding">The encoding to use to convert the string into a byte array</param>
        /// <returns>Returns the length of the escaped data</returns>
        public static long ComputeLength(string data, Encoding encoding)
        {
            return s_defaultEscapeUtility.ComputeLength(data, encoding);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to compute the escaped length for</param>
        /// <param name="flags">The flags to modify the escaping behavior</param>
        /// <returns>Returns the length of the escaped data</returns>
        public static long ComputeLength(string data, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.ComputeLength(data, flags);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to compute the escaped length for</param>
        /// <param name="encoding">The encoding to use to convert the string into a byte array</param>
        /// <param name="flags">The flags to modify the escaping behavior</param>
        /// <returns>Returns the length of the escaped data</returns>
        public static long ComputeLength(string data, Encoding encoding, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.ComputeLength(data, encoding, flags);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to compute the escaped length for</param>
        /// <returns>Returns the length of the escaped data</returns>
        public static long ComputeLength(byte[] data)
        {
            return s_defaultEscapeUtility.ComputeLength(data);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to compute the escaped length for</param>
        /// <param name="flags">The flags to modify the escaping behavior</param>
        /// <returns>Returns the length of the escaped data</returns>
        public static long ComputeLength(byte[] data, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.ComputeLength(data, flags);
        }
    }
}
