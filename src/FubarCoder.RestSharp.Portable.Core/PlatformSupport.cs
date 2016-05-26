using System;

namespace RestSharp.Portable
{
    internal static class PlatformSupport
    {
#if PROFILE328 || PROFILE259
        /// <summary>
        /// Gets a value indicating whether this library runs in the Silverlight environment.
        /// </summary>
        public static bool IsSilverlight
        {
            get
            {
                try
                {
                    var t = Type.GetType("System.Windows.Browser.BrowserInformation, System.Windows.Browser, PublicKeyToken=7cec85d7bea7798e", false);
                    return t != null;
                }
                catch (System.IO.IOException)
                {
                    // System.IO.FileLoadException
                    return true;
                }
            }
        }
#elif SL5
        /// <summary>
        /// Gets a value indicating whether this library runs in the Silverlight environment.
        /// </summary>
        public static bool IsSilverlight => true;
#else
        /// <summary>
        /// Gets a value indicating whether this library runs in the Silverlight environment.
        /// </summary>
        public static bool IsSilverlight => false;
#endif

        /// <summary>
        /// Gets a value indicating whether this library runs in the Mono environment.
        /// </summary>
        public static bool IsMono => Type.GetType("Mono.Runtime") != null;
    }
}
