using System.Collections.Generic;

namespace RestSharp.Portable
{
    /// <summary>
    /// A generic interface to the HTTP headers
    /// </summary>
    public interface IHttpHeaders : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
    {
        /// <summary>
        /// Adds header values
        /// </summary>
        /// <param name="name">The header to add the values for</param>
        /// <param name="values">The values to add</param>
        void Add(string name, IEnumerable<string> values);

        /// <summary>
        /// Add a header value
        /// </summary>
        /// <param name="name">The header to add the value for</param>
        /// <param name="value">The value to add</param>
        void Add(string name, string value);

        /// <summary>
        /// Remove all headers
        /// </summary>
        void Clear();

        /// <summary>
        /// Is there a value for a header?
        /// </summary>
        /// <param name="name">The name of the header</param>
        /// <returns>true, if there is at least one value for the header</returns>
        bool Contains(string name);

        /// <summary>
        /// Returns all values for a given header
        /// </summary>
        /// <remarks>
        /// Throws an exception if the header doesn't exist.
        /// </remarks>
        /// <param name="name">The header name</param>
        /// <returns>The sequence of header values</returns>
        IEnumerable<string> GetValues(string name);

        /// <summary>
        /// Remove the header with the given name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <returns>true, if the header could be removed</returns>
        bool Remove(string name);

        /// <summary>
        /// Try to get the values
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="values">The values of the header to return</param>
        /// <returns>true, if the HTTP header was found</returns>
        bool TryGetValues(string name, out IEnumerable<string> values);

        /// <summary>
        /// Try to add header values without validation
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="values">The values to add</param>
        /// <returns>true, if the HTTP header values could be added</returns>
        bool TryAddWithoutValidation(string name, IEnumerable<string> values);

        /// <summary>
        /// Try to add a header value without validation
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="value">The value to add</param>
        /// <returns>true, if the HTTP header value could be added</returns>
        bool TryAddWithoutValidation(string name, string value);
    }
}
