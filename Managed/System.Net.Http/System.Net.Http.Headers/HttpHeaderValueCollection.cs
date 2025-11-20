using System.Collections;
using System.Collections.Generic;
using Unity;

namespace System.Net.Http.Headers;

/// <summary>Represents a collection of header values.</summary>
/// <typeparam name="T">The header collection type.</typeparam>
public sealed class HttpHeaderValueCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable where T : class
{
	private readonly List<T> list;

	private readonly HttpHeaders headers;

	private readonly HeaderInfo headerInfo;

	private List<string> invalidValues;

	/// <summary>Gets the number of headers in the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" />.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.The number of headers in a collection</returns>
	public int Count => list.Count;

	internal List<string> InvalidValues => invalidValues;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" /> instance is read-only.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" /> instance is read-only; otherwise, false.</returns>
	public bool IsReadOnly => false;

	internal HttpHeaderValueCollection(HttpHeaders headers, HeaderInfo headerInfo)
	{
		list = new List<T>();
		this.headers = headers;
		this.headerInfo = headerInfo;
	}

	/// <summary>Adds an entry to the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" />.</summary>
	/// <param name="item">The item to add to the header collection.</param>
	public void Add(T item)
	{
		list.Add(item);
	}

	internal void AddRange(List<T> values)
	{
		list.AddRange(values);
	}

	internal void AddInvalidValue(string invalidValue)
	{
		if (invalidValues == null)
		{
			invalidValues = new List<string>();
		}
		invalidValues.Add(invalidValue);
	}

	/// <summary>Removes all entries from the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" />.</summary>
	public void Clear()
	{
		list.Clear();
		invalidValues = null;
	}

	/// <summary>Determines if the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" /> contains an item.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the entry is contained in the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" /> instance; otherwise, false</returns>
	/// <param name="item">The item to find to the header collection.</param>
	public bool Contains(T item)
	{
		return list.Contains(item);
	}

	/// <summary>Copies the entire <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	public void CopyTo(T[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	/// <summary>Parses and adds an entry to the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" />.</summary>
	/// <param name="input">The entry to add.</param>
	public void ParseAdd(string input)
	{
		headers.AddValue(input, headerInfo, ignoreInvalid: false);
	}

	/// <summary>Removes the specified item from the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" />.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the <paramref name="item" /> was removed from the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" /> instance; otherwise, false</returns>
	/// <param name="item">The item to remove.</param>
	public bool Remove(T item)
	{
		return list.Remove(item);
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" /> object. object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		string text = string.Join(headerInfo.Separator, list);
		if (invalidValues != null)
		{
			text += string.Join(headerInfo.Separator, invalidValues);
		}
		return text;
	}

	/// <summary>Determines whether the input could be parsed and added to the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" />.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the <paramref name="input" /> could be parsed and added to the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" /> instance; otherwise, false</returns>
	/// <param name="input">The entry to validate.</param>
	public bool TryParseAdd(string input)
	{
		return headers.AddValue(input, headerInfo, ignoreInvalid: true);
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" />.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.IEnumerator`1" />.An enumerator for the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" /> instance.</returns>
	public IEnumerator<T> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" />.</summary>
	/// <returns>Returns <see cref="T:System.Collections.IEnumerator" />.An enumerator for the <see cref="T:System.Net.Http.Headers.HttpHeaderValueCollection`1" /> instance.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal T Find(Predicate<T> predicate)
	{
		return list.Find(predicate);
	}

	internal void Remove(Predicate<T> predicate)
	{
		T val = Find(predicate);
		if (val != null)
		{
			Remove(val);
		}
	}

	internal HttpHeaderValueCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
