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

namespace RestSharp.Portable.Authenticators.OAuth
{
    /// <summary>
    /// Where are the OAuth parameters stored?
    /// </summary>
    public enum OAuthParameterHandling
    {
        /// <summary>
        /// The OAuth parameter is stored in the HTTP header
        /// </summary>
        HttpAuthorizationHeader,

        /// <summary>
        /// The OAuth parameter is stored in the URL or as POST parameter
        /// </summary>
        UrlOrPostParameters
    }
}
