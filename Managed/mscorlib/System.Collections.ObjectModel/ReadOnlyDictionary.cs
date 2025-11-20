using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Unity;

namespace System.Collections.ObjectModel;

/// <summary>Represents a read-only, generic collection of key/value pairs.</summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(DictionaryDebugView<, >))]
public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
{
	[Serializable]
	private struct DictionaryEnumerator : IDictionaryEnumerator, IEnumerator
	{
		private readonly IDictionary<TKey, TValue> _dictionary;

		private IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

		public DictionaryEntry Entry => new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value);

		public object Key => _enumerator.Current.Key;

		public object Value => _enumerator.Current.Value;

		public object Current => Entry;

		public DictionaryEnumerator(IDictionary<TKey, TValue> dictionary)
		{
			_dictionary = dictionary;
			_enumerator = _dictionary.GetEnumerator();
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Reset()
		{
			_enumerator.Reset();
		}
	}

	/// <summary>Represents a read-only collection of the keys of a <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" /> object.</summary>
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(CollectionDebugView<>))]
	public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, IEnumerable, ICollection, IReadOnlyCollection<TKey>
	{
		private readonly ICollection<TKey> _collection;

		[NonSerialized]
		private object _syncRoot;

		/// <summary>Gets the number of elements in the collection.</summary>
		/// <returns>The number of elements in the collection.</returns>
		public int Count => _collection.Count;

		bool ICollection<TKey>.IsReadOnly => true;

		/// <summary>Gets a value that indicates whether access to the collection is synchronized (thread safe).</summary>
		/// <returns>true if access to the collection is synchronized (thread safe); otherwise, false.</returns>
		bool ICollection.IsSynchronized => false;

		/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
		/// <returns>An object that can be used to synchronize access to the collection.</returns>
		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					if (_collection is ICollection collection)
					{
						_syncRoot = collection.SyncRoot;
					}
					else
					{
						Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
					}
				}
				return _syncRoot;
			}
		}

		internal KeyCollection(ICollection<TKey> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			_collection = collection;
		}

		void ICollection<TKey>.Add(TKey item)
		{
			throw new NotSupportedException("Collection is read-only.");
		}

		void ICollection<TKey>.Clear()
		{
			throw new NotSupportedException("Collection is read-only.");
		}

		bool ICollection<TKey>.Contains(TKey item)
		{
			return _collection.Contains(item);
		}

		/// <summary>Copies the elements of the collection to an array, starting at a specific array index.</summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="arrayIndex" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="array" /> is multidimensional.-or-The number of elements in the source collection is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.-or-Type <paramref name="T" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
		public void CopyTo(TKey[] array, int arrayIndex)
		{
			_collection.CopyTo(array, arrayIndex);
		}

		bool ICollection<TKey>.Remove(TKey item)
		{
			throw new NotSupportedException("Collection is read-only.");
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<TKey> GetEnumerator()
		{
			return _collection.GetEnumerator();
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_collection).GetEnumerator();
		}

		/// <summary>Copies the elements of the collection to an array, starting at a specific array index.</summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="array" /> is multidimensional.-or-The number of elements in the source collection is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
		void ICollection.CopyTo(Array array, int index)
		{
			ReadOnlyDictionaryHelpers.CopyToNonGenericICollectionHelper(_collection, array, index);
		}

		internal KeyCollection()
		{
			ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Represents a read-only collection of the values of a <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" /> object.</summary>
	[Serializable]
	[DebuggerTypeProxy(typeof(CollectionDebugView<>))]
	[DebuggerDisplay("Count = {Count}")]
	public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, IEnumerable, ICollection, IReadOnlyCollection<TValue>
	{
		private readonly ICollection<TValue> _collection;

		[NonSerialized]
		private object _syncRoot;

		/// <summary>Gets the number of elements in the collection.</summary>
		/// <returns>The number of elements in the collection.</returns>
		public int Count => _collection.Count;

		bool ICollection<TValue>.IsReadOnly => true;

		/// <summary>Gets a value that indicates whether access to the collection is synchronized (thread safe).</summary>
		/// <returns>true if access to the collection is synchronized (thread safe); otherwise, false.</returns>
		bool ICollection.IsSynchronized => false;

		/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
		/// <returns>An object that can be used to synchronize access to the collection.</returns>
		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					if (_collection is ICollection collection)
					{
						_syncRoot = collection.SyncRoot;
					}
					else
					{
						Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
					}
				}
				return _syncRoot;
			}
		}

		internal ValueCollection(ICollection<TValue> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			_collection = collection;
		}

		void ICollection<TValue>.Add(TValue item)
		{
			throw new NotSupportedException("Collection is read-only.");
		}

		void ICollection<TValue>.Clear()
		{
			throw new NotSupportedException("Collection is read-only.");
		}

		bool ICollection<TValue>.Contains(TValue item)
		{
			return _collection.Contains(item);
		}

		/// <summary>Copies the elements of the collection to an array, starting at a specific array index.</summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="arrayIndex" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="array" /> is multidimensional.-or-The number of elements in the source collection is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.-or-Type <paramref name="T" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
		public void CopyTo(TValue[] array, int arrayIndex)
		{
			_collection.CopyTo(array, arrayIndex);
		}

		bool ICollection<TValue>.Remove(TValue item)
		{
			throw new NotSupportedException("Collection is read-only.");
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<TValue> GetEnumerator()
		{
			return _collection.GetEnumerator();
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_collection).GetEnumerator();
		}

		/// <summary>Copies the elements of the collection to an array, starting at a specific array index.</summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="array" /> is multidimensional.-or-The number of elements in the source collection is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
		void ICollection.CopyTo(Array array, int index)
		{
			ReadOnlyDictionaryHelpers.CopyToNonGenericICollectionHelper(_collection, array, index);
		}

		internal ValueCollection()
		{
			ThrowStub.ThrowNotSupportedException();
		}
	}

	private readonly IDictionary<TKey, TValue> m_dictionary;

	[NonSerialized]
	private object _syncRoot;

	[NonSerialized]
	private KeyCollection _keys;

	[NonSerialized]
	private ValueCollection _values;

	/// <summary>Gets the dictionary that is wrapped by this <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" /> object.</summary>
	/// <returns>The dictionary that is wrapped by this object.</returns>
	protected IDictionary<TKey, TValue> Dictionary => m_dictionary;

	/// <summary>Gets a key collection that contains the keys of the dictionary.</summary>
	/// <returns>A key collection that contains the keys of the dictionary.</returns>
	public KeyCollection Keys
	{
		get
		{
			if (_keys == null)
			{
				_keys = new KeyCollection(m_dictionary.Keys);
			}
			return _keys;
		}
	}

	/// <summary>Gets a collection that contains the values in the dictionary.</summary>
	/// <returns>A collection that contains the values in the object that implements <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" />.</returns>
	public ValueCollection Values
	{
		get
		{
			if (_values == null)
			{
				_values = new ValueCollection(m_dictionary.Values);
			}
			return _values;
		}
	}

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

	ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

	/// <summary>Gets the element that has the specified key.</summary>
	/// <returns>The element that has the specified key.</returns>
	/// <param name="key">The key of the element to get.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> is not found.</exception>
	public TValue this[TKey key] => m_dictionary[key];

	TValue IDictionary<TKey, TValue>.this[TKey key]
	{
		get
		{
			return m_dictionary[key];
		}
		set
		{
			throw new NotSupportedException("Collection is read-only.");
		}
	}

	/// <summary>Gets the number of items in the dictionary.</summary>
	/// <returns>The number of items in the dictionary.</returns>
	public int Count => m_dictionary.Count;

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;

	/// <summary>Gets a value that indicates whether the dictionary has a fixed size.</summary>
	/// <returns>true if the dictionary has a fixed size; otherwise, false.</returns>
	bool IDictionary.IsFixedSize => true;

	/// <summary>Gets a value that indicates whether the dictionary is read-only.</summary>
	/// <returns>true in all cases.</returns>
	bool IDictionary.IsReadOnly => true;

	/// <summary>Gets a collection that contains the keys of the dictionary.</summary>
	/// <returns>A collection that contains the keys of the dictionary.</returns>
	ICollection IDictionary.Keys => Keys;

	/// <summary>Gets a collection that contains the values in the dictionary.</summary>
	/// <returns>A collection that contains the values in the dictionary.</returns>
	ICollection IDictionary.Values => Values;

	/// <summary>Gets the element that has the specified key.</summary>
	/// <returns>The element that has the specified key.</returns>
	/// <param name="key">The key of the element to get or set. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The property is set.-or- The property is set, <paramref name="key" /> does not exist in the collection, and the dictionary has a fixed size. </exception>
	object IDictionary.this[object key]
	{
		get
		{
			if (IsCompatibleKey(key))
			{
				return this[(TKey)key];
			}
			return null;
		}
		set
		{
			throw new NotSupportedException("Collection is read-only.");
		}
	}

	/// <summary>Gets a value that indicates whether access to the dictionary is synchronized (thread safe).</summary>
	/// <returns>true if access to the dictionary is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the dictionary.</summary>
	/// <returns>An object that can be used to synchronize access to the dictionary.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				if (m_dictionary is ICollection collection)
				{
					_syncRoot = collection.SyncRoot;
				}
				else
				{
					Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
				}
			}
			return _syncRoot;
		}
	}

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" /> class that is a wrapper around the specified dictionary.</summary>
	/// <param name="dictionary">The dictionary to wrap.</param>
	public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		m_dictionary = dictionary;
	}

	/// <summary>Determines whether the dictionary contains an element that has the specified key.</summary>
	/// <returns>true if the dictionary contains an element that has the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the dictionary.</param>
	public bool ContainsKey(TKey key)
	{
		return m_dictionary.ContainsKey(key);
	}

	/// <summary>Retrieves the value that is associated with the specified key.</summary>
	/// <returns>true if the object that implements <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key whose value will be retrieved.</param>
	/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
	public bool TryGetValue(TKey key, out TValue value)
	{
		return m_dictionary.TryGetValue(key, out value);
	}

	void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	bool IDictionary<TKey, TValue>.Remove(TKey key)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
	{
		return m_dictionary.Contains(item);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		m_dictionary.CopyTo(array, arrayIndex);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Clear()
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" />.</summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return m_dictionary.GetEnumerator();
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)m_dictionary).GetEnumerator();
	}

	private static bool IsCompatibleKey(object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return key is TKey;
	}

	/// <summary>Throws a <see cref="T:System.NotSupportedException" /> exception in all cases.</summary>
	/// <param name="key">The key of the element to add. </param>
	/// <param name="value">The value of the element to add. </param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	void IDictionary.Add(object key, object value)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	/// <summary>Throws a <see cref="T:System.NotSupportedException" /> exception in all cases.</summary>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	void IDictionary.Clear()
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	/// <summary>Determines whether the dictionary contains an element that has the specified key.</summary>
	/// <returns>true if the dictionary contains an element that has the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the dictionary.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null. </exception>
	bool IDictionary.Contains(object key)
	{
		if (IsCompatibleKey(key))
		{
			return ContainsKey((TKey)key);
		}
		return false;
	}

	/// <summary>Returns an enumerator for the dictionary.</summary>
	/// <returns>An enumerator for the dictionary.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		if (m_dictionary is IDictionary dictionary)
		{
			return dictionary.GetEnumerator();
		}
		return new DictionaryEnumerator(m_dictionary);
	}

	/// <summary>Throws a <see cref="T:System.NotSupportedException" /> exception in all cases.</summary>
	/// <param name="key">The key of the element to remove. </param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	void IDictionary.Remove(object key)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	/// <summary>Copies the elements of the dictionary to an array, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the elements copied from the dictionary. The array must have zero-based indexing. </param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source dictionary is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.-or- The type of the source dictionary cannot be cast automatically to the type of the destination <paramref name="array" /><paramref name="." /></exception>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException("Only single dimensional arrays are supported for the requested action.");
		}
		if (array.GetLowerBound(0) != 0)
		{
			throw new ArgumentException("The lower bound of target array must be zero.");
		}
		if (index < 0 || index > array.Length)
		{
			throw new ArgumentOutOfRangeException("index", "Non-negative number required.");
		}
		if (array.Length - index < Count)
		{
			throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
		}
		if (array is KeyValuePair<TKey, TValue>[] array2)
		{
			m_dictionary.CopyTo(array2, index);
			return;
		}
		if (array is DictionaryEntry[] array3)
		{
			{
				foreach (KeyValuePair<TKey, TValue> item in m_dictionary)
				{
					array3[index++] = new DictionaryEntry(item.Key, item.Value);
				}
				return;
			}
		}
		if (!(array is object[] array4))
		{
			throw new ArgumentException("Target array type is not compatible with the type of items in the collection.");
		}
		try
		{
			foreach (KeyValuePair<TKey, TValue> item2 in m_dictionary)
			{
				array4[index++] = new KeyValuePair<TKey, TValue>(item2.Key, item2.Value);
			}
		}
		catch (ArrayTypeMismatchException)
		{
			throw new ArgumentException("Target array type is not compatible with the type of items in the collection.");
		}
	}
}
