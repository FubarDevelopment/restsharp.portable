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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Authenticators.OAuth
{
    internal class WebParameterCollection : WebPairCollection
    {
        public WebParameterCollection(IEnumerable<WebPair> parameters)
            : base(parameters) { }
        public WebParameterCollection() { }
        public WebParameterCollection(int capacity)
            : base(capacity) { }
        public WebParameterCollection(IDictionary<string, string> collection)
            : base(collection) { }
        public override void Add(string name, string value)
        {
            var parameter = new WebParameter(name, value);
            base.Add(parameter);
        }
    }
}