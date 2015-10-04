using System.Collections.Generic;

namespace RestSharp.Portable.Test.HttpBin
{
    public class HttpBinResponse
    {
        public Dictionary<string, string> Args { get; set; }

        public Dictionary<string, string> Form { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Data { get; }

        public object Json { get; set; }
    }
}
