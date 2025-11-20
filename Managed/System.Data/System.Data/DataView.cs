using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Threading;

namespace System.Data;

/// <summary>Represents a databindable, customized view of a <see cref="T:System.Data.DataTable" /> for sorting, filtering, searching, editing, and navigation. The <see cref="T:System.Data.DataView" /> does not store data, but instead represents a connected view of its corresponding <see cref="T:System.Data.DataTable" />. Changes to the <see cref="T:System.Data.DataView" />’s data will affect the <see cref="T:System.Data.DataTable" />. Changes to the <see cref="T:System.Data.DataTable" />’s data will affect all <see cref="T:System.Data.DataView" />s associated with it.</summary>
/// <filterpriority>1</filterpriority>
[DefaultEvent("PositionChanged")]
[DefaultProperty("Table")]
public class DataView : MarshalByValueComponent, IBindingListView, IBindingList, IList, ICollection, IEnumerable, ITypedList, ISupportInitializeNotification, ISupportInitialize
{
	private sealed class DataRowReferenceComparer : IEqualityComparer<DataRow>
	{
		internal static readonly DataRowReferenceComparer s_default = new DataRowReferenceComparer();

		private DataRowReferenceComparer()
		{
		}

		public bool Equals(DataRow x, DataRow y)
		{
			return x == y;
		}

		public int GetHashCode(DataRow obj)
		{
			return obj._objectID;
		}
	}

	private sealed class RowPredicateFilter : IFilter
	{
		internal readonly Predicate<DataRow> _predicateFilter;

		internal RowPredicateFilter(Predicate<DataRow> predicate)
		{
			_predicateFilter = predicate;
		}

		bool IFilter.Invoke(DataRow row, DataRowVersion version)
		{
			return _predicateFilter(row);
		}
	}

	private DataViewManager _dataViewManager;

	private DataTable _table;

	private bool _locked;

	private Index _index;

	private Dictionary<string, Index> _findIndexes;

	private string _sort = string.Empty;

	private Comparison<DataRow> _comparison;

	private IFilter _rowFilter;

	private DataViewRowState _recordStates = DataViewRowState.CurrentRows;

	private bool _shouldOpen = true;

	private bool _open;

	private bool _allowNew = true;

	private bool _allowEdit = true;

	private bool _allowDelete = true;

	private bool _applyDefaultSort;

	internal DataRow _addNewRow;

	private ListChangedEventArgs _addNewMoved;

	private ListChangedEventHandler _onListChanged;

	internal static ListChangedEventArgs s_resetEventArgs = new ListChangedEventArgs(ListChangedType.Reset, -1);

	private DataTable _delayedTable;

	private string _delayedRowFilter;

	private string _delayedSort;

	private DataViewRowState _delayedRecordStates = (DataViewRowState)(-1);

	private bool _fInitInProgress;

	private bool _fEndInitInProgress;

	private Dictionary<DataRow, DataRowView> _rowViewCache = new Dictionary<DataRow, DataRowView>(DataRowReferenceComparer.s_default);

	private readonly Dictionary<DataRow, DataRowView> _rowViewBuffer = new Dictionary<DataRow, DataRowView>(DataRowReferenceComparer.s_default);

	private DataViewListener _dvListener;

	private static int s_objectTypeCount;

	private readonly int _objectID = Interlocked.Increment(ref s_objectTypeCount);

