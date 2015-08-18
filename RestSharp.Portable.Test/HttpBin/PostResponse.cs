using System.Collections.Generic;

namespace RestSharp.Portable.Test.HttpBin
{
    public class PostResponse
    {
        public Dictionary<string, string> Form { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public object Json { get; set; }
    }
}
