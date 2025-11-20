using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity;

namespace System.Text.RegularExpressions;

/// <summary>Returns the set of captured groups in a single match.</summary>
[Serializable]
[DebuggerTypeProxy(typeof(CollectionDebuggerProxy<Group>))]
[DebuggerDisplay("Count = {Count}")]
public class GroupCollection : IList<Group>, ICollection<Group>, IEnumerable<Group>, IEnumerable, IReadOnlyList<Group>, IReadOnlyCollection<Group>, IList, ICollection
{
	private sealed class Enumerator : IEnumerator<Group>, IDisposable, IEnumerator
	{
		private readonly GroupCollection _collection;

		private int _index;

		public Group Current
		{
			get
			{
				if (_index < 0 || _index >= _collection.Count)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return _collection[_index];
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(GroupCollection collection)
		{
			_collection = collection;
			_index = -1;
		}

		public bool MoveNext()
		{
			int count = _collection.Count;
			if (_index >= count)
			{
				return false;
			}
			_index++;
			return _index < count;
		}

		void IEnumerator.Reset()
		{
			_index = -1;
		}

		void IDisposable.Dispose()
		{
		}
	}

	private readonly Match _match;

	private readonly Hashtable _captureMap;

	private Group[] _groups;

	/// <summary>Gets a value that indicates whether the collection is read-only.</summary>
	/// <returns>true in all cases.</returns>
	public bool IsReadOnly => true;

	/// <summary>Returns the number of groups in the collection.</summary>
	/// <returns>The number of groups in the collection.</returns>
	public int Count => _match._matchcount.Length;

	/// <summary>Enables access to a member of the collection by integer index.</summary>
	/// <returns>The member of the collection specified by <paramref name="groupnum" />.</returns>
	/// <param name="groupnum">The zero-based index of the collection member to be retrieved. </param>
	public Group this[int groupnum] => GetGroup(groupnum);

	/// <summary>Enables access to a member of the collection by string index.</summary>
	/// <returns>The member of the collection specified by <paramref name="groupname" />.</returns>
	/// <param name="groupname">The name of a capturing group. </param>
	public Group this[string groupname]
	{
		get
		{
			if (_match._regex != null)
			{
				return GetGroup(_match._regex.GroupNumberFromName(groupname));
			}
			return Group.s_emptyGroup;
		}
	}

	/// <summary>Gets a value that indicates whether access to the <see cref="T:System.Text.RegularExpressions.GroupCollection" /> is synchronized (thread-safe).</summary>
	/// <returns>false in all cases.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Text.RegularExpressions.GroupCollection" />.</summary>
	/// <returns>A copy of the <see cref="T:System.Text.RegularExpressions.Match" /> object to synchronize.</returns>
	public object SyncRoot => _match;

	Group IList<Group>.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			throw new NotSupportedException("Collection is read-only.");
		}
	}

	bool IList.IsFixedSize => true;

	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			throw new NotSupportedException("Collection is read-only.");
		}
	}

	internal GroupCollection(Match match, Hashtable caps)
	{
		_match = match;
		_captureMap = caps;
	}

	/// <summary>Provides an enumerator that iterates through the collection.</summary>
	/// <returns>An enumerator that contains all <see cref="T:System.Text.RegularExpressions.Group" /> objects in the <see cref="T:System.Text.RegularExpressions.GroupCollection" />.</returns>
	public IEnumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<Group> IEnumerable<Group>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	private Group GetGroup(int groupnum)
	{
		if (_captureMap != null)
		{
			if (_captureMap.TryGetValue<int>(groupnum, out var value))
			{
				return GetGroupImpl(value);
			}
		}
		else if (groupnum < _match._matchcount.Length && groupnum >= 0)
		{
			return GetGroupImpl(groupnum);
		}
		return Group.s_emptyGroup;
	}

	private Group GetGroupImpl(int groupnum)
	{
		if (groupnum == 0)
		{
			return _match;
		}
		if (_groups == null)
		{
			_groups = new Group[_match._matchcount.Length - 1];
			for (int i = 0; i < _groups.Length; i++)
			{
				string name = _match._regex.GroupNameFromNumber(i + 1);
				_groups[i] = new Group(_match.Text, _match._matches[i + 1], _match._matchcount[i + 1], name);
			}
		}
		return _groups[groupnum - 1];
	}

	/// <summary>Copies all the elements of the collection to the given array beginning at the given index.</summary>
	/// <param name="array">The array the collection is to be copied into. </param>
	/// <param name="arrayIndex">The position in the destination array where the copying is to begin. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="arrayIndex" /> is outside the bounds of <paramref name="array" />.-or-<paramref name="arrayIndex" /> plus <see cref="P:System.Text.RegularExpressions.GroupCollection.Count" /> is outside the bounds of <paramref name="array" />.</exception>
	public void CopyTo(Array array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = arrayIndex;
		for (int i = 0; i < Count; i++)
		{
			array.SetValue(this[i], num);
			num++;
		}
	}

	public void CopyTo(Group[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0 || arrayIndex > array.Length)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (array.Length - arrayIndex < Count)
		{
			throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
		}
		int num = arrayIndex;
		for (int i = 0; i < Count; i++)
		{
			array[num] = this[i];
			num++;
		}
	}

	int IList<Group>.IndexOf(Group item)
	{
		EqualityComparer<Group> equalityComparer = EqualityComparer<Group>.Default;
		for (int i = 0; i < Count; i++)
		{
			if (equalityComparer.Equals(this[i], item))
			{
				return i;
			}
		}
		return -1;
	}

	void IList<Group>.Insert(int index, Group item)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	void IList<Group>.RemoveAt(int index)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	void ICollection<Group>.Add(Group item)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	void ICollection<Group>.Clear()
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	bool ICollection<Group>.Contains(Group item)
	{
		return ((IList<Group>)this).IndexOf(item) >= 0;
	}

	bool ICollection<Group>.Remove(Group item)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	int IList.Add(object value)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	void IList.Clear()
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	bool IList.Contains(object value)
	{
		if (value is Group)
		{
			return ((ICollection<Group>)this).Contains((Group)value);
		}
		return false;
	}

	int IList.IndexOf(object value)
	{
		if (!(value is Group))
		{
			return -1;
		}
		return ((IList<Group>)this).IndexOf((Group)value);
	}

	void IList.Insert(int index, object value)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	void IList.Remove(object value)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	internal GroupCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
