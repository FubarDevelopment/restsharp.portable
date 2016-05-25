using JetBrains.Annotations;

namespace RestSharp.Portable
{
    /// <summary>
    /// The key for a parameter
    /// </summary>
    internal sealed class ParameterKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterKey"/> class.
        /// </summary>
        /// <param name="type">The parameter type</param>
        /// <param name="name">The parameter name</param>
        public ParameterKey(ParameterType type, [NotNull] string name)
        {
            Type = type;
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterKey"/> class.
        /// </summary>
        /// <param name="parameter">The parameter to create the key from</param>
        public ParameterKey(Parameter parameter)
            : this(parameter.Type, parameter.Name)
        {
        }

        /// <summary>
        /// Gets the parameter type
        /// </summary>
        public ParameterType Type { get; }

        /// <summary>
        /// Gets the parameter name
        /// </summary>
        [NotNull]
        public string Name { get; }
    }
}
