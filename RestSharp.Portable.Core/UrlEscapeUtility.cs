using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
#if HAS_SYSTEM_WEB
using System.Web;
#endif

namespace RestSharp.Portable
{
    /// <summary>
    /// Helper class for URL escaping of data
    /// </summary>
    internal class UrlEscapeUtility
    {
        private static readonly Encoding s_defaultEncoding = new UTF8Encoding(false);

        private static readonly byte[] s_hexCharsLow = Encoding.UTF8.GetBytes("0123456789abcdef");
        private static readonly byte[] s_hexCharsUp = Encoding.UTF8.GetBytes("0123456789ABCDEF");

        private static readonly Dictionary<UrlEscapeFlags, ISet<byte>> s_allowedBytes = new Dictionary<UrlEscapeFlags, ISet<byte>>
        {
            { UrlEscapeFlags.AllowLikeEscapeDataString, new HashSet<byte>(UrlUtility.AlphaNum.Union(Encoding.UTF8.GetBytes("-_.~"))) },
            { UrlEscapeFlags.AllowAllUnreserved, new HashSet<byte>(UrlUtility.Unreserved) },
            { UrlEscapeFlags.AllowLikeUrlEncode, new HashSet<byte>(UrlUtility.AlphaNum.Union(Encoding.UTF8.GetBytes("-_.!*()"))) },
            { UrlEscapeFlags.AllowLikeEscapeUriString, new HashSet<byte>(UrlUtility.Unreserved.Union(Encoding.UTF8.GetBytes(":&/?+#=$,;@[]"))) },
            { UrlEscapeFlags.AllowLikeWebRequest, new HashSet<byte>(UrlUtility.AlphaNum.Union(Encoding.UTF8.GetBytes("-_.~")).Union(Encoding.UTF8.GetBytes(":"))) },
        };

        private static readonly Dictionary<UrlEscapeFlags, EscapeBuilderDelegate> s_escapeBuilders = new Dictionary<UrlEscapeFlags, EscapeBuilderDelegate>
        {
            { UrlEscapeFlags.BuilderVariantListByte, EscapeToBytes1 },
            { UrlEscapeFlags.BuilderVariantListByteArray, EscapeToBytes2 },
        };

        private readonly bool _allowNativeConversion;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlEscapeUtility"/> class.
        /// </summary>
        public UrlEscapeUtility()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlEscapeUtility"/> class.
        /// </summary>
        /// <param name="allowNativeConversion">Is the usage of the native converison functions allowed?</param>
        public UrlEscapeUtility(bool allowNativeConversion)
        {
            _allowNativeConversion = allowNativeConversion;
        }

