using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.SupportingClasses {
    public class ActionDictionary<K, V> : IDictionary<K, V> {

        protected Dictionary<K, V> Data = new Dictionary<K, V>();

        protected virtual void OnPreDelete(K key) {
        }

        /// <summary>
        /// Decides whether an element is added.
        /// </summary>
        /// <remarks>Default implementation returns true.</remarks>
        /// <returns>True whether an element is added.</returns>
        protected virtual bool IsToAdd(K key, V value) {
            return true;
        }

        protected virtual void OnPreAdd(K key, V value) {
        }

        protected virtual void OnAdded(K key, V value) {
        }

        protected virtual void OnPreSet(K key, V value) {
        }

        protected virtual void OnSet(K key, V value) {
        }

        #region IDictionary<K,V> Members

        public void Add(K key, V value) {
            if (IsToAdd(key, value) == false) {
                return;
            }
            OnPreAdd(key, value);
            Data.Add(key, value);
            OnAdded(key, value);
        }

        public bool ContainsKey(K key) {
            return Data.ContainsKey(key);
        }

        public ICollection<K> Keys {
            get { return Data.Keys; }
        }

        public bool Remove(K key) {
            OnPreDelete(key);
            return Data.Remove(key);
        }

        public bool TryGetValue(K key, out V value) {
            return Data.TryGetValue(key, out value);
        }

        public ICollection<V> Values {
            get { return Data.Values; }
        }

        public V this[K key] {
            get {
                return Data[key];
            }
            set {
                OnPreSet(key, value);
                Data[key] = value;
                OnSet(key, value);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        public void Add(KeyValuePair<K, V> item) {
            Add(item.Key, item.Value);
        }

        public void Clear() {
            foreach (K key in Data.Keys)
                OnPreDelete(key);
            Data.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item) {
            return Data.Contains(item);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {
            ((IDictionary<K, V>)Data).CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return Data.Count; }
        }

        public bool IsReadOnly {
            get { return ((IDictionary<K, V>)Data).IsReadOnly; }
        }

        public bool Remove(KeyValuePair<K, V> item) {
            Data.Remove(item.Key);
            return ((IDictionary<K, V>)Data).Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
            return Data.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return Data.GetEnumerator();
        }

        #endregion
    }
}
