using System;
using System.Diagnostics;

namespace RestSharp.Portable.Tests
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
