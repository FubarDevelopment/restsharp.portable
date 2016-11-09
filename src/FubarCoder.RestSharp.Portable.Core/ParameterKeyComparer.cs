using System;
using System.Collections.Generic;

namespace RestSharp.Portable
{
    /// <summary>
    /// Compares parameter keys
    /// </summary>
    internal sealed class ParameterKeyComparer : IComparer<ParameterKey>, IEqualityComparer<ParameterKey>
    {
        private readonly StringComparer _defaultComparer = StringComparer.Ordinal;
        private readonly StringComparer _httpHeaderComparer = StringComparer.OrdinalIgnoreCase;

        /// <inheritdoc/>
        public int Compare(ParameterKey x, ParameterKey y)
        {
            if (ReferenceEquals(x, y))
                return 0;

            var diff = x.Type.CompareTo(y.Type);
            if (diff != 0)
                return diff;

            var comparer = x.Type == ParameterType.HttpHeader ? _httpHeaderComparer : _defaultComparer;
            diff = comparer.Compare(x.Name, y.Name);
            return diff;
        }

        /// <inheritdoc/>
        public bool Equals(ParameterKey x, ParameterKey y)
        {
            return Compare(x, y) == 0;
        }

        /// <inheritdoc/>
        public int GetHashCode(ParameterKey obj)
        {
            var comparer = obj.Type == ParameterType.HttpHeader ? _httpHeaderComparer : _defaultComparer;
            return obj.Type.GetHashCode() ^ comparer.GetHashCode(obj.Name);
        }
    }
}
