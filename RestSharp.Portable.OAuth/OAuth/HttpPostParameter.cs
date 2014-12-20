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

using System.IO;

namespace RestSharp.Portable.Authenticators.OAuth
{
    internal class HttpPostParameter : WebParameter
    {
        public HttpPostParameter(string name, string value) : base(name, value) { }
        public virtual HttpPostParameterType Type { get; private set; }
        public virtual string FileName { get; private set; }
        public virtual string FilePath { get; private set; }
        public virtual Stream FileStream { get; set; }
        public virtual string ContentType { get; private set; }
        public static HttpPostParameter CreateFile(string name, string fileName, string filePath, string contentType)
        {
            var parameter = new HttpPostParameter(name, string.Empty)
            {
                Type = HttpPostParameterType.File,
                FileName = fileName,
                FilePath = filePath,
                ContentType = contentType,
            };
            return parameter;
        }
        public static HttpPostParameter CreateFile(string name, string fileName, Stream fileStream, string contentType)
        {
            var parameter = new HttpPostParameter(name, string.Empty)
            {
                Type = HttpPostParameterType.File,
                FileName = fileName,
                FileStream = fileStream,
                ContentType = contentType,
            };
            return parameter;
        }
    }
}
