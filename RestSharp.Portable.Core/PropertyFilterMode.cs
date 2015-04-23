using System.Collections.Generic;

namespace RestSharp.Portable
{
    /// <summary>
    /// Filter mode for the <see cref="RestRequestExtensions.AddObject(IRestRequest, object, IEnumerable{string}, PropertyFilterMode)"/> extension method
    /// </summary>
    public enum PropertyFilterMode
    {
        /// <summary>
        /// Include the given properties
        /// </summary>
        Include,

        /// <summary>
        /// Exclude the given properties
        /// </summary>
        Exclude,
    }
}
