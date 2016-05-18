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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RestSharp.Portable.OAuth1
{
    internal class WebParameterCollection : IList<WebParameter>
    {
        private IList<WebParameter> _parameters;
        public virtual WebParameter this[string name]
        {
            get { return this.SingleOrDefault(p => string.Equals(p.Name, name)); }
        }
        public virtual IEnumerable<string> Names
        {
            get { return _parameters.Select(p => p.Name); }
        }
        public virtual IEnumerable<string> Values
        {
            get { return _parameters.Select(p => p.Value); }
        }
        public WebParameterCollection(IEnumerable<WebParameter> parameters)
        {
            _parameters = new List<WebParameter>(parameters);
        }
        public WebParameterCollection()
        {
            _parameters = new List<WebParameter>(0);
        }
        public WebParameterCollection(int capacity)
        {
            _parameters = new List<WebParameter>(capacity);
        }
        private void AddCollection(IEnumerable<WebParameter> collection)
        {
            foreach (var parameter in collection)
            {
                var pair = new WebParameter(parameter.Name, parameter.Value, parameter.Type);
                _parameters.Add(pair);
            }
        }
        public virtual void AddRange(WebParameterCollection collection)
        {
            AddCollection(collection);
        }
        public virtual void AddRange(IEnumerable<WebParameter> collection)
        {
            AddCollection(collection);
        }
        public virtual void Sort(Comparison<WebParameter> comparison)
        {
            var sorted = new List<WebParameter>(_parameters);
            sorted.Sort(comparison);
            _parameters = sorted;
        }
        public virtual bool RemoveAll(IEnumerable<WebParameter> parameters)
        {
            var success = true;
            var array = parameters.ToArray();
            for (var p = 0; p < array.Length; p++)
            {
                var parameter = array[p];
                success &= _parameters.Remove(parameter);
            }
            return success && array.Length > 0;
        }
        public virtual void Add(string name, string value, WebParameterType type)
        {
            var pair = new WebParameter(name, value, type);
            _parameters.Add(pair);
        }
        #region IList<WebParameter> Members
        public virtual IEnumerator<WebParameter> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public virtual void Add(WebParameter parameter)
        {
            _parameters.Add(parameter);
        }
        public virtual void Clear()
        {
            _parameters.Clear();
        }
        public virtual bool Contains(WebParameter parameter)
        {
            return _parameters.Contains(parameter);
        }
        public virtual void CopyTo(WebParameter[] parameters, int arrayIndex)
        {
            _parameters.CopyTo(parameters, arrayIndex);
        }
        public virtual bool Remove(WebParameter parameter)
        {
            return _parameters.Remove(parameter);
        }
        public virtual int Count
        {
            get { return _parameters.Count; }
        }
        public virtual bool IsReadOnly
        {
            get { return _parameters.IsReadOnly; }
        }
        public virtual int IndexOf(WebParameter parameter)
        {
            return _parameters.IndexOf(parameter);
        }
        public virtual void Insert(int index, WebParameter parameter)
        {
            _parameters.Insert(index, parameter);
        }
        public virtual void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }
        public virtual WebParameter this[int index]
        {
            get { return _parameters[index]; }
            set { _parameters[index] = value; }
        }
        #endregion
    }
}
