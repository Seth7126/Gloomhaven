using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace System.Data.Common;

/// <summary>A collection of <see cref="T:System.Data.Common.DataTableMapping" /> objects. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[ListBindable(false)]
public sealed class DataTableMappingCollection : MarshalByRefObject, ITableMappingCollection, IList, ICollection, IEnumerable
{
	private List<DataTableMapping> _items;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
	object ICollection.SyncRoot => this;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => false;

	/// <summary>Gets or sets an item from the collection at a specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the item to get or set.</param>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			ValidateType(value);
			this[index] = (DataTableMapping)value;
		}
	}

	/// <summary>Gets or sets the instance of <see cref="T:System.Data.ITableMapping" /> with the specified <see cref="P:System.Data.ITableMapping.SourceTable" /> name.</summary>
	/// <returns>The instance of <see cref="T:System.Data.ITableMapping" /> with the specified SourceTable name.</returns>
	/// <param name="index">The SourceTable name of the <see cref="T:System.Data.ITableMapping" />.</param>
	object ITableMappingCollection.this[string index]
	{
		get
		{
			return this[index];
		}
		set
		{
			ValidateType(value);
			this[index] = (DataTableMapping)value;
		}
	}

	/// <summary>Gets the number of <see cref="T:System.Data.Common.DataTableMapping" /> objects in the collection.</summary>
	/// <returns>The number of DataTableMapping objects in the collection.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int Count
	{
		get
		{
			if (_items == null)
			{
				return 0;
			}
			return _items.Count;
		}
	}

	private Type ItemType => typeof(DataTableMapping);

	/// <summary>Gets or sets the <see cref="T:System.Data.Common.DataTableMapping" /> object at the specified index.</summary>
	/// <returns>The <see cref="T:System.Data.Common.DataTableMapping" /> object at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Data.Common.DataTableMapping" /> object to return. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DataTableMapping this[int index]
	{
		get
		{
			RangeCheck(index);
			return _items[index];
		}
		set
		{
			RangeCheck(index);
			Replace(index, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.Common.DataTableMapping" /> object with the specified source table name.</summary>
	/// <returns>The <see cref="T:System.Data.Common.DataTableMapping" /> object with the specified source table name.</returns>
	/// <param name="sourceTable">The case-sensitive name of the source table. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public DataTableMapping this[string sourceTable]
	{
		get
		{
			int index = RangeCheck(sourceTable);
			return _items[index];
		}
		set
		{
			int index = RangeCheck(sourceTable);
			Replace(index, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DataTableMappingCollection" /> class. This new instance is empty, that is, it does not yet contain any <see cref="T:System.Data.Common.DataTableMapping" /> objects.</summary>
	public DataTableMappingCollection()
	{
	}

	/// <summary>Adds a table mapping to the collection.</summary>
	/// <returns>A reference to the newly-mapped <see cref="T:System.Data.ITableMapping" /> object.</returns>
	/// <param name="sourceTableName">The case-sensitive name of the source table.</param>
	/// <param name="dataSetTableName">The name of the <see cref="T:System.Data.DataSet" /> table.</param>
	ITableMapping ITableMappingCollection.Add(string sourceTableName, string dataSetTableName)
	{
		return Add(sourceTableName, dataSetTableName);
	}

	/// <summary>Gets the TableMapping object with the specified <see cref="T:System.Data.DataSet" /> table name.</summary>
	/// <returns>The TableMapping object with the specified DataSet table name.</returns>
	/// <param name="dataSetTableName">The name of the DataSet table within the collection.</param>
	ITableMapping ITableMappingCollection.GetByDataSetTable(string dataSetTableName)
	{
		return GetByDataSetTable(dataSetTableName);
	}

	/// <summary>Adds an <see cref="T:System.Object" /> that is a table mapping to the collection.</summary>
	/// <returns>The index of the DataTableMapping object added to the collection.</returns>
	/// <param name="value">A DataTableMapping object to add to the collection. </param>
	/// <exception cref="T:System.InvalidCastException">The object passed in was not a <see cref="T:System.Data.Common.DataTableMapping" /> object. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int Add(object value)
	{
		ValidateType(value);
		Add((DataTableMapping)value);
		return Count - 1;
	}

	private DataTableMapping Add(DataTableMapping value)
	{
		AddWithoutEvents(value);
		return value;
	}

	/// <summary>Copies the elements of the specified <see cref="T:System.Data.Common.DataTableMapping" /> array to the end of the collection.</summary>
	/// <param name="values">The array of <see cref="T:System.Data.Common.DataTableMapping" /> objects to add to the collection. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void AddRange(DataTableMapping[] values)
	{
		AddEnumerableRange(values, doClone: false);
	}

	/// <summary>Copies the elements of the specified <see cref="T:System.Array" /> to the end of the collection.</summary>
	/// <param name="values">An <see cref="T:System.Array" /> of values to add to the collection.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void AddRange(Array values)
	{
		AddEnumerableRange(values, doClone: false);
	}

	private void AddEnumerableRange(IEnumerable values, bool doClone)
	{
		if (values == null)
		{
			throw ADP.ArgumentNull("values");
		}
		foreach (object value2 in values)
		{
			ValidateType(value2);
		}
		if (doClone)
		{
			foreach (ICloneable value3 in values)
			{
				AddWithoutEvents(value3.Clone() as DataTableMapping);
			}
			return;
		}
		foreach (DataTableMapping value4 in values)
		{
			AddWithoutEvents(value4);
		}
	}

	/// <summary>Adds a <see cref="T:System.Data.Common.DataTableMapping" /> object to the collection when given a source table name and a <see cref="T:System.Data.DataSet" /> table name.</summary>
	/// <returns>The <see cref="T:System.Data.Common.DataTableMapping" /> object that was added to the collection.</returns>
	/// <param name="sourceTable">The case-sensitive name of the source table to map from. </param>
	/// <param name="dataSetTable">The name, which is not case-sensitive, of the <see cref="T:System.Data.DataSet" /> table to map to. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataTableMapping Add(string sourceTable, string dataSetTable)
	{
		return Add(new DataTableMapping(sourceTable, dataSetTable));
	}

	private void AddWithoutEvents(DataTableMapping value)
	{
		Validate(-1, value);
		value.Parent = this;
		ArrayList().Add(value);
	}

	private List<DataTableMapping> ArrayList()
	{
		return _items ?? (_items = new List<DataTableMapping>());
	}

	/// <summary>Removes all <see cref="T:System.Data.Common.DataTableMapping" /> objects from the collection.</summary>
	/// <filterpriority>1</filterpriority>
	public void Clear()
	{
		if (0 < Count)
		{
			ClearWithoutEvents();
		}
	}

	private void ClearWithoutEvents()
	{
		if (_items == null)
		{
			return;
		}
		foreach (DataTableMapping item in _items)
		{
			item.Parent = null;
		}
		_items.Clear();
	}

	/// <summary>Gets a value indicating whether a <see cref="T:System.Data.Common.DataTableMapping" /> object with the specified source table name exists in the collection.</summary>
	/// <returns>true if the collection contains a <see cref="T:System.Data.Common.DataTableMapping" /> object with this source table name; otherwise false.</returns>
	/// <param name="value">The case-sensitive source table name containing the <see cref="T:System.Data.Common.DataTableMapping" /> object. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public bool Contains(string value)
	{
		return -1 != IndexOf(value);
	}

	/// <summary>Gets a value indicating whether the given <see cref="T:System.Data.Common.DataTableMapping" /> object exists in the collection.</summary>
	/// <returns>true if this collection contains the specified <see cref="T:System.Data.Common.DataTableMapping" />; otherwise false.</returns>
	/// <param name="value">An <see cref="T:System.Object" /> that is the <see cref="T:System.Data.Common.DataTableMapping" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public bool Contains(object value)
	{
		return -1 != IndexOf(value);
	}

	/// <summary>Copies the elements of the <see cref="T:System.Data.Common.DataTableMappingCollection" /> to the specified array.</summary>
	/// <param name="array">An <see cref="T:System.Array" /> to which to copy the <see cref="T:System.Data.Common.DataTableMappingCollection" /> elements. </param>
	/// <param name="index">The starting index of the array. </param>
	/// <filterpriority>2</filterpriority>
	public void CopyTo(Array array, int index)
	{
		((ICollection)ArrayList()).CopyTo(array, index);
	}

	/// <summary>Copies the elements of the <see cref="T:System.Data.Common.DataTableMapping" /> to the specified array.</summary>
	/// <param name="array">A <see cref="T:System.Data.Common.DataTableMapping" /> to which to copy the <see cref="T:System.Data.Common.DataTableMappingCollection" /> elements.</param>
	/// <param name="index">The starting index of the array.</param>
	/// <filterpriority>2</filterpriority>
	public void CopyTo(DataTableMapping[] array, int index)
	{
		ArrayList().CopyTo(array, index);
	}

	/// <summary>Gets the <see cref="T:System.Data.Common.DataTableMapping" /> object with the specified <see cref="T:System.Data.DataSet" /> table name.</summary>
	/// <returns>The <see cref="T:System.Data.Common.DataTableMapping" /> object with the specified <see cref="T:System.Data.DataSet" /> table name.</returns>
	/// <param name="dataSetTable">The name, which is not case-sensitive, of the <see cref="T:System.Data.DataSet" /> table to find. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DataTableMapping GetByDataSetTable(string dataSetTable)
	{
		int num = IndexOfDataSetTable(dataSetTable);
		if (0 > num)
		{
			throw ADP.TablesDataSetTable(dataSetTable);
		}
		return _items[num];
	}

	/// <summary>Gets an enumerator that can iterate through the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	/// <filterpriority>2</filterpriority>
	public IEnumerator GetEnumerator()
	{
		return ArrayList().GetEnumerator();
	}

	/// <summary>Gets the location of the specified <see cref="T:System.Data.Common.DataTableMapping" /> object within the collection.</summary>
	/// <returns>The zero-based location of the specified <see cref="T:System.Data.Common.DataTableMapping" /> object within the collection.</returns>
	/// <param name="value">An <see cref="T:System.Object" /> that is the <see cref="T:System.Data.Common.DataTableMapping" /> object to find. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int IndexOf(object value)
	{
		if (value != null)
		{
			ValidateType(value);
			for (int i = 0; i < Count; i++)
			{
				if (_items[i] == value)
				{
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>Gets the location of the <see cref="T:System.Data.Common.DataTableMapping" /> object with the specified source table name.</summary>
	/// <returns>The zero-based location of the <see cref="T:System.Data.Common.DataTableMapping" /> object with the specified source table name.</returns>
	/// <param name="sourceTable">The case-sensitive name of the source table. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int IndexOf(string sourceTable)
	{
		if (!string.IsNullOrEmpty(sourceTable))
		{
			for (int i = 0; i < Count; i++)
			{
				string sourceTable2 = _items[i].SourceTable;
				if (sourceTable2 != null && ADP.SrcCompare(sourceTable, sourceTable2) == 0)
				{
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>Gets the location of the <see cref="T:System.Data.Common.DataTableMapping" /> object with the specified <see cref="T:System.Data.DataSet" /> table name.</summary>
	/// <returns>The zero-based location of the <see cref="T:System.Data.Common.DataTableMapping" /> object with the given <see cref="T:System.Data.DataSet" /> table name, or -1 if the <see cref="T:System.Data.Common.DataTableMapping" /> object does not exist in the collection.</returns>
	/// <param name="dataSetTable">The name, which is not case-sensitive, of the DataSet table to find. </param>
	/// <filterpriority>2</filterpriority>
	public int IndexOfDataSetTable(string dataSetTable)
	{
		if (!string.IsNullOrEmpty(dataSetTable))
		{
			for (int i = 0; i < Count; i++)
			{
				string dataSetTable2 = _items[i].DataSetTable;
				if (dataSetTable2 != null && ADP.DstCompare(dataSetTable, dataSetTable2) == 0)
				{
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>Inserts a <see cref="T:System.Data.Common.DataTableMapping" /> object into the <see cref="T:System.Data.Common.DataTableMappingCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Data.Common.DataTableMapping" /> object to insert. </param>
	/// <param name="value">The <see cref="T:System.Data.Common.DataTableMapping" /> object to insert. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Insert(int index, object value)
	{
		ValidateType(value);
		Insert(index, (DataTableMapping)value);
	}

	/// <summary>Inserts a <see cref="T:System.Data.Common.DataTableMapping" /> object into the <see cref="T:System.Data.Common.DataTableMappingCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Data.Common.DataTableMapping" /> object to insert.</param>
	/// <param name="value">The <see cref="T:System.Data.Common.DataTableMapping" /> object to insert.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Insert(int index, DataTableMapping value)
	{
		if (value == null)
		{
			throw ADP.TablesAddNullAttempt("value");
		}
		Validate(-1, value);
		value.Parent = this;
		ArrayList().Insert(index, value);
	}

	private void RangeCheck(int index)
	{
		if (index < 0 || Count <= index)
		{
			throw ADP.TablesIndexInt32(index, this);
		}
	}

	private int RangeCheck(string sourceTable)
	{
		int num = IndexOf(sourceTable);
		if (num < 0)
		{
			throw ADP.TablesSourceIndex(sourceTable);
		}
		return num;
	}

	/// <summary>Removes the <see cref="T:System.Data.Common.DataTableMapping" /> object located at the specified index from the collection.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Data.Common.DataTableMapping" /> object to remove. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">A <see cref="T:System.Data.Common.DataTableMapping" /> object does not exist with the specified index. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void RemoveAt(int index)
	{
		RangeCheck(index);
		RemoveIndex(index);
	}

	/// <summary>Removes the <see cref="T:System.Data.Common.DataTableMapping" /> object with the specified source table name from the collection.</summary>
	/// <param name="sourceTable">The case-sensitive source table name to find. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">A <see cref="T:System.Data.Common.DataTableMapping" /> object does not exist with the specified source table name. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void RemoveAt(string sourceTable)
	{
		int index = RangeCheck(sourceTable);
		RemoveIndex(index);
	}

	private void RemoveIndex(int index)
	{
		_items[index].Parent = null;
		_items.RemoveAt(index);
	}

	/// <summary>Removes the specified <see cref="T:System.Data.Common.DataTableMapping" /> object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.Data.Common.DataTableMapping" /> object to remove. </param>
	/// <exception cref="T:System.InvalidCastException">The object specified was not a <see cref="T:System.Data.Common.DataTableMapping" /> object. </exception>
	/// <exception cref="T:System.ArgumentException">The object specified is not in the collection. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(object value)
	{
		ValidateType(value);
		Remove((DataTableMapping)value);
	}

	/// <summary>Removes the specified <see cref="T:System.Data.Common.DataTableMapping" /> object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.Data.Common.DataTableMapping" /> object to remove.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(DataTableMapping value)
	{
		if (value == null)
		{
			throw ADP.TablesAddNullAttempt("value");
		}
		int num = IndexOf(value);
		if (-1 != num)
		{
			RemoveIndex(num);
			return;
		}
		throw ADP.CollectionRemoveInvalidObject(ItemType, this);
	}

	private void Replace(int index, DataTableMapping newValue)
	{
		Validate(index, newValue);
		_items[index].Parent = null;
		newValue.Parent = this;
		_items[index] = newValue;
	}

	private void ValidateType(object value)
	{
		if (value == null)
		{
			throw ADP.TablesAddNullAttempt("value");
		}
		if (!ItemType.IsInstanceOfType(value))
		{
			throw ADP.NotADataTableMapping(value);
		}
	}

	private void Validate(int index, DataTableMapping value)
	{
		if (value == null)
		{
			throw ADP.TablesAddNullAttempt("value");
		}
		if (value.Parent != null)
		{
			if (this != value.Parent)
			{
				throw ADP.TablesIsNotParent(this);
			}
			if (index != IndexOf(value))
			{
				throw ADP.TablesIsParent(this);
			}
		}
		string sourceTable = value.SourceTable;
		if (string.IsNullOrEmpty(sourceTable))
		{
			index = 1;
			do
			{
				sourceTable = "SourceTable" + index.ToString(CultureInfo.InvariantCulture);
				index++;
			}
			while (-1 != IndexOf(sourceTable));
			value.SourceTable = sourceTable;
		}
		else
		{
			ValidateSourceTable(index, sourceTable);
		}
	}

	internal void ValidateSourceTable(int index, string value)
	{
		int num = IndexOf(value);
		if (-1 != num && index != num)
		{
			throw ADP.TablesUniqueSourceTable(value);
		}
	}

	/// <summary>Gets a <see cref="T:System.Data.Common.DataColumnMapping" /> object with the specified source table name and <see cref="T:System.Data.DataSet" /> table name, using the given <see cref="T:System.Data.MissingMappingAction" />.</summary>
	/// <returns>A <see cref="T:System.Data.Common.DataTableMapping" /> object.</returns>
	/// <param name="tableMappings">The <see cref="T:System.Data.Common.DataTableMappingCollection" /> collection to search. </param>
	/// <param name="sourceTable">The case-sensitive name of the mapped source table. </param>
	/// <param name="dataSetTable">The name, which is not case-sensitive, of the mapped <see cref="T:System.Data.DataSet" /> table. </param>
	/// <param name="mappingAction">One of the <see cref="T:System.Data.MissingMappingAction" /> values. </param>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="mappingAction" /> parameter was set to Error, and no mapping was specified. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static DataTableMapping GetTableMappingBySchemaAction(DataTableMappingCollection tableMappings, string sourceTable, string dataSetTable, MissingMappingAction mappingAction)
	{
		if (tableMappings != null)
		{
			int num = tableMappings.IndexOf(sourceTable);
			if (-1 != num)
			{
				return tableMappings._items[num];
			}
		}
		if (string.IsNullOrEmpty(sourceTable))
		{
			throw ADP.InvalidSourceTable("sourceTable");
		}
		return mappingAction switch
		{
			MissingMappingAction.Passthrough => new DataTableMapping(sourceTable, dataSetTable), 
			MissingMappingAction.Ignore => null, 
			MissingMappingAction.Error => throw ADP.MissingTableMapping(sourceTable), 
			_ => throw ADP.InvalidMissingMappingAction(mappingAction), 
		};
	}
}
