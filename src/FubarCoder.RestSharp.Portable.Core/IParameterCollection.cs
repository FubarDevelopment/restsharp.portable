using System.Collections.Generic;

namespace RestSharp.Portable
{
    /// <summary>
    /// Collection of parameters
    /// </summary>
    public interface IParameterCollection : ICollection<Parameter>
    {
        /// <summary>
        /// Add or update parameter
        /// </summary>
        /// <param name="parameter">The parameter to add or update</param>
        /// <remarks>
        /// This ensures that the parameter with a given name and type exists only once.
        /// </remarks>
        void AddOrUpdate(Parameter parameter);

        /// <summary>
        /// Removes all parameters with the given type and name.
        /// </summary>
        /// <param name="type">The parameter type</param>
        /// <param name="name">The parameter name</param>
        /// <returns><code>true</code> when at least one parameter could be removed</returns>
        bool Remove(ParameterType type, string name);

        /// <summary>
        /// Finds all parameters with the given type and name.
        /// </summary>
        /// <param name="type">The parameter type</param>
        /// <param name="name">The parameter name</param>
        /// <returns>The list of found parameters</returns>
        IList<Parameter> Find(ParameterType type, string name);
    }
}
