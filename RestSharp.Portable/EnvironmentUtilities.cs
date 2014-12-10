using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable
{
    static class EnvironmentUtilities
    {
        private static bool? _isMono;
        public static bool IsMono
        {
            get
            {
                if (_isMono == null)
                    _isMono = Type.GetType("Mono.Runtime") != null;
                return _isMono.Value;
            }
        }

        private static bool? _isSilverlight;
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
