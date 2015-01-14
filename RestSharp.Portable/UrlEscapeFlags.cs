using System;

namespace RestSharp.Portable
{
    /// <summary>
    /// Modifies how the URL escape functions work
    /// </summary>
    [Flags]
    internal enum UrlEscapeFlags
    {
        /// <summary>
        /// Default behavior
        /// </summary>
        Default = 0x0000,

        /// <summary>
        /// Compatibility with Uri.EscapeDataString(string)
        /// </summary>
        LikeEscapeDataString = 0x0000,
        /// <summary>
        /// Compatibility with HttpUtility.UrlEncode(string)
        /// </summary>
        LikeUrlEncode = EscapeSpaceAsPlus | LowerCaseHexCharacters | AllowLikeUrlEncode,

        /// <summary>
        /// Allow all characters that are allowed by EscapeDataString
        /// </summary>
        AllowLikeEscapeDataString = 0x0000,
        /// <summary>
        /// Allow all unreserved characters and not just -_.!~
        /// </summary>
        AllowAllUnreserved = 0x0001,
        /// <summary>
        /// Allow all characters that are allowed by UrlEncode
        /// </summary>
        AllowLikeUrlEncode = 0x0002,
        /// <summary>
        /// The mask for all variants of allowed character sets
        /// </summary>
        AllowMask = 0x000F,

        /// <summary>
        /// Lower case hexadecimal characters
        /// </summary>
        LowerCaseHexCharacters = 0x0010,
        /// <summary>
        /// Escapes a space character (0x20) as plus
        /// </summary>
        EscapeSpaceAsPlus       = 0x0020,

        /// <summary>
        /// The mask for all builder variants
        /// </summary>
        BuilderVariantMask          = 0x0F00,
        /// <summary>
        /// Use a list of byte arrays to build the result
        /// </summary>
        BuilderVariantListByteArray = 0x0000,
        /// <summary>
        /// Use a list of bytes to build the result
        /// </summary>
        BuilderVariantListByte      = 0x0100,
    }
}
