using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Threading;

namespace System.Data;

/// <summary>Represents the collection of <see cref="T:System.Data.DataRelation" /> objects for this <see cref="T:System.Data.DataSet" />.</summary>
/// <filterpriority>1</filterpriority>
[DefaultEvent("CollectionChanged")]
[DefaultProperty("Table")]
public abstract class DataRelationCollection : InternalDataCollectionBase
{
	internal sealed class DataTableRelationCollection : DataRelationCollection
	{
		private readonly DataTable _table;

		private readonly ArrayList _relations;

		private readonly bool _fParentCollection;

		protected override ArrayList List => _relations;

		public override DataRelation this[int index]
		{
			get
			{
				if (index >= 0 && index < _relations.Count)
				{
					return (DataRelation)_relations[index];
				}
				throw ExceptionBuilder.RelationOutOfRange(index);
			}
		}

		public override DataRelation this[string name]
		{
			get
			{
				int num = InternalIndexOf(name);
				if (num == -2)
				{
					throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
				}
				if (num >= 0)
				{
					return (DataRelation)List[num];
				}
				return null;
			}
		}

		internal event CollectionChangeEventHandler RelationPropertyChanged;

		internal DataTableRelationCollection(DataTable table, bool fParentCollection)
		{
			if (table == null)
			{
				throw ExceptionBuilder.RelationTableNull();
			}
			_table = table;
			_fParentCollection = fParentCollection;
			_relations = new ArrayList();
		}

		private void EnsureDataSet()
		{
			if (_table.DataSet == null)
			{
				throw ExceptionBuilder.RelationTableWasRemoved();
			}
		}

		protected override DataSet GetDataSet()
		{
			EnsureDataSet();
			return _table.DataSet;
		}

		internal void OnRelationPropertyChanged(CollectionChangeEventArgs ccevent)
		{
			if (!_fParentCollection)
			{
				_table.UpdatePropertyDescriptorCollectionCache();
			}
			this.RelationPropertyChanged?.Invoke(this, ccevent);
		}

		private void AddCache(DataRelation relation)
		{
			_relations.Add(relation);
			if (!_fParentCollection)
			{
				_table.UpdatePropertyDescriptorCollectionCache();
			}
		}

		protected override void AddCore(DataRelation relation)
		{
			if (_fParentCollection)
			{
				if (relation.ChildTable != _table)
				{
					throw ExceptionBuilder.ChildTableMismatch();
				}
			}
			else if (relation.ParentTable != _table)
			{
				throw ExceptionBuilder.ParentTableMismatch();
			}
			GetDataSet().Relations.Add(relation);
			AddCache(relation);
		}

		public override bool CanRemove(DataRelation relation)
		{
			if (!base.CanRemove(relation))
			{
				return false;
			}
			if (_fParentCollection)
			{
				if (relation.ChildTable != _table)
				{
					return false;
				}
			}
			else if (relation.ParentTable != _table)
			{
				return false;
			}
			return true;
		}

		private void RemoveCache(DataRelation relation)
		{
			for (int i = 0; i < _relations.Count; i++)
			{
				if (relation == _relations[i])
				{
					_relations.RemoveAt(i);
					if (!_fParentCollection)
					{
						_table.UpdatePropertyDescriptorCollectionCache();
					}
					return;
				}
			}
			throw ExceptionBuilder.RelationDoesNotExist();
		}

		protected override void RemoveCore(DataRelation relation)
		{
			if (_fParentCollection)
			{
				if (relation.ChildTable != _table)
				{
					throw ExceptionBuilder.ChildTableMismatch();
				}
			}
			else if (relation.ParentTable != _table)
			{
				throw ExceptionBuilder.ParentTableMismatch();
			}
			GetDataSet().Relations.Remove(relation);
			RemoveCache(relation);
		}
	}

	internal sealed class DataSetRelationCollection : DataRelationCollection
	{
		private readonly DataSet _dataSet;

		private readonly ArrayList _relations;

		private DataRelation[] _delayLoadingRelations;

		protected override ArrayList List => _relations;

		public override DataRelation this[int index]
		{
			get
			{
				if (index >= 0 && index < _relations.Count)
				{
					return (DataRelation)_relations[index];
				}
				throw ExceptionBuilder.RelationOutOfRange(index);
			}
		}

