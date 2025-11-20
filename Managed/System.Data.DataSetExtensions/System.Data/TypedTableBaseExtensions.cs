using System.Collections.Generic;

namespace System.Data;

/// <summary>Contains the extension methods for the <see cref="T:System.Data.TypedTableBase`1" /> class. </summary>
public static class TypedTableBaseExtensions
{
	/// <summary>Filters a sequence of rows based on the specified predicate.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> that contains rows from the input sequence that satisfy the condition.</returns>
	/// <param name="source">A <see cref="T:System.Data.TypedTableBase`1" /> that contains the <see cref="T:System.Data.DataRow" /> elements to filter.  </param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, <see cref="T:System.Data.DataRow" />.</typeparam>
	public static EnumerableRowCollection<TRow> Where<TRow>(this TypedTableBase<TRow> source, Func<TRow, bool> predicate) where TRow : DataRow
	{
		DataSetUtil.CheckArgumentNull(source, "source");
		return new EnumerableRowCollection<TRow>(source).Where(predicate);
	}

	/// <summary>Sorts the rows of a <see cref="T:System.Data.TypedTableBase`1" /> in ascending order according to the specified key.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key.</returns>
	/// <param name="source">A <see cref="T:System.Data.TypedTableBase`1" /> that contains the <see cref="T:System.Data.DataRow" /> elements to be ordered.</param>
	/// <param name="keySelector">A function to extract a key from an element.</param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey>(this TypedTableBase<TRow> source, Func<TRow, TKey> keySelector) where TRow : DataRow
	{
		DataSetUtil.CheckArgumentNull(source, "source");
		return new EnumerableRowCollection<TRow>(source).OrderBy(keySelector);
	}

	/// <summary>Sorts the rows of a <see cref="T:System.Data.TypedTableBase`1" /> in ascending order according to the specified key and comparer.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key and comparer.</returns>
	/// <param name="source">A <see cref="T:System.Data.TypedTableBase`1" /> that contains the <see cref="T:System.Data.DataRow" /> elements to be ordered.</param>
	/// <param name="keySelector">A function to extract a key from an element.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey>(this TypedTableBase<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer) where TRow : DataRow
	{
		DataSetUtil.CheckArgumentNull(source, "source");
		return new EnumerableRowCollection<TRow>(source).OrderBy(keySelector, comparer);
	}

	/// <summary>Sorts the rows of a <see cref="T:System.Data.TypedTableBase`1" /> in descending order according to the specified key.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key.</returns>
	/// <param name="source">A <see cref="T:System.Data.TypedTableBase`1" /> that contains the <see cref="T:System.Data.DataRow" /> elements to be ordered. </param>
	/// <param name="keySelector">A function to extract a key from an element. </param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey>(this TypedTableBase<TRow> source, Func<TRow, TKey> keySelector) where TRow : DataRow
	{
		DataSetUtil.CheckArgumentNull(source, "source");
		return new EnumerableRowCollection<TRow>(source).OrderByDescending(keySelector);
	}

	/// <summary>Sorts the rows of a <see cref="T:System.Data.TypedTableBase`1" /> in descending order according to the specified key and comparer.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key and comparer.</returns>
	/// <param name="source">A <see cref="T:System.Data.TypedTableBase`1" /> that contains the <see cref="T:System.Data.DataRow" /> elements to be ordered. </param>
	/// <param name="keySelector">A function to extract a key from an element.  </param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys. </param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey>(this TypedTableBase<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer) where TRow : DataRow
	{
		DataSetUtil.CheckArgumentNull(source, "source");
		return new EnumerableRowCollection<TRow>(source).OrderByDescending(keySelector, comparer);
	}

	/// <summary>Projects each element of a <see cref="T:System.Data.TypedTableBase`1" /> into a new form.</summary>
	/// <returns>An <see cref="T:System.Data.EnumerableRowCollection`1" /> whose elements are the result of invoking the transformation function on each element of <paramref name="source" />.</returns>
	/// <param name="source">A <see cref="T:System.Data.TypedTableBase`1" /> that contains the <see cref="T:System.Data.DataRow" /> elements to invoke a transformation function upon.</param>
	/// <param name="selector">A transformation function to apply to each element.</param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="S"></typeparam>
	public static EnumerableRowCollection<S> Select<TRow, S>(this TypedTableBase<TRow> source, Func<TRow, S> selector) where TRow : DataRow
	{
		DataSetUtil.CheckArgumentNull(source, "source");
		return new EnumerableRowCollection<TRow>(source).Select(selector);
	}

	/// <summary>Enumerates the data row elements of the <see cref="T:System.Data.TypedTableBase`1" /> and returns an <see cref="T:System.Data.EnumerableRowCollection`1" /> object, where the generic parameter <paramref name="T" /> is <see cref="T:System.Data.DataRow" />. This object can be used in a LINQ expression or method query. </summary>
	/// <returns>An <see cref="T:System.Data.EnumerableRowCollection`1" /> object, where the generic parameter <paramref name="T" /> is <see cref="T:System.Data.DataRow" />.</returns>
	/// <param name="source">The source <see cref="T:System.Data.TypedTableBase`1" /> to make enumerable.</param>
	/// <typeparam name="TRow">The type to convert the elements of the source to.</typeparam>
	public static EnumerableRowCollection<TRow> AsEnumerable<TRow>(this TypedTableBase<TRow> source) where TRow : DataRow
	{
		DataSetUtil.CheckArgumentNull(source, "source");
		return new EnumerableRowCollection<TRow>(source);
	}

	/// <summary>Returns the element at a specified row in a sequence or a default value if the row is out of range.</summary>
	/// <returns>The element at a specified row in a sequence.</returns>
	/// <param name="source">An enumerable object to return an element from.</param>
	/// <param name="index">The zero-based index of the element to retrieve.</param>
	/// <typeparam name="TRow">The type of the elements or the row.</typeparam>
	public static TRow ElementAtOrDefault<TRow>(this TypedTableBase<TRow> source, int index) where TRow : DataRow
	{
		if (index >= 0 && index < source.Rows.Count)
		{
			return (TRow)source.Rows[index];
		}
		return null;
	}
}
