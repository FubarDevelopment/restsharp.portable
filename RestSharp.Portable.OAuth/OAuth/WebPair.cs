using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Authenticators.OAuth
{
    internal class WebPair
    {
        public WebPair(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public string Value { get; set; }
        public string Name { get; set; }
    }
}
