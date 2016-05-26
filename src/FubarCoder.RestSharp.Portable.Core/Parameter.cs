using System;
using System.Globalization;
using System.Text;

using JetBrains.Annotations;

namespace RestSharp.Portable
{
    /// <summary>
    /// Parameter container for REST requests
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter" /> class.
        /// </summary>
        public Parameter()
        {
            ValidateOnAdd = true;
        }

        /// <summary>
        /// Gets or sets the name of the parameter
        /// </summary>
        [CanBeNull]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the parameter
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter
        /// </summary>
        public ParameterType Type { get; set; }

        /// <summary>
        /// Gets or sets the content type of the parameter
        /// </summary>
        /// <remarks>
        /// Applies to the following parameter types:
        /// - RequestBody
        /// </remarks>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this parameter should be validated?
        /// </summary>
        /// <remarks>
        /// Applies to the following parameter types:
        /// - HttpHeader
        /// </remarks>
        public bool ValidateOnAdd { get; set; }

        /// <summary>
        /// Gets or sets the encoding of this parameters value
        /// </summary>
        /// <remarks>
        /// Applies to the following parameter types:
        /// - GetOrPost
        /// - QueryString
        /// - UrlSegment
        /// </remarks>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Returns the parameter value as string
        /// </summary>
        /// <returns>Returns the value as string</returns>
        public string ToRequestString()
        {
            var byteArray = Value as byte[];
            if (byteArray != null)
            {
                return Convert.ToBase64String(byteArray);
            }

            if (Type == ParameterType.HttpHeader)
            {
                var dateTime = Value as DateTime?;
                if (dateTime != null)
                {
                    return dateTime.Value.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture);
                }

                var dateTimeOffset = Value as DateTimeOffset?;
                if (dateTimeOffset != null)
                {
                    return dateTimeOffset.Value.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture);
                }
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}", Value);
        }
    }
}
