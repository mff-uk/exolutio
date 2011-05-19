using System;
using System.Collections.Generic;

namespace EvoX.SupportingClasses
{

	public interface IReadOnlyDictionary<TKey, TValue>
	{
		bool ContainsKey(TKey key);
		
		ICollection<TKey> Keys { get; }
		
		ICollection<TValue> Values { get; }
		
		int Count { get; }
		
		bool IsReadOnly { get; }
		
		bool TryGetValue(TKey key, out TValue value);
		
		TValue this[TKey key] { get; }
		
		bool Contains(KeyValuePair<TKey, TValue> item);
		
		void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex);
		
		IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();
	}

	public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
	{
		readonly IDictionary<TKey, TValue> _dict;

		public ReadOnlyDictionary(IDictionary<TKey, TValue> backingDict)
		{
			_dict = backingDict;
		}

		public void Add(TKey key, TValue value)
		{
			throw new InvalidOperationException();
		}

		public bool ContainsKey(TKey key)
		{
			return _dict.ContainsKey(key);
		}

		public ICollection<TKey> Keys
		{
			get { return _dict.Keys; }
		}

		public bool Remove(TKey key)
		{
			throw new InvalidOperationException();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dict.TryGetValue(key, out value);
		}

		public ICollection<TValue> Values
		{
			get { return _dict.Values; }
		}

		public TValue this[TKey key]
		{
			get { return _dict[key]; }
			set { throw new InvalidOperationException(); }
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new InvalidOperationException();
		}

		public void Clear()
		{
			throw new InvalidOperationException();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return _dict.Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			_dict.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _dict.Count; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new InvalidOperationException();
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _dict.GetEnumerator();
		}

		System.Collections.IEnumerator
			   System.Collections.IEnumerable.GetEnumerator()
		{
			return ((System.Collections.IEnumerable)_dict).GetEnumerator();
		}
	}

	public static class IDictionaryExt
	{
		public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
		{
			return new ReadOnlyDictionary<TKey, TValue>(dictionary);
		}
	}
}