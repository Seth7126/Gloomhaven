using System.Collections.Generic;
using System.Linq;

namespace System.Data;

/// <summary>Contains the extension methods for the data row collection classes. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
public static class EnumerableRowCollectionExtensions
{
	/// <summary>Filters a sequence of rows based on the specified predicate. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code. </summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> that contains rows from the input sequence that satisfy the condition.</returns>
	/// <param name="source">An <see cref="T:System.Data.EnumerableRowCollection" /> containing the <see cref="T:System.Data.DataRow" /> elements to filter.  </param>
	/// <param name="predicate">A function to test each element for a condition. </param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	public static EnumerableRowCollection<TRow> Where<TRow>(this EnumerableRowCollection<TRow> source, Func<TRow, bool> predicate)
	{
		EnumerableRowCollection<TRow> enumerableRowCollection = new EnumerableRowCollection<TRow>(source, Enumerable.Where(source, predicate), null);
		enumerableRowCollection.AddPredicate(predicate);
		return enumerableRowCollection;
	}

	/// <summary>Sorts the rows of a <see cref="T:System.Data.EnumerableRowCollection" /> in ascending order according to the specified key. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key. </returns>
	/// <param name="source">An <see cref="T:System.Data.EnumerableRowCollection" /> containing the <see cref="T:System.Data.DataRow" /> elements to be ordered. </param>
	/// <param name="keySelector">A function to extract a key from an element.</param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey>(this EnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector)
	{
		IEnumerable<TRow> enumerableRows = Enumerable.OrderBy(source, keySelector);
		OrderedEnumerableRowCollection<TRow> orderedEnumerableRowCollection = new OrderedEnumerableRowCollection<TRow>(source, enumerableRows);
		orderedEnumerableRowCollection.AddSortExpression(keySelector, isDescending: false, isOrderBy: true);
		return orderedEnumerableRowCollection;
	}

	/// <summary>Sorts the rows of a <see cref="T:System.Data.EnumerableRowCollection" /> in ascending order according to the specified key and comparer. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key and comparer.</returns>
	/// <param name="source">An <see cref="T:System.Data.EnumerableRowCollection" /> containing the <see cref="T:System.Data.DataRow" /> elements to be ordered. </param>
	/// <param name="keySelector">A function to extract a key from an element. </param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys. </param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey>(this EnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
	{
		IEnumerable<TRow> enumerableRows = Enumerable.OrderBy(source, keySelector, comparer);
		OrderedEnumerableRowCollection<TRow> orderedEnumerableRowCollection = new OrderedEnumerableRowCollection<TRow>(source, enumerableRows);
		orderedEnumerableRowCollection.AddSortExpression(keySelector, comparer, isDescending: false, isOrderBy: true);
		return orderedEnumerableRowCollection;
	}

	/// <summary>Sorts the rows of a <see cref="T:System.Data.EnumerableRowCollection" /> in descending order according to the specified key. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key. </returns>
	/// <param name="source">An <see cref="T:System.Data.EnumerableRowCollection" /> containing the <see cref="T:System.Data.DataRow" /> elements to be ordered. </param>
	/// <param name="keySelector">A function to extract a key from an element. </param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey>(this EnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector)
	{
		IEnumerable<TRow> enumerableRows = Enumerable.OrderByDescending(source, keySelector);
		OrderedEnumerableRowCollection<TRow> orderedEnumerableRowCollection = new OrderedEnumerableRowCollection<TRow>(source, enumerableRows);
		orderedEnumerableRowCollection.AddSortExpression(keySelector, isDescending: true, isOrderBy: true);
		return orderedEnumerableRowCollection;
	}

	/// <summary>Sorts the rows of a <see cref="T:System.Data.EnumerableRowCollection" /> in descending order according to the specified key and comparer. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key and comparer. </returns>
	/// <param name="source">An <see cref="T:System.Data.EnumerableRowCollection" /> containing the <see cref="T:System.Data.DataRow" /> elements to be ordered. </param>
	/// <param name="keySelector">A function to extract a key from an element.  </param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys. </param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey>(this EnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
	{
		IEnumerable<TRow> enumerableRows = Enumerable.OrderByDescending(source, keySelector, comparer);
		OrderedEnumerableRowCollection<TRow> orderedEnumerableRowCollection = new OrderedEnumerableRowCollection<TRow>(source, enumerableRows);
		orderedEnumerableRowCollection.AddSortExpression(keySelector, comparer, isDescending: true, isOrderBy: true);
		return orderedEnumerableRowCollection;
	}

	/// <summary>Performs a secondary ordering of the rows of a <see cref="T:System.Data.EnumerableRowCollection" /> in ascending order according to the specified key. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key. </returns>
	/// <param name="source">An <see cref="T:System.Data.EnumerableRowCollection" /> containing the <see cref="T:System.Data.DataRow" /> elements to be ordered.  </param>
	/// <param name="keySelector">A function to extract a key from an element. </param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> ThenBy<TRow, TKey>(this OrderedEnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector)
	{
		IEnumerable<TRow> enumerableRows = ((IOrderedEnumerable<TRow>)source.EnumerableRows).ThenBy(keySelector);
		OrderedEnumerableRowCollection<TRow> orderedEnumerableRowCollection = new OrderedEnumerableRowCollection<TRow>(source, enumerableRows);
		orderedEnumerableRowCollection.AddSortExpression(keySelector, isDescending: false, isOrderBy: false);
		return orderedEnumerableRowCollection;
	}

