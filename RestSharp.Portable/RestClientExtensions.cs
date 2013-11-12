using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable
{
    public static class RestClientExtensions
    {
        public static IRestClient AddDefaultParameter(this IRestClient client, string name, object value)
        {
            return client.AddDefaultParameter(new Parameter { Name = name, Value = value, Type = ParameterType.GetOrPost });
        }

        public static IRestClient AddDefaultParameter(this IRestClient client, string name, object value, ParameterType type)
        {
            return client.AddDefaultParameter(new Parameter { Name = name, Value = value, Type = type });
        }

        public static IRestClient AddDefaultParameter(this IRestClient client, Parameter parameter)
        {
            if (parameter.Type == ParameterType.RequestBody)
                throw new NotSupportedException("Cannot set request body from default headers. Use Request.AddBody() instead.");
            client.DefaultParameters.Add(parameter);
            return client;
        }

        public static IRestClient RemoveDefaultParameter(this IRestClient client, string name)
        {
            var parameter = client.DefaultParameters.SingleOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (parameter != null)
                client.DefaultParameters.Remove(parameter);
            return client;
        }
    }
}
