using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zlib.Portable.IO
{
    static class Path2
    {
        /// <summary>Provides a platform-specific character used to separate directory levels in a path string that reflects a hierarchical file system organization.</summary>
        /// <filterpriority>1</filterpriority>
        public readonly static char DirectorySeparatorChar;

        /// <summary>Provides a platform-specific alternate character used to separate directory levels in a path string that reflects a hierarchical file system organization.</summary>
        /// <filterpriority>1</filterpriority>
        public readonly static char AltDirectorySeparatorChar;

        /// <summary>Provides a platform-specific volume separator character.</summary>
        /// <filterpriority>1</filterpriority>
        public readonly static char VolumeSeparatorChar;

        /// <summary>A platform-specific separator character used to separate path strings in environment variables.</summary>
        /// <filterpriority>1</filterpriority>
        public readonly static char PathSeparator;

        static Path2()
        {
            DirectorySeparatorChar = '\\';
            AltDirectorySeparatorChar = '/';
            VolumeSeparatorChar = ':';
            PathSeparator = ';';
        }

        internal static void CheckInvalidPathChars(string path, bool checkAdditional = false)
        {
            if (path == null) 
                throw new ArgumentNullException("path");
            if (!HasIllegalCharacters(path, checkAdditional))
                return;
            throw new ArgumentException("The path has invalid characters.", "path");
        }

        internal static bool HasIllegalCharacters(string path, bool checkAdditional)
        {
            int num = 0;
            while (num < path.Length)
            {
                int num1 = path[num];
                if (num1 == 34 || num1 == 60 || num1 == 62 || num1 == 124 || num1 < 32)
                {
                    return true;
                }
                if (!checkAdditional || num1 != 63 && num1 != 42)
                {
                    num++;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetFileName(string path)
        {
            if (path == null)
                return null;
            CheckInvalidPathChars(path, false);
            var length = path.Length;
            var num = length;
            char chr;
            do
            {
                var num1 = num - 1;
                num = num1;
                if (num1 < 0)
                {
                    return path;
                }
                chr = path[num];
            }
            while (chr != DirectorySeparatorChar && chr != AltDirectorySeparatorChar && chr != VolumeSeparatorChar);
            return path.Substring(num + 1, length - num - 1);
        }
    }
}
