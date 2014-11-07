﻿#region License
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
using System.Text;

namespace RestSharp.Portable.Authenticators.OAuth
{
    internal class WebPairCollection : IList<WebPair>
    {
        private IList<WebPair> _parameters;
        public virtual WebPair this[string name]
        {
            get { return this.SingleOrDefault(p => p.Name.Equals(name)); }
        }
        public virtual IEnumerable<string> Names
        {
            get { return _parameters.Select(p => p.Name); }
        }
        public virtual IEnumerable<string> Values
        {
            get { return _parameters.Select(p => p.Value); }
        }
        public WebPairCollection(IEnumerable<WebPair> parameters)
        {
            _parameters = new List<WebPair>(parameters);
        }
        public WebPairCollection(IDictionary<string, string> collection)
            : this()
        {
            AddCollection(collection);
        }
        public void AddCollection(IDictionary<string, string> collection)
        {
            foreach (var key in collection.Keys)
            {
                var parameter = new WebPair(key, collection[key]);
                _parameters.Add(parameter);
            }
        }
        public WebPairCollection()
        {
            _parameters = new List<WebPair>(0);
        }
        public WebPairCollection(int capacity)
        {
            _parameters = new List<WebPair>(capacity);
        }
        private void AddCollection(IEnumerable<WebPair> collection)
        {
            foreach (var parameter in collection)
            {
                var pair = new WebPair(parameter.Name, parameter.Value);
                _parameters.Add(pair);
            }
        }
        public virtual void AddRange(WebPairCollection collection)
        {
            AddCollection(collection);
        }
        public virtual void AddRange(IEnumerable<WebPair> collection)
        {
            AddCollection(collection);
        }
        public virtual void Sort(Comparison<WebPair> comparison)
        {
            var sorted = new List<WebPair>(_parameters);
            sorted.Sort(comparison);
            _parameters = sorted;
        }
        public virtual bool RemoveAll(IEnumerable<WebPair> parameters)
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
        public virtual void Add(string name, string value)
        {
            var pair = new WebPair(name, value);
            _parameters.Add(pair);
        }
        #region IList<WebParameter> Members
        public virtual IEnumerator<WebPair> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public virtual void Add(WebPair parameter)
        {
            _parameters.Add(parameter);
        }
        public virtual void Clear()
        {
            _parameters.Clear();
        }
        public virtual bool Contains(WebPair parameter)
        {
            return _parameters.Contains(parameter);
        }
        public virtual void CopyTo(WebPair[] parameters, int arrayIndex)
        {
            _parameters.CopyTo(parameters, arrayIndex);
        }
        public virtual bool Remove(WebPair parameter)
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
        public virtual int IndexOf(WebPair parameter)
        {
            return _parameters.IndexOf(parameter);
        }
        public virtual void Insert(int index, WebPair parameter)
        {
            _parameters.Insert(index, parameter);
        }
        public virtual void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }
        public virtual WebPair this[int index]
        {
            get { return _parameters[index]; }
            set { _parameters[index] = value; }
        }
        #endregion
    }
}
