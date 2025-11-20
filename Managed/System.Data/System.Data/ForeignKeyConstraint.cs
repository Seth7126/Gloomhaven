using System.ComponentModel;
using System.Data.Common;

namespace System.Data;

/// <summary>Represents an action restriction enforced on a set of columns in a primary key/foreign key relationship when a value or row is either deleted or updated.</summary>
/// <filterpriority>1</filterpriority>
[DefaultProperty("ConstraintName")]
public class ForeignKeyConstraint : Constraint
{
	internal const Rule Rule_Default = Rule.Cascade;

	internal const AcceptRejectRule AcceptRejectRule_Default = AcceptRejectRule.None;

	internal Rule _deleteRule = Rule.Cascade;

	internal Rule _updateRule = Rule.Cascade;

	internal AcceptRejectRule _acceptRejectRule;

	private DataKey _childKey;

	private DataKey _parentKey;

	internal string _constraintName;

	internal string[] _parentColumnNames;

	internal string[] _childColumnNames;

	internal string _parentTableName;

	internal string _parentTableNamespace;

	internal DataKey ChildKey
	{
		get
		{
			CheckStateForProperty();
			return _childKey;
		}
	}

	/// <summary>Gets the child columns of this constraint.</summary>
	/// <returns>An array of <see cref="T:System.Data.DataColumn" /> objects that are the child columns of the constraint.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ReadOnly(true)]
	public virtual DataColumn[] Columns
	{
		get
		{
			CheckStateForProperty();
			return _childKey.ToArray();
		}
	}

	/// <summary>Gets the child table of this constraint.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> that is the child table in the constraint.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ReadOnly(true)]
	public override DataTable Table
	{
		get
		{
			CheckStateForProperty();
			return _childKey.Table;
		}
	}

	internal string[] ParentColumnNames => _parentKey.GetColumnNames();

	internal string[] ChildColumnNames => _childKey.GetColumnNames();

	/// <summary>Indicates the action that should take place across this constraint when <see cref="M:System.Data.DataTable.AcceptChanges" /> is invoked.</summary>
	/// <returns>One of the <see cref="T:System.Data.AcceptRejectRule" /> values. Possible values include None, and Cascade. The default is None.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DefaultValue(AcceptRejectRule.None)]
	public virtual AcceptRejectRule AcceptRejectRule
	{
		get
		{
			CheckStateForProperty();
			return _acceptRejectRule;
		}
		set
		{
			if ((uint)value <= 1u)
			{
				_acceptRejectRule = value;
				return;
			}
			throw ADP.InvalidAcceptRejectRule(value);
		}
	}

	/// <summary>Gets or sets the action that occurs across this constraint when a row is deleted.</summary>
	/// <returns>One of the <see cref="T:System.Data.Rule" /> values. The default is Cascade.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DefaultValue(Rule.Cascade)]
	public virtual Rule DeleteRule
	{
		get
		{
			CheckStateForProperty();
			return _deleteRule;
		}
		set
		{
			if ((uint)value <= 3u)
			{
				_deleteRule = value;
				return;
			}
			throw ADP.InvalidRule(value);
		}
	}

	/// <summary>The parent columns of this constraint.</summary>
	/// <returns>An array of <see cref="T:System.Data.DataColumn" /> objects that are the parent columns of the constraint.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ReadOnly(true)]
	public virtual DataColumn[] RelatedColumns
	{
		get
		{
			CheckStateForProperty();
			return _parentKey.ToArray();
		}
	}

	internal DataColumn[] RelatedColumnsReference
	{
		get
		{
			CheckStateForProperty();
			return _parentKey.ColumnsReference;
		}
	}

	internal DataKey ParentKey
	{
		get
		{
			CheckStateForProperty();
			return _parentKey;
		}
	}

	/// <summary>Gets the parent table of this constraint.</summary>
	/// <returns>The parent <see cref="T:System.Data.DataTable" /> of this constraint.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[ReadOnly(true)]
	public virtual DataTable RelatedTable
	{
		get
		{
			CheckStateForProperty();
			return _parentKey.Table;
		}
	}

