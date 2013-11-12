using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    public enum ParameterType
    {
        GetOrPost,
        UrlSegment,
        QueryString,
        HttpHeader,
        RequestBody,
    }

    public class Parameter
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public ParameterType Type { get; set; }

        public MediaTypeHeaderValue ContentType { get; set; }
    }
}
