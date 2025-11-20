using System.Collections;
using System.Collections.Generic;

namespace System.Security.Cryptography;

/// <summary>Contains a set of <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> objects.</summary>
public sealed class CryptographicAttributeObjectCollection : ICollection, IEnumerable
{
	private readonly List<CryptographicAttributeObject> _list;

	/// <summary>Gets the <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object at the specified index in the collection.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object at the specified index.</returns>
	/// <param name="index">An <see cref="T:System.Int32" /> value that represents the zero-based index of the <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object to retrieve.</param>
	public CryptographicAttributeObject this[int index] => _list[index];

	/// <summary>Gets the number of items in the collection.</summary>
	/// <returns>The number of items in the collection.</returns>
	public int Count => _list.Count;

	/// <summary>Gets a value that indicates whether access to the collection is synchronized, or thread safe.</summary>
	/// <returns>true if access to the collection is thread safe; otherwise false.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets an <see cref="T:System.Object" /> object used to synchronize access to the collection.</summary>
	/// <returns>An <see cref="T:System.Object" /> object used to synchronize access to the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</returns>
	public object SyncRoot => this;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> class.</summary>
	public CryptographicAttributeObjectCollection()
	{
		_list = new List<CryptographicAttributeObject>();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> class, adding a specified <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> to the collection.</summary>
	/// <param name="attribute">A <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object that is added to the collection.</param>
	public CryptographicAttributeObjectCollection(CryptographicAttributeObject attribute)
	{
		_list = new List<CryptographicAttributeObject>();
		_list.Add(attribute);
	}

	/// <summary>Adds the specified <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object to the collection.</summary>
	/// <returns>true if the method returns the zero-based index of the added item; otherwise, false.</returns>
	/// <param name="asnEncodedData">The <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object to add to the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asnEncodedData" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public int Add(AsnEncodedData asnEncodedData)
	{
		if (asnEncodedData == null)
		{
			throw new ArgumentNullException("asnEncodedData");
		}
		return Add(new CryptographicAttributeObject(asnEncodedData.Oid, new AsnEncodedDataCollection(asnEncodedData)));
	}

	/// <summary>Adds the specified <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object to the collection.</summary>
	/// <returns>true if the method returns the zero-based index of the added item; otherwise, false.</returns>
	/// <param name="attribute">The <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object to add to the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asnEncodedData" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The specified item already exists in the collection.</exception>
	public int Add(CryptographicAttributeObject attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		string value = attribute.Oid.Value;
		for (int i = 0; i < _list.Count; i++)
		{
			CryptographicAttributeObject cryptographicAttributeObject = _list[i];
			if (cryptographicAttributeObject.Values == attribute.Values)
			{
				throw new InvalidOperationException("Duplicate items are not allowed in the collection.");
			}
			string value2 = cryptographicAttributeObject.Oid.Value;
			if (string.Equals(value, value2, StringComparison.OrdinalIgnoreCase))
			{
				if (string.Equals(value, "1.2.840.113549.1.9.5", StringComparison.OrdinalIgnoreCase))
				{
					throw new CryptographicException("Cannot add multiple PKCS 9 signing time attributes.");
				}
				AsnEncodedDataEnumerator enumerator = attribute.Values.GetEnumerator();
				while (enumerator.MoveNext())
				{
					AsnEncodedData current = enumerator.Current;
					cryptographicAttributeObject.Values.Add(current);
				}
				return i;
			}
		}
		int count = _list.Count;
		_list.Add(attribute);
		return count;
	}

	internal void AddWithoutMerge(CryptographicAttributeObject attribute)
	{
		_list.Add(attribute);
	}

	/// <summary>Removes the specified <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object from the collection.</summary>
	/// <param name="attribute">The <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object to remove from the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attribute" /> is null.</exception>
	public void Remove(CryptographicAttributeObject attribute)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		_list.Remove(attribute);
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectEnumerator" /> object for the collection.</summary>
	/// <returns>true if the method returns a <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectEnumerator" /> object that can be used to enumerate the collection; otherwise, false.</returns>
	public CryptographicAttributeObjectEnumerator GetEnumerator()
	{
		return new CryptographicAttributeObjectEnumerator(this);
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new CryptographicAttributeObjectEnumerator(this);
	}

	/// <summary>Copies the elements of this <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection to an <see cref="T:System.Array" /> array, starting at a particular index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> array that is the destination of the elements copied from this <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" />. The <see cref="T:System.Array" /> array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
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
		if (index < 0 || index >= array.Length)
		{
			throw new ArgumentOutOfRangeException("index", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (index > array.Length - Count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		for (int i = 0; i < Count; i++)
		{
			array.SetValue(this[i], index);
			index++;
		}
	}

	/// <summary>Copies the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection to an array of <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> objects.</summary>
	/// <param name="array">An array of <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> objects that the collection is copied to.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> to which the collection is to be copied.</param>
	/// <exception cref="T:System.ArgumentException">One of the arguments provided to a method was not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">null was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of an argument was outside the allowable range of values as defined by the called method.</exception>
	public void CopyTo(CryptographicAttributeObject[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || index >= array.Length)
		{
			throw new ArgumentOutOfRangeException("index", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (index > array.Length - Count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		_list.CopyTo(array, index);
	}
}