	/// <summary>Sets or gets a value that indicates whether deletes are allowed.</summary>
	/// <returns>true, if deletes are allowed; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DefaultValue(true)]
	public bool AllowDelete
	{
		get
		{
			return _allowDelete;
		}
		set
		{
			if (_allowDelete != value)
			{
				_allowDelete = value;
				OnListChanged(s_resetEventArgs);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to use the default sort. The default sort is (ascending) by all primary keys as specified by <see cref="P:System.Data.DataTable.PrimaryKey" />.</summary>
	/// <returns>true, if the default sort is used; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[RefreshProperties(RefreshProperties.All)]
	[DefaultValue(false)]
	public bool ApplyDefaultSort
	{
		get
		{
			return _applyDefaultSort;
		}
		set
		{
			DataCommonEventSource.Log.Trace("<ds.DataView.set_ApplyDefaultSort|API> {0}, {1}", ObjectID, value);
			if (_applyDefaultSort != value)
			{
				_comparison = null;
				_applyDefaultSort = value;
				UpdateIndex(force: true);
				OnListChanged(s_resetEventArgs);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether edits are allowed.</summary>
	/// <returns>true, if edits are allowed; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DefaultValue(true)]
	public bool AllowEdit
	{
		get
		{
			return _allowEdit;
		}
		set
		{
			if (_allowEdit != value)
			{
				_allowEdit = value;
				OnListChanged(s_resetEventArgs);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether the new rows can be added by using the <see cref="M:System.Data.DataView.AddNew" /> method.</summary>
	/// <returns>true, if new rows can be added; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DefaultValue(true)]
	public bool AllowNew
	{
		get
		{
			return _allowNew;
		}
		set
		{
			if (_allowNew != value)
			{
				_allowNew = value;
				OnListChanged(s_resetEventArgs);
			}
		}
	}

	/// <summary>Gets the number of records in the <see cref="T:System.Data.DataView" /> after <see cref="P:System.Data.DataView.RowFilter" /> and <see cref="P:System.Data.DataView.RowStateFilter" /> have been applied.</summary>
	/// <returns>The number of records in the <see cref="T:System.Data.DataView" />.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public int Count => _rowViewCache.Count;

	private int CountFromIndex => ((_index != null) ? _index.RecordCount : 0) + ((_addNewRow != null) ? 1 : 0);

	/// <summary>Gets the <see cref="T:System.Data.DataViewManager" /> associated with this view.</summary>
	/// <returns>The DataViewManager that created this view. If this is the default <see cref="T:System.Data.DataView" /> for a <see cref="T:System.Data.DataTable" />, the DataViewManager property returns the default DataViewManager for the DataSet. Otherwise, if the DataView was created without a DataViewManager, this property is null.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public DataViewManager DataViewManager => _dataViewManager;

	/// <summary>Gets a value that indicates whether the component is initialized.</summary>
	/// <returns>true to indicate the component has completed initialization; otherwise, false. </returns>
	[Browsable(false)]
	public bool IsInitialized => !_fInitInProgress;

	/// <summary>Gets a value that indicates whether the data source is currently open and projecting views of data on the <see cref="T:System.Data.DataTable" />.</summary>
	/// <returns>true, if the source is open; otherwise, false.</returns>
	[Browsable(false)]
	protected bool IsOpen => _open;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets or sets the expression used to filter which rows are viewed in the <see cref="T:System.Data.DataView" />.</summary>
	/// <returns>A string that specifies how rows are to be filtered. For more information, see the Remarks section.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[DefaultValue("")]
	public virtual string RowFilter
	{
		get
		{
			if (_rowFilter is DataExpression dataExpression)
			{
				return dataExpression.Expression;
			}
			return "";
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			DataCommonEventSource.Log.Trace("<ds.DataView.set_RowFilter|API> {0}, '{1}'", ObjectID, value);
			if (_fInitInProgress)
			{
				_delayedRowFilter = value;
				return;
			}
			CultureInfo culture = ((_table != null) ? _table.Locale : CultureInfo.CurrentCulture);
			if (_rowFilter == null || string.Compare(RowFilter, value, ignoreCase: false, culture) != 0)
			{
				DataExpression newRowFilter = new DataExpression(_table, value);
				SetIndex(_sort, _recordStates, newRowFilter);
			}
		}
	}

	internal Predicate<DataRow> RowPredicate
	{
		get
		{
			if (!(GetFilter() is RowPredicateFilter rowPredicateFilter))
			{
				return null;
			}
			return rowPredicateFilter._predicateFilter;
		}
		set
		{
			if ((object)RowPredicate != value)
			{
				SetIndex(Sort, RowStateFilter, (value != null) ? new RowPredicateFilter(value) : null);
			}
		}
	}

	/// <summary>Gets or sets the row state filter used in the <see cref="T:System.Data.DataView" />.</summary>
	/// <returns>One of the <see cref="T:System.Data.DataViewRowState" /> values.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[DefaultValue(DataViewRowState.CurrentRows)]
	public DataViewRowState RowStateFilter
	{
		get
		{
			return _recordStates;
		}
		set
		{
			DataCommonEventSource.Log.Trace("<ds.DataView.set_RowStateFilter|API> {0}, {1}", ObjectID, value);
			if (_fInitInProgress)
			{
				_delayedRecordStates = value;
				return;
			}
			if ((value & ~(DataViewRowState.OriginalRows | DataViewRowState.Added | DataViewRowState.ModifiedCurrent)) != DataViewRowState.None)
			{
				throw ExceptionBuilder.RecordStateRange();
			}
			if ((value & DataViewRowState.ModifiedOriginal) != DataViewRowState.None && (value & DataViewRowState.ModifiedCurrent) != DataViewRowState.None)
			{
				throw ExceptionBuilder.SetRowStateFilter();
			}
			if (_recordStates != value)
			{
				SetIndex(_sort, value, _rowFilter);
			}
		}
	}

	/// <summary>Gets or sets the sort column or columns, and sort order for the <see cref="T:System.Data.DataView" />.</summary>
	/// <returns>A string that contains the column name followed by "ASC" (ascending) or "DESC" (descending). Columns are sorted ascending by default. Multiple columns can be separated by commas.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[DefaultValue("")]
	public string Sort
	{
		get
		{
			if (_sort.Length == 0 && _applyDefaultSort && _table != null && _table._primaryIndex.Length != 0)
			{
				return _table.FormatSortString(_table._primaryIndex);
			}
			return _sort;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			DataCommonEventSource.Log.Trace("<ds.DataView.set_Sort|API> {0}, '{1}'", ObjectID, value);
			if (_fInitInProgress)
			{
				_delayedSort = value;
				return;
			}
			CultureInfo culture = ((_table != null) ? _table.Locale : CultureInfo.CurrentCulture);
			if (string.Compare(_sort, value, ignoreCase: false, culture) != 0 || _comparison != null)
			{
				CheckSort(value);
				_comparison = null;
				SetIndex(value, _recordStates, _rowFilter);
			}
		}
	}

	internal Comparison<DataRow> SortComparison
	{
		get
		{
			return _comparison;
		}
		set
		{
			DataCommonEventSource.Log.Trace("<ds.DataView.set_SortComparison|API> {0}", ObjectID);
			if ((object)_comparison != value)
			{
				_comparison = value;
				SetIndex("", _recordStates, _rowFilter);
			}
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</returns>
	object ICollection.SyncRoot => this;

	/// <summary>Gets or sets the source <see cref="T:System.Data.DataTable" />.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> that provides the data for this view.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[DefaultValue(null)]
	[TypeConverter(typeof(DataTableTypeConverter))]
	[RefreshProperties(RefreshProperties.All)]
	public DataTable Table
	{
		get
		{
			return _table;
		}
		set
		{
			DataCommonEventSource.Log.Trace("<ds.DataView.set_Table|API> {0}, {1}", ObjectID, value?.ObjectID ?? 0);
			if (_fInitInProgress && value != null)
			{
				_delayedTable = value;
				return;
			}
			if (_locked)
			{
				throw ExceptionBuilder.SetTable();
			}
			if (_dataViewManager != null)
			{
				throw ExceptionBuilder.CanNotSetTable();
			}
			if (value != null && value.TableName.Length == 0)
			{
				throw ExceptionBuilder.CanNotBindTable();
			}
			if (_table != value)
			{
				_dvListener.UnregisterMetaDataEvents();
				_table = value;
				if (_table != null)
				{
					_dvListener.RegisterMetaDataEvents(_table);
				}
				SetIndex2("", DataViewRowState.CurrentRows, null, fireEvent: false);
				if (_table != null)
				{
					OnListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, new DataTablePropertyDescriptor(_table)));
				}
				OnListChanged(s_resetEventArgs);
			}
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />.</returns>
	/// <param name="recordIndex">A <see cref="System.Int32" /> value.</param>
	object IList.this[int recordIndex]
	{
		get
		{
			return this[recordIndex];
		}
		set
		{
			throw ExceptionBuilder.SetIListObject();
		}
	}

	/// <summary>Gets a row of data from a specified table.</summary>
	/// <returns>A <see cref="T:System.Data.DataRowView" /> of the row that you want.</returns>
	/// <param name="recordIndex">The index of a record in the <see cref="T:System.Data.DataTable" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataRowView this[int recordIndex] => GetRowView(GetRow(recordIndex));

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</returns>
	bool IList.IsReadOnly => false;

	/// <summary>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</returns>
	bool IList.IsFixedSize => false;

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.AllowNew" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.AllowNew" />.</returns>
	bool IBindingList.AllowNew => AllowNew;

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.AllowEdit" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.AllowEdit" />.</returns>
	bool IBindingList.AllowEdit => AllowEdit;

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.AllowRemove" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.AllowRemove" />.</returns>
	bool IBindingList.AllowRemove => AllowDelete;

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SupportsChangeNotification" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SupportsChangeNotification" />.</returns>
	bool IBindingList.SupportsChangeNotification => true;

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SupportsSearching" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SupportsSearching" />.</returns>
	bool IBindingList.SupportsSearching => true;

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SupportsSorting" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SupportsSorting" />.</returns>
	bool IBindingList.SupportsSorting => true;

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.IsSorted" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.IsSorted" />.</returns>
	bool IBindingList.IsSorted => Sort.Length != 0;

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SortProperty" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SortProperty" />.</returns>
	PropertyDescriptor IBindingList.SortProperty => GetSortProperty();

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SortDirection" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SortDirection" />.</returns>
	ListSortDirection IBindingList.SortDirection
	{
		get
		{
			if (_index._indexFields.Length != 1 || !_index._indexFields[0].IsDescending)
			{
				return ListSortDirection.Ascending;
			}
			return ListSortDirection.Descending;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingListView.Filter" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingListView.Filter" />.</returns>
	string IBindingListView.Filter
	{
		get
		{
			return RowFilter;
		}
		set
		{
			RowFilter = value;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingListView.SortDescriptions" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingListView.SortDescriptions" />.</returns>
	ListSortDescriptionCollection IBindingListView.SortDescriptions => GetSortDescriptions();

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingListView.SupportsAdvancedSorting" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingListView.SupportsAdvancedSorting" />.</returns>
	bool IBindingListView.SupportsAdvancedSorting => true;

	/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingListView.SupportsFiltering" />.</summary>
	/// <returns>For a description of this member, see <see cref="P:System.ComponentModel.IBindingListView.SupportsFiltering" />.</returns>
	bool IBindingListView.SupportsFiltering => true;

	internal int ObjectID => _objectID;

	/// <summary>Occurs when the list managed by the <see cref="T:System.Data.DataView" /> changes.</summary>
	/// <filterpriority>2</filterpriority>
	public event ListChangedEventHandler ListChanged
	{
		add
		{
			DataCommonEventSource.Log.Trace("<ds.DataView.add_ListChanged|API> {0}", ObjectID);
			_onListChanged = (ListChangedEventHandler)Delegate.Combine(_onListChanged, value);
		}
		remove
		{
			DataCommonEventSource.Log.Trace("<ds.DataView.remove_ListChanged|API> {0}", ObjectID);
			_onListChanged = (ListChangedEventHandler)Delegate.Remove(_onListChanged, value);
		}
	}

	/// <summary>Occurs when initialization of the <see cref="T:System.Data.DataView" /> is completed.</summary>
	public event EventHandler Initialized;

	internal DataView(DataTable table, bool locked)
	{
		GC.SuppressFinalize(this);
		DataCommonEventSource.Log.Trace("<ds.DataView.DataView|INFO> {0}, table={1}, locked={2}", ObjectID, table?.ObjectID ?? 0, locked);
		_dvListener = new DataViewListener(this);
		_locked = locked;
		_table = table;
		_dvListener.RegisterMetaDataEvents(_table);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DataView" /> class.</summary>
	public DataView()
		: this(null)
	{
		SetIndex2("", DataViewRowState.CurrentRows, null, fireEvent: true);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DataView" /> class with the specified <see cref="T:System.Data.DataTable" />.</summary>
	/// <param name="table">A <see cref="T:System.Data.DataTable" /> to add to the <see cref="T:System.Data.DataView" />. </param>
	public DataView(DataTable table)
		: this(table, locked: false)
	{
		SetIndex2("", DataViewRowState.CurrentRows, null, fireEvent: true);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DataView" /> class with the specified <see cref="T:System.Data.DataTable" />, <see cref="P:System.Data.DataView.RowFilter" />, <see cref="P:System.Data.DataView.Sort" />, and <see cref="T:System.Data.DataViewRowState" />.</summary>
	/// <param name="table">A <see cref="T:System.Data.DataTable" /> to add to the <see cref="T:System.Data.DataView" />. </param>
	/// <param name="RowFilter">A <see cref="P:System.Data.DataView.RowFilter" /> to apply to the <see cref="T:System.Data.DataView" />. </param>
	/// <param name="Sort">A <see cref="P:System.Data.DataView.Sort" /> to apply to the <see cref="T:System.Data.DataView" />. </param>
	/// <param name="RowState">A <see cref="T:System.Data.DataViewRowState" /> to apply to the <see cref="T:System.Data.DataView" />. </param>
	public DataView(DataTable table, string RowFilter, string Sort, DataViewRowState RowState)
	{
		GC.SuppressFinalize(this);
		DataCommonEventSource.Log.Trace("<ds.DataView.DataView|API> {0}, table={1}, RowFilter='{2}', Sort='{3}', RowState={4}", ObjectID, table?.ObjectID ?? 0, RowFilter, Sort, RowState);
		if (table == null)
		{
			throw ExceptionBuilder.CanNotUse();
		}
		_dvListener = new DataViewListener(this);
		_locked = false;
		_table = table;
		_dvListener.RegisterMetaDataEvents(_table);
		if ((RowState & ~(DataViewRowState.OriginalRows | DataViewRowState.Added | DataViewRowState.ModifiedCurrent)) != DataViewRowState.None)
		{
			throw ExceptionBuilder.RecordStateRange();
		}
		if ((RowState & DataViewRowState.ModifiedOriginal) != DataViewRowState.None && (RowState & DataViewRowState.ModifiedCurrent) != DataViewRowState.None)
		{
			throw ExceptionBuilder.SetRowStateFilter();
		}
		if (Sort == null)
		{
			Sort = string.Empty;
		}
		if (RowFilter == null)
		{
			RowFilter = string.Empty;
		}
		DataExpression newRowFilter = new DataExpression(table, RowFilter);
		SetIndex(Sort, RowState, newRowFilter);
	}

	/// <summary>Adds a new row to the <see cref="T:System.Data.DataView" />.</summary>
	/// <returns>A new <see cref="T:System.Data.DataRowView" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public virtual DataRowView AddNew()
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataView.AddNew|API> {0}", ObjectID);
		try
		{
			CheckOpen();
			if (!AllowNew)
			{
				throw ExceptionBuilder.AddNewNotAllowNull();
			}
			if (_addNewRow != null)
			{
				_rowViewCache[_addNewRow].EndEdit();
			}
			_addNewRow = _table.NewRow();
			DataRowView dataRowView = new DataRowView(this, _addNewRow);
			_rowViewCache.Add(_addNewRow, dataRowView);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, IndexOf(dataRowView)));
			return dataRowView;
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Starts the initialization of a <see cref="T:System.Data.DataView" /> that is used on a form or used by another component. The initialization occurs at runtime.</summary>
	/// <filterpriority>2</filterpriority>
	public void BeginInit()
	{
		_fInitInProgress = true;
	}

	/// <summary>Ends the initialization of a <see cref="T:System.Data.DataView" /> that is used on a form or used by another component. The initialization occurs at runtime.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public void EndInit()
	{
		if (_delayedTable != null && _delayedTable.fInitInProgress)
		{
			_delayedTable._delayedViews.Add(this);
			return;
		}
		_fInitInProgress = false;
		_fEndInitInProgress = true;
		if (_delayedTable != null)
		{
			Table = _delayedTable;
			_delayedTable = null;
		}
		if (_delayedSort != null)
		{
			Sort = _delayedSort;
			_delayedSort = null;
		}
		if (_delayedRowFilter != null)
		{
			RowFilter = _delayedRowFilter;
			_delayedRowFilter = null;
		}
		if (_delayedRecordStates != (DataViewRowState)(-1))
		{
			RowStateFilter = _delayedRecordStates;
			_delayedRecordStates = (DataViewRowState)(-1);
		}
		_fEndInitInProgress = false;
		SetIndex(Sort, RowStateFilter, _rowFilter);
		OnInitialized();
	}

	private void CheckOpen()
	{
		if (!IsOpen)
		{
			throw ExceptionBuilder.NotOpen();
		}
	}

	private void CheckSort(string sort)
	{
		if (_table == null)
		{
			throw ExceptionBuilder.CanNotUse();
		}
		if (sort.Length != 0)
		{
			_table.ParseSortString(sort);
		}
	}

	/// <summary>Closes the <see cref="T:System.Data.DataView" />.</summary>
	protected void Close()
	{
		_shouldOpen = false;
		UpdateIndex();
		_dvListener.UnregisterMetaDataEvents();
	}

	/// <summary>Copies items into an array. Only for Web Forms Interfaces.</summary>
	/// <param name="array">array to copy into. </param>
	/// <param name="index">index to start at. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void CopyTo(Array array, int index)
	{
		if (_index != null)
		{
			RBTree<int>.RBTreeEnumerator enumerator = _index.GetEnumerator(0);
			while (enumerator.MoveNext())
			{
				array.SetValue(GetRowView(enumerator.Current), index);
				index = checked(index + 1);
			}
		}
		if (_addNewRow != null)
		{
			array.SetValue(_rowViewCache[_addNewRow], index);
		}
	}

	private void CopyTo(DataRowView[] array, int index)
	{
		if (_index != null)
		{
			RBTree<int>.RBTreeEnumerator enumerator = _index.GetEnumerator(0);
			while (enumerator.MoveNext())
			{
				array[index] = GetRowView(enumerator.Current);
				index = checked(index + 1);
			}
		}
		if (_addNewRow != null)
		{
			array[index] = _rowViewCache[_addNewRow];
		}
	}

	/// <summary>Deletes a row at the specified index.</summary>
	/// <param name="index">The index of the row to delete. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public void Delete(int index)
	{
		Delete(GetRow(index));
	}

	internal void Delete(DataRow row)
	{
		if (row == null)
		{
			return;
		}
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataView.Delete|API> {0}, row={1}", ObjectID, row._objectID);
		try
		{
			CheckOpen();
			if (row == _addNewRow)
			{
				FinishAddNew(success: false);
				return;
			}
			if (!AllowDelete)
			{
				throw ExceptionBuilder.CanNotDelete();
			}
			row.Delete();
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Disposes of the resources (other than memory) used by the <see cref="T:System.Data.DataView" /> object.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Close();
		}
		base.Dispose(disposing);
	}

	/// <summary>Finds a row in the <see cref="T:System.Data.DataView" /> by the specified sort key value.</summary>
	/// <returns>The index of the row in the <see cref="T:System.Data.DataView" /> that contains the sort key value specified; otherwise -1 if the sort key value does not exist.</returns>
	/// <param name="key">The object to search for. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int Find(object key)
	{
		return FindByKey(key);
	}

	internal virtual int FindByKey(object key)
	{
		return _index.FindRecordByKey(key);
	}

	/// <summary>Finds a row in the <see cref="T:System.Data.DataView" /> by the specified sort key values.</summary>
	/// <returns>The index of the position of the first row in the <see cref="T:System.Data.DataView" /> that matches the sort key values specified; otherwise -1 if there are no matching sort key values. </returns>
	/// <param name="key">An array of values, typed as <see cref="T:System.Object" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int Find(object[] key)
	{
		return FindByKey(key);
	}

	internal virtual int FindByKey(object[] key)
	{
		return _index.FindRecordByKey(key);
	}

	/// <summary>Returns an array of <see cref="T:System.Data.DataRowView" /> objects whose columns match the specified sort key value.</summary>
	/// <returns>An array of DataRowView objects whose columns match the specified sort key value; or, if no rows contain the specified sort key values, an empty DataRowView array.</returns>
	/// <param name="key">The column value, typed as <see cref="T:System.Object" />, to search for. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataRowView[] FindRows(object key)
	{
		return FindRowsByKey(new object[1] { key });
	}

	/// <summary>Returns an array of <see cref="T:System.Data.DataRowView" /> objects whose columns match the specified sort key value.</summary>
	/// <returns>An array of DataRowView objects whose columns match the specified sort key value; or, if no rows contain the specified sort key values, an empty DataRowView array.</returns>
	/// <param name="key">An array of column values, typed as <see cref="T:System.Object" />, to search for. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataRowView[] FindRows(object[] key)
	{
		return FindRowsByKey(key);
	}

	internal virtual DataRowView[] FindRowsByKey(object[] key)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataView.FindRows|API> {0}", ObjectID);
		try
		{
			Range range = _index.FindRecords(key);
			return GetDataRowViewFromRange(range);
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	internal DataRowView[] GetDataRowViewFromRange(Range range)
	{
		if (range.IsNull)
		{
			return Array.Empty<DataRowView>();
		}
		DataRowView[] array = new DataRowView[range.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = this[i + range.Min];
		}
		return array;
	}

	internal void FinishAddNew(bool success)
	{
		DataCommonEventSource.Log.Trace("<ds.DataView.FinishAddNew|INFO> {0}, success={1}", ObjectID, success);
		DataRow addNewRow = _addNewRow;
		if (success)
		{
			if (DataRowState.Detached == addNewRow.RowState)
			{
				_table.Rows.Add(addNewRow);
			}
			else
			{
				addNewRow.EndEdit();
			}
		}
		if (addNewRow == _addNewRow)
		{
			_rowViewCache.Remove(_addNewRow);
			_addNewRow = null;
			if (!success)
			{
				addNewRow.CancelEdit();
			}
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, Count));
		}
	}

	/// <summary>Gets an enumerator for this <see cref="T:System.Data.DataView" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for navigating through the list.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public IEnumerator GetEnumerator()
	{
		DataRowView[] array = new DataRowView[Count];
		CopyTo(array, 0);
		return array.GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</returns>
	/// <param name="value">A <see cref="System.Object" /> value.</param>
	int IList.Add(object value)
	{
		if (value == null)
		{
			AddNew();
			return Count - 1;
		}
		throw ExceptionBuilder.AddExternalObject();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Clear" />.</summary>
	void IList.Clear()
	{
		throw ExceptionBuilder.CanNotClear();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</returns>
	/// <param name="value">A <see cref="System.Object" /> value.</param>
	bool IList.Contains(object value)
	{
		return 0 <= IndexOf(value as DataRowView);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</returns>
	/// <param name="value">A <see cref="System.Object" /> value.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as DataRowView);
	}

	internal int IndexOf(DataRowView rowview)
	{
		if (rowview != null)
		{
			if (_addNewRow == rowview.Row)
			{
				return Count - 1;
			}
			if (_index != null && DataRowState.Detached != rowview.Row.RowState && _rowViewCache.TryGetValue(rowview.Row, out var value) && value == rowview)
			{
				return IndexOfDataRowView(rowview);
			}
		}
		return -1;
	}

	private int IndexOfDataRowView(DataRowView rowview)
	{
		return _index.GetIndex(rowview.Row.GetRecordFromVersion(rowview.Row.GetDefaultRowVersion(RowStateFilter) & (DataRowVersion)(-1025)));
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">A <see cref="System.Int32" /> value.</param>
	/// <param name="value">A <see cref="System.Object" /> value to be inserted.</param>
	void IList.Insert(int index, object value)
	{
		throw ExceptionBuilder.InsertExternalObject();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">A <see cref="System.Object" /> value.</param>
	void IList.Remove(object value)
	{
		int num = IndexOf(value as DataRowView);
		if (0 <= num)
		{
			((IList)this).RemoveAt(num);
			return;
		}
		throw ExceptionBuilder.RemoveExternalObject();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IList.RemoveAt(System.Int32)" />.</summary>
	/// <param name="index">A <see cref="System.Int32" /> value.</param>
	void IList.RemoveAt(int index)
	{
		Delete(index);
	}

	internal Index GetFindIndex(string column, bool keepIndex)
	{
		if (_findIndexes == null)
		{
			_findIndexes = new Dictionary<string, Index>();
		}
		if (_findIndexes.TryGetValue(column, out var value))
		{
			if (!keepIndex)
			{
				_findIndexes.Remove(column);
				value.RemoveRef();
				if (value.RefCount == 1)
				{
					value.RemoveRef();
				}
			}
		}
		else if (keepIndex)
		{
			value = _table.GetIndex(column, _recordStates, GetFilter());
			_findIndexes[column] = value;
			value.AddRef();
		}
		return value;
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingList.AddNew" />.</summary>
	/// <returns>The item added to the list.</returns>
	object IBindingList.AddNew()
	{
		return AddNew();
	}

	internal PropertyDescriptor GetSortProperty()
	{
		if (_table != null && _index != null && _index._indexFields.Length == 1)
		{
			return new DataColumnPropertyDescriptor(_index._indexFields[0].Column);
		}
		return null;
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingList.AddIndex(System.ComponentModel.PropertyDescriptor)" />.</summary>
	/// <param name="property">A <see cref="System.ComponentModel.PropertyDescriptor" /> object.</param>
	void IBindingList.AddIndex(PropertyDescriptor property)
	{
		GetFindIndex(property.Name, keepIndex: true);
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)" />.</summary>
	/// <param name="property">A <see cref="System.ComponentModel.PropertyDescriptor" /> object.</param>
	/// <param name="direction">A <see cref="System.ComponentModel.ListSortDirection" /> object.</param>
	void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
	{
		Sort = CreateSortString(property, direction);
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)" />.</summary>
	/// <returns>For a description of this member, see <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)" />.</returns>
	/// <param name="property">A <see cref="System.ComponentModel.PropertyDescriptor" /> object.</param>
	/// <param name="key">A <see cref="System.Object" /> value.</param>
	int IBindingList.Find(PropertyDescriptor property, object key)
	{
		if (property != null)
		{
			bool flag = false;
			Index value = null;
			try
			{
				if (_findIndexes == null || !_findIndexes.TryGetValue(property.Name, out value))
				{
					flag = true;
					value = _table.GetIndex(property.Name, _recordStates, GetFilter());
					value.AddRef();
				}
				Range range = value.FindRecords(key);
				if (!range.IsNull)
				{
					return _index.GetIndex(value.GetRecord(range.Min));
				}
			}
			finally
			{
				if (flag && value != null)
				{
					value.RemoveRef();
					if (value.RefCount == 1)
					{
						value.RemoveRef();
					}
				}
			}
		}
		return -1;
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingList.RemoveIndex(System.ComponentModel.PropertyDescriptor)" />.</summary>
	/// <param name="property">A <see cref="System.ComponentModel.PropertyDescriptor" /> object.</param>
	void IBindingList.RemoveIndex(PropertyDescriptor property)
	{
		GetFindIndex(property.Name, keepIndex: false);
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingList.RemoveSort" />.</summary>
	void IBindingList.RemoveSort()
	{
		DataCommonEventSource.Log.Trace("<ds.DataView.RemoveSort|API> {0}", ObjectID);
		Sort = string.Empty;
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingListView.ApplySort(System.ComponentModel.ListSortDescriptionCollection)" />.</summary>
	/// <param name="sorts">A <see cref="System.ComponentModel.ListSortDescriptionCollection" /> object.</param>
	void IBindingListView.ApplySort(ListSortDescriptionCollection sorts)
	{
		if (sorts == null)
		{
			throw ExceptionBuilder.ArgumentNull("sorts");
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		foreach (ListSortDescription item in (IEnumerable)sorts)
		{
			ListSortDescription obj = item ?? throw ExceptionBuilder.ArgumentContainsNull("sorts");
			PropertyDescriptor propertyDescriptor = obj.PropertyDescriptor;
			if (propertyDescriptor == null)
			{
				throw ExceptionBuilder.ArgumentNull("PropertyDescriptor");
			}
			if (!_table.Columns.Contains(propertyDescriptor.Name))
			{
				throw ExceptionBuilder.ColumnToSortIsOutOfRange(propertyDescriptor.Name);
			}
			ListSortDirection sortDirection = obj.SortDirection;
			if (flag)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append(CreateSortString(propertyDescriptor, sortDirection));
			if (!flag)
			{
				flag = true;
			}
		}
		Sort = stringBuilder.ToString();
	}

	private string CreateSortString(PropertyDescriptor property, ListSortDirection direction)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('[');
		stringBuilder.Append(property.Name);
		stringBuilder.Append(']');
		if (ListSortDirection.Descending == direction)
		{
			stringBuilder.Append(" DESC");
		}
		return stringBuilder.ToString();
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingListView.RemoveFilter" />.</summary>
	void IBindingListView.RemoveFilter()
	{
		DataCommonEventSource.Log.Trace("<ds.DataView.RemoveFilter|API> {0}", ObjectID);
		RowFilter = string.Empty;
	}

	internal ListSortDescriptionCollection GetSortDescriptions()
	{
		ListSortDescription[] array = Array.Empty<ListSortDescription>();
		if (_table != null && _index != null && _index._indexFields.Length != 0)
		{
			array = new ListSortDescription[_index._indexFields.Length];
			for (int i = 0; i < _index._indexFields.Length; i++)
			{
				DataColumnPropertyDescriptor property = new DataColumnPropertyDescriptor(_index._indexFields[i].Column);
				if (_index._indexFields[i].IsDescending)
				{
					array[i] = new ListSortDescription(property, ListSortDirection.Descending);
				}
				else
				{
					array[i] = new ListSortDescription(property, ListSortDirection.Ascending);
				}
			}
		}
		return new ListSortDescriptionCollection(array);
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.ITypedList.GetListName(System.ComponentModel.PropertyDescriptor[])" />.</summary>
	/// <returns>For a description of this member, see <see cref="M:System.ComponentModel.ITypedList.GetListName(System.ComponentModel.PropertyDescriptor[])" />.</returns>
	/// <param name="listAccessors">An array of <see cref="System.ComponentModel.PropertyDescriptor" /> objects.</param>
	string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
	{
		if (_table != null)
		{
			if (listAccessors == null || listAccessors.Length == 0)
			{
				return _table.TableName;
			}
			DataSet dataSet = _table.DataSet;
			if (dataSet != null)
			{
				DataTable dataTable = dataSet.FindTable(_table, listAccessors, 0);
				if (dataTable != null)
				{
					return dataTable.TableName;
				}
			}
		}
		return string.Empty;
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.ITypedList.GetItemProperties(System.ComponentModel.PropertyDescriptor[])" />.</summary>
	PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
	{
		if (_table != null)
		{
			if (listAccessors == null || listAccessors.Length == 0)
			{
				return _table.GetPropertyDescriptorCollection(null);
			}
			DataSet dataSet = _table.DataSet;
			if (dataSet == null)
			{
				return new PropertyDescriptorCollection(null);
			}
			DataTable dataTable = dataSet.FindTable(_table, listAccessors, 0);
			if (dataTable != null)
			{
				return dataTable.GetPropertyDescriptorCollection(null);
			}
		}
		return new PropertyDescriptorCollection(null);
	}

	internal virtual IFilter GetFilter()
	{
		return _rowFilter;
	}

	private int GetRecord(int recordIndex)
	{
		if ((uint)Count <= (uint)recordIndex)
		{
			throw ExceptionBuilder.RowOutOfRange(recordIndex);
		}
		if (recordIndex != _index.RecordCount)
		{
			return _index.GetRecord(recordIndex);
		}
		return _addNewRow.GetDefaultRecord();
	}

	internal DataRow GetRow(int index)
	{
		int count = Count;
		if ((uint)count <= (uint)index)
		{
			throw ExceptionBuilder.GetElementIndex(index);
		}
		if (index == count - 1 && _addNewRow != null)
		{
			return _addNewRow;
		}
		return _table._recordManager[GetRecord(index)];
	}

	private DataRowView GetRowView(int record)
	{
		return GetRowView(_table._recordManager[record]);
	}

	private DataRowView GetRowView(DataRow dr)
	{
		return _rowViewCache[dr];
	}

	/// <summary>Occurs after a <see cref="T:System.Data.DataView" /> has been changed successfully.</summary>
	/// <param name="sender">The source of the event. </param>
	/// <param name="e">A <see cref="T:System.ComponentModel.ListChangedEventArgs" /> that contains the event data. </param>
	protected virtual void IndexListChanged(object sender, ListChangedEventArgs e)
	{
		if (e.ListChangedType != ListChangedType.Reset)
		{
			OnListChanged(e);
		}
		if (_addNewRow != null && _index.RecordCount == 0)
		{
			FinishAddNew(success: false);
		}
		if (e.ListChangedType == ListChangedType.Reset)
		{
			OnListChanged(e);
		}
	}

	internal void IndexListChangedInternal(ListChangedEventArgs e)
	{
		_rowViewBuffer.Clear();
		if (ListChangedType.ItemAdded == e.ListChangedType && _addNewMoved != null && _addNewMoved.NewIndex != _addNewMoved.OldIndex)
		{
			ListChangedEventArgs addNewMoved = _addNewMoved;
			_addNewMoved = null;
			IndexListChanged(this, addNewMoved);
		}
		IndexListChanged(this, e);
	}

	internal void MaintainDataView(ListChangedType changedType, DataRow row, bool trackAddRemove)
	{
		DataRowView value = null;
		switch (changedType)
		{
		case ListChangedType.ItemAdded:
			if (trackAddRemove && _rowViewBuffer.TryGetValue(row, out value))
			{
				_rowViewBuffer.Remove(row);
			}
			if (row == _addNewRow)
			{
				int newIndex = IndexOfDataRowView(_rowViewCache[_addNewRow]);
				_addNewRow = null;
				_addNewMoved = new ListChangedEventArgs(ListChangedType.ItemMoved, newIndex, Count - 1);
			}
			else if (!_rowViewCache.ContainsKey(row))
			{
				_rowViewCache.Add(row, value ?? new DataRowView(this, row));
			}
			break;
		case ListChangedType.ItemDeleted:
			if (trackAddRemove)
			{
				_rowViewCache.TryGetValue(row, out value);
				if (value != null)
				{
					_rowViewBuffer.Add(row, value);
				}
			}
			_rowViewCache.Remove(row);
			break;
		case ListChangedType.Reset:
			ResetRowViewCache();
			break;
		case ListChangedType.ItemMoved:
		case ListChangedType.ItemChanged:
		case ListChangedType.PropertyDescriptorAdded:
		case ListChangedType.PropertyDescriptorDeleted:
		case ListChangedType.PropertyDescriptorChanged:
			break;
		}
	}

	/// <summary>Raises the <see cref="E:System.Data.DataView.ListChanged" /> event.</summary>
	/// <param name="e">A <see cref="T:System.ComponentModel.ListChangedEventArgs" /> that contains the event data. </param>
	protected virtual void OnListChanged(ListChangedEventArgs e)
	{
		DataCommonEventSource.Log.Trace("<ds.DataView.OnListChanged|INFO> {0}, ListChangedType={1}", ObjectID, e.ListChangedType);
		try
		{
			DataColumn dataColumn = null;
			string text = null;
			switch (e.ListChangedType)
			{
			case ListChangedType.ItemMoved:
			case ListChangedType.ItemChanged:
				if (0 <= e.NewIndex)
				{
					DataRow row = GetRow(e.NewIndex);
					if (row.HasPropertyChanged)
					{
						dataColumn = row.LastChangedColumn;
						text = ((dataColumn != null) ? dataColumn.ColumnName : string.Empty);
					}
				}
				break;
			}
			if (_onListChanged != null)
			{
				if (dataColumn != null && e.NewIndex == e.OldIndex)
				{
					ListChangedEventArgs e2 = new ListChangedEventArgs(e.ListChangedType, e.NewIndex, new DataColumnPropertyDescriptor(dataColumn));
					_onListChanged(this, e2);
				}
				else
				{
					_onListChanged(this, e);
				}
			}
			if (text != null)
			{
				this[e.NewIndex].RaisePropertyChangedEvent(text);
			}
		}
		catch (Exception e3) when (ADP.IsCatchableExceptionType(e3))
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e3);
		}
	}

	private void OnInitialized()
	{
		this.Initialized?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Opens a <see cref="T:System.Data.DataView" />.</summary>
	protected void Open()
	{
		_shouldOpen = true;
		UpdateIndex();
		_dvListener.RegisterMetaDataEvents(_table);
	}

	/// <summary>Reserved for internal use only.</summary>
	protected void Reset()
	{
		if (IsOpen)
		{
			_index.Reset();
		}
	}

	internal void ResetRowViewCache()
	{
		Dictionary<DataRow, DataRowView> dictionary = new Dictionary<DataRow, DataRowView>(CountFromIndex, DataRowReferenceComparer.s_default);
		DataRowView value;
		if (_index != null)
		{
			RBTree<int>.RBTreeEnumerator enumerator = _index.GetEnumerator(0);
			while (enumerator.MoveNext())
			{
				DataRow dataRow = _table._recordManager[enumerator.Current];
				if (!_rowViewCache.TryGetValue(dataRow, out value))
				{
					value = new DataRowView(this, dataRow);
				}
				dictionary.Add(dataRow, value);
			}
		}
		if (_addNewRow != null)
		{
			_rowViewCache.TryGetValue(_addNewRow, out value);
			dictionary.Add(_addNewRow, value);
		}
		_rowViewCache = dictionary;
	}

	internal void SetDataViewManager(DataViewManager dataViewManager)
	{
		if (_table == null)
		{
			throw ExceptionBuilder.CanNotUse();
		}
		if (_dataViewManager == dataViewManager)
		{
			return;
		}
		if (dataViewManager != null)
		{
			dataViewManager._nViews--;
		}
		_dataViewManager = dataViewManager;
		if (dataViewManager != null)
		{
			dataViewManager._nViews++;
			DataViewSetting dataViewSetting = dataViewManager.DataViewSettings[_table];
			try
			{
				_applyDefaultSort = dataViewSetting.ApplyDefaultSort;
				DataExpression newRowFilter = new DataExpression(_table, dataViewSetting.RowFilter);
				SetIndex(dataViewSetting.Sort, dataViewSetting.RowStateFilter, newRowFilter);
			}
			catch (Exception e) when (ADP.IsCatchableExceptionType(e))
			{
				ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			}
			_locked = true;
		}
		else
		{
			SetIndex("", DataViewRowState.CurrentRows, null);
		}
	}

	internal virtual void SetIndex(string newSort, DataViewRowState newRowStates, IFilter newRowFilter)
	{
		SetIndex2(newSort, newRowStates, newRowFilter, fireEvent: true);
	}

	internal void SetIndex2(string newSort, DataViewRowState newRowStates, IFilter newRowFilter, bool fireEvent)
	{
		DataCommonEventSource.Log.Trace("<ds.DataView.SetIndex|INFO> {0}, newSort='{1}', newRowStates={2}", ObjectID, newSort, newRowStates);
		_sort = newSort;
		_recordStates = newRowStates;
		_rowFilter = newRowFilter;
		if (_fEndInitInProgress)
		{
			return;
		}
		if (fireEvent)
		{
			UpdateIndex(force: true);
		}
		else
		{
			UpdateIndex(force: true, fireEvent: false);
		}
		if (_findIndexes == null)
		{
			return;
		}
		Dictionary<string, Index> findIndexes = _findIndexes;
		_findIndexes = null;
		foreach (KeyValuePair<string, Index> item in findIndexes)
		{
			item.Value.RemoveRef();
		}
	}

	/// <summary>Reserved for internal use only.</summary>
	protected void UpdateIndex()
	{
		UpdateIndex(force: false);
	}

	/// <summary>Reserved for internal use only.</summary>
	/// <param name="force">Reserved for internal use only. </param>
	protected virtual void UpdateIndex(bool force)
	{
		UpdateIndex(force, fireEvent: true);
	}

	internal void UpdateIndex(bool force, bool fireEvent)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataView.UpdateIndex|INFO> {0}, force={1}", ObjectID, force);
		try
		{
			if (!(_open != _shouldOpen || force))
			{
				return;
			}
			_open = _shouldOpen;
			Index index = null;
			if (_open && _table != null)
			{
				if (SortComparison != null)
				{
					index = new Index(_table, SortComparison, _recordStates, GetFilter());
					index.AddRef();
				}
				else
				{
					index = _table.GetIndex(Sort, _recordStates, GetFilter());
				}
			}
			if (_index != index)
			{
				if (_index == null)
				{
					_ = index.Table;
				}
				else
				{
					_ = _index.Table;
				}
				if (_index != null)
				{
					_dvListener.UnregisterListChangedEvent();
				}
				_index = index;
				if (_index != null)
				{
					_dvListener.RegisterListChangedEvent(_index);
				}
				ResetRowViewCache();
				if (fireEvent)
				{
					OnListChanged(s_resetEventArgs);
				}
			}
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	internal void ChildRelationCollectionChanged(object sender, CollectionChangeEventArgs e)
	{
		DataRelationPropertyDescriptor propDesc = null;
		OnListChanged((e.Action == CollectionChangeAction.Add) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataRelationPropertyDescriptor((DataRelation)e.Element)) : ((e.Action == CollectionChangeAction.Refresh) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, propDesc) : ((e.Action == CollectionChangeAction.Remove) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataRelationPropertyDescriptor((DataRelation)e.Element)) : null)));
	}

	internal void ParentRelationCollectionChanged(object sender, CollectionChangeEventArgs e)
	{
		DataRelationPropertyDescriptor propDesc = null;
		OnListChanged((e.Action == CollectionChangeAction.Add) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataRelationPropertyDescriptor((DataRelation)e.Element)) : ((e.Action == CollectionChangeAction.Refresh) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, propDesc) : ((e.Action == CollectionChangeAction.Remove) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataRelationPropertyDescriptor((DataRelation)e.Element)) : null)));
	}

	/// <summary>Occurs after a <see cref="T:System.Data.DataColumnCollection" /> has been changed successfully.</summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A <see cref="T:System.ComponentModel.ListChangedEventArgs" /> that contains the event data. </param>
	protected virtual void ColumnCollectionChanged(object sender, CollectionChangeEventArgs e)
	{
		DataColumnPropertyDescriptor propDesc = null;
		OnListChanged((e.Action == CollectionChangeAction.Add) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataColumnPropertyDescriptor((DataColumn)e.Element)) : ((e.Action == CollectionChangeAction.Refresh) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, propDesc) : ((e.Action == CollectionChangeAction.Remove) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataColumnPropertyDescriptor((DataColumn)e.Element)) : null)));
	}

	internal void ColumnCollectionChangedInternal(object sender, CollectionChangeEventArgs e)
	{
		ColumnCollectionChanged(sender, e);
	}

	/// <summary>Creates and returns a new <see cref="T:System.Data.DataTable" /> based on rows in an existing <see cref="T:System.Data.DataView" />.</summary>
	/// <returns>A new <see cref="T:System.Data.DataTable" /> instance that contains the requested rows and columns.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public DataTable ToTable()
	{
		return ToTable(null, false);
	}

	/// <summary>Creates and returns a new <see cref="T:System.Data.DataTable" /> based on rows in an existing <see cref="T:System.Data.DataView" />.</summary>
	/// <returns>A new <see cref="T:System.Data.DataTable" /> instance that contains the requested rows and columns.</returns>
	/// <param name="tableName">The name of the returned <see cref="T:System.Data.DataTable" />.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public DataTable ToTable(string tableName)
	{
		return ToTable(tableName, false);
	}

	/// <summary>Creates and returns a new <see cref="T:System.Data.DataTable" /> based on rows in an existing <see cref="T:System.Data.DataView" />.</summary>
	/// <returns>A new <see cref="T:System.Data.DataTable" /> instance that contains the requested rows and columns.</returns>
	/// <param name="distinct">If true, the returned <see cref="T:System.Data.DataTable" /> contains rows that have distinct values for all its columns. The default value is false.</param>
	/// <param name="columnNames">A string array that contains a list of the column names to be included in the returned <see cref="T:System.Data.DataTable" />. The <see cref="T:System.Data.DataTable" /> contains the specified columns in the order they appear within this array.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public DataTable ToTable(bool distinct, params string[] columnNames)
	{
		return ToTable(null, distinct, columnNames);
	}

	/// <summary>Creates and returns a new <see cref="T:System.Data.DataTable" /> based on rows in an existing <see cref="T:System.Data.DataView" />.</summary>
	/// <returns>A new <see cref="T:System.Data.DataTable" /> instance that contains the requested rows and columns.</returns>
	/// <param name="tableName">The name of the returned <see cref="T:System.Data.DataTable" />.</param>
	/// <param name="distinct">If true, the returned <see cref="T:System.Data.DataTable" /> contains rows that have distinct values for all its columns. The default value is false.</param>
	/// <param name="columnNames">A string array that contains a list of the column names to be included in the returned <see cref="T:System.Data.DataTable" />. The DataTable contains the specified columns in the order they appear within this array.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public DataTable ToTable(string tableName, bool distinct, params string[] columnNames)
	{
		DataCommonEventSource.Log.Trace("<ds.DataView.ToTable|API> {0}, TableName='{1}', distinct={2}", ObjectID, tableName, distinct);
		if (columnNames == null)
		{
			throw ExceptionBuilder.ArgumentNull("columnNames");
		}
		DataTable dataTable = new DataTable();
		dataTable.Locale = _table.Locale;
		dataTable.CaseSensitive = _table.CaseSensitive;
		dataTable.TableName = ((tableName != null) ? tableName : _table.TableName);
		dataTable.Namespace = _table.Namespace;
		dataTable.Prefix = _table.Prefix;
		if (columnNames.Length == 0)
		{
			columnNames = new string[Table.Columns.Count];
			for (int i = 0; i < columnNames.Length; i++)
			{
				columnNames[i] = Table.Columns[i].ColumnName;
			}
		}
		int[] array = new int[columnNames.Length];
		List<object[]> list = new List<object[]>();
		for (int j = 0; j < columnNames.Length; j++)
		{
			DataColumn dataColumn = Table.Columns[columnNames[j]];
			if (dataColumn == null)
			{
				throw ExceptionBuilder.ColumnNotInTheUnderlyingTable(columnNames[j], Table.TableName);
			}
			dataTable.Columns.Add(dataColumn.Clone());
			array[j] = Table.Columns.IndexOf(dataColumn);
		}
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				DataRowView dataRowView = (DataRowView)enumerator.Current;
				object[] array2 = new object[columnNames.Length];
				for (int k = 0; k < array.Length; k++)
				{
					array2[k] = dataRowView[array[k]];
				}
				if (!distinct || !RowExist(list, array2))
				{
					dataTable.Rows.Add(array2);
					list.Add(array2);
				}
			}
			return dataTable;
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	private bool RowExist(List<object[]> arraylist, object[] objectArray)
	{
		for (int i = 0; i < arraylist.Count; i++)
		{
			object[] array = arraylist[i];
			bool flag = true;
			for (int j = 0; j < objectArray.Length; j++)
			{
				flag &= array[j].Equals(objectArray[j]);
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Data.DataView" /> instances are considered equal. </summary>
	/// <returns>true if the two <see cref="T:System.Data.DataView" /> instances are equal; otherwise, false. </returns>
	/// <param name="view">The <see cref="T:System.Data.DataView" /> to be compared.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual bool Equals(DataView view)
	{
		if (view == null || Table != view.Table || Count != view.Count || !string.Equals(RowFilter, view.RowFilter, StringComparison.OrdinalIgnoreCase) || !string.Equals(Sort, view.Sort, StringComparison.OrdinalIgnoreCase) || (object)SortComparison != view.SortComparison || (object)RowPredicate != view.RowPredicate || RowStateFilter != view.RowStateFilter || DataViewManager != view.DataViewManager || AllowDelete != view.AllowDelete || AllowNew != view.AllowNew || AllowEdit != view.AllowEdit)
		{
			return false;
		}
		return true;
	}
}