		public override DataRelation this[string name]
		{
			get
			{
				int num = InternalIndexOf(name);
				if (num == -2)
				{
					throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
				}
				if (num >= 0)
				{
					return (DataRelation)List[num];
				}
				return null;
			}
		}

		internal DataSetRelationCollection(DataSet dataSet)
		{
			if (dataSet == null)
			{
				throw ExceptionBuilder.RelationDataSetNull();
			}
			_dataSet = dataSet;
			_relations = new ArrayList();
		}

		public override void AddRange(DataRelation[] relations)
		{
			if (_dataSet._fInitInProgress)
			{
				_delayLoadingRelations = relations;
			}
			else
			{
				if (relations == null)
				{
					return;
				}
				foreach (DataRelation dataRelation in relations)
				{
					if (dataRelation != null)
					{
						Add(dataRelation);
					}
				}
			}
		}

		public override void Clear()
		{
			base.Clear();
			if (_dataSet._fInitInProgress && _delayLoadingRelations != null)
			{
				_delayLoadingRelations = null;
			}
		}

		protected override DataSet GetDataSet()
		{
			return _dataSet;
		}

		protected override void AddCore(DataRelation relation)
		{
			base.AddCore(relation);
			if (relation.ChildTable.DataSet != _dataSet || relation.ParentTable.DataSet != _dataSet)
			{
				throw ExceptionBuilder.ForeignRelation();
			}
			relation.CheckState();
			if (relation.Nested)
			{
				relation.CheckNestedRelations();
			}
			if (relation._relationName.Length == 0)
			{
				relation._relationName = AssignName();
			}
			else
			{
				RegisterName(relation._relationName);
			}
			DataKey childKey = relation.ChildKey;
			for (int i = 0; i < _relations.Count; i++)
			{
				if (childKey.ColumnsEqual(((DataRelation)_relations[i]).ChildKey) && relation.ParentKey.ColumnsEqual(((DataRelation)_relations[i]).ParentKey))
				{
					throw ExceptionBuilder.RelationAlreadyExists();
				}
			}
			_relations.Add(relation);
			((DataTableRelationCollection)relation.ParentTable.ChildRelations).Add(relation);
			((DataTableRelationCollection)relation.ChildTable.ParentRelations).Add(relation);
			relation.SetDataSet(_dataSet);
			relation.ChildKey.GetSortIndex().AddRef();
			if (relation.Nested)
			{
				relation.ChildTable.CacheNestedParent();
			}
			ForeignKeyConstraint foreignKeyConstraint = relation.ChildTable.Constraints.FindForeignKeyConstraint(relation.ParentColumnsReference, relation.ChildColumnsReference);
			if (relation._createConstraints && foreignKeyConstraint == null)
			{
				relation.ChildTable.Constraints.Add(foreignKeyConstraint = new ForeignKeyConstraint(relation.ParentColumnsReference, relation.ChildColumnsReference));
				try
				{
					foreignKeyConstraint.ConstraintName = relation.RelationName;
				}
				catch (Exception e) when (ADP.IsCatchableExceptionType(e))
				{
					ExceptionBuilder.TraceExceptionWithoutRethrow(e);
				}
			}
			UniqueConstraint parentKeyConstraint = relation.ParentTable.Constraints.FindKeyConstraint(relation.ParentColumnsReference);
			relation.SetParentKeyConstraint(parentKeyConstraint);
			relation.SetChildKeyConstraint(foreignKeyConstraint);
		}

		protected override void RemoveCore(DataRelation relation)
		{
			base.RemoveCore(relation);
			_dataSet.OnRemoveRelationHack(relation);
			relation.SetDataSet(null);
			relation.ChildKey.GetSortIndex().RemoveRef();
			if (relation.Nested)
			{
				relation.ChildTable.CacheNestedParent();
			}
			for (int i = 0; i < _relations.Count; i++)
			{
				if (relation == _relations[i])
				{
					_relations.RemoveAt(i);
					((DataTableRelationCollection)relation.ParentTable.ChildRelations).Remove(relation);
					((DataTableRelationCollection)relation.ChildTable.ParentRelations).Remove(relation);
					if (relation.Nested)
					{
						relation.ChildTable.CacheNestedParent();
					}
					UnregisterName(relation.RelationName);
					relation.SetParentKeyConstraint(null);
					relation.SetChildKeyConstraint(null);
					return;
				}
			}
			throw ExceptionBuilder.RelationDoesNotExist();
		}

