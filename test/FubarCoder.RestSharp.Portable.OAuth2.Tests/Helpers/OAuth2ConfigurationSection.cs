using System;

using RestSharp.Portable.OAuth2.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.Specialized;

using RestSharp.Portable.Collections;

namespace RestSharp.Portable.OAuth2.Tests.Helpers
{
    public class OAuth2ConfigurationSection : IOAuth2Configuration
    {
        public ObservableDictionary<string, RuntimeClientConfiguration> Services { get; }

        public OAuth2ConfigurationSection()
        {
            var services = new ObservableDictionary<string, RuntimeClientConfiguration>();
            services.CollectionChanged += ServicesOnCollectionChanged;
            Services = services;
        }

        private void ServicesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    foreach (KeyValuePair<string, RuntimeClientConfiguration> newItem in notifyCollectionChangedEventArgs.NewItems)
                    {
                        newItem.Value.ClientTypeName = newItem.Key;
                    }
                    break;
            }
        }

        public IClientConfiguration this[string clientTypeName]
        {
            get
            {
                return Services[clientTypeName];
            }
        }

        public IEnumerator<IClientConfiguration> GetEnumerator()
        {
            return Services.Values.Cast<IClientConfiguration>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
