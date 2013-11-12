using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable
{
    public class ParameterNameComparer : IEqualityComparer<Parameter>
    {
        public bool Equals(Parameter x, Parameter y)
        {
            return (x.Name ?? string.Empty) == (y.Name ?? string.Empty) && x.Type == y.Type;
        }

        public int GetHashCode(Parameter obj)
        {
            return (obj.Name ?? string.Empty).GetHashCode() ^ obj.Type.GetHashCode();
        }

        private static readonly ParameterNameComparer _default = new ParameterNameComparer();
        public static ParameterNameComparer Default { get { return _default; } }
    }
}
