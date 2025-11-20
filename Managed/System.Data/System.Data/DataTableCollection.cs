using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Unity;

namespace System.Data;

/// <summary>Represents the collection of tables for the <see cref="T:System.Data.DataSet" />.</summary>
/// <filterpriority>1</filterpriority>
[DefaultEvent("CollectionChanged")]
[ListBindable(false)]
public sealed class DataTableCollection : InternalDataCollectionBase
{
	private readonly DataSet _dataSet;

	private readonly ArrayList _list;

	private int _defaultNameIndex;

	private DataTable[] _delayedAddRangeTables;

	private CollectionChangeEventHandler _onCollectionChangedDelegate;

	private CollectionChangeEventHandler _onCollectionChangingDelegate;

	private static int s_objectTypeCount;

	private readonly int _objectID;

	protected override ArrayList List => _list;

	internal int ObjectID => _objectID;

	/// <summary>Gets the <see cref="T:System.Data.DataTable" /> object at the specified index.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" />with the specified index; otherwise null if the <see cref="T:System.Data.DataTable" /> does not exist.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Data.DataTable" /> to find. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The index value is greater than the number of items in the collection. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataTable this[int index]
	{
		get
		{
			try
			{
				return (DataTable)_list[index];
			}
			catch (ArgumentOutOfRangeException)
			{
				throw ExceptionBuilder.TableOutOfRange(index);
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Data.DataTable" /> object with the specified name.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> with the specified name; otherwise null if the <see cref="T:System.Data.DataTable" /> does not exist.</returns>
	/// <param name="name">The name of the DataTable to find. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataTable this[string name]
	{
		get
		{
			int num = InternalIndexOf(name);
			if (num == -2)
			{
				throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
			}
			if (num == -3)
			{
				throw ExceptionBuilder.NamespaceNameConflict(name);
			}
			if (num >= 0)
			{
				return (DataTable)_list[num];
			}
			return null;
		}
	}

	/// <summary>Gets the <see cref="T:System.Data.DataTable" /> object with the specified name in the specified namespace.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> with the specified name; otherwise null if the <see cref="T:System.Data.DataTable" /> does not exist.</returns>
	/// <param name="name">The name of the DataTable to find.</param>
	/// <param name="tableNamespace">The name of the <see cref="T:System.Data.DataTable" /> namespace to look in.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataTable this[string name, string tableNamespace]
	{
		get
		{
			if (tableNamespace == null)
			{
				throw ExceptionBuilder.ArgumentNull("tableNamespace");
			}
			int num = InternalIndexOf(name, tableNamespace);
			if (num == -2)
			{
				throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
			}
			if (num >= 0)
			{
				return (DataTable)_list[num];
			}
			return null;
		}
	}

	/// <summary>Occurs after the <see cref="T:System.Data.DataTableCollection" /> is changed because of <see cref="T:System.Data.DataTable" /> objects being added or removed.</summary>
	/// <filterpriority>2</filterpriority>
	public event CollectionChangeEventHandler CollectionChanged
	{
		add
		{
			DataCommonEventSource.Log.Trace("<ds.DataTableCollection.add_CollectionChanged|API> {0}", ObjectID);
			_onCollectionChangedDelegate = (CollectionChangeEventHandler)Delegate.Combine(_onCollectionChangedDelegate, value);
		}
		remove
		{
			DataCommonEventSource.Log.Trace("<ds.DataTableCollection.remove_CollectionChanged|API> {0}", ObjectID);
			_onCollectionChangedDelegate = (CollectionChangeEventHandler)Delegate.Remove(_onCollectionChangedDelegate, value);
		}
	}

	/// <summary>Occurs while the <see cref="T:System.Data.DataTableCollection" /> is changing because of <see cref="T:System.Data.DataTable" /> objects being added or removed.</summary>
	/// <filterpriority>2</filterpriority>
	public event CollectionChangeEventHandler CollectionChanging
	{
		add
		{
			DataCommonEventSource.Log.Trace("<ds.DataTableCollection.add_CollectionChanging|API> {0}", ObjectID);
			_onCollectionChangingDelegate = (CollectionChangeEventHandler)Delegate.Combine(_onCollectionChangingDelegate, value);
		}
		remove
		{
			DataCommonEventSource.Log.Trace("<ds.DataTableCollection.remove_CollectionChanging|API> {0}", ObjectID);
			_onCollectionChangingDelegate = (CollectionChangeEventHandler)Delegate.Remove(_onCollectionChangingDelegate, value);
		}
	}

	internal DataTableCollection(DataSet dataSet)
	{
		_list = new ArrayList();
		_defaultNameIndex = 1;
		_objectID = Interlocked.Increment(ref s_objectTypeCount);
		base._002Ector();
		DataCommonEventSource.Log.Trace("<ds.DataTableCollection.DataTableCollection|INFO> {0}, dataSet={1}", ObjectID, dataSet?.ObjectID ?? 0);
		_dataSet = dataSet;
	}

	internal DataTable GetTable(string name, string ns)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			DataTable dataTable = (DataTable)_list[i];
			if (dataTable.TableName == name && dataTable.Namespace == ns)
			{
				return dataTable;
			}
		}
		return null;
	}

	internal DataTable GetTableSmart(string name, string ns)
	{
		int num = 0;
		DataTable result = null;
		for (int i = 0; i < _list.Count; i++)
		{
			DataTable dataTable = (DataTable)_list[i];
			if (dataTable.TableName == name)
			{
				if (dataTable.Namespace == ns)
				{
					return dataTable;
				}
				num++;
				result = dataTable;
			}
		}
		if (num != 1)
		{
			return null;
		}
		return result;
	}

	/// <summary>Adds the specified DataTable to the collection.</summary>
	/// <param name="table">The DataTable object to add. </param>
	/// <exception cref="T:System.ArgumentNullException">The value specified for the table is null. </exception>
	/// <exception cref="T:System.ArgumentException">The table already belongs to this collection, or belongs to another collection. </exception>
	/// <exception cref="T:System.Data.DuplicateNameException">A table in the collection has the same name. The comparison is not case sensitive. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Add(DataTable table)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataTableCollection.Add|API> {0}, table={1}", ObjectID, table?.ObjectID ?? 0);
		try
		{
			OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, table));
			BaseAdd(table);
			ArrayAdd(table);
			if (table.SetLocaleValue(_dataSet.Locale, userSet: false, resetIndexes: false) || table.SetCaseSensitiveValue(_dataSet.CaseSensitive, userSet: false, resetIndexes: false))
			{
				table.ResetIndexes();
			}
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, table));
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Copies the elements of the specified <see cref="T:System.Data.DataTable" /> array to the end of the collection.</summary>
	/// <param name="tables">The array of <see cref="T:System.Data.DataTable" /> objects to add to the collection. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void AddRange(DataTable[] tables)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataTableCollection.AddRange|API> {0}", ObjectID);
		try
		{
			if (_dataSet._fInitInProgress)
			{
				_delayedAddRangeTables = tables;
			}
			else
			{
				if (tables == null)
				{
					return;
				}
				foreach (DataTable dataTable in tables)
				{
					if (dataTable != null)
					{
						Add(dataTable);
					}
				}
			}
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Creates a <see cref="T:System.Data.DataTable" /> object by using the specified name and adds it to the collection.</summary>
	/// <returns>The newly created <see cref="T:System.Data.DataTable" />.</returns>
	/// <param name="name">The name to give the created <see cref="T:System.Data.DataTable" />. </param>
	/// <exception cref="T:System.Data.DuplicateNameException">A table in the collection has the same name. (The comparison is not case sensitive.) </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataTable Add(string name)
	{
		DataTable dataTable = new DataTable(name);
		Add(dataTable);
		return dataTable;
	}

	/// <summary>Creates a <see cref="T:System.Data.DataTable" /> object by using the specified name and adds it to the collection.</summary>
	/// <returns>The newly created <see cref="T:System.Data.DataTable" />.</returns>
	/// <param name="name">The name to give the created <see cref="T:System.Data.DataTable" />.</param>
	/// <param name="tableNamespace">The namespace to give the created <see cref="T:System.Data.DataTable" />.</param>
	/// <exception cref="T:System.Data.DuplicateNameException">A table in the collection has the same name. (The comparison is not case sensitive.) </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataTable Add(string name, string tableNamespace)
	{
		DataTable dataTable = new DataTable(name, tableNamespace);
		Add(dataTable);
		return dataTable;
	}

	/// <summary>Creates a new <see cref="T:System.Data.DataTable" /> object by using a default name and adds it to the collection.</summary>
	/// <returns>The newly created <see cref="T:System.Data.DataTable" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataTable Add()
	{
		DataTable dataTable = new DataTable();
		Add(dataTable);
		return dataTable;
	}

	private void ArrayAdd(DataTable table)
	{
		_list.Add(table);
	}

	internal string AssignName()
	{
		string text = null;
		while (Contains(text = MakeName(_defaultNameIndex)))
		{
			_defaultNameIndex++;
		}
		return text;
	}

	private void BaseAdd(DataTable table)
	{
		if (table == null)
		{
			throw ExceptionBuilder.ArgumentNull("table");
		}
		if (table.DataSet == _dataSet)
		{
			throw ExceptionBuilder.TableAlreadyInTheDataSet();
		}
		if (table.DataSet != null)
		{
			throw ExceptionBuilder.TableAlreadyInOtherDataSet();
		}
		if (table.TableName.Length == 0)
		{
			table.TableName = AssignName();
		}
		else
		{
			if (NamesEqual(table.TableName, _dataSet.DataSetName, fCaseSensitive: false, _dataSet.Locale) != 0 && !table._fNestedInDataset)
			{
				throw ExceptionBuilder.DatasetConflictingName(_dataSet.DataSetName);
			}
			RegisterName(table.TableName, table.Namespace);
		}
		table.SetDataSet(_dataSet);
	}

	private void BaseGroupSwitch(DataTable[] oldArray, int oldLength, DataTable[] newArray, int newLength)
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
			if (!flag && oldArray[i].DataSet == _dataSet)
			{
				BaseRemove(oldArray[i]);
			}
		}
		for (int k = 0; k < newLength; k++)
		{
			if (newArray[k].DataSet != _dataSet)
			{
				BaseAdd(newArray[k]);
				_list.Add(newArray[k]);
			}
		}
	}

	private void BaseRemove(DataTable table)
	{
		if (CanRemove(table, fThrowException: true))
		{
			UnregisterName(table.TableName);
			table.SetDataSet(null);
		}
		_list.Remove(table);
		_dataSet.OnRemovedTable(table);
	}

	/// <summary>Verifies whether the specified <see cref="T:System.Data.DataTable" /> object can be removed from the collection.</summary>
	/// <returns>true if the table can be removed; otherwise false.</returns>
	/// <param name="table">The DataTable in the collection to perform the check against. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public bool CanRemove(DataTable table)
	{
		return CanRemove(table, fThrowException: false);
	}

	internal bool CanRemove(DataTable table, bool fThrowException)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataTableCollection.CanRemove|INFO> {0}, table={1}, fThrowException={2}", ObjectID, table?.ObjectID ?? 0, fThrowException);
		try
		{
			if (table == null)
			{
				if (!fThrowException)
				{
					return false;
				}
				throw ExceptionBuilder.ArgumentNull("table");
			}
			if (table.DataSet != _dataSet)
			{
				if (!fThrowException)
				{
					return false;
				}
				throw ExceptionBuilder.TableNotInTheDataSet(table.TableName);
			}
			_dataSet.OnRemoveTable(table);
			if (table.ChildRelations.Count != 0 || table.ParentRelations.Count != 0)
			{
				if (!fThrowException)
				{
					return false;
				}
				throw ExceptionBuilder.TableInRelation();
			}
			ParentForeignKeyConstraintEnumerator parentForeignKeyConstraintEnumerator = new ParentForeignKeyConstraintEnumerator(_dataSet, table);
			while (parentForeignKeyConstraintEnumerator.GetNext())
			{
				ForeignKeyConstraint foreignKeyConstraint = parentForeignKeyConstraintEnumerator.GetForeignKeyConstraint();
				if (foreignKeyConstraint.Table != table || foreignKeyConstraint.RelatedTable != table)
				{
					if (!fThrowException)
					{
						return false;
					}
					throw ExceptionBuilder.TableInConstraint(table, foreignKeyConstraint);
				}
			}
			ChildForeignKeyConstraintEnumerator childForeignKeyConstraintEnumerator = new ChildForeignKeyConstraintEnumerator(_dataSet, table);
			while (childForeignKeyConstraintEnumerator.GetNext())
			{
				ForeignKeyConstraint foreignKeyConstraint2 = childForeignKeyConstraintEnumerator.GetForeignKeyConstraint();
				if (foreignKeyConstraint2.Table != table || foreignKeyConstraint2.RelatedTable != table)
				{
					if (!fThrowException)
					{
						return false;
					}
					throw ExceptionBuilder.TableInConstraint(table, foreignKeyConstraint2);
				}
			}
			return true;
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Clears the collection of all <see cref="T:System.Data.DataTable" /> objects.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Clear()
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataTableCollection.Clear|API> {0}", ObjectID);
		try
		{
			int count = _list.Count;
			DataTable[] array = new DataTable[_list.Count];
			_list.CopyTo(array, 0);
			OnCollectionChanging(InternalDataCollectionBase.s_refreshEventArgs);
			if (_dataSet._fInitInProgress && _delayedAddRangeTables != null)
			{
				_delayedAddRangeTables = null;
			}
			BaseGroupSwitch(array, count, null, 0);
			_list.Clear();
			OnCollectionChanged(InternalDataCollectionBase.s_refreshEventArgs);
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Gets a value that indicates whether a <see cref="T:System.Data.DataTable" /> object with the specified name exists in the collection.</summary>
	/// <returns>true if the specified table exists; otherwise false.</returns>
	/// <param name="name">The name of the <see cref="T:System.Data.DataTable" /> to find. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public bool Contains(string name)
	{
		return InternalIndexOf(name) >= 0;
	}

	/// <summary>Gets a value that indicates whether a <see cref="T:System.Data.DataTable" /> object with the specified name and table namespace exists in the collection.</summary>
	/// <returns>true if the specified table exists; otherwise false.</returns>
	/// <param name="name">The name of the <see cref="T:System.Data.DataTable" /> to find.</param>
	/// <param name="tableNamespace">The name of the <see cref="T:System.Data.DataTable" /> namespace to look in.</param>
	/// <filterpriority>1</filterpriority>
	public bool Contains(string name, string tableNamespace)
	{
		if (name == null)
		{
			throw ExceptionBuilder.ArgumentNull("name");
		}
		if (tableNamespace == null)
		{
			throw ExceptionBuilder.ArgumentNull("tableNamespace");
		}
		return InternalIndexOf(name, tableNamespace) >= 0;
	}

	internal bool Contains(string name, string tableNamespace, bool checkProperty, bool caseSensitive)
	{
		if (!caseSensitive)
		{
			return InternalIndexOf(name) >= 0;
		}
		int count = _list.Count;
		for (int i = 0; i < count; i++)
		{
			DataTable dataTable = (DataTable)_list[i];
			string text = (checkProperty ? dataTable.Namespace : dataTable._tableNamespace);
			if (NamesEqual(dataTable.TableName, name, fCaseSensitive: true, _dataSet.Locale) == 1 && text == tableNamespace)
			{
				return true;
			}
		}
		return false;
	}

	internal bool Contains(string name, bool caseSensitive)
	{
		if (!caseSensitive)
		{
			return InternalIndexOf(name) >= 0;
		}
		int count = _list.Count;
		for (int i = 0; i < count; i++)
		{
			DataTable dataTable = (DataTable)_list[i];
			if (NamesEqual(dataTable.TableName, name, fCaseSensitive: true, _dataSet.Locale) == 1)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Copies all the elements of the current <see cref="T:System.Data.DataTableCollection" /> to a one-dimensional <see cref="T:System.Array" />, starting at the specified destination array index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to copy the current <see cref="T:System.Data.DataTableCollection" /> object's elements into.</param>
	/// <param name="index">The destination <see cref="T:System.Array" /> index to start copying into.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void CopyTo(DataTable[] array, int index)
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
			array[index + i] = (DataTable)_list[i];
		}
	}

	/// <summary>Gets the index of the specified <see cref="T:System.Data.DataTable" /> object.</summary>
	/// <returns>The zero-based index of the table, or -1 if the table is not found in the collection.</returns>
	/// <param name="table">The DataTable to search for. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int IndexOf(DataTable table)
	{
		int count = _list.Count;
		for (int i = 0; i < count; i++)
		{
			if (table == (DataTable)_list[i])
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Gets the index in the collection of the <see cref="T:System.Data.DataTable" /> object with the specified name.</summary>
	/// <returns>The zero-based index of the DataTable with the specified name, or -1 if the table does not exist in the collection.NoteReturns -1 when two or more tables have the same name but different namespaces. The call does not succeed if there is any ambiguity when matching a table name to exactly one table.</returns>
	/// <param name="tableName">The name of the DataTable object to look for. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int IndexOf(string tableName)
	{
		int num = InternalIndexOf(tableName);
		if (num >= 0)
		{
			return num;
		}
		return -1;
	}

	/// <summary>Gets the index in the collection of the specified <see cref="T:System.Data.DataTable" /> object.</summary>
	/// <returns>The zero-based index of the <see cref="T:System.Data.DataTable" /> with the specified name, or -1 if the table does not exist in the collection.</returns>
	/// <param name="tableName">The name of the <see cref="T:System.Data.DataTable" /> object to look for.</param>
	/// <param name="tableNamespace">The name of the <see cref="T:System.Data.DataTable" /> namespace to look in.</param>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(string tableName, string tableNamespace)
	{
		return IndexOf(tableName, tableNamespace, chekforNull: true);
	}

	internal int IndexOf(string tableName, string tableNamespace, bool chekforNull)
	{
		if (chekforNull)
		{
			if (tableName == null)
			{
				throw ExceptionBuilder.ArgumentNull("tableName");
			}
			if (tableNamespace == null)
			{
				throw ExceptionBuilder.ArgumentNull("tableNamespace");
			}
		}
		int num = InternalIndexOf(tableName, tableNamespace);
		if (num >= 0)
		{
			return num;
		}
		return -1;
	}

	internal void ReplaceFromInference(List<DataTable> tableList)
	{
		_list.Clear();
		_list.AddRange(tableList);
	}

	internal int InternalIndexOf(string tableName)
	{
		int num = -1;
		if (tableName != null && 0 < tableName.Length)
		{
			int count = _list.Count;
			int num2 = 0;
			for (int i = 0; i < count; i++)
			{
				DataTable dataTable = (DataTable)_list[i];
				switch (NamesEqual(dataTable.TableName, tableName, fCaseSensitive: false, _dataSet.Locale))
				{
				case 1:
				{
					for (int j = i + 1; j < count; j++)
					{
						DataTable dataTable2 = (DataTable)_list[j];
						if (NamesEqual(dataTable2.TableName, tableName, fCaseSensitive: false, _dataSet.Locale) == 1)
						{
							return -3;
						}
					}
					return i;
				}
				case -1:
					num = ((num == -1) ? i : (-2));
					break;
				}
			}
		}
		return num;
	}

	internal int InternalIndexOf(string tableName, string tableNamespace)
	{
		int num = -1;
		if (tableName != null && 0 < tableName.Length)
		{
			int count = _list.Count;
			int num2 = 0;
			for (int i = 0; i < count; i++)
			{
				DataTable dataTable = (DataTable)_list[i];
				num2 = NamesEqual(dataTable.TableName, tableName, fCaseSensitive: false, _dataSet.Locale);
				if (num2 == 1 && dataTable.Namespace == tableNamespace)
				{
					return i;
				}
				if (num2 == -1 && dataTable.Namespace == tableNamespace)
				{
					num = ((num == -1) ? i : (-2));
				}
			}
		}
		return num;
	}

	internal void FinishInitCollection()
	{
		if (_delayedAddRangeTables == null)
		{
			return;
		}
		DataTable[] delayedAddRangeTables = _delayedAddRangeTables;
		foreach (DataTable dataTable in delayedAddRangeTables)
		{
			if (dataTable != null)
			{
				Add(dataTable);
			}
		}
		_delayedAddRangeTables = null;
	}

	private string MakeName(int index)
	{
		if (1 != index)
		{
			return "Table" + index.ToString(CultureInfo.InvariantCulture);
		}
		return "Table1";
	}

	private void OnCollectionChanged(CollectionChangeEventArgs ccevent)
	{
		if (_onCollectionChangedDelegate != null)
		{
			DataCommonEventSource.Log.Trace("<ds.DataTableCollection.OnCollectionChanged|INFO> {0}", ObjectID);
			_onCollectionChangedDelegate(this, ccevent);
		}
	}

	private void OnCollectionChanging(CollectionChangeEventArgs ccevent)
	{
		if (_onCollectionChangingDelegate != null)
		{
			DataCommonEventSource.Log.Trace("<ds.DataTableCollection.OnCollectionChanging|INFO> {0}", ObjectID);
			_onCollectionChangingDelegate(this, ccevent);
		}
	}

	internal void RegisterName(string name, string tbNamespace)
	{
		DataCommonEventSource.Log.Trace("<ds.DataTableCollection.RegisterName|INFO> {0}, name='{1}', tbNamespace='{2}'", ObjectID, name, tbNamespace);
		CultureInfo locale = _dataSet.Locale;
		int count = _list.Count;
		for (int i = 0; i < count; i++)
		{
			DataTable dataTable = (DataTable)_list[i];
			if (NamesEqual(name, dataTable.TableName, fCaseSensitive: true, locale) != 0 && tbNamespace == dataTable.Namespace)
			{
				throw ExceptionBuilder.DuplicateTableName(((DataTable)_list[i]).TableName);
			}
		}
		if (NamesEqual(name, MakeName(_defaultNameIndex), fCaseSensitive: true, locale) != 0)
		{
			_defaultNameIndex++;
		}
	}

	/// <summary>Removes the specified <see cref="T:System.Data.DataTable" /> object from the collection.</summary>
	/// <param name="table">The DataTable to remove. </param>
	/// <exception cref="T:System.ArgumentNullException">The value specified for the table is null. </exception>
	/// <exception cref="T:System.ArgumentException">The table does not belong to this collection.-or- The table is part of a relationship. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(DataTable table)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataTableCollection.Remove|API> {0}, table={1}", ObjectID, table?.ObjectID ?? 0);
		try
		{
			OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Remove, table));
			BaseRemove(table);
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, table));
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Removes the <see cref="T:System.Data.DataTable" /> object at the specified index from the collection.</summary>
	/// <param name="index">The index of the DataTable to remove. </param>
	/// <exception cref="T:System.ArgumentException">The collection does not have a table at the specified index. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void RemoveAt(int index)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataTableCollection.RemoveAt|API> {0}, index={1}", ObjectID, index);
		try
		{
			DataTable dataTable = this[index];
			if (dataTable == null)
			{
				throw ExceptionBuilder.TableOutOfRange(index);
			}
			Remove(dataTable);
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Removes the <see cref="T:System.Data.DataTable" /> object with the specified name from the collection.</summary>
	/// <param name="name">The name of the <see cref="T:System.Data.DataTable" /> object to remove. </param>
	/// <exception cref="T:System.ArgumentException">The collection does not have a table with the specified name. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(string name)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataTableCollection.Remove|API> {0}, name='{1}'", ObjectID, name);
		try
		{
			DataTable dataTable = this[name];
			if (dataTable == null)
			{
				throw ExceptionBuilder.TableNotInTheDataSet(name);
			}
			Remove(dataTable);
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Removes the <see cref="T:System.Data.DataTable" /> object with the specified name from the collection.</summary>
	/// <param name="name">The name of the <see cref="T:System.Data.DataTable" /> object to remove.</param>
	/// <param name="tableNamespace">The name of the <see cref="T:System.Data.DataTable" /> namespace to look in.</param>
	/// <exception cref="T:System.ArgumentException">The collection does not have a table with the specified name. </exception>
	/// <filterpriority>1</filterpriority>
	public void Remove(string name, string tableNamespace)
	{
		if (name == null)
		{
			throw ExceptionBuilder.ArgumentNull("name");
		}
		if (tableNamespace == null)
		{
			throw ExceptionBuilder.ArgumentNull("tableNamespace");
		}
		DataTable dataTable = this[name, tableNamespace];
		if (dataTable == null)
		{
			throw ExceptionBuilder.TableNotInTheDataSet(name);
		}
		Remove(dataTable);
	}

	internal void UnregisterName(string name)
	{
		DataCommonEventSource.Log.Trace("<ds.DataTableCollection.UnregisterName|INFO> {0}, name='{1}'", ObjectID, name);
		if (NamesEqual(name, MakeName(_defaultNameIndex - 1), fCaseSensitive: true, _dataSet.Locale) != 0)
		{
			do
			{
				_defaultNameIndex--;
			}
			while (_defaultNameIndex > 1 && !Contains(MakeName(_defaultNameIndex - 1)));
		}
	}

	internal DataTableCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
