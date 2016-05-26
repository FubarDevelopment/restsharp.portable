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

using System.Collections.Generic;
using System.Linq;

namespace RestSharp.Portable.Authenticators.OAuth
{
    internal class WebParameterCollection : List<WebParameter>
    {
        public WebParameterCollection(IEnumerable<WebParameter> parameters)
            : base(parameters)
        {
        }

        public WebParameterCollection()
        {
        }

        public WebParameterCollection(int capacity)
            : base(capacity)
        {
        }

        public virtual IEnumerable<string> Names
        {
            get { return this.Select(p => p.Name); }
        }

        public virtual IEnumerable<string> Values
        {
            get { return this.Select(p => p.Value); }
        }

        public virtual WebParameter this[string name]
        {
            get { return this.SingleOrDefault(p => string.Equals(p.Name, name)); }
        }

        public virtual void AddRange(WebParameterCollection collection)
        {
            AddCollection(collection);
        }

        public virtual bool RemoveAll(IEnumerable<WebParameter> parameters)
        {
            var success = true;
            var array = parameters.ToArray();
            for (var p = 0; p < array.Length; p++)
            {
                var parameter = array[p];
                success &= Remove(parameter);
            }
            return success && array.Length > 0;
        }
        public virtual void Add(string name, string value, WebParameterType type)
        {
            var pair = new WebParameter(name, value, type);
            Add(pair);
        }

        private void AddCollection(IEnumerable<WebParameter> collection)
        {
            foreach (var parameter in collection)
            {
                var pair = new WebParameter(parameter.Name, parameter.Value, parameter.Type);
                Add(pair);
            }
        }
    }
}
