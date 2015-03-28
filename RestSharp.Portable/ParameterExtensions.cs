using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable
{
    /// <summary>
    /// Extension methods for Parameter(s)
    /// </summary>
    public static class ParameterExtensions
    {
        internal static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Get the GetOrPost parameters (by default without file parameters, which are POST-only)
        /// </summary>
        /// <param name="parameters">
        /// The list of parameters to filter
        /// </param>
        /// <returns>
        /// The list of GET or POST parameters
        /// </returns>
        public static IEnumerable<Parameter> GetGetOrPostParameters(this IEnumerable<Parameter> parameters)
        {
            return GetGetOrPostParameters(parameters, false);
        }

        /// <summary>
        /// Get the GetOrPost parameters (by default without file parameters, which are POST-only)
        /// </summary>
        /// <param name="parameters">
        /// The list of parameters to filter
        /// </param>
        /// <param name="withFile">
        /// true == with file parameters, but those are POST-only!
        /// </param>
        /// <returns>
        /// The list of GET or POST parameters
        /// </returns>
        public static IEnumerable<Parameter> GetGetOrPostParameters(this IEnumerable<Parameter> parameters, bool withFile)
        {
            return parameters.Where(x => x.Type == ParameterType.GetOrPost && (withFile || !(x is FileParameter)));
        }

        /// <summary>
        /// Get the file parameters
        /// </summary>
        /// <param name="parameters">
        /// The list of parameters to filter
        /// </param>
        /// <returns>
        /// The list of POST file parameters
        /// </returns>
        public static IEnumerable<FileParameter> GetFileParameters(this IEnumerable<Parameter> parameters)
        {
            return parameters.OfType<FileParameter>();
        }

        /// <summary>
        /// Remove a parameter by name
        /// </summary>
        /// <param name="parameters">The parameters where the parameter is removed from</param>
        /// <param name="parameterName">The name of the parameter to remove</param>
        /// <param name="parameterNameComparer">The parameter name comparer</param>
        public static void RemoveParameter(this IList<Parameter> parameters, string parameterName, StringComparer parameterNameComparer)
        {
            var parametersToDelete = parameters.Where(x => parameterNameComparer.Equals(x.Name, parameterName)).ToList();
            foreach (var parameter in parametersToDelete)
                parameters.Remove(parameter);
        }

        /// <summary>
        /// Test if a parameter with a given value exists
        /// </summary>
        /// <param name="parameters">The list of parameters to search</param>
        /// <param name="parameterName">The parameter name to search for</param>
        /// <param name="parameterValue">The parameter value to search for</param>
        /// <param name="parameterNameComparer">The parameter name comparer</param>
        /// <returns>true when the parameter with the given name exists</returns>
        public static bool HasParameterWithValue(this IList<Parameter> parameters, string parameterName, string parameterValue, StringComparer parameterNameComparer)
        {
            var parameterValues = parameters
                .Where(x => parameterNameComparer.Equals(x.Name, parameterName) && (x.Value == null || x.Value is string))
                .Select(x => (string)x.Value)
                .ToList();
            return parameterValues.Any(x => (x == null && parameterValue == null) || (x != null && parameterValue != null && x == parameterValue));
        }

        internal static string ToEncodedString(this Parameter parameter, bool spaceAsPlus = false)
        {
            switch (parameter.Type)
            {
                case ParameterType.GetOrPost:
                case ParameterType.QueryString:
                case ParameterType.UrlSegment:
                    return UrlEncode(parameter, parameter.Encoding ?? DefaultEncoding, spaceAsPlus);
            }

            throw new NotSupportedException(string.Format("Parameter of type {0} doesn't support an encoding.", parameter.Type));
        }

        private static string UrlEncode(Parameter parameter, Encoding encoding, bool spaceAsPlus)
        {
            var flags = spaceAsPlus ? UrlEscapeFlags.EscapeSpaceAsPlus : UrlEscapeFlags.Default;

            if (parameter.Value == null)
                return string.Empty;

            var s = parameter.Value as string;
            if (s != null)
                return UrlUtility.Escape(s, encoding, flags);

            var bytes = parameter.Value as byte[];
            if (bytes != null)
                return UrlUtility.Escape(bytes, flags);

            return UrlUtility.Escape(parameter.AsString(), encoding, flags);
        }
    }
}
