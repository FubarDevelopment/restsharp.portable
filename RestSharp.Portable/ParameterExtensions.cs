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
        /// Is the given parameter a content parameter?
        /// </summary>
        /// <param name="parameter">the parameter to test</param>
        /// <returns>true when the parameter is a content parameter</returns>
        public static bool IsContentParameter(this Parameter parameter)
        {
            return !string.IsNullOrEmpty(parameter.Name) && parameter.Name.StartsWith("Content-", StringComparison.OrdinalIgnoreCase);
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
