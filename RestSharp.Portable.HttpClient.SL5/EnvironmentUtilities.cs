using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.HttpClient
{
    internal static class EnvironmentUtilities
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

        public static bool IsSilverlight
        {
            get
            {
                return true;
            }
        }
    }
}