	/// <summary>Gets or sets the action that occurs across this constraint on when a row is updated.</summary>
	/// <returns>One of the <see cref="T:System.Data.Rule" /> values. The default is Cascade.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DefaultValue(Rule.Cascade)]
	public virtual Rule UpdateRule
	{
		get
		{
			CheckStateForProperty();
			return _updateRule;
		}
		set
		{
			if ((uint)value <= 3u)
			{
				_updateRule = value;
				return;
			}
			throw ADP.InvalidRule(value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.ForeignKeyConstraint" /> class with the specified parent and child <see cref="T:System.Data.DataColumn" /> objects.</summary>
	/// <param name="parentColumn">The parent <see cref="T:System.Data.DataColumn" /> in the constraint. </param>
	/// <param name="childColumn">The child <see cref="T:System.Data.DataColumn" /> in the constraint. </param>
	/// <exception cref="T:System.ArgumentNullException">One or both of the columns is null. </exception>
	/// <exception cref="T:System.Data.InvalidConstraintException">The columns have different data types.-Or - The tables don't belong to the same <see cref="T:System.Data.DataSet" />. </exception>
	public ForeignKeyConstraint(DataColumn parentColumn, DataColumn childColumn)
		: this(null, parentColumn, childColumn)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.ForeignKeyConstraint" /> class with the specified name, parent and child <see cref="T:System.Data.DataColumn" /> objects.</summary>
	/// <param name="constraintName">The name of the constraint. </param>
	/// <param name="parentColumn">The parent <see cref="T:System.Data.DataColumn" /> in the constraint. </param>
	/// <param name="childColumn">The child <see cref="T:System.Data.DataColumn" /> in the constraint. </param>
	/// <exception cref="T:System.ArgumentNullException">One or both of the columns is null. </exception>
	/// <exception cref="T:System.Data.InvalidConstraintException">The columns have different data types.-Or - The tables don't belong to the same <see cref="T:System.Data.DataSet" />. </exception>
	public ForeignKeyConstraint(string constraintName, DataColumn parentColumn, DataColumn childColumn)
	{
		DataColumn[] parentColumns = new DataColumn[1] { parentColumn };
		DataColumn[] childColumns = new DataColumn[1] { childColumn };
		Create(constraintName, parentColumns, childColumns);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.ForeignKeyConstraint" /> class with the specified arrays of parent and child <see cref="T:System.Data.DataColumn" /> objects.</summary>
	/// <param name="parentColumns">An array of parent <see cref="T:System.Data.DataColumn" /> in the constraint. </param>
	/// <param name="childColumns">An array of child <see cref="T:System.Data.DataColumn" /> in the constraint. </param>
	/// <exception cref="T:System.ArgumentNullException">One or both of the columns is null. </exception>
	/// <exception cref="T:System.Data.InvalidConstraintException">The columns have different data types.-Or - The tables don't belong to the same <see cref="T:System.Data.DataSet" />. </exception>
	public ForeignKeyConstraint(DataColumn[] parentColumns, DataColumn[] childColumns)
		: this(null, parentColumns, childColumns)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.ForeignKeyConstraint" /> class with the specified name, and arrays of parent and child <see cref="T:System.Data.DataColumn" /> objects.</summary>
	/// <param name="constraintName">The name of the <see cref="T:System.Data.ForeignKeyConstraint" />. If null or empty string, a default name will be given when added to the constraints collection. </param>
	/// <param name="parentColumns">An array of parent <see cref="T:System.Data.DataColumn" /> in the constraint. </param>
	/// <param name="childColumns">An array of child <see cref="T:System.Data.DataColumn" /> in the constraint. </param>
	/// <exception cref="T:System.ArgumentNullException">One or both of the columns is null. </exception>
	/// <exception cref="T:System.Data.InvalidConstraintException">The columns have different data types.-Or - The tables don't belong to the same <see cref="T:System.Data.DataSet" />. </exception>
	public ForeignKeyConstraint(string constraintName, DataColumn[] parentColumns, DataColumn[] childColumns)
	{
		Create(constraintName, parentColumns, childColumns);
	}

	/// <summary>This constructor is provided for design time support in the Visual Studio  environment. <see cref="T:System.Data.ForeignKeyConstraint" /> objects created by using this constructor must then be added to the collection via <see cref="M:System.Data.ConstraintCollection.AddRange(System.Data.Constraint[])" />. Tables and columns with the specified names must exist at the time the method is called, or if <see cref="M:System.Data.DataTable.BeginInit" /> has been called prior to calling this constructor, the tables and columns with the specified names must exist at the time that <see cref="M:System.Data.DataTable.EndInit" /> is called.</summary>
	/// <param name="constraintName">The name of the constraint. </param>
	/// <param name="parentTableName">The name of the parent <see cref="T:System.Data.DataTable" /> that contains parent <see cref="T:System.Data.DataColumn" /> objects in the constraint. </param>
	/// <param name="parentColumnNames">An array of the names of parent <see cref="T:System.Data.DataColumn" /> objects in the constraint. </param>
	/// <param name="childColumnNames">An array of the names of child <see cref="T:System.Data.DataColumn" /> objects in the constraint. </param>
	/// <param name="acceptRejectRule">One of the <see cref="T:System.Data.AcceptRejectRule" /> values. Possible values include None, Cascade, and Default. </param>
	/// <param name="deleteRule">One of the <see cref="T:System.Data.Rule" /> values to use when a row is deleted. The default is Cascade. Possible values include: None, Cascade, SetNull, SetDefault, and Default. </param>
	/// <param name="updateRule">One of the <see cref="T:System.Data.Rule" /> values to use when a row is updated. The default is Cascade. Possible values include: None, Cascade, SetNull, SetDefault, and Default. </param>
	/// <exception cref="T:System.ArgumentNullException">One or both of the columns is null. </exception>
	/// <exception cref="T:System.Data.InvalidConstraintException">The columns have different data types.-Or - The tables don't belong to the same <see cref="T:System.Data.DataSet" />. </exception>
	[Browsable(false)]
	public ForeignKeyConstraint(string constraintName, string parentTableName, string[] parentColumnNames, string[] childColumnNames, AcceptRejectRule acceptRejectRule, Rule deleteRule, Rule updateRule)
	{
		_constraintName = constraintName;
		_parentColumnNames = parentColumnNames;
		_childColumnNames = childColumnNames;
		_parentTableName = parentTableName;
		_acceptRejectRule = acceptRejectRule;
		_deleteRule = deleteRule;
		_updateRule = updateRule;
	}

	/// <summary>This constructor is provided for design time support in the Visual Studio  environment. <see cref="T:System.Data.ForeignKeyConstraint" /> objects created by using this constructor must then be added to the collection via <see cref="M:System.Data.ConstraintCollection.AddRange(System.Data.Constraint[])" />. Tables and columns with the specified names must exist at the time the method is called, or if <see cref="M:System.Data.DataTable.BeginInit" /> has been called prior to calling this constructor, the tables and columns with the specified names must exist at the time that <see cref="M:System.Data.DataTable.EndInit" /> is called.</summary>
	/// <param name="constraintName">The name of the constraint. </param>
	/// <param name="parentTableName">The name of the parent <see cref="T:System.Data.DataTable" /> that contains parent <see cref="T:System.Data.DataColumn" /> objects in the constraint. </param>
	/// <param name="parentTableNamespace">The name of the <see cref="P:System.Data.DataTable.Namespace" />. </param>
	/// <param name="parentColumnNames">An array of the names of parent <see cref="T:System.Data.DataColumn" /> objects in the constraint. </param>
	/// <param name="childColumnNames">An array of the names of child <see cref="T:System.Data.DataColumn" /> objects in the constraint. </param>
	/// <param name="acceptRejectRule">One of the <see cref="T:System.Data.AcceptRejectRule" /> values. Possible values include None, Cascade, and Default. </param>
	/// <param name="deleteRule">One of the <see cref="T:System.Data.Rule" /> values to use when a row is deleted. The default is Cascade. Possible values include: None, Cascade, SetNull, SetDefault, and Default. </param>
	/// <param name="updateRule">One of the <see cref="T:System.Data.Rule" /> values to use when a row is updated. The default is Cascade. Possible values include: None, Cascade, SetNull, SetDefault, and Default. </param>
	/// <exception cref="T:System.ArgumentNullException">One or both of the columns is null. </exception>
	/// <exception cref="T:System.Data.InvalidConstraintException">The columns have different data types.-Or - The tables don't belong to the same <see cref="T:System.Data.DataSet" />. </exception>
	[Browsable(false)]
	public ForeignKeyConstraint(string constraintName, string parentTableName, string parentTableNamespace, string[] parentColumnNames, string[] childColumnNames, AcceptRejectRule acceptRejectRule, Rule deleteRule, Rule updateRule)
	{
		_constraintName = constraintName;
		_parentColumnNames = parentColumnNames;
		_childColumnNames = childColumnNames;
		_parentTableName = parentTableName;
		_parentTableNamespace = parentTableNamespace;
		_acceptRejectRule = acceptRejectRule;
		_deleteRule = deleteRule;
		_updateRule = updateRule;
	}

	internal override void CheckCanAddToCollection(ConstraintCollection constraints)
	{
		if (Table != constraints.Table)
		{
			throw ExceptionBuilder.ConstraintAddFailed(constraints.Table);
		}
		if (Table.Locale.LCID != RelatedTable.Locale.LCID || Table.CaseSensitive != RelatedTable.CaseSensitive)
		{
			throw ExceptionBuilder.CaseLocaleMismatch();
		}
	}

	internal override bool CanBeRemovedFromCollection(ConstraintCollection constraints, bool fThrowException)
	{
		return true;
	}

	internal bool IsKeyNull(object[] values)
	{
		for (int i = 0; i < values.Length; i++)
		{
			if (!DataStorage.IsObjectNull(values[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal override bool IsConstraintViolated()
	{
		Index sortIndex = _childKey.GetSortIndex();
		object[] uniqueKeyValues = sortIndex.GetUniqueKeyValues();
		bool result = false;
		Index sortIndex2 = _parentKey.GetSortIndex();
		for (int i = 0; i < uniqueKeyValues.Length; i++)
		{
			object[] array = (object[])uniqueKeyValues[i];
			if (!IsKeyNull(array) && !sortIndex2.IsKeyInIndex(array))
			{
				DataRow[] rows = sortIndex.GetRows(sortIndex.FindRecords(array));
				string rowError = global::SR.Format("ForeignKeyConstraint {0} requires the child key values ({1}) to exist in the parent table.", ConstraintName, ExceptionBuilder.KeysToString(array));
				for (int j = 0; j < rows.Length; j++)
				{
					rows[j].RowError = rowError;
				}
				result = true;
			}
		}
		return result;
	}

	internal override bool CanEnableConstraint()
	{
		if (Table.DataSet == null || !Table.DataSet.EnforceConstraints)
		{
			return true;
		}
		object[] uniqueKeyValues = _childKey.GetSortIndex().GetUniqueKeyValues();
		Index sortIndex = _parentKey.GetSortIndex();
		for (int i = 0; i < uniqueKeyValues.Length; i++)
		{
			object[] array = (object[])uniqueKeyValues[i];
			if (!IsKeyNull(array) && !sortIndex.IsKeyInIndex(array))
			{
				return false;
			}
		}
		return true;
	}

	internal void CascadeCommit(DataRow row)
	{
		if (row.RowState == DataRowState.Detached || _acceptRejectRule != AcceptRejectRule.Cascade)
		{
			return;
		}
		Index sortIndex = _childKey.GetSortIndex((row.RowState == DataRowState.Deleted) ? DataViewRowState.Deleted : DataViewRowState.CurrentRows);
		object[] keyValues = row.GetKeyValues(_parentKey, (row.RowState == DataRowState.Deleted) ? DataRowVersion.Original : DataRowVersion.Default);
		if (IsKeyNull(keyValues))
		{
			return;
		}
		Range range = sortIndex.FindRecords(keyValues);
		if (range.IsNull)
		{
			return;
		}
		DataRow[] rows = sortIndex.GetRows(range);
		foreach (DataRow dataRow in rows)
		{
			if (DataRowState.Detached != dataRow.RowState && !dataRow._inCascade)
			{
				dataRow.AcceptChanges();
			}
		}
	}

	internal void CascadeDelete(DataRow row)
	{
		if (-1 == row._newRecord)
		{
			return;
		}
		object[] keyValues = row.GetKeyValues(_parentKey, DataRowVersion.Current);
		if (IsKeyNull(keyValues))
		{
			return;
		}
		Index sortIndex = _childKey.GetSortIndex();
		switch (DeleteRule)
		{
		case Rule.None:
			if (row.Table.DataSet.EnforceConstraints)
			{
				Range range2 = sortIndex.FindRecords(keyValues);
				if (!range2.IsNull && (range2.Count != 1 || sortIndex.GetRow(range2.Min) != row))
				{
					throw ExceptionBuilder.FailedCascadeDelete(ConstraintName);
				}
			}
			break;
		case Rule.Cascade:
		{
			object[] keyValues2 = row.GetKeyValues(_parentKey, DataRowVersion.Default);
			Range range4 = sortIndex.FindRecords(keyValues2);
			if (range4.IsNull)
			{
				break;
			}
			DataRow[] rows3 = sortIndex.GetRows(range4);
			foreach (DataRow dataRow in rows3)
			{
				if (!dataRow._inCascade)
				{
					dataRow.Table.DeleteRow(dataRow);
				}
			}
			break;
		}
		case Rule.SetNull:
		{
			object[] array2 = new object[_childKey.ColumnsReference.Length];
			for (int k = 0; k < _childKey.ColumnsReference.Length; k++)
			{
				array2[k] = DBNull.Value;
			}
			Range range3 = sortIndex.FindRecords(keyValues);
			if (range3.IsNull)
			{
				break;
			}
			DataRow[] rows2 = sortIndex.GetRows(range3);
			for (int l = 0; l < rows2.Length; l++)
			{
				if (row != rows2[l])
				{
					rows2[l].SetKeyValues(_childKey, array2);
				}
			}
			break;
		}
		case Rule.SetDefault:
		{
			object[] array = new object[_childKey.ColumnsReference.Length];
			for (int i = 0; i < _childKey.ColumnsReference.Length; i++)
			{
				array[i] = _childKey.ColumnsReference[i].DefaultValue;
			}
			Range range = sortIndex.FindRecords(keyValues);
			if (range.IsNull)
			{
				break;
			}
			DataRow[] rows = sortIndex.GetRows(range);
			for (int j = 0; j < rows.Length; j++)
			{
				if (row != rows[j])
				{
					rows[j].SetKeyValues(_childKey, array);
				}
			}
			break;
		}
		}
	}

	internal void CascadeRollback(DataRow row)
	{
		Index sortIndex = _childKey.GetSortIndex((row.RowState == DataRowState.Deleted) ? DataViewRowState.OriginalRows : DataViewRowState.CurrentRows);
		object[] keyValues = row.GetKeyValues(_parentKey, (row.RowState == DataRowState.Modified) ? DataRowVersion.Current : DataRowVersion.Default);
		if (IsKeyNull(keyValues))
		{
			return;
		}
		Range range = sortIndex.FindRecords(keyValues);
		if (_acceptRejectRule == AcceptRejectRule.Cascade)
		{
			if (range.IsNull)
			{
				return;
			}
			DataRow[] rows = sortIndex.GetRows(range);
			for (int i = 0; i < rows.Length; i++)
			{
				if (!rows[i]._inCascade)
				{
					rows[i].RejectChanges();
				}
			}
		}
		else if (row.RowState != DataRowState.Deleted && row.Table.DataSet.EnforceConstraints && !range.IsNull && (range.Count != 1 || sortIndex.GetRow(range.Min) != row) && row.HasKeyChanged(_parentKey))
		{
			throw ExceptionBuilder.FailedCascadeUpdate(ConstraintName);
		}
	}

	internal void CascadeUpdate(DataRow row)
	{
		if (-1 == row._newRecord)
		{
			return;
		}
		object[] keyValues = row.GetKeyValues(_parentKey, DataRowVersion.Current);
		if (!Table.DataSet._fInReadXml && IsKeyNull(keyValues))
		{
			return;
		}
		Index sortIndex = _childKey.GetSortIndex();
		switch (UpdateRule)
		{
		case Rule.None:
			if (row.Table.DataSet.EnforceConstraints && !sortIndex.FindRecords(keyValues).IsNull)
			{
				throw ExceptionBuilder.FailedCascadeUpdate(ConstraintName);
			}
			break;
		case Rule.Cascade:
		{
			Range range3 = sortIndex.FindRecords(keyValues);
			if (!range3.IsNull)
			{
				object[] keyValues2 = row.GetKeyValues(_parentKey, DataRowVersion.Proposed);
				DataRow[] rows3 = sortIndex.GetRows(range3);
				for (int m = 0; m < rows3.Length; m++)
				{
					rows3[m].SetKeyValues(_childKey, keyValues2);
				}
			}
			break;
		}
		case Rule.SetNull:
		{
			object[] array2 = new object[_childKey.ColumnsReference.Length];
			for (int k = 0; k < _childKey.ColumnsReference.Length; k++)
			{
				array2[k] = DBNull.Value;
			}
			Range range2 = sortIndex.FindRecords(keyValues);
			if (!range2.IsNull)
			{
				DataRow[] rows2 = sortIndex.GetRows(range2);
				for (int l = 0; l < rows2.Length; l++)
				{
					rows2[l].SetKeyValues(_childKey, array2);
				}
			}
			break;
		}
		case Rule.SetDefault:
		{
			object[] array = new object[_childKey.ColumnsReference.Length];
			for (int i = 0; i < _childKey.ColumnsReference.Length; i++)
			{
				array[i] = _childKey.ColumnsReference[i].DefaultValue;
			}
			Range range = sortIndex.FindRecords(keyValues);
			if (!range.IsNull)
			{
				DataRow[] rows = sortIndex.GetRows(range);
				for (int j = 0; j < rows.Length; j++)
				{
					rows[j].SetKeyValues(_childKey, array);
				}
			}
			break;
		}
		}
	}

	internal void CheckCanClearParentTable(DataTable table)
	{
		if (Table.DataSet.EnforceConstraints && Table.Rows.Count > 0)
		{
			throw ExceptionBuilder.FailedClearParentTable(table.TableName, ConstraintName, Table.TableName);
		}
	}

	internal void CheckCanRemoveParentRow(DataRow row)
	{
		if (!Table.DataSet.EnforceConstraints || DataRelation.GetChildRows(ParentKey, ChildKey, row, DataRowVersion.Default).Length == 0)
		{
			return;
		}
		throw ExceptionBuilder.RemoveParentRow(this);
	}

	internal void CheckCascade(DataRow row, DataRowAction action)
	{
		if (row._inCascade)
		{
			return;
		}
		row._inCascade = true;
		try
		{
			switch (action)
			{
			case DataRowAction.Change:
				if (row.HasKeyChanged(_parentKey))
				{
					CascadeUpdate(row);
				}
				break;
			case DataRowAction.Delete:
				CascadeDelete(row);
				break;
			case DataRowAction.Commit:
				CascadeCommit(row);
				break;
			case DataRowAction.Rollback:
				CascadeRollback(row);
				break;
			default:
				_ = 16;
				break;
			}
		}
		finally
		{
			row._inCascade = false;
		}
	}

	internal override void CheckConstraint(DataRow childRow, DataRowAction action)
	{
		if ((action != DataRowAction.Change && action != DataRowAction.Add && action != DataRowAction.Rollback) || Table.DataSet == null || !Table.DataSet.EnforceConstraints || !childRow.HasKeyChanged(_childKey))
		{
			return;
		}
		DataRowVersion dataRowVersion = ((action == DataRowAction.Rollback) ? DataRowVersion.Original : DataRowVersion.Current);
		object[] keyValues = childRow.GetKeyValues(_childKey);
		if (childRow.HasVersion(dataRowVersion))
		{
			DataRow parentRow = DataRelation.GetParentRow(ParentKey, ChildKey, childRow, dataRowVersion);
			if (parentRow != null && parentRow._inCascade)
			{
				object[] keyValues2 = parentRow.GetKeyValues(_parentKey, (action == DataRowAction.Rollback) ? dataRowVersion : DataRowVersion.Default);
				int num = childRow.Table.NewRecord();
				childRow.Table.SetKeyValues(_childKey, keyValues2, num);
				if (_childKey.RecordsEqual(childRow._tempRecord, num))
				{
					return;
				}
			}
		}
		object[] keyValues3 = childRow.GetKeyValues(_childKey);
		if (IsKeyNull(keyValues3) || _parentKey.GetSortIndex().IsKeyInIndex(keyValues3))
		{
			return;
		}
		if (_childKey.Table == _parentKey.Table && childRow._tempRecord != -1)
		{
			int num2 = 0;
			for (num2 = 0; num2 < keyValues3.Length; num2++)
			{
				DataColumn dataColumn = _parentKey.ColumnsReference[num2];
				object value = dataColumn.ConvertValue(keyValues3[num2]);
				if (dataColumn.CompareValueTo(childRow._tempRecord, value) != 0)
				{
					break;
				}
			}
			if (num2 == keyValues3.Length)
			{
				return;
			}
		}
		throw ExceptionBuilder.ForeignKeyViolation(ConstraintName, keyValues);
	}

	private void NonVirtualCheckState()
	{
		if (_DataSet != null)
		{
			return;
		}
		_parentKey.CheckState();
		_childKey.CheckState();
		if (_parentKey.Table.DataSet != _childKey.Table.DataSet)
		{
			throw ExceptionBuilder.TablesInDifferentSets();
		}
		for (int i = 0; i < _parentKey.ColumnsReference.Length; i++)
		{
			if (_parentKey.ColumnsReference[i].DataType != _childKey.ColumnsReference[i].DataType || (_parentKey.ColumnsReference[i].DataType == typeof(DateTime) && _parentKey.ColumnsReference[i].DateTimeMode != _childKey.ColumnsReference[i].DateTimeMode && (_parentKey.ColumnsReference[i].DateTimeMode & _childKey.ColumnsReference[i].DateTimeMode) != DataSetDateTime.Unspecified))
			{
				throw ExceptionBuilder.ColumnsTypeMismatch();
			}
		}
		if (_childKey.ColumnsEqual(_parentKey))
		{
			throw ExceptionBuilder.KeyColumnsIdentical();
		}
	}

	internal override void CheckState()
	{
		NonVirtualCheckState();
	}

	internal override bool ContainsColumn(DataColumn column)
	{
		if (!_parentKey.ContainsColumn(column))
		{
			return _childKey.ContainsColumn(column);
		}
		return true;
	}

	internal override Constraint Clone(DataSet destination)
	{
		return Clone(destination, ignorNSforTableLookup: false);
	}

	internal override Constraint Clone(DataSet destination, bool ignorNSforTableLookup)
	{
		int num = ((!ignorNSforTableLookup) ? destination.Tables.IndexOf(Table.TableName, Table.Namespace, chekforNull: false) : destination.Tables.IndexOf(Table.TableName));
		if (num < 0)
		{
			return null;
		}
		DataTable dataTable = destination.Tables[num];
		num = ((!ignorNSforTableLookup) ? destination.Tables.IndexOf(RelatedTable.TableName, RelatedTable.Namespace, chekforNull: false) : destination.Tables.IndexOf(RelatedTable.TableName));
		if (num < 0)
		{
			return null;
		}
		DataTable dataTable2 = destination.Tables[num];
		int num2 = Columns.Length;
		DataColumn[] array = new DataColumn[num2];
		DataColumn[] array2 = new DataColumn[num2];
		for (int i = 0; i < num2; i++)
		{
			DataColumn dataColumn = Columns[i];
			num = dataTable.Columns.IndexOf(dataColumn.ColumnName);
			if (num < 0)
			{
				return null;
			}
			array[i] = dataTable.Columns[num];
			dataColumn = RelatedColumnsReference[i];
			num = dataTable2.Columns.IndexOf(dataColumn.ColumnName);
			if (num < 0)
			{
				return null;
			}
			array2[i] = dataTable2.Columns[num];
		}
		ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint(ConstraintName, array2, array);
		foreignKeyConstraint.UpdateRule = UpdateRule;
		foreignKeyConstraint.DeleteRule = DeleteRule;
		foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule;
		foreach (object key in base.ExtendedProperties.Keys)
		{
			foreignKeyConstraint.ExtendedProperties[key] = base.ExtendedProperties[key];
		}
		return foreignKeyConstraint;
	}

	internal ForeignKeyConstraint Clone(DataTable destination)
	{
		int num = Columns.Length;
		DataColumn[] array = new DataColumn[num];
		DataColumn[] array2 = new DataColumn[num];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			DataColumn dataColumn = Columns[i];
			num2 = destination.Columns.IndexOf(dataColumn.ColumnName);
			if (num2 < 0)
			{
				return null;
			}
			array[i] = destination.Columns[num2];
			dataColumn = RelatedColumnsReference[i];
			num2 = destination.Columns.IndexOf(dataColumn.ColumnName);
			if (num2 < 0)
			{
				return null;
			}
			array2[i] = destination.Columns[num2];
		}
		ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint(ConstraintName, array2, array);
		foreignKeyConstraint.UpdateRule = UpdateRule;
		foreignKeyConstraint.DeleteRule = DeleteRule;
		foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule;
		foreach (object key in base.ExtendedProperties.Keys)
		{
			foreignKeyConstraint.ExtendedProperties[key] = base.ExtendedProperties[key];
		}
		return foreignKeyConstraint;
	}

	private void Create(string relationName, DataColumn[] parentColumns, DataColumn[] childColumns)
	{
		if (parentColumns.Length == 0 || childColumns.Length == 0)
		{
			throw ExceptionBuilder.KeyLengthZero();
		}
		if (parentColumns.Length != childColumns.Length)
		{
			throw ExceptionBuilder.KeyLengthMismatch();
		}
		for (int i = 0; i < parentColumns.Length; i++)
		{
			if (parentColumns[i].Computed)
			{
				throw ExceptionBuilder.ExpressionInConstraint(parentColumns[i]);
			}
			if (childColumns[i].Computed)
			{
				throw ExceptionBuilder.ExpressionInConstraint(childColumns[i]);
			}
		}
		_parentKey = new DataKey(parentColumns, copyColumns: true);
		_childKey = new DataKey(childColumns, copyColumns: true);
		ConstraintName = relationName;
		NonVirtualCheckState();
	}

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Data.ForeignKeyConstraint" /> is identical to the specified object.</summary>
	/// <returns>true, if the objects are identical; otherwise, false.</returns>
	/// <param name="key">The object to which this <see cref="T:System.Data.ForeignKeyConstraint" /> is compared. Two <see cref="T:System.Data.ForeignKeyConstraint" /> are equal if they constrain the same columns. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object key)
	{
		if (!(key is ForeignKeyConstraint))
		{
			return false;
		}
		ForeignKeyConstraint foreignKeyConstraint = (ForeignKeyConstraint)key;
		if (ParentKey.ColumnsEqual(foreignKeyConstraint.ParentKey))
		{
			return ChildKey.ColumnsEqual(foreignKeyConstraint.ChildKey);
		}
		return false;
	}

	/// <summary>Gets the hash code of this instance of the <see cref="T:System.Data.ForeignKeyConstraint" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	internal DataRelation FindParentRelation()
	{
		DataRelationCollection parentRelations = Table.ParentRelations;
		for (int i = 0; i < parentRelations.Count; i++)
		{
			if (parentRelations[i].ChildKeyConstraint == this)
			{
				return parentRelations[i];
			}
		}
		return null;
	}
}
