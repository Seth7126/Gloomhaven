using System.ComponentModel;
using System.Diagnostics;

namespace System.Data;

/// <summary>Represents a restriction on a set of columns in which all values must be unique.</summary>
/// <filterpriority>1</filterpriority>
[DefaultProperty("ConstraintName")]
public class UniqueConstraint : Constraint
{
	private DataKey _key;

	private Index _constraintIndex;

	internal bool _bPrimaryKey;

	internal string _constraintName;

	internal string[] _columnNames;

	internal string[] ColumnNames => _key.GetColumnNames();

	internal Index ConstraintIndex => _constraintIndex;

	/// <summary>Gets the array of columns that this constraint affects.</summary>
	/// <returns>An array of <see cref="T:System.Data.DataColumn" /> objects.</returns>
	/// <filterpriority>1</filterpriority>
	[ReadOnly(true)]
	public virtual DataColumn[] Columns => _key.ToArray();

	internal DataColumn[] ColumnsReference => _key.ColumnsReference;

	/// <summary>Gets a value indicating whether or not the constraint is on a primary key.</summary>
	/// <returns>true, if the constraint is on a primary key; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public bool IsPrimaryKey
	{
		get
		{
			if (Table == null)
			{
				return false;
			}
			return this == Table._primaryKey;
		}
	}

	internal override bool InCollection
	{
		set
		{
			base.InCollection = value;
			if (_key.ColumnsReference.Length == 1)
			{
				_key.ColumnsReference[0].InternalUnique(value);
			}
		}
	}

	internal DataKey Key => _key;

