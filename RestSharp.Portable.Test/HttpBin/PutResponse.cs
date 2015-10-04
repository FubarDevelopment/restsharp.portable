using System.Collections.Generic;

namespace RestSharp.Portable.Test.HttpBin
{
    public class PutResponse
    {
        public Dictionary<string, string> Headers { get; set; }

        public string Data { get; set; }
    }
}
