using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace System.Data;

/// <summary>Provides the base functionality for creating collections.</summary>
/// <filterpriority>2</filterpriority>
public class InternalDataCollectionBase : ICollection, IEnumerable
{
	internal static readonly CollectionChangeEventArgs s_refreshEventArgs = new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null);

	/// <summary>Gets the total number of elements in a collection.</summary>
	/// <returns>The total number of elements in a collection.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public virtual int Count => List.Count;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.InternalDataCollectionBase" /> is read-only.</summary>
	/// <returns>true if the collection is read-only; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public bool IsReadOnly => false;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.InternalDataCollectionBase" /> is synchonized.</summary>
	/// <returns>true if the collection is synchronized; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize the collection.</summary>
	/// <returns>The <see cref="T:System.object" /> used to synchronize the collection.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public object SyncRoot => this;

	/// <summary>Gets the items of the collection as a list.</summary>
	/// <returns>An <see cref="T:System.Collections.ArrayList" /> that contains the collection.</returns>
	protected virtual ArrayList List => null;

	/// <summary>Copies all the elements of the current <see cref="T:System.Data.InternalDataCollectionBase" /> to a one-dimensional <see cref="T:System.Array" />, starting at the specified <see cref="T:System.Data.InternalDataCollectionBase" /> index.</summary>
	/// <param name="ar">The one-dimensional <see cref="T:System.Array" /> to copy the current <see cref="T:System.Data.InternalDataCollectionBase" /> object's elements into. </param>
	/// <param name="index">The destination <see cref="T:System.Array" /> index to start copying into. </param>
	/// <filterpriority>2</filterpriority>
	public virtual void CopyTo(Array ar, int index)
	{
		List.CopyTo(ar, index);
	}

	/// <summary>Gets an <see cref="T:System.Collections.IEnumerator" /> for the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the collection.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual IEnumerator GetEnumerator()
	{
		return List.GetEnumerator();
	}

	internal int NamesEqual(string s1, string s2, bool fCaseSensitive, CultureInfo locale)
	{
		if (fCaseSensitive)
		{
			if (string.Compare(s1, s2, ignoreCase: false, locale) != 0)
			{
				return 0;
			}
			return 1;
		}
		if (locale.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth) == 0)
		{
			if (string.Compare(s1, s2, ignoreCase: false, locale) != 0)
			{
				return -1;
			}
			return 1;
		}
		return 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.InternalDataCollectionBase" /> class.</summary>
	public InternalDataCollectionBase()
	{
	}
}
