using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Portable.Authenticators.OAuth
{
    /// <summary>
    /// This delegate is used to specify the function to create a timestamp
    /// </summary>
    /// <returns></returns>
    public delegate string OAuthCreateTimestampDelegate();
}
