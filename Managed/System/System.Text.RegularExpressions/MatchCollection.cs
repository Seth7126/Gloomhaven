using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity;

namespace System.Text.RegularExpressions;

/// <summary>Represents the set of successful matches found by iteratively applying a regular expression pattern to the input string.</summary>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(CollectionDebuggerProxy<Match>))]
public class MatchCollection : IList<Match>, ICollection<Match>, IEnumerable<Match>, IEnumerable, IReadOnlyList<Match>, IReadOnlyCollection<Match>, IList, ICollection
{
	[Serializable]
	private sealed class Enumerator : IEnumerator<Match>, IDisposable, IEnumerator
	{
		private readonly MatchCollection _collection;

		private int _index;

		public Match Current
		{
			get
			{
				if (_index < 0)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return _collection.GetMatch(_index);
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(MatchCollection collection)
		{
			_collection = collection;
			_index = -1;
		}

		public bool MoveNext()
		{
			if (_index == -2)
			{
				return false;
			}
			_index++;
			if (_collection.GetMatch(_index) == null)
			{
				_index = -2;
				return false;
			}
			return true;
		}

		void IEnumerator.Reset()
		{
			_index = -1;
		}

		void IDisposable.Dispose()
		{
		}
	}

	private readonly Regex _regex;

	private readonly List<Match> _matches;

	private bool _done;

	private readonly string _input;

	private readonly int _beginning;

	private readonly int _length;

	private int _startat;

	private int _prevlen;

	/// <summary>Gets a value that indicates whether the collection is read only.</summary>
	/// <returns>true in all cases. </returns>
	public bool IsReadOnly => true;

	/// <summary>Gets the number of matches.</summary>
	/// <returns>The number of matches.</returns>
	/// <exception cref="T:System.Text.RegularExpressions.RegexMatchTimeoutException">A time-out occurred.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int Count
	{
		get
		{
			EnsureInitialized();
			return _matches.Count;
		}
	}

	/// <summary>Gets an individual member of the collection.</summary>
	/// <returns>The captured substring at position <paramref name="i" /> in the collection.</returns>
	/// <param name="i">Index into the <see cref="T:System.Text.RegularExpressions.Match" /> collection. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="i" /> is less than 0 or greater than or equal to <see cref="P:System.Text.RegularExpressions.MatchCollection.Count" />. </exception>
	/// <exception cref="T:System.Text.RegularExpressions.RegexMatchTimeoutException">A time-out occurred.</exception>
	public virtual Match this[int i]
	{
		get
		{
			if (i < 0)
			{
				throw new ArgumentOutOfRangeException("i");
			}
			return GetMatch(i) ?? throw new ArgumentOutOfRangeException("i");
		}
	}

	/// <summary>Gets a value indicating whether access to the collection is synchronized (thread-safe).</summary>
	/// <returns>false in all cases.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
	/// <returns>An object that can be used to synchronize access to the collection. This property always returns the object itself.</returns>
	public object SyncRoot => this;

	Match IList<Match>.this[int index]
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

	internal MatchCollection(Regex regex, string input, int beginning, int length, int startat)
	{
		if (startat < 0 || startat > input.Length)
		{
			throw new ArgumentOutOfRangeException("startat", "Start index cannot be less than 0 or greater than input length.");
		}
		_regex = regex;
		_input = input;
		_beginning = beginning;
		_length = length;
		_startat = startat;
		_prevlen = -1;
		_matches = new List<Match>();
		_done = false;
	}

	/// <summary>Provides an enumerator that iterates through the collection.</summary>
	/// <returns>An object that contains all <see cref="T:System.Text.RegularExpressions.Match" /> objects within the <see cref="T:System.Text.RegularExpressions.MatchCollection" />.</returns>
	/// <exception cref="T:System.Text.RegularExpressions.RegexMatchTimeoutException">A time-out occurred.</exception>
	public IEnumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<Match> IEnumerable<Match>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	private Match GetMatch(int i)
	{
		if (_matches.Count > i)
		{
			return _matches[i];
		}
		if (_done)
		{
			return null;
		}
		Match match;
		do
		{
			match = _regex.Run(quick: false, _prevlen, _input, _beginning, _length, _startat);
			if (!match.Success)
			{
				_done = true;
				return null;
			}
			_matches.Add(match);
			_prevlen = match.Length;
			_startat = match._textpos;
		}
		while (_matches.Count <= i);
		return match;
	}

	private void EnsureInitialized()
	{
		if (!_done)
		{
			GetMatch(int.MaxValue);
		}
	}

	/// <summary>Copies all the elements of the collection to the given array starting at the given index.</summary>
	/// <param name="array">The array the collection is to be copied into. </param>
	/// <param name="arrayIndex">The position in the array where copying is to begin. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is a multi-dimensional array.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="arrayIndex" /> is outside the bounds of <paramref name="array" />.-or-<paramref name="arrayIndex" /> plus <see cref="P:System.Text.RegularExpressions.MatchCollection.Count" /> is outside the bounds of <paramref name="array" />.</exception>
	/// <exception cref="T:System.Text.RegularExpressions.RegexMatchTimeoutException">A time-out occurred.</exception>
	public void CopyTo(Array array, int arrayIndex)
	{
		EnsureInitialized();
		((ICollection)_matches).CopyTo(array, arrayIndex);
	}

	public void CopyTo(Match[] array, int arrayIndex)
	{
		EnsureInitialized();
		_matches.CopyTo(array, arrayIndex);
	}

	int IList<Match>.IndexOf(Match item)
	{
		EnsureInitialized();
		return _matches.IndexOf(item);
	}

	void IList<Match>.Insert(int index, Match item)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	void IList<Match>.RemoveAt(int index)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	void ICollection<Match>.Add(Match item)
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	void ICollection<Match>.Clear()
	{
		throw new NotSupportedException("Collection is read-only.");
	}

	bool ICollection<Match>.Contains(Match item)
	{
		EnsureInitialized();
		return _matches.Contains(item);
	}

	bool ICollection<Match>.Remove(Match item)
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
		if (value is Match)
		{
			return ((ICollection<Match>)this).Contains((Match)value);
		}
		return false;
	}

	int IList.IndexOf(object value)
	{
		if (!(value is Match))
		{
			return -1;
		}
		return ((IList<Match>)this).IndexOf((Match)value);
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

	internal MatchCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
