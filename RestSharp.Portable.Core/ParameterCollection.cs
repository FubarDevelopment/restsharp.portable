using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RestSharp.Portable
{
    /// <summary>
    /// Implements the <see cref="IParameterCollection"/> interface
    /// </summary>
    internal class ParameterCollection : IParameterCollection
    {
        private readonly MultiValueDictionary<ParameterKey, ParameterEntry> _dictionary = new MultiValueDictionary<ParameterKey, ParameterEntry>(new ParameterKeyComparer());

        private ulong _order = 0;

        /// <inheritdoc/>
        public int Count => _dictionary.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public IList<Parameter> Find(ParameterType type, string name)
        {
            var key = new ParameterKey(type, name);
            if (!_dictionary.ContainsKey(key))
                return new List<Parameter>();
            var foundParameters = _dictionary[key];
            var result = new List<Parameter>(foundParameters.Count);
            result.AddRange(foundParameters.Select(x => x.Parameter));
            return result;
        }

        /// <inheritdoc/>
        public void Add(Parameter item)
        {
            _dictionary.Add(new ParameterKey(item), new ParameterEntry(_order++, item));
        }

        /// <inheritdoc/>
        public void AddOrUpdate(Parameter parameter)
        {
            ulong order;
            var key = new ParameterKey(parameter);
            IReadOnlyCollection<ParameterEntry> oldEntries;
            if (_dictionary.TryGetValue(key, out oldEntries))
            {
                order = oldEntries.First().Order;
                _dictionary.Remove(key);
            }
            else
            {
                order = _order++;
            }
            _dictionary.Add(key, new ParameterEntry(order, parameter));
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(Parameter item)
        {
            return _dictionary.ContainsKey(new ParameterKey(item));
        }

        /// <inheritdoc/>
        public void CopyTo(Parameter[] array, int arrayIndex)
        {
            foreach (var dictItem in _dictionary)
            {
                foreach (var item in dictItem.Value)
                {
                    array[arrayIndex++] = item.Parameter;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerator<Parameter> GetEnumerator()
        {
            foreach (var dictItem in _dictionary.OrderBy(x => x.Value.First().Order))
            {
                foreach (var item in dictItem.Value)
                {
                    yield return item.Parameter;
                }
            }
        }

        /// <inheritdoc/>
        public bool Remove(Parameter item)
        {
            var key = new ParameterKey(item);
            IReadOnlyCollection<ParameterEntry> entries;
            if (!_dictionary.TryGetValue(key, out entries))
                return false;
            var entry = entries.FirstOrDefault(x => ReferenceEquals(x.Parameter, item));
            if (entry == null)
                return false;
            return _dictionary.Remove(key, entry);
        }

        /// <inheritdoc/>
        public bool Remove(ParameterType type, string name)
        {
            var key = new ParameterKey(type, name);
            return _dictionary.Remove(key);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class ParameterEntry
        {
            public ParameterEntry(ulong order, Parameter param)
            {
                Order = order;
                Parameter = param;
            }

            public ulong Order { get; }
            public Parameter Parameter { get; }
        }
    }
}