	/// <summary>Performs a secondary ordering of the rows of a <see cref="T:System.Data.EnumerableRowCollection" /> in ascending order according to the specified key and comparer. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key and comparer.</returns>
	/// <param name="source">An <see cref="T:System.Data.EnumerableRowCollection" /> containing the <see cref="T:System.Data.DataRow" /> elements to be ordered. </param>
	/// <param name="keySelector">A function to extract a key from an element. </param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys. </param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> ThenBy<TRow, TKey>(this OrderedEnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
	{
		IEnumerable<TRow> enumerableRows = ((IOrderedEnumerable<TRow>)source.EnumerableRows).ThenBy(keySelector, comparer);
		OrderedEnumerableRowCollection<TRow> orderedEnumerableRowCollection = new OrderedEnumerableRowCollection<TRow>(source, enumerableRows);
		orderedEnumerableRowCollection.AddSortExpression(keySelector, comparer, isDescending: false, isOrderBy: false);
		return orderedEnumerableRowCollection;
	}

	/// <summary>Performs a secondary ordering of the rows of a <see cref="T:System.Data.EnumerableRowCollection" /> in descending order according to the specified key. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key. </returns>
	/// <param name="source">An <see cref="T:System.Data.EnumerableRowCollection" /> containing the <see cref="T:System.Data.DataRow" /> elements to be ordered.  </param>
	/// <param name="keySelector">A function to extract a key from an element. </param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> ThenByDescending<TRow, TKey>(this OrderedEnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector)
	{
		IEnumerable<TRow> enumerableRows = ((IOrderedEnumerable<TRow>)source.EnumerableRows).ThenByDescending(keySelector);
		OrderedEnumerableRowCollection<TRow> orderedEnumerableRowCollection = new OrderedEnumerableRowCollection<TRow>(source, enumerableRows);
		orderedEnumerableRowCollection.AddSortExpression(keySelector, isDescending: true, isOrderBy: false);
		return orderedEnumerableRowCollection;
	}

	/// <summary>Performs a secondary ordering of the rows of a <see cref="T:System.Data.EnumerableRowCollection" /> in descending order according to the specified key and comparer. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Data.OrderedEnumerableRowCollection`1" /> whose elements are sorted by the specified key and comparer. </returns>
	/// <param name="source">An <see cref="T:System.Data.EnumerableRowCollection" /> containing the <see cref="T:System.Data.DataRow" /> elements to be ordered.  </param>
	/// <param name="keySelector">A function to extract a key from an element. </param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys. </param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />. </typeparam>
	public static OrderedEnumerableRowCollection<TRow> ThenByDescending<TRow, TKey>(this OrderedEnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
	{
		IEnumerable<TRow> enumerableRows = ((IOrderedEnumerable<TRow>)source.EnumerableRows).ThenByDescending(keySelector, comparer);
		OrderedEnumerableRowCollection<TRow> orderedEnumerableRowCollection = new OrderedEnumerableRowCollection<TRow>(source, enumerableRows);
		orderedEnumerableRowCollection.AddSortExpression(keySelector, comparer, isDescending: true, isOrderBy: false);
		return orderedEnumerableRowCollection;
	}

	/// <summary>Projects each element of an <see cref="T:System.Data.EnumerableRowCollection`1" /> into a new form. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Data.EnumerableRowCollection`1" /> whose elements are the result of invoking the transform function on each element of <paramref name="source" />. </returns>
	/// <param name="source">An <see cref="T:System.Data.EnumerableRowCollection`1" /> containing the <see cref="T:System.Data.DataRow" /> elements to invoke a transform function upon. </param>
	/// <param name="selector">A transform function to apply to each element. </param>
	/// <typeparam name="TRow">The type of the row elements in <paramref name="source" />, typically <see cref="T:System.Data.DataRow" />.</typeparam>
	/// <typeparam name="S">The type that <paramref name="TRow" /> will be transformed into.</typeparam>
	public static EnumerableRowCollection<S> Select<TRow, S>(this EnumerableRowCollection<TRow> source, Func<TRow, S> selector)
	{
		IEnumerable<S> enumerableRows = Enumerable.Select(source, selector);
		return new EnumerableRowCollection<S>(source as EnumerableRowCollection<S>, enumerableRows, selector as Func<S, S>);
	}

	/// <summary>Converts the elements of an <see cref="T:System.Data.EnumerableRowCollection" /> to the specified type. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Data.EnumerableRowCollection" /> that contains each element of the source sequence converted to the specified type.</returns>
	/// <param name="source">The <see cref="T:System.Data.EnumerableRowCollection" /> that contains the elements to be converted.</param>
	/// <typeparam name="TResult">The type to convert the elements of source to.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">An element in the sequence cannot be cast to type <paramref name="TResult" />.</exception>
	public static EnumerableRowCollection<TResult> Cast<TResult>(this EnumerableRowCollection source)
	{
		if (source != null && source.ElementType.Equals(typeof(TResult)))
		{
			return (EnumerableRowCollection<TResult>)source;
		}
		return new EnumerableRowCollection<TResult>(Enumerable.Cast<TResult>(source), typeof(TResult).IsAssignableFrom(source.ElementType) && typeof(DataRow).IsAssignableFrom(typeof(TResult)), source.Table);
	}
}
