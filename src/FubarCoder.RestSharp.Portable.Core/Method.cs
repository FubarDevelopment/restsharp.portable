using System.Diagnostics.CodeAnalysis;

namespace RestSharp.Portable
{
    /// <summary>
    /// HTTP method to use when making requests
    /// </summary>
    [SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules", Justification = "The casing matches the HTTP method types")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The casing matches the HTTP method types")]
    public enum Method
    {
        /// <summary>
        /// GET request
        /// </summary>
        GET,

        /// <summary>
        /// POST request
        /// </summary>
        POST,

        /// <summary>
        /// PUT request
        /// </summary>
        PUT,

        /// <summary>
        /// DELETE request
        /// </summary>
        DELETE,

        /// <summary>
        /// HEAD request
        /// </summary>
        HEAD,

        /// <summary>
        /// OPTIONS request
        /// </summary>
        OPTIONS,

        /// <summary>
        /// PATCH request
        /// </summary>
        PATCH,

        /// <summary>
        /// MERGE request
        /// </summary>
        MERGE,
    }
}