	/// <summary>Gets the table to which this constraint belongs.</summary>
	/// <returns>The <see cref="T:System.Data.DataTable" /> to which the constraint belongs.</returns>
	/// <filterpriority>1</filterpriority>
	[ReadOnly(true)]
	public override DataTable Table
	{
		get
		{
			if (_key.HasValue)
			{
				return _key.Table;
			}
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.UniqueConstraint" /> class with the specified name and <see cref="T:System.Data.DataColumn" />.</summary>
	/// <param name="name">The name of the constraint. </param>
	/// <param name="column">The <see cref="T:System.Data.DataColumn" /> to constrain. </param>
	public UniqueConstraint(string name, DataColumn column)
	{
		Create(name, new DataColumn[1] { column });
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.UniqueConstraint" /> class with the specified <see cref="T:System.Data.DataColumn" />.</summary>
	/// <param name="column">The <see cref="T:System.Data.DataColumn" /> to constrain. </param>
	public UniqueConstraint(DataColumn column)
	{
		Create(null, new DataColumn[1] { column });
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.UniqueConstraint" /> class with the specified name and array of <see cref="T:System.Data.DataColumn" /> objects.</summary>
	/// <param name="name">The name of the constraint. </param>
	/// <param name="columns">The array of <see cref="T:System.Data.DataColumn" /> objects to constrain. </param>
	public UniqueConstraint(string name, DataColumn[] columns)
	{
		Create(name, columns);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.UniqueConstraint" /> class with the given array of <see cref="T:System.Data.DataColumn" /> objects.</summary>
	/// <param name="columns">The array of <see cref="T:System.Data.DataColumn" /> objects to constrain. </param>
	public UniqueConstraint(DataColumn[] columns)
	{
		Create(null, columns);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.UniqueConstraint" /> class with the specified name, an array of <see cref="T:System.Data.DataColumn" /> objects to constrain, and a value specifying whether the constraint is a primary key.</summary>
	/// <param name="name">The name of the constraint. </param>
	/// <param name="columnNames">An array of <see cref="T:System.Data.DataColumn" /> objects to constrain. </param>
	/// <param name="isPrimaryKey">true to indicate that the constraint is a primary key; otherwise, false. </param>
	[Browsable(false)]
	public UniqueConstraint(string name, string[] columnNames, bool isPrimaryKey)
	{
		_constraintName = name;
		_columnNames = columnNames;
		_bPrimaryKey = isPrimaryKey;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.UniqueConstraint" /> class with the specified name, the <see cref="T:System.Data.DataColumn" /> to constrain, and a value specifying whether the constraint is a primary key.</summary>
	/// <param name="name">The name of the constraint. </param>
	/// <param name="column">The <see cref="T:System.Data.DataColumn" /> to constrain. </param>
	/// <param name="isPrimaryKey">true to indicate that the constraint is a primary key; otherwise, false. </param>
	public UniqueConstraint(string name, DataColumn column, bool isPrimaryKey)
	{
		DataColumn[] columns = new DataColumn[1] { column };
		_bPrimaryKey = isPrimaryKey;
		Create(name, columns);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.UniqueConstraint" /> class with the <see cref="T:System.Data.DataColumn" /> to constrain, and a value specifying whether the constraint is a primary key.</summary>
	/// <param name="column">The <see cref="T:System.Data.DataColumn" /> to constrain. </param>
	/// <param name="isPrimaryKey">true to indicate that the constraint is a primary key; otherwise, false. </param>
	public UniqueConstraint(DataColumn column, bool isPrimaryKey)
	{
		DataColumn[] columns = new DataColumn[1] { column };
		_bPrimaryKey = isPrimaryKey;
		Create(null, columns);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.UniqueConstraint" /> class with the specified name, an array of <see cref="T:System.Data.DataColumn" /> objects to constrain, and a value specifying whether the constraint is a primary key.</summary>
	/// <param name="name">The name of the constraint. </param>
	/// <param name="columns">An array of <see cref="T:System.Data.DataColumn" /> objects to constrain. </param>
	/// <param name="isPrimaryKey">true to indicate that the constraint is a primary key; otherwise, false. </param>
	public UniqueConstraint(string name, DataColumn[] columns, bool isPrimaryKey)
	{
		_bPrimaryKey = isPrimaryKey;
		Create(name, columns);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.UniqueConstraint" /> class with an array of <see cref="T:System.Data.DataColumn" /> objects to constrain, and a value specifying whether the constraint is a primary key.</summary>
	/// <param name="columns">An array of <see cref="T:System.Data.DataColumn" /> objects to constrain. </param>
	/// <param name="isPrimaryKey">true to indicate that the constraint is a primary key; otherwise, false. </param>
	public UniqueConstraint(DataColumn[] columns, bool isPrimaryKey)
	{
		_bPrimaryKey = isPrimaryKey;
		Create(null, columns);
	}

	[Conditional("DEBUG")]
	private void AssertConstraintAndKeyIndexes()
	{
		DataColumn[] array = new DataColumn[_constraintIndex._indexFields.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _constraintIndex._indexFields[i].Column;
		}
	}

	internal void ConstraintIndexClear()
	{
		if (_constraintIndex != null)
		{
			_constraintIndex.RemoveRef();
			_constraintIndex = null;
		}
	}

	internal void ConstraintIndexInitialize()
	{
		if (_constraintIndex == null)
		{
			_constraintIndex = _key.GetSortIndex();
			_constraintIndex.AddRef();
		}
	}

	internal override void CheckState()
	{
		NonVirtualCheckState();
	}

	private void NonVirtualCheckState()
	{
		_key.CheckState();
	}

	internal override void CheckCanAddToCollection(ConstraintCollection constraints)
	{
	}

	internal override bool CanBeRemovedFromCollection(ConstraintCollection constraints, bool fThrowException)
	{
		if (Equals(constraints.Table._primaryKey))
		{
			if (!fThrowException)
			{
				return false;
			}
			throw ExceptionBuilder.RemovePrimaryKey(constraints.Table);
		}
		ParentForeignKeyConstraintEnumerator parentForeignKeyConstraintEnumerator = new ParentForeignKeyConstraintEnumerator(Table.DataSet, Table);
		while (parentForeignKeyConstraintEnumerator.GetNext())
		{
			ForeignKeyConstraint foreignKeyConstraint = parentForeignKeyConstraintEnumerator.GetForeignKeyConstraint();
			if (_key.ColumnsEqual(foreignKeyConstraint.ParentKey))
			{
				if (!fThrowException)
				{
					return false;
				}
				throw ExceptionBuilder.NeededForForeignKeyConstraint(this, foreignKeyConstraint);
			}
		}
		return true;
	}

	internal override bool CanEnableConstraint()
	{
		if (Table.EnforceConstraints)
		{
			return ConstraintIndex.CheckUnique();
		}
		return true;
	}

	internal override bool IsConstraintViolated()
	{
		bool result = false;
		Index constraintIndex = ConstraintIndex;
		if (constraintIndex.HasDuplicates)
		{
			object[] uniqueKeyValues = constraintIndex.GetUniqueKeyValues();
			for (int i = 0; i < uniqueKeyValues.Length; i++)
			{
				Range range = constraintIndex.FindRecords((object[])uniqueKeyValues[i]);
				if (1 >= range.Count)
				{
					continue;
				}
				DataRow[] rows = constraintIndex.GetRows(range);
				string text = ExceptionBuilder.UniqueConstraintViolationText(_key.ColumnsReference, (object[])uniqueKeyValues[i]);
				for (int j = 0; j < rows.Length; j++)
				{
					rows[j].RowError = text;
					DataColumn[] columnsReference = _key.ColumnsReference;
					foreach (DataColumn column in columnsReference)
					{
						rows[j].SetColumnError(column, text);
					}
				}
				result = true;
			}
		}
		return result;
	}

	internal override void CheckConstraint(DataRow row, DataRowAction action)
	{
		if (!Table.EnforceConstraints)
		{
			return;
		}
		switch (action)
		{
		case DataRowAction.Rollback:
			if (row._tempRecord == -1)
			{
				break;
			}
			goto case DataRowAction.Change;
		case DataRowAction.Change:
		case DataRowAction.Add:
			if (row.HaveValuesChanged(ColumnsReference) && ConstraintIndex.IsKeyRecordInIndex(row.GetDefaultRecord()))
			{
				object[] columnValues = row.GetColumnValues(ColumnsReference);
				throw ExceptionBuilder.ConstraintViolation(ColumnsReference, columnValues);
			}
			break;
		}
	}

	internal override bool ContainsColumn(DataColumn column)
	{
		return _key.ContainsColumn(column);
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
		int num2 = ColumnsReference.Length;
		DataColumn[] array = new DataColumn[num2];
		for (int i = 0; i < num2; i++)
		{
			DataColumn dataColumn = ColumnsReference[i];
			num = dataTable.Columns.IndexOf(dataColumn.ColumnName);
			if (num < 0)
			{
				return null;
			}
			array[i] = dataTable.Columns[num];
		}
		UniqueConstraint uniqueConstraint = new UniqueConstraint(ConstraintName, array);
		foreach (object key in base.ExtendedProperties.Keys)
		{
			uniqueConstraint.ExtendedProperties[key] = base.ExtendedProperties[key];
		}
		return uniqueConstraint;
	}

	internal UniqueConstraint Clone(DataTable table)
	{
		int num = ColumnsReference.Length;
		DataColumn[] array = new DataColumn[num];
		for (int i = 0; i < num; i++)
		{
			DataColumn dataColumn = ColumnsReference[i];
			int num2 = table.Columns.IndexOf(dataColumn.ColumnName);
			if (num2 < 0)
			{
				return null;
			}
			array[i] = table.Columns[num2];
		}
		UniqueConstraint uniqueConstraint = new UniqueConstraint(ConstraintName, array);
		foreach (object key in base.ExtendedProperties.Keys)
		{
			uniqueConstraint.ExtendedProperties[key] = base.ExtendedProperties[key];
		}
		return uniqueConstraint;
	}

	private void Create(string constraintName, DataColumn[] columns)
	{
		for (int i = 0; i < columns.Length; i++)
		{
			if (columns[i].Computed)
			{
				throw ExceptionBuilder.ExpressionInConstraint(columns[i]);
			}
		}
		_key = new DataKey(columns, copyColumns: true);
		ConstraintName = constraintName;
		NonVirtualCheckState();
	}

	/// <summary>Compares this constraint to a second to determine if both are identical.</summary>
	/// <returns>true, if the contraints are equal; otherwise, false.</returns>
	/// <param name="key2">The object to which this <see cref="T:System.Data.UniqueConstraint" /> is compared. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object key2)
	{
		if (!(key2 is UniqueConstraint))
		{
			return false;
		}
		return Key.ColumnsEqual(((UniqueConstraint)key2).Key);
	}

	/// <summary>Gets the hash code of this instance of the <see cref="T:System.Data.UniqueConstraint" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
