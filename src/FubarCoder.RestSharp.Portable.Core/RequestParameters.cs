using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace RestSharp.Portable
{
    /// <summary>
    /// Merged parameters - partially split by usage
    /// </summary>
    public sealed class RequestParameters
    {
        internal RequestParameters([CanBeNull] IRestClient client, [CanBeNull] IRestRequest request)
        {
            var parameters = GetParameters(client, request);
            Parameters = parameters;
            foreach (var parameter in parameters)
            {
                if (parameter.IsContentParameter())
                {
                    ContentHeaderParameters.Add(parameter);
                }
                else
                {
                    OtherParameters.Add(parameter);
                }
            }
        }

        /// <summary>
        /// Gets all parameters
        /// </summary>
        public IList<Parameter> Parameters { get; }

        /// <summary>
        /// Gets all parameters that aren't content header parameters
        /// </summary>
        public IList<Parameter> OtherParameters { get; } = new List<Parameter>();

        /// <summary>
        /// Gets all content header parameters
        /// </summary>
        public IList<Parameter> ContentHeaderParameters { get; } = new List<Parameter>();

        private static IList<Parameter> GetParameters([CanBeNull] IRestClient client, [CanBeNull]  IRestRequest request)
        {
            var usedRequestParameters = new Dictionary<Parameter, bool>();
            var result = new List<Parameter>();
            if (client != null)
            {
                foreach (var parameter in client.DefaultParameters)
                {
                    var requestParameters = request?.Parameters.Find(parameter.Type, parameter.Name);
                    if (requestParameters != null && requestParameters.Count != 0)
                    {
                        foreach (var requestParameter in requestParameters)
                        {
                            result.Add(requestParameter);
                            usedRequestParameters[requestParameter] = true;
                        }
                    }
                    else
                    {
                        result.Add(parameter);
                    }
                }
            }

            if (request != null)
            {
                foreach (var parameter in request.Parameters.Where(x => !usedRequestParameters.ContainsKey(x)))
                {
                    result.Add(parameter);
                }
            }

            return result;
        }
    }
}
