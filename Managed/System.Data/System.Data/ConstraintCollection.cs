using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using Unity;

namespace System.Data;

/// <summary>Represents a collection of constraints for a <see cref="T:System.Data.DataTable" />.</summary>
/// <filterpriority>1</filterpriority>
[DefaultEvent("CollectionChanged")]
public sealed class ConstraintCollection : InternalDataCollectionBase
{
	private readonly DataTable _table;

	private readonly ArrayList _list;

	private int _defaultNameIndex;

	private CollectionChangeEventHandler _onCollectionChanged;

	private Constraint[] _delayLoadingConstraints;

	private bool _fLoadForeignKeyConstraintsOnly;

	protected override ArrayList List => _list;

	/// <summary>Gets the <see cref="T:System.Data.Constraint" /> from the collection at the specified index.</summary>
	/// <returns>The <see cref="T:System.Data.Constraint" /> at the specified index.</returns>
	/// <param name="index">The index of the constraint to return. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The index value is greater than the number of items in the collection. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public Constraint this[int index]
	{
		get
		{
			if (index >= 0 && index < List.Count)
			{
				return (Constraint)List[index];
			}
			throw ExceptionBuilder.ConstraintOutOfRange(index);
		}
	}

	internal DataTable Table => _table;

	/// <summary>Gets the <see cref="T:System.Data.Constraint" /> from the collection with the specified name.</summary>
	/// <returns>The <see cref="T:System.Data.Constraint" /> with the specified name; otherwise a null value if the <see cref="T:System.Data.Constraint" /> does not exist.</returns>
	/// <param name="name">The <see cref="P:System.Data.Constraint.ConstraintName" /> of the constraint to return. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public Constraint this[string name]
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
				return (Constraint)List[num];
			}
			return null;
		}
	}

	/// <summary>Occurs whenever the <see cref="T:System.Data.ConstraintCollection" /> is changed because of <see cref="T:System.Data.Constraint" /> objects being added or removed.</summary>
	/// <filterpriority>2</filterpriority>
	public event CollectionChangeEventHandler CollectionChanged
	{
		add
		{
			_onCollectionChanged = (CollectionChangeEventHandler)Delegate.Combine(_onCollectionChanged, value);
		}
		remove
		{
			_onCollectionChanged = (CollectionChangeEventHandler)Delegate.Remove(_onCollectionChanged, value);
		}
	}

	internal ConstraintCollection(DataTable table)
	{
		_list = new ArrayList();
		_defaultNameIndex = 1;
		base._002Ector();
		_table = table;
	}

	/// <summary>Adds the specified <see cref="T:System.Data.Constraint" /> object to the collection.</summary>
	/// <param name="constraint">The Constraint to add. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="constraint" /> argument is null. </exception>
	/// <exception cref="T:System.ArgumentException">The constraint already belongs to this collection, or belongs to another collection. </exception>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a constraint with the same name. (The comparison is not case-sensitive.) </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Add(Constraint constraint)
	{
		Add(constraint, addUniqueWhenAddingForeign: true);
	}

	internal void Add(Constraint constraint, bool addUniqueWhenAddingForeign)
	{
		if (constraint == null)
		{
			throw ExceptionBuilder.ArgumentNull("constraint");
		}
		if (FindConstraint(constraint) != null)
		{
			throw ExceptionBuilder.DuplicateConstraint(FindConstraint(constraint).ConstraintName);
		}
		if (1 < _table.NestedParentRelations.Length && !AutoGenerated(constraint))
		{
			throw ExceptionBuilder.CantAddConstraintToMultipleNestedTable(_table.TableName);
		}
		if (constraint is UniqueConstraint)
		{
			if (((UniqueConstraint)constraint)._bPrimaryKey && Table._primaryKey != null)
			{
				throw ExceptionBuilder.AddPrimaryKeyConstraint();
			}
			AddUniqueConstraint((UniqueConstraint)constraint);
		}
		else if (constraint is ForeignKeyConstraint)
		{
			ForeignKeyConstraint foreignKeyConstraint = (ForeignKeyConstraint)constraint;
			if (addUniqueWhenAddingForeign)
			{
				UniqueConstraint uniqueConstraint = foreignKeyConstraint.RelatedTable.Constraints.FindKeyConstraint(foreignKeyConstraint.RelatedColumnsReference);
				if (uniqueConstraint == null)
				{
					if (constraint.ConstraintName.Length == 0)
					{
						constraint.ConstraintName = AssignName();
					}
					else
					{
						RegisterName(constraint.ConstraintName);
					}
					uniqueConstraint = new UniqueConstraint(foreignKeyConstraint.RelatedColumnsReference);
					foreignKeyConstraint.RelatedTable.Constraints.Add(uniqueConstraint);
				}
			}
			AddForeignKeyConstraint((ForeignKeyConstraint)constraint);
		}
		BaseAdd(constraint);
		ArrayAdd(constraint);
		OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, constraint));
		if (constraint is UniqueConstraint && ((UniqueConstraint)constraint)._bPrimaryKey)
		{
			Table.PrimaryKey = ((UniqueConstraint)constraint).ColumnsReference;
		}
	}

	/// <summary>Constructs a new <see cref="T:System.Data.UniqueConstraint" /> with the specified name, array of <see cref="T:System.Data.DataColumn" /> objects, and value that indicates whether the column is a primary key, and adds it to the collection.</summary>
	/// <returns>A new UniqueConstraint.</returns>
	/// <param name="name">The name of the <see cref="T:System.Data.UniqueConstraint" />. </param>
	/// <param name="columns">An array of <see cref="T:System.Data.DataColumn" /> objects to which the constraint applies. </param>
	/// <param name="primaryKey">Specifies whether the column should be the primary key. If true, the column will be a primary key column.</param>
	/// <exception cref="T:System.ArgumentException">The constraint already belongs to this collection.-Or- The constraint belongs to another collection. </exception>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a constraint with the specified name. (The comparison is not case-sensitive.) </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public Constraint Add(string name, DataColumn[] columns, bool primaryKey)
	{
		UniqueConstraint uniqueConstraint = new UniqueConstraint(name, columns);
		Add(uniqueConstraint);
		if (primaryKey)
		{
			Table.PrimaryKey = columns;
		}
		return uniqueConstraint;
	}

	/// <summary>Constructs a new <see cref="T:System.Data.UniqueConstraint" /> with the specified name, <see cref="T:System.Data.DataColumn" />, and value that indicates whether the column is a primary key, and adds it to the collection.</summary>
	/// <returns>A new UniqueConstraint.</returns>
	/// <param name="name">The name of the UniqueConstraint. </param>
	/// <param name="column">The <see cref="T:System.Data.DataColumn" /> to which the constraint applies. </param>
	/// <param name="primaryKey">Specifies whether the column should be the primary key. If true, the column will be a primary key column. </param>
	/// <exception cref="T:System.ArgumentException">The constraint already belongs to this collection.-Or- The constraint belongs to another collection. </exception>
	/// <exception cref="T:System.Data.DuplicateNameException">The collection already has a constraint with the specified name. (The comparison is not case-sensitive.) </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public Constraint Add(string name, DataColumn column, bool primaryKey)
	{
		UniqueConstraint uniqueConstraint = new UniqueConstraint(name, column);
		Add(uniqueConstraint);
		if (primaryKey)
		{
			Table.PrimaryKey = uniqueConstraint.ColumnsReference;
		}
		return uniqueConstraint;
	}

	/// <summary>Constructs a new <see cref="T:System.Data.ForeignKeyConstraint" /> with the specified name, parent column, and child column, and adds the constraint to the collection.</summary>
	/// <returns>A new ForeignKeyConstraint.</returns>
	/// <param name="name">The name of the <see cref="T:System.Data.ForeignKeyConstraint" />. </param>
	/// <param name="primaryKeyColumn">The primary key, or parent, <see cref="T:System.Data.DataColumn" />. </param>
	/// <param name="foreignKeyColumn">The foreign key, or child, <see cref="T:System.Data.DataColumn" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public Constraint Add(string name, DataColumn primaryKeyColumn, DataColumn foreignKeyColumn)
	{
		ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint(name, primaryKeyColumn, foreignKeyColumn);
		Add(foreignKeyConstraint);
		return foreignKeyConstraint;
	}

	/// <summary>Constructs a new <see cref="T:System.Data.ForeignKeyConstraint" />, with the specified arrays of parent columns and child columns, and adds the constraint to the collection.</summary>
	/// <returns>A new ForeignKeyConstraint.</returns>
	/// <param name="name">The name of the <see cref="T:System.Data.ForeignKeyConstraint" />. </param>
	/// <param name="primaryKeyColumns">An array of <see cref="T:System.Data.DataColumn" /> objects that are the primary key, or parent, columns. </param>
	/// <param name="foreignKeyColumns">An array of <see cref="T:System.Data.DataColumn" /> objects that are the foreign key, or child, columns. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public Constraint Add(string name, DataColumn[] primaryKeyColumns, DataColumn[] foreignKeyColumns)
	{
		ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint(name, primaryKeyColumns, foreignKeyColumns);
		Add(foreignKeyConstraint);
		return foreignKeyConstraint;
	}

	/// <summary>Copies the elements of the specified <see cref="T:System.Data.ConstraintCollection" /> array to the end of the collection.</summary>
	/// <param name="constraints">An array of <see cref="T:System.Data.ConstraintCollection" /> objects to add to the collection. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void AddRange(Constraint[] constraints)
	{
		if (_table.fInitInProgress)
		{
			_delayLoadingConstraints = constraints;
			_fLoadForeignKeyConstraintsOnly = false;
		}
		else
		{
			if (constraints == null)
			{
				return;
			}
			foreach (Constraint constraint in constraints)
			{
				if (constraint != null)
				{
					Add(constraint);
				}
			}
		}
	}

	private void AddUniqueConstraint(UniqueConstraint constraint)
	{
		DataColumn[] columnsReference = constraint.ColumnsReference;
		for (int i = 0; i < columnsReference.Length; i++)
		{
			if (columnsReference[i].Table != _table)
			{
				throw ExceptionBuilder.ConstraintForeignTable();
			}
		}
		constraint.ConstraintIndexInitialize();
		if (!constraint.CanEnableConstraint())
		{
			constraint.ConstraintIndexClear();
			throw ExceptionBuilder.UniqueConstraintViolation();
		}
	}

	private void AddForeignKeyConstraint(ForeignKeyConstraint constraint)
	{
		if (!constraint.CanEnableConstraint())
		{
			throw ExceptionBuilder.ConstraintParentValues();
		}
		constraint.CheckCanAddToCollection(this);
	}

	private bool AutoGenerated(Constraint constraint)
	{
		if (constraint is ForeignKeyConstraint fk)
		{
			return XmlTreeGen.AutoGenerated(fk, checkRelation: false);
		}
		return XmlTreeGen.AutoGenerated((UniqueConstraint)constraint);
	}

	private void ArrayAdd(Constraint constraint)
	{
		List.Add(constraint);
	}

	private void ArrayRemove(Constraint constraint)
	{
		List.Remove(constraint);
	}

	internal string AssignName()
	{
		string result = MakeName(_defaultNameIndex);
		_defaultNameIndex++;
		return result;
	}

	private void BaseAdd(Constraint constraint)
	{
		if (constraint == null)
		{
			throw ExceptionBuilder.ArgumentNull("constraint");
		}
		if (constraint.ConstraintName.Length == 0)
		{
			constraint.ConstraintName = AssignName();
		}
		else
		{
			RegisterName(constraint.ConstraintName);
		}
		constraint.InCollection = true;
	}

	private void BaseGroupSwitch(Constraint[] oldArray, int oldLength, Constraint[] newArray, int newLength)
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
			if (!flag)
			{
				BaseRemove(oldArray[i]);
				List.Remove(oldArray[i]);
			}
		}
		for (int k = 0; k < newLength; k++)
		{
			if (!newArray[k].InCollection)
			{
				BaseAdd(newArray[k]);
			}
			List.Add(newArray[k]);
		}
	}

	private void BaseRemove(Constraint constraint)
	{
		if (constraint == null)
		{
			throw ExceptionBuilder.ArgumentNull("constraint");
		}
		if (constraint.Table != _table)
		{
			throw ExceptionBuilder.ConstraintRemoveFailed();
		}
		UnregisterName(constraint.ConstraintName);
		constraint.InCollection = false;
		if (constraint is UniqueConstraint)
		{
			for (int i = 0; i < Table.ChildRelations.Count; i++)
			{
				DataRelation dataRelation = Table.ChildRelations[i];
				if (dataRelation.ParentKeyConstraint == constraint)
				{
					dataRelation.SetParentKeyConstraint(null);
				}
			}
			((UniqueConstraint)constraint).ConstraintIndexClear();
		}
		else
		{
			if (!(constraint is ForeignKeyConstraint))
			{
				return;
			}
			for (int j = 0; j < Table.ParentRelations.Count; j++)
			{
				DataRelation dataRelation2 = Table.ParentRelations[j];
				if (dataRelation2.ChildKeyConstraint == constraint)
				{
					dataRelation2.SetChildKeyConstraint(null);
				}
			}
		}
	}

	/// <summary>Indicates whether a <see cref="T:System.Data.Constraint" /> can be removed.</summary>
	/// <returns>true if the <see cref="T:System.Data.Constraint" /> can be removed from collection; otherwise, false.</returns>
	/// <param name="constraint">The <see cref="T:System.Data.Constraint" /> to be tested for removal from the collection. </param>
	/// <filterpriority>2</filterpriority>
	public bool CanRemove(Constraint constraint)
	{
		return CanRemove(constraint, fThrowException: false);
	}

	internal bool CanRemove(Constraint constraint, bool fThrowException)
	{
		return constraint.CanBeRemovedFromCollection(this, fThrowException);
	}

	/// <summary>Clears the collection of any <see cref="T:System.Data.Constraint" /> objects.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Clear()
	{
		if (_table != null)
		{
			_table.PrimaryKey = null;
			for (int i = 0; i < _table.ParentRelations.Count; i++)
			{
				_table.ParentRelations[i].SetChildKeyConstraint(null);
			}
			for (int j = 0; j < _table.ChildRelations.Count; j++)
			{
				_table.ChildRelations[j].SetParentKeyConstraint(null);
			}
		}
		if (_table.fInitInProgress && _delayLoadingConstraints != null)
		{
			_delayLoadingConstraints = null;
			_fLoadForeignKeyConstraintsOnly = false;
		}
		int count = List.Count;
		Constraint[] array = new Constraint[List.Count];
		List.CopyTo(array, 0);
		try
		{
			BaseGroupSwitch(array, count, null, 0);
		}
		catch (Exception e) when (ADP.IsCatchableOrSecurityExceptionType(e))
		{
			BaseGroupSwitch(null, 0, array, count);
			List.Clear();
			for (int k = 0; k < count; k++)
			{
				List.Add(array[k]);
			}
			throw;
		}
		List.Clear();
		OnCollectionChanged(InternalDataCollectionBase.s_refreshEventArgs);
	}

	/// <summary>Indicates whether the <see cref="T:System.Data.Constraint" /> object specified by name exists in the collection.</summary>
	/// <returns>true if the collection contains the specified constraint; otherwise, false.</returns>
	/// <param name="name">The <see cref="P:System.Data.Constraint.ConstraintName" /> of the constraint. </param>
	/// <filterpriority>1</filterpriority>
	public bool Contains(string name)
	{
		return InternalIndexOf(name) >= 0;
	}

	internal bool Contains(string name, bool caseSensitive)
	{
		if (!caseSensitive)
		{
			return Contains(name);
		}
		int num = InternalIndexOf(name);
		if (num < 0)
		{
			return false;
		}
		return name == ((Constraint)List[num]).ConstraintName;
	}

	/// <summary>Copies the collection objects to a one-dimensional <see cref="T:System.Array" /> instance starting at the specified index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the values copied from the collection.</param>
	/// <param name="index">The index of the array at which to start inserting. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void CopyTo(Constraint[] array, int index)
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
			array[index + i] = (Constraint)_list[i];
		}
	}

	internal Constraint FindConstraint(Constraint constraint)
	{
		int count = List.Count;
		for (int i = 0; i < count; i++)
		{
			if (((Constraint)List[i]).Equals(constraint))
			{
				return (Constraint)List[i];
			}
		}
		return null;
	}

	internal UniqueConstraint FindKeyConstraint(DataColumn[] columns)
	{
		int count = List.Count;
		for (int i = 0; i < count; i++)
		{
			if (List[i] is UniqueConstraint { Key: var key } uniqueConstraint && CompareArrays(key.ColumnsReference, columns))
			{
				return uniqueConstraint;
			}
		}
		return null;
	}

	internal UniqueConstraint FindKeyConstraint(DataColumn column)
	{
		int count = List.Count;
		for (int i = 0; i < count; i++)
		{
			if (List[i] is UniqueConstraint { Key: var key } uniqueConstraint && key.ColumnsReference.Length == 1 && uniqueConstraint.Key.ColumnsReference[0] == column)
			{
				return uniqueConstraint;
			}
		}
		return null;
	}

	internal ForeignKeyConstraint FindForeignKeyConstraint(DataColumn[] parentColumns, DataColumn[] childColumns)
	{
		int count = List.Count;
		for (int i = 0; i < count; i++)
		{
			if (List[i] is ForeignKeyConstraint { ParentKey: var parentKey } foreignKeyConstraint && CompareArrays(parentKey.ColumnsReference, parentColumns) && CompareArrays(foreignKeyConstraint.ChildKey.ColumnsReference, childColumns))
			{
				return foreignKeyConstraint;
			}
		}
		return null;
	}

	private static bool CompareArrays(DataColumn[] a1, DataColumn[] a2)
	{
		if (a1.Length != a2.Length)
		{
			return false;
		}
		for (int i = 0; i < a1.Length; i++)
		{
			bool flag = false;
			for (int j = 0; j < a2.Length; j++)
			{
				if (a1[i] == a2[j])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Gets the index of the specified <see cref="T:System.Data.Constraint" />.</summary>
	/// <returns>The zero-based index of the <see cref="T:System.Data.Constraint" /> if it is in the collection; otherwise, -1.</returns>
	/// <param name="constraint">The <see cref="T:System.Data.Constraint" /> to search for. </param>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(Constraint constraint)
	{
		if (constraint != null)
		{
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				if (constraint == (Constraint)List[i])
				{
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>Gets the index of the <see cref="T:System.Data.Constraint" /> specified by name.</summary>
	/// <returns>The index of the <see cref="T:System.Data.Constraint" /> if it is in the collection; otherwise, -1.</returns>
	/// <param name="constraintName">The name of the <see cref="T:System.Data.Constraint" />. </param>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(string constraintName)
	{
		int num = InternalIndexOf(constraintName);
		if (num >= 0)
		{
			return num;
		}
		return -1;
	}

	internal int InternalIndexOf(string constraintName)
	{
		int num = -1;
		if (constraintName != null && 0 < constraintName.Length)
		{
			int count = List.Count;
			int num2 = 0;
			for (int i = 0; i < count; i++)
			{
				Constraint constraint = (Constraint)List[i];
				switch (NamesEqual(constraint.ConstraintName, constraintName, fCaseSensitive: false, _table.Locale))
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

	private string MakeName(int index)
	{
		if (1 == index)
		{
			return "Constraint1";
		}
		return "Constraint" + index.ToString(CultureInfo.InvariantCulture);
	}

	private void OnCollectionChanged(CollectionChangeEventArgs ccevent)
	{
		_onCollectionChanged?.Invoke(this, ccevent);
	}

	internal void RegisterName(string name)
	{
		int count = List.Count;
		for (int i = 0; i < count; i++)
		{
			if (NamesEqual(name, ((Constraint)List[i]).ConstraintName, fCaseSensitive: true, _table.Locale) != 0)
			{
				throw ExceptionBuilder.DuplicateConstraintName(((Constraint)List[i]).ConstraintName);
			}
		}
		if (NamesEqual(name, MakeName(_defaultNameIndex), fCaseSensitive: true, _table.Locale) != 0)
		{
			_defaultNameIndex++;
		}
	}

	/// <summary>Removes the specified <see cref="T:System.Data.Constraint" /> from the collection.</summary>
	/// <param name="constraint">The <see cref="T:System.Data.Constraint" /> to remove. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="constraint" /> argument is null. </exception>
	/// <exception cref="T:System.ArgumentException">The constraint does not belong to the collection. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(Constraint constraint)
	{
		if (constraint == null)
		{
			throw ExceptionBuilder.ArgumentNull("constraint");
		}
		if (CanRemove(constraint, fThrowException: true))
		{
			BaseRemove(constraint);
			ArrayRemove(constraint);
			if (constraint is UniqueConstraint && ((UniqueConstraint)constraint).IsPrimaryKey)
			{
				Table.PrimaryKey = null;
			}
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, constraint));
		}
	}

	/// <summary>Removes the <see cref="T:System.Data.Constraint" /> object at the specified index from the collection.</summary>
	/// <param name="index">The index of the <see cref="T:System.Data.Constraint" /> to remove. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The collection does not have a constraint at this index. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void RemoveAt(int index)
	{
		Constraint constraint = this[index];
		if (constraint == null)
		{
			throw ExceptionBuilder.ConstraintOutOfRange(index);
		}
		Remove(constraint);
	}

	/// <summary>Removes the <see cref="T:System.Data.Constraint" /> object specified by name from the collection.</summary>
	/// <param name="name">The name of the <see cref="T:System.Data.Constraint" /> to remove. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(string name)
	{
		Constraint constraint = this[name];
		if (constraint == null)
		{
			throw ExceptionBuilder.ConstraintNotInTheTable(name);
		}
		Remove(constraint);
	}

	internal void UnregisterName(string name)
	{
		if (NamesEqual(name, MakeName(_defaultNameIndex - 1), fCaseSensitive: true, _table.Locale) != 0)
		{
			do
			{
				_defaultNameIndex--;
			}
			while (_defaultNameIndex > 1 && !Contains(MakeName(_defaultNameIndex - 1)));
		}
	}

	internal void FinishInitConstraints()
	{
		if (_delayLoadingConstraints == null)
		{
			return;
		}
		for (int i = 0; i < _delayLoadingConstraints.Length; i++)
		{
			int num;
			DataColumn[] array;
			if (_delayLoadingConstraints[i] is UniqueConstraint)
			{
				if (_fLoadForeignKeyConstraintsOnly)
				{
					continue;
				}
				UniqueConstraint uniqueConstraint = (UniqueConstraint)_delayLoadingConstraints[i];
				if (uniqueConstraint._columnNames == null)
				{
					Add(uniqueConstraint);
					continue;
				}
				num = uniqueConstraint._columnNames.Length;
				array = new DataColumn[num];
				for (int j = 0; j < num; j++)
				{
					array[j] = _table.Columns[uniqueConstraint._columnNames[j]];
				}
				if (uniqueConstraint._bPrimaryKey)
				{
					if (_table._primaryKey != null)
					{
						throw ExceptionBuilder.AddPrimaryKeyConstraint();
					}
					Add(uniqueConstraint.ConstraintName, array, primaryKey: true);
				}
				else
				{
					UniqueConstraint constraint = new UniqueConstraint(uniqueConstraint._constraintName, array);
					if (FindConstraint(constraint) == null)
					{
						Add(constraint);
					}
				}
				continue;
			}
			ForeignKeyConstraint foreignKeyConstraint = (ForeignKeyConstraint)_delayLoadingConstraints[i];
			if (foreignKeyConstraint._parentColumnNames == null || foreignKeyConstraint._childColumnNames == null)
			{
				Add(foreignKeyConstraint);
				continue;
			}
			if (_table.DataSet == null)
			{
				_fLoadForeignKeyConstraintsOnly = true;
				continue;
			}
			num = foreignKeyConstraint._parentColumnNames.Length;
			array = new DataColumn[num];
			DataColumn[] array2 = new DataColumn[num];
			for (int k = 0; k < num; k++)
			{
				if (foreignKeyConstraint._parentTableNamespace == null)
				{
					array[k] = _table.DataSet.Tables[foreignKeyConstraint._parentTableName].Columns[foreignKeyConstraint._parentColumnNames[k]];
				}
				else
				{
					array[k] = _table.DataSet.Tables[foreignKeyConstraint._parentTableName, foreignKeyConstraint._parentTableNamespace].Columns[foreignKeyConstraint._parentColumnNames[k]];
				}
				array2[k] = _table.Columns[foreignKeyConstraint._childColumnNames[k]];
			}
			ForeignKeyConstraint foreignKeyConstraint2 = new ForeignKeyConstraint(foreignKeyConstraint._constraintName, array, array2);
			foreignKeyConstraint2.AcceptRejectRule = foreignKeyConstraint._acceptRejectRule;
			foreignKeyConstraint2.DeleteRule = foreignKeyConstraint._deleteRule;
			foreignKeyConstraint2.UpdateRule = foreignKeyConstraint._updateRule;
			Add(foreignKeyConstraint2);
		}
		if (!_fLoadForeignKeyConstraintsOnly)
		{
			_delayLoadingConstraints = null;
		}
	}

	internal ConstraintCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
