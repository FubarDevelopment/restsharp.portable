using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable
{
    /// <summary>
    /// Compares parameters by name
    /// </summary>
    public class ParameterNameComparer : IEqualityComparer<Parameter>
    {
        /// <summary>
        /// Parameters have the same name?
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(Parameter x, Parameter y)
        {
            return (x.Name ?? string.Empty) == (y.Name ?? string.Empty) && x.Type == y.Type;
        }

        /// <summary>
        /// Calculate the hash code for a given parameter
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(Parameter obj)
        {
            return (obj.Name ?? string.Empty).GetHashCode() ^ obj.Type.GetHashCode();
        }

        private static readonly ParameterNameComparer _default = new ParameterNameComparer();

        /// <summary>
        /// The default parameter name comparer instance
        /// </summary>
        public static ParameterNameComparer Default { get { return _default; } }
    }
}