		internal void FinishInitRelations()
		{
			if (_delayLoadingRelations == null)
			{
				return;
			}
			for (int i = 0; i < _delayLoadingRelations.Length; i++)
			{
				DataRelation dataRelation = _delayLoadingRelations[i];
				if (dataRelation._parentColumnNames == null || dataRelation._childColumnNames == null)
				{
					Add(dataRelation);
					continue;
				}
				int num = dataRelation._parentColumnNames.Length;
				DataColumn[] array = new DataColumn[num];
				DataColumn[] array2 = new DataColumn[num];
				for (int j = 0; j < num; j++)
				{
					if (dataRelation._parentTableNamespace == null)
					{
						array[j] = _dataSet.Tables[dataRelation._parentTableName].Columns[dataRelation._parentColumnNames[j]];
					}
					else
					{
						array[j] = _dataSet.Tables[dataRelation._parentTableName, dataRelation._parentTableNamespace].Columns[dataRelation._parentColumnNames[j]];
					}
					if (dataRelation._childTableNamespace == null)
					{
						array2[j] = _dataSet.Tables[dataRelation._childTableName].Columns[dataRelation._childColumnNames[j]];
					}
					else
					{
						array2[j] = _dataSet.Tables[dataRelation._childTableName, dataRelation._childTableNamespace].Columns[dataRelation._childColumnNames[j]];
					}
				}
				DataRelation dataRelation2 = new DataRelation(dataRelation._relationName, array, array2, createConstraints: false);
				dataRelation2.Nested = dataRelation._nested;
				Add(dataRelation2);
			}
			_delayLoadingRelations = null;
		}
	}

	private DataRelation _inTransition;

	private int _defaultNameIndex = 1;

	private CollectionChangeEventHandler _onCollectionChangedDelegate;

	private CollectionChangeEventHandler _onCollectionChangingDelegate;

	private static int s_objectTypeCount;

	private readonly int _objectID = Interlocked.Increment(ref s_objectTypeCount);

	internal int ObjectID => _objectID;

	/// <summary>Gets the <see cref="T:System.Data.DataRelation" /> object at the specified index.</summary>
	/// <returns>The <see cref="T:System.Data.DataRelation" />, or a null value if the specified <see cref="T:System.Data.DataRelation" /> does not exist.</returns>
	/// <param name="index">The zero-based index to find. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The index value is greater than the number of items in the collection. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract DataRelation this[int index] { get; }

	/// <summary>Gets the <see cref="T:System.Data.DataRelation" /> object specified by name.</summary>
	/// <returns>The named <see cref="T:System.Data.DataRelation" />, or a null value if the specified <see cref="T:System.Data.DataRelation" /> does not exist.</returns>
	/// <param name="name">The name of the relation to find. </param>
	/// <filterpriority>1</filterpriority>
	public abstract DataRelation this[string name] { get; }

	/// <summary>Occurs when the collection has changed.</summary>
	/// <filterpriority>2</filterpriority>
	public event CollectionChangeEventHandler CollectionChanged
	{
		add
		{
			DataCommonEventSource.Log.Trace("<ds.DataRelationCollection.add_CollectionChanged|API> {0}", ObjectID);
			_onCollectionChangedDelegate = (CollectionChangeEventHandler)Delegate.Combine(_onCollectionChangedDelegate, value);
		}
		remove
		{
			DataCommonEventSource.Log.Trace("<ds.DataRelationCollection.remove_CollectionChanged|API> {0}", ObjectID);
			_onCollectionChangedDelegate = (CollectionChangeEventHandler)Delegate.Remove(_onCollectionChangedDelegate, value);
		}
	}

	internal event CollectionChangeEventHandler CollectionChanging
	{
		add
		{
			DataCommonEventSource.Log.Trace("<ds.DataRelationCollection.add_CollectionChanging|INFO> {0}", ObjectID);
			_onCollectionChangingDelegate = (CollectionChangeEventHandler)Delegate.Combine(_onCollectionChangingDelegate, value);
		}
		remove
		{
			DataCommonEventSource.Log.Trace("<ds.DataRelationCollection.remove_CollectionChanging|INFO> {0}", ObjectID);
			_onCollectionChangingDelegate = (CollectionChangeEventHandler)Delegate.Remove(_onCollectionChangingDelegate, value);
		}
	}

