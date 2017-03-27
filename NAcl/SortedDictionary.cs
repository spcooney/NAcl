using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace NAcl
{
    public class SortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        List<TKey> keys;
        List<KeyValuePair<TKey, TValue>> values;

        private IComparer<TKey> comparer;

        public SortedDictionary()
            : this(Comparer<TKey>.Default)
        {
        }

        public SortedDictionary(IComparer<TKey> comparer)
        {
            this.comparer = comparer;
            keys = new List<TKey>();
            values = new List<KeyValuePair<TKey, TValue>>();
        }



        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            int i = 0;
            while (keys.Count > i && comparer.Compare(key, keys[i]) > 0)
                i++;
            keys.Insert(i, key);
            values.Insert(i, new KeyValuePair<TKey, TValue>(key, value));
        }

        private int IndexOf(TKey key)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (comparer.Compare(keys[i], key) == 0)
                    return i;
            }
            return -1;
        }

        public bool ContainsKey(TKey key)
        {
            return IndexOf(key) >= 0;
        }

        public ICollection<TKey> Keys
        {
            get { return new List<TKey>(keys); }
        }

        public bool Remove(TKey key)
        {
            int indexToRemove = IndexOf(key);
            if (indexToRemove < 0)
                return false;
            keys.RemoveAt(indexToRemove);
            values.RemoveAt(indexToRemove);
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = IndexOf(key);
            value = default(TValue);
            if (index < 0)
                return false;
            value = values[index].Value;
            return true;
        }

        public ICollection<TValue> Values
        {
            get { return new List<TValue>(values.Select(v => v.Value)); }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (!TryGetValue(key, out result))
                    throw new KeyNotFoundException("The key " + key + " could not be found");
                return result;

            }
            set
            {
                int index = IndexOf(key);
                if (index < 0)
                    Add(key, value);
                else
                    values[index] = new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            int index = IndexOf(item.Key);
            return index >= 0 && EqualityComparer<TValue>.Default.Equals(values[index].Value, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array.Length - arrayIndex < Count)
                throw new ArgumentOutOfRangeException("array");
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = values[i];
            }
        }

        public int Count
        {
            get { return keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!Contains(item))
                return false;
            return Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
