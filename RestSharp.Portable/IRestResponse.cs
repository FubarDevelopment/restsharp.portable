using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable
{
    public interface IRestResponse
    {
        IRestRequest Request { get; }
        Uri ResponseUri { get; }
        byte[] RawBytes { get; }
    }

    public interface IRestResponse<T> : IRestResponse
    {
        T Data { get; }
    }
}
