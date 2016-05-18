using System;

namespace RestSharp.Portable.HttpClient
{
    /// <summary>
    /// Provides hints about the environment (.NET runtime)
    /// </summary>
    internal static class EnvironmentUtilities
    {
        private static bool? _isMono;

        private static bool? _isSilverlight;

        /// <summary>
        /// Gets a value indicating whether this library runs in the Mono environment.
        /// </summary>
        public static bool IsMono
        {
            get
            {
                if (_isMono == null)
                {
                    _isMono = Type.GetType("Mono.Runtime") != null;
                }

                return _isMono.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this library runs in the Silverlight environment.
        /// </summary>
        public static bool IsSilverlight
        {
            get
            {
                if (_isSilverlight == null)
                {
                    try
                    {
                        var t = Type.GetType("System.Windows.Browser.BrowserInformation, System.Windows.Browser, PublicKeyToken=7cec85d7bea7798e", false);
                        _isSilverlight = t != null;
                    }
                    catch (System.IO.IOException)
                    {
                        // System.IO.FileLoadException
                        _isSilverlight = true;
                    }
                }
                return _isSilverlight.Value;
            }
        }
    }
}
