using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using Unity;

namespace System.Data;

/// <summary>Represents a collection of <see cref="T:System.Data.DataColumn" /> objects for a <see cref="T:System.Data.DataTable" />.</summary>
/// <filterpriority>1</filterpriority>
[DefaultEvent("CollectionChanged")]
public sealed class DataColumnCollection : InternalDataCollectionBase
{
	private readonly DataTable _table;

	private readonly ArrayList _list;

	private int _defaultNameIndex;

	private DataColumn[] _delayedAddRangeColumns;

	private readonly Dictionary<string, DataColumn> _columnFromName;

	private bool _fInClear;

	private DataColumn[] _columnsImplementingIChangeTracking;

	private int _nColumnsImplementingIChangeTracking;

	private int _nColumnsImplementingIRevertibleChangeTracking;

	protected override ArrayList List => _list;

	internal DataColumn[] ColumnsImplementingIChangeTracking => _columnsImplementingIChangeTracking;

	internal int ColumnsImplementingIChangeTrackingCount => _nColumnsImplementingIChangeTracking;

	internal int ColumnsImplementingIRevertibleChangeTrackingCount => _nColumnsImplementingIRevertibleChangeTracking;

	/// <summary>Gets the <see cref="T:System.Data.DataColumn" /> from the collection at the specified index.</summary>
	/// <returns>The <see cref="T:System.Data.DataColumn" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the column to return. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The index value is greater than the number of items in the collection. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataColumn this[int index]
	{
		get
		{
			try
			{
				return (DataColumn)_list[index];
			}
			catch (ArgumentOutOfRangeException)
			{
				throw ExceptionBuilder.ColumnOutOfRange(index);
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Data.DataColumn" /> from the collection with the specified name.</summary>
	/// <returns>The <see cref="T:System.Data.DataColumn" /> in the collection with the specified <see cref="P:System.Data.DataColumn.ColumnName" />; otherwise a null value if the <see cref="T:System.Data.DataColumn" /> does not exist.</returns>
	/// <param name="name">The <see cref="P:System.Data.DataColumn.ColumnName" /> of the column to return. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataColumn this[string name]
	{
		get
		{
			if (name == null)
			{
				throw ExceptionBuilder.ArgumentNull("name");
			}
			if (!_columnFromName.TryGetValue(name, out var value) || value == null)
			{
				int num = IndexOfCaseInsensitive(name);
				if (0 <= num)
				{
					return (DataColumn)_list[num];
				}
				if (-2 == num)
				{
					throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
				}
			}
			return value;
		}
	}

	internal DataColumn this[string name, string ns]
	{
		get
		{
			if (_columnFromName.TryGetValue(name, out var value) && value != null && value.Namespace == ns)
			{
				return value;
			}
			return null;
		}
	}

	/// <summary>Occurs when the columns collection changes, either by adding or removing a column.</summary>
	/// <filterpriority>2</filterpriority>
	public event CollectionChangeEventHandler CollectionChanged;

	internal event CollectionChangeEventHandler CollectionChanging;

	internal event CollectionChangeEventHandler ColumnPropertyChanged;

	internal DataColumnCollection(DataTable table)
	{
		_list = new ArrayList();
		_defaultNameIndex = 1;
		_columnsImplementingIChangeTracking = Array.Empty<DataColumn>();
		base._002Ector();
		_table = table;
		_columnFromName = new Dictionary<string, DataColumn>();
	}

	internal void EnsureAdditionalCapacity(int capacity)
	{
		if (_list.Capacity < capacity + _list.Count)
		{
			_list.Capacity = capacity + _list.Count;
		}
	}

	/// <summary>Creates and adds the specified <see cref="T:System.Data.DataColumn" /> object to the <see cref="T:System.Data.DataColumnCollection" />.</summary>
	/// <param name="column">The <see cref="T:System.Data.DataColumn" /> to add. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="column" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The column already belongs to this collection, or to another collection. </exception>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a column with the specified name. (The comparison is not case-sensitive.) </exception>
	/// <exception cref="T:System.Data.InvalidExpressionException">The expression is invalid. See the <see cref="P:System.Data.DataColumn.Expression" /> property for more information about how to create expressions. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public void Add(DataColumn column)
	{
		AddAt(-1, column);
	}

	internal void AddAt(int index, DataColumn column)
	{
		if (column != null && column.ColumnMapping == MappingType.SimpleContent)
		{
			if (_table.XmlText != null && _table.XmlText != column)
			{
				throw ExceptionBuilder.CannotAddColumn3();
			}
			if (_table.ElementColumnCount > 0)
			{
				throw ExceptionBuilder.CannotAddColumn4(column.ColumnName);
			}
			OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, column));
			BaseAdd(column);
			if (index != -1)
			{
				ArrayAdd(index, column);
			}
			else
			{
				ArrayAdd(column);
			}
			_table.XmlText = column;
		}
		else
		{
			OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, column));
			BaseAdd(column);
			if (index != -1)
			{
				ArrayAdd(index, column);
			}
			else
			{
				ArrayAdd(column);
			}
			if (column.ColumnMapping == MappingType.Element)
			{
				_table.ElementColumnCount++;
			}
		}
		if (!_table.fInitInProgress && column != null && column.Computed)
		{
			column.Expression = column.Expression;
		}
		OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, column));
	}

	/// <summary>Copies the elements of the specified <see cref="T:System.Data.DataColumn" /> array to the end of the collection.</summary>
	/// <param name="columns">The array of <see cref="T:System.Data.DataColumn" /> objects to add to the collection. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public void AddRange(DataColumn[] columns)
	{
		if (_table.fInitInProgress)
		{
			_delayedAddRangeColumns = columns;
		}
		else
		{
			if (columns == null)
			{
				return;
			}
			foreach (DataColumn dataColumn in columns)
			{
				if (dataColumn != null)
				{
					Add(dataColumn);
				}
			}
		}
	}

	/// <summary>Creates and adds a <see cref="T:System.Data.DataColumn" /> object that has the specified name, type, and expression to the <see cref="T:System.Data.DataColumnCollection" />.</summary>
	/// <returns>The newly created <see cref="T:System.Data.DataColumn" />.</returns>
	/// <param name="columnName">The name to use when you create the column. </param>
	/// <param name="type">The <see cref="P:System.Data.DataColumn.DataType" /> of the new column. </param>
	/// <param name="expression">The expression to assign to the <see cref="P:System.Data.DataColumn.Expression" /> property. </param>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a column with the specified name. (The comparison is not case-sensitive.) </exception>
	/// <exception cref="T:System.Data.InvalidExpressionException">The expression is invalid. See the <see cref="P:System.Data.DataColumn.Expression" /> property for more information about how to create expressions. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public DataColumn Add(string columnName, Type type, string expression)
	{
		DataColumn dataColumn = new DataColumn(columnName, type, expression);
		Add(dataColumn);
		return dataColumn;
	}

	/// <summary>Creates and adds a <see cref="T:System.Data.DataColumn" /> object that has the specified name and type to the <see cref="T:System.Data.DataColumnCollection" />.</summary>
	/// <returns>The newly created <see cref="T:System.Data.DataColumn" />.</returns>
	/// <param name="columnName">The <see cref="P:System.Data.DataColumn.ColumnName" /> to use when you create the column. </param>
	/// <param name="type">The <see cref="P:System.Data.DataColumn.DataType" /> of the new column. </param>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a column with the specified name. (The comparison is not case-sensitive.) </exception>
	/// <exception cref="T:System.Data.InvalidExpressionException">The expression is invalid. See the <see cref="P:System.Data.DataColumn.Expression" /> property for more information about how to create expressions. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public DataColumn Add(string columnName, Type type)
	{
		DataColumn dataColumn = new DataColumn(columnName, type);
		Add(dataColumn);
		return dataColumn;
	}

	/// <summary>Creates and adds a <see cref="T:System.Data.DataColumn" /> object that has the specified name to the <see cref="T:System.Data.DataColumnCollection" />.</summary>
	/// <returns>The newly created <see cref="T:System.Data.DataColumn" />.</returns>
	/// <param name="columnName">The name of the column. </param>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a column with the specified name. (The comparison is not case-sensitive.) </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public DataColumn Add(string columnName)
	{
		DataColumn dataColumn = new DataColumn(columnName);
		Add(dataColumn);
		return dataColumn;
	}

	/// <summary>Creates and adds a <see cref="T:System.Data.DataColumn" /> object to the <see cref="T:System.Data.DataColumnCollection" />.</summary>
	/// <returns>The newly created <see cref="T:System.Data.DataColumn" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public DataColumn Add()
	{
		DataColumn dataColumn = new DataColumn();
		Add(dataColumn);
		return dataColumn;
	}

	private void ArrayAdd(DataColumn column)
	{
		_list.Add(column);
		column.SetOrdinalInternal(_list.Count - 1);
		CheckIChangeTracking(column);
	}

	private void ArrayAdd(int index, DataColumn column)
	{
		_list.Insert(index, column);
		CheckIChangeTracking(column);
	}

	private void ArrayRemove(DataColumn column)
	{
		column.SetOrdinalInternal(-1);
		_list.Remove(column);
		int count = _list.Count;
		for (int i = 0; i < count; i++)
		{
			((DataColumn)_list[i]).SetOrdinalInternal(i);
		}
		if (column.ImplementsIChangeTracking)
		{
			RemoveColumnsImplementingIChangeTrackingList(column);
		}
	}

	internal string AssignName()
	{
		string text = MakeName(_defaultNameIndex++);
		while (_columnFromName.ContainsKey(text))
		{
			text = MakeName(_defaultNameIndex++);
		}
		return text;
	}

	private void BaseAdd(DataColumn column)
	{
		if (column == null)
		{
			throw ExceptionBuilder.ArgumentNull("column");
		}
		if (column._table == _table)
		{
			throw ExceptionBuilder.CannotAddColumn1(column.ColumnName);
		}
		if (column._table != null)
		{
			throw ExceptionBuilder.CannotAddColumn2(column.ColumnName);
		}
		if (column.ColumnName.Length == 0)
		{
			column.ColumnName = AssignName();
		}
		RegisterColumnName(column.ColumnName, column);
		try
		{
			column.SetTable(_table);
			if (!_table.fInitInProgress && column.Computed && column.DataExpression.DependsOn(column))
			{
				throw ExceptionBuilder.ExpressionCircular();
			}
			if (0 < _table.RecordCapacity)
			{
				column.SetCapacity(_table.RecordCapacity);
			}
			for (int i = 0; i < _table.RecordCapacity; i++)
			{
				column.InitializeRecord(i);
			}
			if (_table.DataSet != null)
			{
				column.OnSetDataSet();
			}
		}
		catch (Exception e) when (ADP.IsCatchableOrSecurityExceptionType(e))
		{
			UnregisterName(column.ColumnName);
			throw;
		}
	}

	private void BaseGroupSwitch(DataColumn[] oldArray, int oldLength, DataColumn[] newArray, int newLength)
	{
		int num = 0;
		for (int i = 0; i < oldLength; i++)
		{
			bool flag = false;
			for (int j = num; j < newLength; j++)
			{
				if (oldArray[i] == newArray[j])
				{
					if (num == j)
					{
						num++;
					}
					flag = true;
					break;
				}
			}
			if (!flag && oldArray[i].Table == _table)
			{
				BaseRemove(oldArray[i]);
				_list.Remove(oldArray[i]);
				oldArray[i].SetOrdinalInternal(-1);
			}
		}
		for (int k = 0; k < newLength; k++)
		{
			if (newArray[k].Table != _table)
			{
				BaseAdd(newArray[k]);
				_list.Add(newArray[k]);
			}
			newArray[k].SetOrdinalInternal(k);
		}
	}

	private void BaseRemove(DataColumn column)
	{
		if (!CanRemove(column, fThrowException: true))
		{
			return;
		}
		if (column._errors > 0)
		{
			for (int i = 0; i < _table.Rows.Count; i++)
			{
				_table.Rows[i].ClearError(column);
			}
		}
		UnregisterName(column.ColumnName);
		column.SetTable(null);
	}

	/// <summary>Checks whether a specific column can be removed from the collection.</summary>
	/// <returns>true if the column can be removed. false if,The <paramref name="column" /> parameter is null.The column does not belong to this collection.The column is part of a relationship.Another column's expression depends on this column.</returns>
	/// <param name="column">A <see cref="T:System.Data.DataColumn" /> in the collection.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public bool CanRemove(DataColumn column)
	{
		return CanRemove(column, fThrowException: false);
	}

	internal bool CanRemove(DataColumn column, bool fThrowException)
	{
		if (column == null)
		{
			if (!fThrowException)
			{
				return false;
			}
			throw ExceptionBuilder.ArgumentNull("column");
		}
		if (column._table != _table)
		{
			if (!fThrowException)
			{
				return false;
			}
			throw ExceptionBuilder.CannotRemoveColumn();
		}
		_table.OnRemoveColumnInternal(column);
		if (_table._primaryKey != null && _table._primaryKey.Key.ContainsColumn(column))
		{
			if (!fThrowException)
			{
				return false;
			}
			throw ExceptionBuilder.CannotRemovePrimaryKey();
		}
		for (int i = 0; i < _table.ParentRelations.Count; i++)
		{
			if (_table.ParentRelations[i].ChildKey.ContainsColumn(column))
			{
				if (!fThrowException)
				{
					return false;
				}
				throw ExceptionBuilder.CannotRemoveChildKey(_table.ParentRelations[i].RelationName);
			}
		}
		for (int j = 0; j < _table.ChildRelations.Count; j++)
		{
			if (_table.ChildRelations[j].ParentKey.ContainsColumn(column))
			{
				if (!fThrowException)
				{
					return false;
				}
				throw ExceptionBuilder.CannotRemoveChildKey(_table.ChildRelations[j].RelationName);
			}
		}
		for (int k = 0; k < _table.Constraints.Count; k++)
		{
			if (_table.Constraints[k].ContainsColumn(column))
			{
				if (!fThrowException)
				{
					return false;
				}
				throw ExceptionBuilder.CannotRemoveConstraint(_table.Constraints[k].ConstraintName, _table.Constraints[k].Table.TableName);
			}
		}
		if (_table.DataSet != null)
		{
			ParentForeignKeyConstraintEnumerator parentForeignKeyConstraintEnumerator = new ParentForeignKeyConstraintEnumerator(_table.DataSet, _table);
			while (parentForeignKeyConstraintEnumerator.GetNext())
			{
				Constraint constraint = parentForeignKeyConstraintEnumerator.GetConstraint();
				if (((ForeignKeyConstraint)constraint).ParentKey.ContainsColumn(column))
				{
					if (!fThrowException)
					{
						return false;
					}
					throw ExceptionBuilder.CannotRemoveConstraint(constraint.ConstraintName, constraint.Table.TableName);
				}
			}
		}
		if (column._dependentColumns != null)
		{
			for (int l = 0; l < column._dependentColumns.Count; l++)
			{
				DataColumn dataColumn = column._dependentColumns[l];
				if ((_fInClear && (dataColumn.Table == _table || dataColumn.Table == null)) || dataColumn.Table == null)
				{
					continue;
				}
				DataExpression dataExpression = dataColumn.DataExpression;
				if (dataExpression != null && dataExpression.DependsOn(column))
				{
					if (!fThrowException)
					{
						return false;
					}
					throw ExceptionBuilder.CannotRemoveExpression(dataColumn.ColumnName, dataColumn.Expression);
				}
			}
		}
		foreach (Index liveIndex in _table.LiveIndexes)
		{
			_ = liveIndex;
		}
		return true;
	}

	private void CheckIChangeTracking(DataColumn column)
	{
		if (column.ImplementsIRevertibleChangeTracking)
		{
			_nColumnsImplementingIRevertibleChangeTracking++;
			_nColumnsImplementingIChangeTracking++;
			AddColumnsImplementingIChangeTrackingList(column);
		}
		else if (column.ImplementsIChangeTracking)
		{
			_nColumnsImplementingIChangeTracking++;
			AddColumnsImplementingIChangeTrackingList(column);
		}
	}

	/// <summary>Clears the collection of any columns.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public void Clear()
	{
		int count = _list.Count;
		DataColumn[] array = new DataColumn[_list.Count];
		_list.CopyTo(array, 0);
		OnCollectionChanging(InternalDataCollectionBase.s_refreshEventArgs);
		if (_table.fInitInProgress && _delayedAddRangeColumns != null)
		{
			_delayedAddRangeColumns = null;
		}
		try
		{
			_fInClear = true;
			BaseGroupSwitch(array, count, null, 0);
			_fInClear = false;
		}
		catch (Exception e) when (ADP.IsCatchableOrSecurityExceptionType(e))
		{
			_fInClear = false;
			BaseGroupSwitch(null, 0, array, count);
			_list.Clear();
			for (int i = 0; i < count; i++)
			{
				_list.Add(array[i]);
			}
			throw;
		}
		_list.Clear();
		_table.ElementColumnCount = 0;
		OnCollectionChanged(InternalDataCollectionBase.s_refreshEventArgs);
	}

	/// <summary>Checks whether the collection contains a column with the specified name.</summary>
	/// <returns>true if a column exists with this name; otherwise, false.</returns>
	/// <param name="name">The <see cref="P:System.Data.DataColumn.ColumnName" /> of the column to look for. </param>
	/// <filterpriority>1</filterpriority>
	public bool Contains(string name)
	{
		if (_columnFromName.TryGetValue(name, out var value) && value != null)
		{
			return true;
		}
		return IndexOfCaseInsensitive(name) >= 0;
	}

	internal bool Contains(string name, bool caseSensitive)
	{
		if (_columnFromName.TryGetValue(name, out var value) && value != null)
		{
			return true;
		}
		if (!caseSensitive)
		{
			return IndexOfCaseInsensitive(name) >= 0;
		}
		return false;
	}

	/// <summary>Copies the entire collection into an existing array, starting at a specified index within the array.</summary>
	/// <param name="array">An array of <see cref="T:System.Data.DataColumn" /> objects to copy the collection into. </param>
	/// <param name="index">The index to start from.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void CopyTo(DataColumn[] array, int index)
	{
		if (array == null)
		{
			throw ExceptionBuilder.ArgumentNull("array");
		}
		if (index < 0)
		{
			throw ExceptionBuilder.ArgumentOutOfRange("index");
		}
		if (array.Length - index < _list.Count)
		{
			throw ExceptionBuilder.InvalidOffsetLength();
		}
		for (int i = 0; i < _list.Count; i++)
		{
			array[index + i] = (DataColumn)_list[i];
		}
	}

	/// <summary>Gets the index of a column specified by name.</summary>
	/// <returns>The index of the column specified by <paramref name="column" /> if it is found; otherwise, -1.</returns>
	/// <param name="column">The name of the column to return. </param>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(DataColumn column)
	{
		int count = _list.Count;
		for (int i = 0; i < count; i++)
		{
			if (column == (DataColumn)_list[i])
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Gets the index of the column with the specific name (the name is not case sensitive).</summary>
	/// <returns>The zero-based index of the column with the specified name, or -1 if the column does not exist in the collection.</returns>
	/// <param name="columnName">The name of the column to find. </param>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(string columnName)
	{
		if (columnName != null && 0 < columnName.Length)
		{
			int count = Count;
			if (!_columnFromName.TryGetValue(columnName, out var value) || value == null)
			{
				int num = IndexOfCaseInsensitive(columnName);
				if (num >= 0)
				{
					return num;
				}
				return -1;
			}
			for (int i = 0; i < count; i++)
			{
				if (value == _list[i])
				{
					return i;
				}
			}
		}
		return -1;
	}

	internal int IndexOfCaseInsensitive(string name)
	{
		int specialHashCode = _table.GetSpecialHashCode(name);
		int num = -1;
		DataColumn dataColumn = null;
		for (int i = 0; i < Count; i++)
		{
			dataColumn = (DataColumn)_list[i];
			if ((specialHashCode == 0 || dataColumn._hashCode == 0 || dataColumn._hashCode == specialHashCode) && NamesEqual(dataColumn.ColumnName, name, fCaseSensitive: false, _table.Locale) != 0)
			{
				if (num != -1)
				{
					return -2;
				}
				num = i;
			}
		}
		return num;
	}

	internal void FinishInitCollection()
	{
		if (_delayedAddRangeColumns == null)
		{
			return;
		}
		DataColumn[] delayedAddRangeColumns = _delayedAddRangeColumns;
		foreach (DataColumn dataColumn in delayedAddRangeColumns)
		{
			if (dataColumn != null)
			{
				Add(dataColumn);
			}
		}
		delayedAddRangeColumns = _delayedAddRangeColumns;
		for (int i = 0; i < delayedAddRangeColumns.Length; i++)
		{
			delayedAddRangeColumns[i]?.FinishInitInProgress();
		}
		_delayedAddRangeColumns = null;
	}

	private string MakeName(int index)
	{
		if (index != 1)
		{
			return "Column" + index.ToString(CultureInfo.InvariantCulture);
		}
		return "Column1";
	}

	internal void MoveTo(DataColumn column, int newPosition)
	{
		if (0 > newPosition || newPosition > Count - 1)
		{
			throw ExceptionBuilder.InvalidOrdinal("ordinal", newPosition);
		}
		if (column.ImplementsIChangeTracking)
		{
			RemoveColumnsImplementingIChangeTrackingList(column);
		}
		_list.Remove(column);
		_list.Insert(newPosition, column);
		int count = _list.Count;
		for (int i = 0; i < count; i++)
		{
			((DataColumn)_list[i]).SetOrdinalInternal(i);
		}
		CheckIChangeTracking(column);
		OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, column));
	}

	private void OnCollectionChanged(CollectionChangeEventArgs ccevent)
	{
		_table.UpdatePropertyDescriptorCollectionCache();
		if (ccevent != null && !_table.SchemaLoading && !_table.fInitInProgress)
		{
			_ = (DataColumn)ccevent.Element;
		}
		this.CollectionChanged?.Invoke(this, ccevent);
	}

	private void OnCollectionChanging(CollectionChangeEventArgs ccevent)
	{
		this.CollectionChanging?.Invoke(this, ccevent);
	}

	internal void OnColumnPropertyChanged(CollectionChangeEventArgs ccevent)
	{
		_table.UpdatePropertyDescriptorCollectionCache();
		this.ColumnPropertyChanged?.Invoke(this, ccevent);
	}

	internal void RegisterColumnName(string name, DataColumn column)
	{
		try
		{
			_columnFromName.Add(name, column);
			if (column != null)
			{
				column._hashCode = _table.GetSpecialHashCode(name);
			}
		}
		catch (ArgumentException)
		{
			if (_columnFromName[name] != null)
			{
				if (column != null)
				{
					throw ExceptionBuilder.CannotAddDuplicate(name);
				}
				throw ExceptionBuilder.CannotAddDuplicate3(name);
			}
			throw ExceptionBuilder.CannotAddDuplicate2(name);
		}
		if (column == null && NamesEqual(name, MakeName(_defaultNameIndex), fCaseSensitive: true, _table.Locale) != 0)
		{
			do
			{
				_defaultNameIndex++;
			}
			while (Contains(MakeName(_defaultNameIndex)));
		}
	}

	internal bool CanRegisterName(string name)
	{
		return !_columnFromName.ContainsKey(name);
	}

	/// <summary>Removes the specified <see cref="T:System.Data.DataColumn" /> object from the collection.</summary>
	/// <param name="column">The <see cref="T:System.Data.DataColumn" /> to remove. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="column" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The column does not belong to this collection.-Or- The column is part of a relationship.-Or- Another column's expression depends on this column. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(DataColumn column)
	{
		OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Remove, column));
		BaseRemove(column);
		ArrayRemove(column);
		OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, column));
		if (column.ColumnMapping == MappingType.Element)
		{
			_table.ElementColumnCount--;
		}
	}

	/// <summary>Removes the column at the specified index from the collection.</summary>
	/// <param name="index">The index of the column to remove. </param>
	/// <exception cref="T:System.ArgumentException">The collection does not have a column at the specified index. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void RemoveAt(int index)
	{
		DataColumn dataColumn = this[index];
		if (dataColumn == null)
		{
			throw ExceptionBuilder.ColumnOutOfRange(index);
		}
		Remove(dataColumn);
	}

	/// <summary>Removes the <see cref="T:System.Data.DataColumn" /> object that has the specified name from the collection.</summary>
	/// <param name="name">The name of the column to remove. </param>
	/// <exception cref="T:System.ArgumentException">The collection does not have a column with the specified name. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(string name)
	{
		DataColumn dataColumn = this[name];
		if (dataColumn == null)
		{
			throw ExceptionBuilder.ColumnNotInTheTable(name, _table.TableName);
		}
		Remove(dataColumn);
	}

	internal void UnregisterName(string name)
	{
		_columnFromName.Remove(name);
		if (NamesEqual(name, MakeName(_defaultNameIndex - 1), fCaseSensitive: true, _table.Locale) != 0)
		{
			do
			{
				_defaultNameIndex--;
			}
			while (_defaultNameIndex > 1 && !Contains(MakeName(_defaultNameIndex - 1)));
		}
	}

	private void AddColumnsImplementingIChangeTrackingList(DataColumn dataColumn)
	{
		DataColumn[] columnsImplementingIChangeTracking = _columnsImplementingIChangeTracking;
		DataColumn[] array = new DataColumn[columnsImplementingIChangeTracking.Length + 1];
		columnsImplementingIChangeTracking.CopyTo(array, 0);
		array[columnsImplementingIChangeTracking.Length] = dataColumn;
		_columnsImplementingIChangeTracking = array;
	}

	private void RemoveColumnsImplementingIChangeTrackingList(DataColumn dataColumn)
	{
		DataColumn[] columnsImplementingIChangeTracking = _columnsImplementingIChangeTracking;
		DataColumn[] array = new DataColumn[columnsImplementingIChangeTracking.Length - 1];
		int i = 0;
		int num = 0;
		for (; i < columnsImplementingIChangeTracking.Length; i++)
		{
			if (columnsImplementingIChangeTracking[i] != dataColumn)
			{
				array[num++] = columnsImplementingIChangeTracking[i];
			}
		}
		_columnsImplementingIChangeTracking = array;
	}

	internal DataColumnCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
