using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Impl
{
    /// <summary>
    /// A default implementation of the HTTP header collection
    /// </summary>
    public class GenericHttpHeaders : IHttpHeaders
    {
        private readonly Dictionary<string, IEnumerable<string>> _headers = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<string>> _headersToChange = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        private readonly object _syncRoot = new object();

        /// <summary>
        /// Gets the enumerator for all HTTP headers
        /// </summary>
        /// <returns>The enumerator for all HTTP headers</returns>
        public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator()
        {
            lock (_headers)
            {
                // Kopie erzeugen?
                return _headers.Select(x => new KeyValuePair<string, IEnumerable<string>>(x.Key, new List<string>(x.Value))).ToList().GetEnumerator();
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds header values
        /// </summary>
        /// <param name="name">The header to add the values for</param>
        /// <param name="values">The values to add</param>
        public void Add(string name, IEnumerable<string> values)
        {
            lock (_syncRoot)
            {
                List<string> currentValues;
                if (!_headersToChange.TryGetValue(name, out currentValues))
                {
                    currentValues = new List<string>();
                    _headers.Add(name, currentValues);
                    _headersToChange.Add(name, currentValues);
                }

                currentValues.AddRange(values);
            }
        }

        /// <summary>
        /// Add a header value
        /// </summary>
        /// <param name="name">The header to add the value for</param>
        /// <param name="value">The value to add</param>
        public void Add(string name, string value)
        {
            Add(name, new[] { value });
        }

        /// <summary>
        /// Remove all headers
        /// </summary>
        public void Clear()
        {
            lock (_syncRoot)
            {
                _headers.Clear();
                _headersToChange.Clear();
            }
        }

        /// <summary>
        /// Is there a value for a header?
        /// </summary>
        /// <param name="name">The name of the header</param>
        /// <returns>true, if there is at least one value for the header</returns>
        public bool Contains(string name)
        {
            lock (_syncRoot)
            {
                return _headers.ContainsKey(name);
            }
        }

        /// <summary>
        /// Returns all values for a given header
        /// </summary>
        /// <remarks>
        /// Throws an exception if the header doesn't exist.
        /// </remarks>
        /// <param name="name">The header name</param>
        /// <returns>The sequence of header values</returns>
        public IEnumerable<string> GetValues(string name)
        {
            lock (_syncRoot)
            {
                return _headers[name].ToList();
            }
        }

        /// <summary>
        /// Remove the header with the given name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <returns>true, if the header could be removed</returns>
        public bool Remove(string name)
        {
            lock (_syncRoot)
            {
                _headers.Remove(name);
                return _headersToChange.Remove(name);
            }
        }

        /// <summary>
        /// Try to get the values
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="values">The values of the header to return</param>
        /// <returns>true, if the HTTP header was found</returns>
        public bool TryGetValues(string name, out IEnumerable<string> values)
        {
            lock (_syncRoot)
            {
                List<string> currentValues;
                if (!_headersToChange.TryGetValue(name, out currentValues))
                {
                    values = null;
                    return false;
                }

                values = currentValues;
                return true;
            }
        }

        /// <summary>
        /// Try to add header values without validation
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="values">The values to add</param>
        /// <returns>true, if the HTTP header values could be added</returns>
        public bool TryAddWithoutValidation(string name, IEnumerable<string> values)
        {
            Add(name, values);
            return true;
        }

        /// <summary>
        /// Try to add a header value without validation
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="value">The value to add</param>
        /// <returns>true, if the HTTP header value could be added</returns>
        public bool TryAddWithoutValidation(string name, string value)
        {
            Add(name, value);
            return true;
        }

        /// <summary>
        /// Returns a text representation of all HTTP headers in this collection
        /// </summary>
        /// <returns>The text representation of all HTTP headers</returns>
        public override string ToString()
        {
            var result = new StringBuilder();
            lock (_syncRoot)
            {
                foreach (var header in _headers)
                {
                    result.AppendFormat("{0}: {1}", header.Key, string.Join(", ", header.Value)).AppendLine();
                }
            }

            return result.ToString();
        }
    }
}
