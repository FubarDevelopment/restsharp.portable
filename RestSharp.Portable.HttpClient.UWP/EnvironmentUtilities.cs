using System;

namespace RestSharp.Portable.HttpClient
{
    /// <summary>
    /// Provides hints about the environment (.NET runtime)
    /// </summary>
    internal static class EnvironmentUtilities
    {
        /// <summary>
        /// Gets a value indicating whether this library runs in the Mono environment.
        /// </summary>
        public static bool IsMono => false;

        /// <summary>
        /// Gets a value indicating whether this library runs in the Silverlight environment.
        /// </summary>
        public static bool IsSilverlight => false;
    }
}
