#region License
// Copyright 2010 John Sheehan
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Authenticators.OAuth.Extensions
{
    internal static class OAuthExtensions
    {
        public static string ToRequestValue(this OAuthSignatureMethod signatureMethod)
        {
            var value = signatureMethod.ToString().ToUpper();
            var shaIndex = value.IndexOf("SHA1");
            return shaIndex > -1 ? value.Insert(shaIndex, "-") : value;
        }
        public static OAuthSignatureMethod FromRequestValue(this string signatureMethod)
        {
            switch (signatureMethod)
            {
                case "HMAC-SHA1":
                    return OAuthSignatureMethod.HmacSha1;
                case "RSA-SHA1":
                    return OAuthSignatureMethod.RsaSha1;
                default:
                    return OAuthSignatureMethod.PlainText;
            }
        }
        public static string HashWith(this string input, Org.BouncyCastle.Crypto.IMac algorithm)
        {
            var data = Encoding.UTF8.GetBytes(input);
            algorithm.BlockUpdate(data, 0, data.Length);
            var hash = Org.BouncyCastle.Security.MacUtilities.DoFinal(algorithm);
            return Convert.ToBase64String(hash);
        }
    }
}
