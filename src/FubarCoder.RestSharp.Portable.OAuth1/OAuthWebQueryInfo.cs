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

namespace RestSharp.Portable.OAuth1
{
    class OAuthWebQueryInfo
    {
        public virtual string ConsumerKey { get; set; }
        public virtual string Token { get; set; }
        public virtual string Nonce { get; set; }
        public virtual string Timestamp { get; set; }
        public virtual string SignatureMethod { get; set; }
        public virtual string Signature { get; set; }
        public virtual string Version { get; set; }
        public virtual string Callback { get; set; }
        public virtual string Verifier { get; set; }
        public virtual string ClientMode { get; set; }
        public virtual string ClientUsername { get; set; }
        public virtual string ClientPassword { get; set; }
        public virtual string UserAgent { get; set; }
        public virtual string WebMethod { get; set; }
        public virtual OAuthParameterHandling ParameterHandling { get; set; }
        public virtual OAuthSignatureTreatment SignatureTreatment { get; set; }
        internal virtual string ConsumerSecret { get; set; }
        internal virtual string TokenSecret { get; set; }
    }
}
