using System.Collections;
using Unity;

namespace System.Runtime.Serialization;

/// <summary>Provides a formatter-friendly mechanism for parsing the data in <see cref="T:System.Runtime.Serialization.SerializationInfo" />. This class cannot be inherited.</summary>
public sealed class SerializationInfoEnumerator : IEnumerator
{
	private readonly string[] _members;

	private readonly object[] _data;

	private readonly Type[] _types;

	private readonly int _numItems;

	private int _currItem;

	private bool _current;

	/// <summary>Gets the current item in the collection.</summary>
	/// <returns>A <see cref="T:System.Runtime.Serialization.SerializationEntry" /> that contains the current serialization data.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumeration has not started or has already ended. </exception>
	object IEnumerator.Current => Current;

	/// <summary>Gets the item currently being examined.</summary>
	/// <returns>The item currently being examined.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator has not started enumerating items or has reached the end of the enumeration. </exception>
	public SerializationEntry Current
	{
		get
		{
			if (!_current)
			{
				throw new InvalidOperationException("Enumeration has either not started or has already finished.");
			}
			return new SerializationEntry(_members[_currItem], _data[_currItem], _types[_currItem]);
		}
	}

	/// <summary>Gets the name for the item currently being examined.</summary>
	/// <returns>The item name.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator has not started enumerating items or has reached the end of the enumeration. </exception>
	public string Name
	{
		get
		{
			if (!_current)
			{
				throw new InvalidOperationException("Enumeration has either not started or has already finished.");
			}
			return _members[_currItem];
		}
	}

	/// <summary>Gets the value of the item currently being examined.</summary>
	/// <returns>The value of the item currently being examined.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator has not started enumerating items or has reached the end of the enumeration. </exception>
	public object Value
	{
		get
		{
			if (!_current)
			{
				throw new InvalidOperationException("Enumeration has either not started or has already finished.");
			}
			return _data[_currItem];
		}
	}

	/// <summary>Gets the type of the item currently being examined.</summary>
	/// <returns>The type of the item currently being examined.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator has not started enumerating items or has reached the end of the enumeration. </exception>
	public Type ObjectType
	{
		get
		{
			if (!_current)
			{
				throw new InvalidOperationException("Enumeration has either not started or has already finished.");
			}
			return _types[_currItem];
		}
	}

	internal SerializationInfoEnumerator(string[] members, object[] info, Type[] types, int numItems)
	{
		_members = members;
		_data = info;
		_types = types;
		_numItems = numItems - 1;
		_currItem = -1;
		_current = false;
	}

	/// <summary>Updates the enumerator to the next item.</summary>
	/// <returns>true if a new element is found; otherwise, false.</returns>
	public bool MoveNext()
	{
		if (_currItem < _numItems)
		{
			_currItem++;
			_current = true;
		}
		else
		{
			_current = false;
		}
		return _current;
	}

	/// <summary>Resets the enumerator to the first item.</summary>
	public void Reset()
	{
		_currItem = -1;
		_current = false;
	}

	internal SerializationInfoEnumerator()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
