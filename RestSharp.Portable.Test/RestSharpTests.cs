using System;
using System.Diagnostics;
using System.Web.Compilation;

namespace RestSharp.Portable.Test
{
    public abstract class RestSharpTests
    {
        protected IHttpClientFactory CreateClientFactory(Type clientFactoryType, bool setCredentials)
        {
            var constructor = clientFactoryType.GetConstructor(new[] { typeof(bool) });
            Debug.Assert(constructor != null, "constructor != null");
            return (IHttpClientFactory)constructor.Invoke(new object[] { setCredentials });
        }
    }
}
