using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace RestSharp.Portable
{
    internal sealed class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";
        private const string KeysName = "Keys";
        private const string ValuesName = "Values";

        private readonly IDictionary<TKey, TValue> _dictionary;

        #region Constructors

        public ObservableDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = new Dictionary<TKey, TValue>(dictionary);
        }

        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public ObservableDictionary(int capacity)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        #endregion

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys => _dictionary.Keys;

        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            TValue value;
            _dictionary.TryGetValue(key, out value);
            var removed = _dictionary.Remove(key);
            if (removed)
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value));

            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values => _dictionary.Values;

        public TValue this[TKey key]
        {
            get { return _dictionary[key]; }
            set { Insert(key, value, false); }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Insert(item.Key, item.Value, true);
        }

        public void Clear()
        {
            if (_dictionary.Count > 0)
            {
                _dictionary.Clear();
                OnCollectionChanged();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public int Count => _dictionary.Count;

        public bool IsReadOnly => _dictionary.IsReadOnly;

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void AddRange(IDictionary<TKey, TValue> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Count <= 0)
                return;

            if (_dictionary.Count > 0)
            {
                if (items.Keys.Any((k) => _dictionary.ContainsKey(k)))
                    throw new ArgumentException("An item with the same key has already been added.");
            }

            foreach (var item in items)
                _dictionary.Add(item);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, items.ToArray());
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            TValue item;
            if (_dictionary.TryGetValue(key, out item))
            {
                if (add) throw new ArgumentException("An item with the same key has already been added.");
                if (Equals(item, value)) return;
                _dictionary[key] = value;

                OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value),
                    new KeyValuePair<TKey, TValue>(key, item));
            }
            else
            {
                _dictionary[key] = value;

                OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        private void OnPropertyChanged()
        {
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnPropertyChanged(KeysName);
            OnPropertyChanged(ValuesName);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCollectionChanged()
        {
            OnPropertyChanged();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
        {
            OnPropertyChanged();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItem));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem)
        {
            OnPropertyChanged();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
        {
            OnPropertyChanged();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItems));
        }
    }
}
