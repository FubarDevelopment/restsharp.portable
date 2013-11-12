using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    public enum ParameterType
    {
        Body,
        GetOrPost,
        UrlSegment,
        QueryString,
        HttpHeader,
    }

    public class Parameter
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public ParameterType Type { get; set; }
    }
}
