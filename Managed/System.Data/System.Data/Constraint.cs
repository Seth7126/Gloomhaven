using System.ComponentModel;
using System.Data.Common;
using System.Globalization;

namespace System.Data;

/// <summary>Represents a constraint that can be enforced on one or more <see cref="T:System.Data.DataColumn" /> objects.</summary>
/// <filterpriority>1</filterpriority>
[DefaultProperty("ConstraintName")]
[TypeConverter(typeof(ConstraintConverter))]
public abstract class Constraint
{
	private string _schemaName = string.Empty;

	private bool _inCollection;

	private DataSet _dataSet;

	internal string _name = string.Empty;

	internal PropertyCollection _extendedProperties;

	/// <summary>The name of a constraint in the <see cref="T:System.Data.ConstraintCollection" />.</summary>
	/// <returns>The name of the <see cref="T:System.Data.Constraint" />.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Data.Constraint" /> name is a null value or empty string. </exception>
	/// <exception cref="T:System.Data.DuplicateNameException">The <see cref="T:System.Data.ConstraintCollection" /> already contains a <see cref="T:System.Data.Constraint" /> with the same name (The comparison is not case-sensitive.). </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DefaultValue("")]
	public virtual string ConstraintName
	{
		get
		{
			return _name;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			if (string.IsNullOrEmpty(value) && Table != null && InCollection)
			{
				throw ExceptionBuilder.NoConstraintName();
			}
			CultureInfo culture = ((Table != null) ? Table.Locale : CultureInfo.CurrentCulture);
			if (string.Compare(_name, value, ignoreCase: true, culture) != 0)
			{
				if (Table != null && InCollection)
				{
					Table.Constraints.RegisterName(value);
					if (_name.Length != 0)
					{
						Table.Constraints.UnregisterName(_name);
					}
				}
				_name = value;
			}
			else if (string.Compare(_name, value, ignoreCase: false, culture) != 0)
			{
				_name = value;
			}
		}
	}

	internal string SchemaName
	{
		get
		{
			if (!string.IsNullOrEmpty(_schemaName))
			{
				return _schemaName;
			}
			return ConstraintName;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				_schemaName = value;
			}
		}
	}

	internal virtual bool InCollection
	{
		get
		{
			return _inCollection;
		}
		set
		{
			_inCollection = value;
			_dataSet = (value ? Table.DataSet : null);
		}
	}

	/// <summary>Gets the <see cref="T:System.Data.DataTable" /> to which the constraint applies.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> to which the constraint applies.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract DataTable Table { get; }

	/// <summary>Gets the collection of user-defined constraint properties.</summary>
	/// <returns>A <see cref="T:System.Data.PropertyCollection" /> of custom information.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public PropertyCollection ExtendedProperties => _extendedProperties ?? (_extendedProperties = new PropertyCollection());

	/// <summary>Gets the <see cref="T:System.Data.DataSet" /> to which this constraint belongs.</summary>
	/// <returns>The <see cref="T:System.Data.DataSet" /> to which the constraint belongs.</returns>
	[CLSCompliant(false)]
	protected virtual DataSet _DataSet => _dataSet;

	internal abstract bool ContainsColumn(DataColumn column);

	internal abstract bool CanEnableConstraint();

	internal abstract Constraint Clone(DataSet destination);

	internal abstract Constraint Clone(DataSet destination, bool ignoreNSforTableLookup);

	internal void CheckConstraint()
	{
		if (!CanEnableConstraint())
		{
			throw ExceptionBuilder.ConstraintViolation(ConstraintName);
		}
	}

	internal abstract void CheckCanAddToCollection(ConstraintCollection constraint);

	internal abstract bool CanBeRemovedFromCollection(ConstraintCollection constraint, bool fThrowException);

	internal abstract void CheckConstraint(DataRow row, DataRowAction action);

	internal abstract void CheckState();

	/// <summary>Gets the <see cref="T:System.Data.DataSet" /> to which this constraint belongs.</summary>
	protected void CheckStateForProperty()
	{
		try
		{
			CheckState();
		}
		catch (Exception ex) when (ADP.IsCatchableExceptionType(ex))
		{
			throw ExceptionBuilder.BadObjectPropertyAccess(ex.Message);
		}
	}

	/// <summary>Sets the constraint's <see cref="T:System.Data.DataSet" />.</summary>
	/// <param name="dataSet">The <see cref="T:System.Data.DataSet" /> to which this constraint will belong. </param>
	protected internal void SetDataSet(DataSet dataSet)
	{
		_dataSet = dataSet;
	}

	internal abstract bool IsConstraintViolated();

	/// <summary>Gets the <see cref="P:System.Data.Constraint.ConstraintName" />, if there is one, as a string.</summary>
	/// <returns>The string value of the <see cref="P:System.Data.Constraint.ConstraintName" />.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		return ConstraintName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Constraint" /> class.</summary>
	protected Constraint()
	{
	}
}
