using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Encodings
{
    public interface IEncoding
    {
        byte[] Encode(byte[] data);
        byte[] Decode(byte[] data);
    }
}
