using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable
{
    internal static class EnvironmentUtilities
    {
        public static bool IsMono
        {
            get { return true; }
        }

        public static bool IsSilverlight
        {
            get
            {
                return false;
            }
        }
    }
}
