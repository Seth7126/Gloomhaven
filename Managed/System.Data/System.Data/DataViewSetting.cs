using System.ComponentModel;

namespace System.Data;

/// <summary>Represents the default settings for <see cref="P:System.Data.DataView.ApplyDefaultSort" />, <see cref="P:System.Data.DataView.DataViewManager" />, <see cref="P:System.Data.DataView.RowFilter" />, <see cref="P:System.Data.DataView.RowStateFilter" />, <see cref="P:System.Data.DataView.Sort" />, and <see cref="P:System.Data.DataView.Table" /> for DataViews created from the <see cref="T:System.Data.DataViewManager" />.</summary>
/// <filterpriority>1</filterpriority>
[TypeConverter(typeof(ExpandableObjectConverter))]
public class DataViewSetting
{
	private DataViewManager _dataViewManager;

	private DataTable _table;

	private string _sort = string.Empty;

	private string _rowFilter = string.Empty;

	private DataViewRowState _rowStateFilter = DataViewRowState.CurrentRows;

	private bool _applyDefaultSort;

	/// <summary>Gets or sets a value indicating whether to use the default sort.</summary>
	/// <returns>true if the default sort is used; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	public bool ApplyDefaultSort
	{
		get
		{
			return _applyDefaultSort;
		}
		set
		{
			if (_applyDefaultSort != value)
			{
				_applyDefaultSort = value;
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Data.DataViewManager" /> that contains this <see cref="T:System.Data.DataViewSetting" />.</summary>
	/// <returns>A <see cref="T:System.Data.DataViewManager" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public DataViewManager DataViewManager => _dataViewManager;

	/// <summary>Gets the <see cref="T:System.Data.DataTable" /> to which the <see cref="T:System.Data.DataViewSetting" /> properties apply.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public DataTable Table => _table;

	/// <summary>Gets or sets the filter to apply in the <see cref="T:System.Data.DataView" />. See <see cref="P:System.Data.DataView.RowFilter" /> for a code sample using RowFilter.</summary>
	/// <returns>A string that contains the filter to apply.</returns>
	/// <filterpriority>1</filterpriority>
	public string RowFilter
	{
		get
		{
			return _rowFilter;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			if (_rowFilter != value)
			{
				_rowFilter = value;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether to display Current, Deleted, Modified Current, ModifiedOriginal, New, Original, Unchanged, or no rows in the <see cref="T:System.Data.DataView" />.</summary>
	/// <returns>A value that indicates which rows to display.</returns>
	/// <filterpriority>1</filterpriority>
	public DataViewRowState RowStateFilter
	{
		get
		{
			return _rowStateFilter;
		}
		set
		{
			if (_rowStateFilter != value)
			{
				_rowStateFilter = value;
			}
		}
	}

	/// <summary>Gets or sets a value indicating the sort to apply in the <see cref="T:System.Data.DataView" />. </summary>
	/// <returns>The sort to apply in the <see cref="T:System.Data.DataView" />.</returns>
	/// <filterpriority>1</filterpriority>
	public string Sort
	{
		get
		{
			return _sort;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			if (_sort != value)
			{
				_sort = value;
			}
		}
	}

	internal DataViewSetting()
	{
	}

	internal void SetDataViewManager(DataViewManager dataViewManager)
	{
		if (_dataViewManager != dataViewManager)
		{
			_dataViewManager = dataViewManager;
		}
	}

	internal void SetDataTable(DataTable table)
	{
		if (_table != table)
		{
			_table = table;
		}
	}
}
