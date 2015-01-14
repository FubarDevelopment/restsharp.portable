using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RestSharp.Portable
{
    /// <summary>
    /// URL utility functions
    /// </summary>
    internal static class UrlUtility
    {
        //private static readonly byte[] s_reserved = Encoding.UTF8.GetBytes(";/?:@&=+$,");
        private static readonly byte[] s_lowalpha = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz");
        private static readonly byte[] s_upalpha = Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        private static readonly byte[] s_alpha = s_lowalpha.Union(s_upalpha).ToArray();
        private static readonly byte[] s_digit = Encoding.UTF8.GetBytes("0123456789");
        internal static readonly byte[] s_alphanum = s_alpha.Union(s_digit).ToArray();
        private static readonly byte[] s_mark = Encoding.UTF8.GetBytes("-_.!~*'()");
        internal static readonly byte[] s_unreserved = s_alphanum.Union(s_mark).ToArray();
        //private static readonly byte[] s_control = Encoding.UTF8.GetBytes("\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\u0009\u000a\u000b\u000c\u000d\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f");
        //private static readonly byte[] s_space = Encoding.UTF8.GetBytes(" ");
        //private static readonly byte[] s_delims = Encoding.UTF8.GetBytes("<>#%\"");
        //private static readonly byte[] s_unwise = Encoding.UTF8.GetBytes("{}|\\^[]`");

        private static readonly UrlEscapeUtility s_defaultEscapeUtility = new UrlEscapeUtility();

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Escape(string data)
        {
            return s_defaultEscapeUtility.Escape(data);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Escape(string data, Encoding encoding)
        {
            return s_defaultEscapeUtility.Escape(data, encoding);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Escape(byte[] data)
        {
            return s_defaultEscapeUtility.Escape(data);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] EscapeToBytes(string data)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] EscapeToBytes(string data, Encoding encoding)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data, encoding);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] EscapeToBytes(byte[] data)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static string Escape(string data, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.Escape(data, flags);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static string Escape(string data, Encoding encoding, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.Escape(data, encoding, flags);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static string Escape(byte[] data, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.Escape(data, flags);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static byte[] EscapeToBytes(string data, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data, flags);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static byte[] EscapeToBytes(string data, Encoding encoding, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data, encoding, flags);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static byte[] EscapeToBytes(byte[] data, UrlEscapeFlags flags)
        {
            return s_defaultEscapeUtility.EscapeToBytes(data, flags);
        }
    }
}
