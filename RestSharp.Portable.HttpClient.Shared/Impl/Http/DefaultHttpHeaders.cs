using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;

using JetBrains.Annotations;

namespace RestSharp.Portable.HttpClient.Impl.Http
{
    /// <summary>
    /// <see cref="IHttpHeaders"/> implementation using an underlying <see cref="HttpHeaders"/>.
    /// </summary>
    public class DefaultHttpHeaders : IHttpHeaders
    {
        private readonly HttpHeaders _headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpHeaders"/> class.
        /// </summary>
        /// <param name="headers">The headers collection to use in the background</param>
        public DefaultHttpHeaders([NotNull] HttpHeaders headers)
        {
            _headers = headers;
        }


        /// <summary>
        /// Gets the underlying HTTP headers
        /// </summary>
        public HttpHeaders Headers => _headers;

        /// <summary>
        /// Returns an enumerator for all headers and their values.
        /// </summary>
        /// <returns>An enumerator for all headers and their values</returns>
        public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator()
        {
            return _headers.GetEnumerator();
        }

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
            _headers.Add(name, values);
        }

        /// <summary>
        /// Add a header value
        /// </summary>
        /// <param name="name">The header to add the value for</param>
        /// <param name="value">The value to add</param>
        public void Add(string name, string value)
        {
            _headers.Add(name, value);
        }

        /// <summary>
        /// Remove all headers
        /// </summary>
        public void Clear()
        {
            _headers.Clear();
        }

        /// <summary>
        /// Is there a value for a header?
        /// </summary>
        /// <param name="name">The name of the header</param>
        /// <returns>true, if there is at least one value for the header</returns>
        public bool Contains(string name)
        {
            return _headers.Contains(name);
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
            return _headers.GetValues(name);
        }

        /// <summary>
        /// Remove the header with the given name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <returns>true, if the header could be removed</returns>
        public bool Remove(string name)
        {
            return _headers.Remove(name);
        }

        /// <summary>
        /// Try to get the values
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="values">The values of the header to return</param>
        /// <returns>true, if the HTTP header was found</returns>
        public bool TryGetValues(string name, out IEnumerable<string> values)
        {
            return _headers.TryGetValues(name, out values);
        }

        /// <summary>
        /// Try to add header values without validation
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="values">The values to add</param>
        /// <returns>true, if the HTTP header values could be added</returns>
        public bool TryAddWithoutValidation(string name, IEnumerable<string> values)
        {
            return _headers.TryAddWithoutValidation(name, values);
        }

        /// <summary>
        /// Try to add a header value without validation
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="value">The value to add</param>
        /// <returns>true, if the HTTP header value could be added</returns>
        public bool TryAddWithoutValidation(string name, string value)
        {
            return _headers.TryAddWithoutValidation(name, value);
        }
    }
}
