using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace RestSharp.Portable.Serializers
{
    public interface ISerializer
    {
        byte[] Serialize(object obj);
        string DateFormat { get; set; }
        MediaTypeHeaderValue ContentType { get; set; }
    }
}
