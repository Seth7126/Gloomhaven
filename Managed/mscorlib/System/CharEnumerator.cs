using System.Collections;
using System.Collections.Generic;
using Unity;

namespace System;

/// <summary>Supports iterating over a <see cref="T:System.String" /> object and reading its individual characters. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class CharEnumerator : IEnumerator, IEnumerator<char>, IDisposable, ICloneable
{
	private string _str;

	private int _index;

	private char _currentElement;

	/// <summary>Gets the currently referenced character in the string enumerated by this <see cref="T:System.CharEnumerator" /> object. For a description of this member, see <see cref="P:System.Collections.IEnumerator.Current" />. </summary>
	/// <returns>The boxed Unicode character currently referenced by this <see cref="T:System.CharEnumerator" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">Enumeration has not started.-or-Enumeration has ended.</exception>
	object IEnumerator.Current => Current;

	/// <summary>Gets the currently referenced character in the string enumerated by this <see cref="T:System.CharEnumerator" /> object.</summary>
	/// <returns>The Unicode character currently referenced by this <see cref="T:System.CharEnumerator" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The index is invalid; that is, it is before the first or after the last character of the enumerated string. </exception>
	/// <filterpriority>2</filterpriority>
	public char Current
	{
		get
		{
			if (_index == -1)
			{
				throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
			}
			if (_index >= _str.Length)
			{
				throw new InvalidOperationException("Enumeration already finished.");
			}
			return _currentElement;
		}
	}

	internal CharEnumerator(string str)
	{
		_str = str;
		_index = -1;
	}

	/// <summary>Creates a copy of the current <see cref="T:System.CharEnumerator" /> object.</summary>
	/// <returns>An <see cref="T:System.Object" /> that is a copy of the current <see cref="T:System.CharEnumerator" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Increments the internal index of the current <see cref="T:System.CharEnumerator" /> object to the next character of the enumerated string.</summary>
	/// <returns>true if the index is successfully incremented and within the enumerated string; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool MoveNext()
	{
		if (_index < _str.Length - 1)
		{
			_index++;
			_currentElement = _str[_index];
			return true;
		}
		_index = _str.Length;
		return false;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.CharEnumerator" /> class.</summary>
	public void Dispose()
	{
		if (_str != null)
		{
			_index = _str.Length;
		}
		_str = null;
	}

	/// <summary>Initializes the index to a position logically before the first character of the enumerated string.</summary>
	/// <filterpriority>2</filterpriority>
	public void Reset()
	{
		_currentElement = '\0';
		_index = -1;
	}

	internal CharEnumerator()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
