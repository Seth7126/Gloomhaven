using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace System.Data;

/// <summary>Represents a collection of <see cref="T:System.Data.DataRow" /> objects returned from a LINQ to DataSet query. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code. </summary>
public abstract class EnumerableRowCollection : IEnumerable
{
	internal abstract Type ElementType { get; }

	internal abstract DataTable Table { get; }

	internal EnumerableRowCollection()
	{
	}

	/// <summary>Returns an enumerator for the collection of <see cref="T:System.Data.DataRow" /> objects. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to traverse the collection of <see cref="T:System.Data.DataRow" /> objects.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return null;
	}
}
/// <summary>Represents a collection of <see cref="T:System.Data.DataRow" /> objects returned from a query. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
/// <typeparam name="TRow">The type of objects in the source sequence, typically <see cref="T:System.Data.DataRow" />.</typeparam>
public class EnumerableRowCollection<TRow> : EnumerableRowCollection, IEnumerable<TRow>, IEnumerable
{
	private readonly DataTable _table;

	private readonly IEnumerable<TRow> _enumerableRows;

	private readonly List<Func<TRow, bool>> _listOfPredicates;

	private readonly SortExpressionBuilder<TRow> _sortExpression;

	private readonly Func<TRow, TRow> _selector;

	internal override Type ElementType => typeof(TRow);

	internal IEnumerable<TRow> EnumerableRows => _enumerableRows;

	internal override DataTable Table => _table;

	internal EnumerableRowCollection(IEnumerable<TRow> enumerableRows, bool isDataViewable, DataTable table)
	{
		_enumerableRows = enumerableRows;
		if (isDataViewable)
		{
			_table = table;
		}
		_listOfPredicates = new List<Func<TRow, bool>>();
		_sortExpression = new SortExpressionBuilder<TRow>();
	}

	internal EnumerableRowCollection(DataTable table)
	{
		_table = table;
		_enumerableRows = table.Rows.Cast<TRow>();
		_listOfPredicates = new List<Func<TRow, bool>>();
		_sortExpression = new SortExpressionBuilder<TRow>();
	}

	internal EnumerableRowCollection(EnumerableRowCollection<TRow> source, IEnumerable<TRow> enumerableRows, Func<TRow, TRow> selector)
	{
		_enumerableRows = enumerableRows;
		_selector = selector;
		if (source != null)
		{
			if (source._selector == null)
			{
				_table = source._table;
			}
			_listOfPredicates = new List<Func<TRow, bool>>(source._listOfPredicates);
			_sortExpression = source._sortExpression.Clone();
		}
		else
		{
			_listOfPredicates = new List<Func<TRow, bool>>();
			_sortExpression = new SortExpressionBuilder<TRow>();
		}
	}

	/// <summary>Returns an enumerator for the collection of <see cref="T:System.Data.DataRow" /> objects. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to traverse the collection of <see cref="T:System.Data.DataRow" /> objects. </returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Returns an enumerator for the collection of contained row objects. This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>A strongly-typed <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to traverse the collection of <paramref name="TRow" /> objects. </returns>
	/// <filterpriority>2</filterpriority>
	public IEnumerator<TRow> GetEnumerator()
	{
		return _enumerableRows.GetEnumerator();
	}

	internal void AddPredicate(Func<TRow, bool> pred)
	{
		_listOfPredicates.Add(pred);
	}

	internal void AddSortExpression<TKey>(Func<TRow, TKey> keySelector, bool isDescending, bool isOrderBy)
	{
		AddSortExpression(keySelector, Comparer<TKey>.Default, isDescending, isOrderBy);
	}

	internal void AddSortExpression<TKey>(Func<TRow, TKey> keySelector, IComparer<TKey> comparer, bool isDescending, bool isOrderBy)
	{
		DataSetUtil.CheckArgumentNull(keySelector, "keySelector");
		DataSetUtil.CheckArgumentNull(comparer, "comparer");
		_sortExpression.Add((TRow input) => keySelector(input), (object val1, object val2) => ((!isDescending) ? 1 : (-1)) * comparer.Compare((TKey)val1, (TKey)val2), isOrderBy);
	}

	internal EnumerableRowCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