	/// <summary>Adds a <see cref="T:System.Data.DataRelation" /> to the <see cref="T:System.Data.DataRelationCollection" />.</summary>
	/// <param name="relation">The DataRelation to add to the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="relation" /> parameter is a null value. </exception>
	/// <exception cref="T:System.ArgumentException">The relation already belongs to this collection, or it belongs to another collection. </exception>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a relation with the specified name. (The comparison is not case sensitive.) </exception>
	/// <exception cref="T:System.Data.InvalidConstraintException">The relation has entered an invalid state since it was created. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Add(DataRelation relation)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataRelationCollection.Add|API> {0}, relation={1}", ObjectID, relation?.ObjectID ?? 0);
		try
		{
			if (_inTransition == relation)
			{
				return;
			}
			_inTransition = relation;
			try
			{
				OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, relation));
				AddCore(relation);
				OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, relation));
			}
			finally
			{
				_inTransition = null;
			}
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Copies the elements of the specified <see cref="T:System.Data.DataRelation" /> array to the end of the collection.</summary>
	/// <param name="relations">The array of <see cref="T:System.Data.DataRelation" /> objects to add to the collection. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual void AddRange(DataRelation[] relations)
	{
		if (relations == null)
		{
			return;
		}
		foreach (DataRelation dataRelation in relations)
		{
			if (dataRelation != null)
			{
				Add(dataRelation);
			}
		}
	}

	/// <summary>Creates a <see cref="T:System.Data.DataRelation" /> with the specified name and arrays of parent and child columns, and adds it to the collection.</summary>
	/// <returns>The created DataRelation.</returns>
	/// <param name="name">The name of the DataRelation to create. </param>
	/// <param name="parentColumns">An array of parent <see cref="T:System.Data.DataColumn" /> objects. </param>
	/// <param name="childColumns">An array of child DataColumn objects. </param>
	/// <exception cref="T:System.ArgumentNullException">The relation name is a null value. </exception>
	/// <exception cref="T:System.ArgumentException">The relation already belongs to this collection, or it belongs to another collection. </exception>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a relation with the same name. (The comparison is not case sensitive.) </exception>
	/// <exception cref="T:System.Data.InvalidConstraintException">The relation has entered an invalid state since it was created. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual DataRelation Add(string name, DataColumn[] parentColumns, DataColumn[] childColumns)
	{
		DataRelation dataRelation = new DataRelation(name, parentColumns, childColumns);
		Add(dataRelation);
		return dataRelation;
	}

	/// <summary>Creates a <see cref="T:System.Data.DataRelation" /> with the specified name, arrays of parent and child columns, and value specifying whether to create a constraint, and adds it to the collection.</summary>
	/// <returns>The created relation.</returns>
	/// <param name="name">The name of the DataRelation to create. </param>
	/// <param name="parentColumns">An array of parent <see cref="T:System.Data.DataColumn" /> objects. </param>
	/// <param name="childColumns">An array of child DataColumn objects. </param>
	/// <param name="createConstraints">true to create a constraint; otherwise false. </param>
	/// <exception cref="T:System.ArgumentNullException">The relation name is a null value. </exception>
	/// <exception cref="T:System.ArgumentException">The relation already belongs to this collection, or it belongs to another collection. </exception>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a relation with the same name. (The comparison is not case sensitive.) </exception>
	/// <exception cref="T:System.Data.InvalidConstraintException">The relation has entered an invalid state since it was created. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual DataRelation Add(string name, DataColumn[] parentColumns, DataColumn[] childColumns, bool createConstraints)
	{
		DataRelation dataRelation = new DataRelation(name, parentColumns, childColumns, createConstraints);
		Add(dataRelation);
		return dataRelation;
	}

	/// <summary>Creates a <see cref="T:System.Data.DataRelation" /> with the specified parent and child columns, and adds it to the collection.</summary>
	/// <returns>The created relation.</returns>
	/// <param name="parentColumns">The parent columns of the relation. </param>
	/// <param name="childColumns">The child columns of the relation. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="relation" /> argument is a null value. </exception>
	/// <exception cref="T:System.ArgumentException">The relation already belongs to this collection, or it belongs to another collection. </exception>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a relation with the same name. (The comparison is not case sensitive.) </exception>
	/// <exception cref="T:System.Data.InvalidConstraintException">The relation has entered an invalid state since it was created. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual DataRelation Add(DataColumn[] parentColumns, DataColumn[] childColumns)
	{
		DataRelation dataRelation = new DataRelation(null, parentColumns, childColumns);
		Add(dataRelation);
		return dataRelation;
	}

	/// <summary>Creates a <see cref="T:System.Data.DataRelation" /> with the specified name, and parent and child columns, and adds it to the collection.</summary>
	/// <returns>The created relation.</returns>
	/// <param name="name">The name of the relation. </param>
	/// <param name="parentColumn">The parent column of the relation. </param>
	/// <param name="childColumn">The child column of the relation. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual DataRelation Add(string name, DataColumn parentColumn, DataColumn childColumn)
	{
		DataRelation dataRelation = new DataRelation(name, parentColumn, childColumn);
		Add(dataRelation);
		return dataRelation;
	}

	/// <summary>Creates a <see cref="T:System.Data.DataRelation" /> with the specified name, parent and child columns, with optional constraints according to the value of the <paramref name="createConstraints" /> parameter, and adds it to the collection.</summary>
	/// <returns>The created relation.</returns>
	/// <param name="name">The name of the relation. </param>
	/// <param name="parentColumn">The parent column of the relation. </param>
	/// <param name="childColumn">The child column of the relation. </param>
	/// <param name="createConstraints">true to create constraints; otherwise false. (The default is true). </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual DataRelation Add(string name, DataColumn parentColumn, DataColumn childColumn, bool createConstraints)
	{
		DataRelation dataRelation = new DataRelation(name, parentColumn, childColumn, createConstraints);
		Add(dataRelation);
		return dataRelation;
	}

	/// <summary>Creates a <see cref="T:System.Data.DataRelation" /> with a specified parent and child column, and adds it to the collection.</summary>
	/// <returns>The created relation.</returns>
	/// <param name="parentColumn">The parent column of the relation. </param>
	/// <param name="childColumn">The child column of the relation. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual DataRelation Add(DataColumn parentColumn, DataColumn childColumn)
	{
		DataRelation dataRelation = new DataRelation(null, parentColumn, childColumn);
		Add(dataRelation);
		return dataRelation;
	}

	/// <summary>Performs verification on the table.</summary>
	/// <param name="relation">The relation to check.</param>
	/// <exception cref="T:System.ArgumentNullException">The relation is null. </exception>
	/// <exception cref="T:System.ArgumentException">The relation already belongs to this collection, or it belongs to another collection. </exception>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a relation with the same name. (The comparison is not case sensitive.) </exception>
	protected virtual void AddCore(DataRelation relation)
	{
		DataCommonEventSource.Log.Trace("<ds.DataRelationCollection.AddCore|INFO> {0}, relation={1}", ObjectID, relation?.ObjectID ?? 0);
		if (relation == null)
		{
			throw ExceptionBuilder.ArgumentNull("relation");
		}
		relation.CheckState();
		DataSet dataSet = GetDataSet();
		if (relation.DataSet == dataSet)
		{
			throw ExceptionBuilder.RelationAlreadyInTheDataSet();
		}
		if (relation.DataSet != null)
		{
			throw ExceptionBuilder.RelationAlreadyInOtherDataSet();
		}
		if (relation.ChildTable.Locale.LCID != relation.ParentTable.Locale.LCID || relation.ChildTable.CaseSensitive != relation.ParentTable.CaseSensitive)
		{
			throw ExceptionBuilder.CaseLocaleMismatch();
		}
		if (relation.Nested)
		{
			relation.CheckNamespaceValidityForNestedRelations(relation.ParentTable.Namespace);
			relation.ValidateMultipleNestedRelations();
			relation.ParentTable.ElementColumnCount++;
		}
	}

	internal string AssignName()
	{
		string result = MakeName(_defaultNameIndex);
		_defaultNameIndex++;
		return result;
	}

	/// <summary>Clears the collection of any relations.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual void Clear()
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<ds.DataRelationCollection.Clear|API> {0}", ObjectID);
		try
		{
			int count = Count;
			OnCollectionChanging(InternalDataCollectionBase.s_refreshEventArgs);
			for (int num = count - 1; num >= 0; num--)
			{
				_inTransition = this[num];
				RemoveCore(_inTransition);
			}
			OnCollectionChanged(InternalDataCollectionBase.s_refreshEventArgs);
			_inTransition = null;
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Verifies whether a <see cref="T:System.Data.DataRelation" /> with the specific name (case insensitive) exists in the collection.</summary>
	/// <returns>true, if a relation with the specified name exists; otherwise false.</returns>
	/// <param name="name">The name of the relation to find. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual bool Contains(string name)
	{
		return InternalIndexOf(name) >= 0;
	}

	/// <summary>Copies the collection of <see cref="T:System.Data.DataRelation" /> objects starting at the specified index.</summary>
	/// <param name="array">The array of <see cref="T:System.Data.DataRelation" /> objects to copy the collection to.</param>
	/// <param name="index">The index to start from.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void CopyTo(DataRelation[] array, int index)
	{
		if (array == null)
		{
			throw ExceptionBuilder.ArgumentNull("array");
		}
		if (index < 0)
		{
			throw ExceptionBuilder.ArgumentOutOfRange("index");
		}
		ArrayList list = List;
		if (array.Length - index < list.Count)
		{
			throw ExceptionBuilder.InvalidOffsetLength();
		}
		for (int i = 0; i < list.Count; i++)
		{
			array[index + i] = (DataRelation)list[i];
		}
	}

	/// <summary>Gets the index of the specified <see cref="T:System.Data.DataRelation" /> object.</summary>
	/// <returns>The 0-based index of the relation, or -1 if the relation is not found in the collection.</returns>
	/// <param name="relation">The relation to search for. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual int IndexOf(DataRelation relation)
	{
		int count = List.Count;
		for (int i = 0; i < count; i++)
		{
			if (relation == (DataRelation)List[i])
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Gets the index of the <see cref="T:System.Data.DataRelation" /> specified by name.</summary>
	/// <returns>The zero-based index of the relation with the specified name, or -1 if the relation does not exist in the collection.</returns>
	/// <param name="relationName">The name of the relation to find. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual int IndexOf(string relationName)
	{
		int num = InternalIndexOf(relationName);
		if (num >= 0)
		{
			return num;
		}
		return -1;
	}

	internal int InternalIndexOf(string name)
	{
		int num = -1;
		if (name != null && 0 < name.Length)
		{
			int count = List.Count;
			int num2 = 0;
			for (int i = 0; i < count; i++)
			{
				DataRelation dataRelation = (DataRelation)List[i];
				switch (NamesEqual(dataRelation.RelationName, name, fCaseSensitive: false, GetDataSet().Locale))
				{
				case 1:
					return i;
				case -1:
					num = ((num == -1) ? i : (-2));
					break;
				}
			}
		}
		return num;
	}

	/// <summary>This method supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The referenced DataSet.</returns>
	protected abstract DataSet GetDataSet();

	private string MakeName(int index)
	{
		if (index != 1)
		{
			return "Relation" + index.ToString(CultureInfo.InvariantCulture);
		}
		return "Relation1";
	}

	/// <summary>Raises the <see cref="E:System.Data.DataRelationCollection.CollectionChanged" /> event.</summary>
	/// <param name="ccevent">A <see cref="T:System.ComponentModel.CollectionChangeEventArgs" /> that contains the event data. </param>
	protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent)
	{
		if (_onCollectionChangedDelegate != null)
		{
			DataCommonEventSource.Log.Trace("<ds.DataRelationCollection.OnCollectionChanged|INFO> {0}", ObjectID);
			_onCollectionChangedDelegate(this, ccevent);
		}
	}

	/// <summary>Raises the <see cref="E:System.Data.DataRelationCollection.CollectionChanged" /> event.</summary>
	/// <param name="ccevent">A <see cref="T:System.ComponentModel.CollectionChangeEventArgs" /> that contains the event data. </param>
	protected virtual void OnCollectionChanging(CollectionChangeEventArgs ccevent)
	{
		if (_onCollectionChangingDelegate != null)
		{
			DataCommonEventSource.Log.Trace("<ds.DataRelationCollection.OnCollectionChanging|INFO> {0}", ObjectID);
			_onCollectionChangingDelegate(this, ccevent);
		}
	}

	internal void RegisterName(string name)
	{
		DataCommonEventSource.Log.Trace("<ds.DataRelationCollection.RegisterName|INFO> {0}, name='{1}'", ObjectID, name);
		CultureInfo locale = GetDataSet().Locale;
		int count = Count;
		for (int i = 0; i < count; i++)
		{
			if (NamesEqual(name, this[i].RelationName, fCaseSensitive: true, locale) != 0)
			{
				throw ExceptionBuilder.DuplicateRelation(this[i].RelationName);
			}
		}
		if (NamesEqual(name, MakeName(_defaultNameIndex), fCaseSensitive: true, locale) != 0)
		{
			_defaultNameIndex++;
		}
	}

	/// <summary>Verifies whether the specified <see cref="T:System.Data.DataRelation" /> can be removed from the collection.</summary>
	/// <returns>true if the <see cref="T:System.Data.DataRelation" /> can be removed; otherwise, false.</returns>
	/// <param name="relation">The relation to perform the check against. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual bool CanRemove(DataRelation relation)
	{
		if (relation != null)
		{
			return relation.DataSet == GetDataSet();
		}
		return false;
	}

	/// <summary>Removes the specified relation from the collection.</summary>
	/// <param name="relation">The relation to remove. </param>
	/// <exception cref="T:System.ArgumentNullException">The relation is a null value.</exception>
	/// <exception cref="T:System.ArgumentException">The relation does not belong to the collection.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(DataRelation relation)
	{
		DataCommonEventSource.Log.Trace("<ds.DataRelationCollection.Remove|API> {0}, relation={1}", ObjectID, relation?.ObjectID ?? 0);
		if (_inTransition == relation)
		{
			return;
		}
		_inTransition = relation;
		try
		{
			OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Remove, relation));
			RemoveCore(relation);
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, relation));
		}
		finally
		{
			_inTransition = null;
		}
	}

	/// <summary>Removes the relation at the specified index from the collection.</summary>
	/// <param name="index">The index of the relation to remove. </param>
	/// <exception cref="T:System.ArgumentException">The collection does not have a relation at the specified index. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void RemoveAt(int index)
	{
		DataRelation dataRelation = this[index];
		if (dataRelation == null)
		{
			throw ExceptionBuilder.RelationOutOfRange(index);
		}
		Remove(dataRelation);
	}

	/// <summary>Removes the relation with the specified name from the collection.</summary>
	/// <param name="name">The name of the relation to remove. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The collection does not have a relation with the specified name.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(string name)
	{
		DataRelation dataRelation = this[name];
		if (dataRelation == null)
		{
			throw ExceptionBuilder.RelationNotInTheDataSet(name);
		}
		Remove(dataRelation);
	}

	/// <summary>Performs a verification on the specified <see cref="T:System.Data.DataRelation" /> object.</summary>
	/// <param name="relation">The DataRelation object to verify. </param>
	/// <exception cref="T:System.ArgumentNullException">The collection does not have a relation at the specified index. </exception>
	/// <exception cref="T:System.ArgumentException">The specified relation does not belong to this collection, or it belongs to another collection. </exception>
	protected virtual void RemoveCore(DataRelation relation)
	{
		DataCommonEventSource.Log.Trace("<ds.DataRelationCollection.RemoveCore|INFO> {0}, relation={1}", ObjectID, relation?.ObjectID ?? 0);
		if (relation == null)
		{
			throw ExceptionBuilder.ArgumentNull("relation");
		}
		DataSet dataSet = GetDataSet();
		if (relation.DataSet != dataSet)
		{
			throw ExceptionBuilder.RelationNotInTheDataSet(relation.RelationName);
		}
		if (relation.Nested)
		{
			relation.ParentTable.ElementColumnCount--;
			relation.ParentTable.Columns.UnregisterName(relation.ChildTable.TableName);
		}
	}

	internal void UnregisterName(string name)
	{
		DataCommonEventSource.Log.Trace("<ds.DataRelationCollection.UnregisterName|INFO> {0}, name='{1}'", ObjectID, name);
		if (NamesEqual(name, MakeName(_defaultNameIndex - 1), fCaseSensitive: true, GetDataSet().Locale) != 0)
		{
			do
			{
				_defaultNameIndex--;
			}
			while (_defaultNameIndex > 1 && !Contains(MakeName(_defaultNameIndex - 1)));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DataRelationCollection" /> class. </summary>
	protected DataRelationCollection()
	{
	}
}