        private delegate byte[] EscapeBuilderDelegate(byte[] data, UrlEscapeFlags flags, byte[] hexChars, ISet<byte> allowedBytes);

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <returns>The escaped data</returns>
        public static long ComputeLength(string data)
        {
            return ComputeLength(data, UrlEscapeFlags.Default);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="encoding">The encoding to use to convert the string to a byte array</param>
        /// <returns>The escaped data</returns>
        public static long ComputeLength(string data, Encoding encoding)
        {
            return ComputeLength(data, encoding, UrlEscapeFlags.Default);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <returns>The escaped data</returns>
        public static long ComputeLength(string data, UrlEscapeFlags flags)
        {
            return ComputeLength(data, s_defaultEncoding, flags);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="encoding">The encoding to use to convert the string to a byte array</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <returns>The escaped data</returns>
        public static long ComputeLength(string data, Encoding encoding, UrlEscapeFlags flags)
        {
            return ComputeLength(encoding.GetBytes(data), flags);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <returns>The escaped data</returns>
        public static long ComputeLength(byte[] data)
        {
            return ComputeLength(data, UrlEscapeFlags.Default);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <returns>The escaped data</returns>
        public static long ComputeLength(byte[] data, UrlEscapeFlags flags)
        {
            var allowedBytesIndex = flags & UrlEscapeFlags.AllowMask;
            ISet<byte> allowedBytes;
            if (!s_allowedBytes.TryGetValue(allowedBytesIndex, out allowedBytes))
            {
                allowedBytes = s_allowedBytes[UrlEscapeFlags.Default];
            }

            return ComputeLength(data, flags, allowedBytes);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <returns>The escaped data</returns>
        public string Escape(string data)
        {
            return Escape(data, UrlEscapeFlags.Default);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="encoding">The encoding to use to convert the string to a byte array</param>
        /// <returns>The escaped data</returns>
        public string Escape(string data, Encoding encoding)
        {
            return Escape(data, encoding, UrlEscapeFlags.Default);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <returns>The escaped data</returns>
        public string Escape(byte[] data)
        {
            return Escape(data, UrlEscapeFlags.Default);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <returns>The escaped data</returns>
        public byte[] EscapeToBytes(string data)
        {
            return EscapeToBytes(data, UrlEscapeFlags.Default);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="encoding">The encoding to use to convert the string to a byte array</param>
        /// <returns>The escaped data</returns>
        public byte[] EscapeToBytes(string data, Encoding encoding)
        {
            return EscapeToBytes(data, encoding, UrlEscapeFlags.Default);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <returns>The escaped data</returns>
        public byte[] EscapeToBytes(byte[] data)
        {
            return EscapeToBytes(data, UrlEscapeFlags.Default);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <returns>The escaped data</returns>
        public string Escape(string data, UrlEscapeFlags flags)
        {
#if HAS_SYSTEM_WEB
            if (_allowNativeConversion && flags == UrlEscapeFlags.LikeUrlEncode)
                return HttpUtility.UrlEncode(data);
#endif
            return Escape(data, s_defaultEncoding, flags);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="encoding">The encoding to use to convert the string to a byte array</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <returns>The escaped data</returns>
        public string Escape(string data, Encoding encoding, UrlEscapeFlags flags)
        {
#if HAS_SYSTEM_WEB
            if (_allowNativeConversion && flags == UrlEscapeFlags.LikeUrlEncode)
                return HttpUtility.UrlEncode(data, encoding);
#endif
            if (_allowNativeConversion && (flags == UrlEscapeFlags.LikeEscapeDataString) && string.Equals(encoding.WebName, "utf-8", StringComparison.OrdinalIgnoreCase))
                return EscapeDataString(data);
            return ConvertEscapedBytesToString(EscapeToBytes(data, encoding, flags));
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <returns>The escaped data</returns>
        public string Escape(byte[] data, UrlEscapeFlags flags)
        {
#if HAS_SYSTEM_WEB
            if (_allowNativeConversion && flags == UrlEscapeFlags.LikeUrlEncode)
                return HttpUtility.UrlEncode(data);
#endif
            return ConvertEscapedBytesToString(EscapeToBytes(data, flags));
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <returns>The escaped data</returns>
        public byte[] EscapeToBytes(string data, UrlEscapeFlags flags)
        {
            return EscapeToBytes(data, s_defaultEncoding, flags);
        }

        /// <summary>
        /// URL escape
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="encoding">The encoding to use to convert the string to a byte array</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <returns>The escaped data</returns>
        public byte[] EscapeToBytes(string data, Encoding encoding, UrlEscapeFlags flags)
        {
#if HAS_SYSTEM_WEB
            if (_allowNativeConversion && flags == UrlEscapeFlags.LikeUrlEncode)
                return HttpUtility.UrlEncodeToBytes(data, encoding);
#endif
            return EscapeToBytes(encoding.GetBytes(data), flags);
        }

        /// <summary>
        /// URL escape for bytes
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <returns>The escaped data</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Cannot be static when HAS_SYSTEM_WEB is defined.")]
        public byte[] EscapeToBytes(byte[] data, UrlEscapeFlags flags)
        {
#if HAS_SYSTEM_WEB
            if (_allowNativeConversion && flags == UrlEscapeFlags.LikeUrlEncode)
                return HttpUtility.UrlEncodeToBytes(data);
#endif
            var hexChars = ((flags & UrlEscapeFlags.LowerCaseHexCharacters) != UrlEscapeFlags.Default) ? s_hexCharsLow : s_hexCharsUp;

            var allowedBytesIndex = flags & UrlEscapeFlags.AllowMask;
            ISet<byte> allowedBytes;
            if (!s_allowedBytes.TryGetValue(allowedBytesIndex, out allowedBytes))
            {
                allowedBytes = s_allowedBytes[UrlEscapeFlags.Default];
            }

            var builderVariant = flags & UrlEscapeFlags.BuilderVariantMask;
            EscapeBuilderDelegate builder;
            if (!s_escapeBuilders.TryGetValue(builderVariant, out builder))
            {
                builder = EscapeToBytes2;
            }

            return builder(data, flags, hexChars, allowedBytes);
        }

        /// <summary>
        /// Compute length of the data after escaping its values
        /// </summary>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <param name="allowedBytes">The bytes that don't need to be escaped</param>
        /// <returns>The escaped data</returns>
        private static long ComputeLength(byte[] data, UrlEscapeFlags flags, ISet<byte> allowedBytes)
        {
            var escapeSpaceAsPlus = (flags & UrlEscapeFlags.EscapeSpaceAsPlus) == UrlEscapeFlags.EscapeSpaceAsPlus;
            long computedLength = 0;

#if PCL || SILVERLIGHT
            long len = data.Length;
#else
            var len = data.LongLength;
#endif
            for (long i = 0; i != len; ++i)
            {
                var v = data[i];
                if (allowedBytes.Contains(v))
                {
                    computedLength += 1;
                }
                else if (v == 0x20)
                {
                    if (escapeSpaceAsPlus)
                    {
                        computedLength += 1;
                    }
                    else
                    {
                        computedLength += 3;
                    }
                }
                else
                {
                    computedLength += 3;
                }
            }

            return computedLength;
        }

        /// <summary>
        /// Variant 1 of EscapeToBytes
        /// </summary>
        /// <remarks>
        /// This variant uses a list of bytes.
        /// </remarks>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <param name="hexChars">The HEX characters used to write the output</param>
        /// <param name="allowedBytes">The bytes that don't need to be escaped</param>
        /// <returns>The escaped data</returns>
        private static byte[] EscapeToBytes1(byte[] data, UrlEscapeFlags flags, byte[] hexChars, ISet<byte> allowedBytes)
        {
            var encodedByte = new byte[3];
            encodedByte[0] = (byte)'%';

            var escapeSpaceAsPlus = (flags & UrlEscapeFlags.EscapeSpaceAsPlus) == UrlEscapeFlags.EscapeSpaceAsPlus;
            var output = new List<byte>(data.Length);
            var len = data.Length;
            var escapeByte = false;
            for (var i = 0; i != len; ++i)
            {
                var v = data[i];
                if (allowedBytes.Contains(v))
                {
                    output.Add(v);
                }
                else if (v == 0x20)
                {
                    if (escapeSpaceAsPlus)
                    {
                        output.Add((byte)'+');
                    }
                    else
                    {
                        escapeByte = true;
                    }
                }
                else
                {
                    escapeByte = true;
                }

                if (escapeByte)
                {
                    escapeByte = false;
                    var hiByte = (v >> 4) & 0x0F;
                    var loByte = v & 0x0F;
                    encodedByte[1] = hexChars[hiByte];
                    encodedByte[2] = hexChars[loByte];
                    output.AddRange(encodedByte);
                }
            }

            return output.ToArray();
        }

        /// <summary>
        /// Variant 2 of EscapeToBytes
        /// </summary>
        /// <remarks>
        /// This variant uses a list of byte arrays
        /// </remarks>
        /// <param name="data">The data to escape</param>
        /// <param name="flags">The flags to modify the behavior</param>
        /// <param name="hexChars">The HEX characters used to write the output</param>
        /// <param name="allowedBytes">The bytes that don't need to be escaped</param>
        /// <returns>The escaped data</returns>
        private static byte[] EscapeToBytes2(byte[] data, UrlEscapeFlags flags, byte[] hexChars, ISet<byte> allowedBytes)
        {
            var escapeSpaceAsPlus = (flags & UrlEscapeFlags.EscapeSpaceAsPlus) == UrlEscapeFlags.EscapeSpaceAsPlus;
            var output = new List<byte[]>();
            var outputEscaped = new List<byte>();
            var outputSize = 0;
            var len = data.Length;
            var afterLastValid = 0;
            var escapeByte = false;
            for (var i = 0; i != len; ++i)
            {
                var v = data[i];
                if (allowedBytes.Contains(v))
                {
                    continue;
                }

                var validBytes = i - afterLastValid;
                if (validBytes != 0)
                {
                    if (outputEscaped.Count != 0)
                    {
                        var tempEscaped = outputEscaped.ToArray();
                        outputSize += tempEscaped.Length;
                        output.Add(tempEscaped);
                        outputEscaped = new List<byte>();
                    }

                    var temp = new byte[validBytes];
                    Array.Copy(data, afterLastValid, temp, 0, validBytes);
                    output.Add(temp);
                    outputSize += validBytes;
                }

                if (v == 0x20)
                {
                    if (escapeSpaceAsPlus)
                    {
                        outputEscaped.Add((byte)'+');
                    }
                    else
                    {
                        escapeByte = true;
                    }
                }
                else
                {
                    escapeByte = true;
                }

                if (escapeByte)
                {
                    escapeByte = false;
                    var hiByte = (v >> 4) & 0x0F;
                    var loByte = v & 0x0F;
                    outputEscaped.Add((byte)'%');
                    outputEscaped.Add(hexChars[hiByte]);
                    outputEscaped.Add(hexChars[loByte]);
                }

                afterLastValid = i + 1;
            }

            if (outputEscaped.Count != 0)
            {
                var tempEscaped = outputEscaped.ToArray();
                outputSize += tempEscaped.Length;
                output.Add(tempEscaped);
            }

            {
                var validBytes = len - afterLastValid;
                if (validBytes != 0)
                {
                    var temp = new byte[validBytes];
                    Array.Copy(data, afterLastValid, temp, 0, validBytes);
                    output.Add(temp);
                    outputSize += validBytes;
                }
            }

            var result = new byte[outputSize];
            var destOffset = 0;
            foreach (var part in output)
            {
                var length = part.Length;
                Array.Copy(part, 0, result, destOffset, length);
                destOffset += length;
            }

            return result;
        }

        private static string EscapeDataString(string input)
        {
            const int MaxLength = 32766;
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.Length <= MaxLength)
            {
                return Uri.EscapeDataString(input);
            }

            StringBuilder sb = new StringBuilder(input.Length * 2);
            int index = 0;
            while (index < input.Length)
            {
                int length = Math.Min(input.Length - index, MaxLength);
                string subString = input.Substring(index, length);
                sb.Append(Uri.EscapeDataString(subString));
                index += subString.Length;
            }

            return sb.ToString();
        }

        private static string ConvertEscapedBytesToString(byte[] data)
        {
            return s_defaultEncoding.GetString(data, 0, data.Length);
        }
    }
}
