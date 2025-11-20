using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Data.Common;

/// <summary>Contains a description of a mapped relationship between a source table and a <see cref="T:System.Data.DataTable" />. This class is used by a <see cref="T:System.Data.Common.DataAdapter" /> when populating a <see cref="T:System.Data.DataSet" />.</summary>
/// <filterpriority>1</filterpriority>
[TypeConverter(typeof(DataTableMappingConverter))]
public sealed class DataTableMapping : MarshalByRefObject, ITableMapping, ICloneable
{
	internal sealed class DataTableMappingConverter : ExpandableObjectConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (typeof(InstanceDescriptor) == destinationType)
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (null == destinationType)
			{
				throw ADP.ArgumentNull("destinationType");
			}
			if (typeof(InstanceDescriptor) == destinationType && value is DataTableMapping)
			{
				DataTableMapping dataTableMapping = (DataTableMapping)value;
				DataColumnMapping[] array = new DataColumnMapping[dataTableMapping.ColumnMappings.Count];
				dataTableMapping.ColumnMappings.CopyTo(array, 0);
				object[] arguments = new object[3] { dataTableMapping.SourceTable, dataTableMapping.DataSetTable, array };
				Type[] types = new Type[3]
				{
					typeof(string),
					typeof(string),
					typeof(DataColumnMapping[])
				};
				return new InstanceDescriptor(typeof(DataTableMapping).GetConstructor(types), arguments);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	private DataTableMappingCollection _parent;

	private DataColumnMappingCollection _columnMappings;

	private string _dataSetTableName;

	private string _sourceTableName;

	/// <summary>Gets the derived <see cref="T:System.Data.Common.DataColumnMappingCollection" /> for the <see cref="T:System.Data.DataTable" />.</summary>
	/// <returns>A data column mapping collection.</returns>
	IColumnMappingCollection ITableMapping.ColumnMappings => ColumnMappings;

	/// <summary>Gets the <see cref="T:System.Data.Common.DataColumnMappingCollection" /> for the <see cref="T:System.Data.DataTable" />.</summary>
	/// <returns>A data column mapping collection.</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public DataColumnMappingCollection ColumnMappings
	{
		get
		{
			DataColumnMappingCollection dataColumnMappingCollection = _columnMappings;
			if (dataColumnMappingCollection == null)
			{
				dataColumnMappingCollection = (_columnMappings = new DataColumnMappingCollection());
			}
			return dataColumnMappingCollection;
		}
	}

	/// <summary>Gets or sets the table name from a <see cref="T:System.Data.DataSet" />.</summary>
	/// <returns>The table name from a <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	[DefaultValue("")]
	public string DataSetTable
	{
		get
		{
			return _dataSetTableName ?? string.Empty;
		}
		set
		{
			_dataSetTableName = value;
		}
	}

	internal DataTableMappingCollection Parent
	{
		get
		{
			return _parent;
		}
		set
		{
			_parent = value;
		}
	}

	/// <summary>Gets or sets the case-sensitive source table name from a data source.</summary>
	/// <returns>The case-sensitive source table name from a data source.</returns>
	/// <filterpriority>1</filterpriority>
	[DefaultValue("")]
	public string SourceTable
	{
		get
		{
			return _sourceTableName ?? string.Empty;
		}
		set
		{
			if (Parent != null && ADP.SrcCompare(_sourceTableName, value) != 0)
			{
				Parent.ValidateSourceTable(-1, value);
			}
			_sourceTableName = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DataTableMapping" /> class.</summary>
	public DataTableMapping()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DataTableMapping" /> class with a source when given a source table name and a <see cref="T:System.Data.DataTable" /> name.</summary>
	/// <param name="sourceTable">The case-sensitive source table name from a data source. </param>
	/// <param name="dataSetTable">The table name from a <see cref="T:System.Data.DataSet" /> to map to. </param>
	public DataTableMapping(string sourceTable, string dataSetTable)
	{
		SourceTable = sourceTable;
		DataSetTable = dataSetTable;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DataTableMapping" /> class when given a source table name, a <see cref="T:System.Data.DataTable" /> name, and an array of <see cref="T:System.Data.Common.DataColumnMapping" /> objects.</summary>
	/// <param name="sourceTable">The case-sensitive source table name from a data source. </param>
	/// <param name="dataSetTable">The table name from a <see cref="T:System.Data.DataSet" /> to map to. </param>
	/// <param name="columnMappings">An array of <see cref="T:System.Data.Common.DataColumnMapping" /> objects. </param>
	public DataTableMapping(string sourceTable, string dataSetTable, DataColumnMapping[] columnMappings)
	{
		SourceTable = sourceTable;
		DataSetTable = dataSetTable;
		if (columnMappings != null && columnMappings.Length != 0)
		{
			ColumnMappings.AddRange(columnMappings);
		}
	}

	/// <summary>Creates a new object that is a copy of the current instance.</summary>
	/// <returns>A new object that is a copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		DataTableMapping dataTableMapping = new DataTableMapping();
		dataTableMapping._dataSetTableName = _dataSetTableName;
		dataTableMapping._sourceTableName = _sourceTableName;
		if (_columnMappings != null && 0 < ColumnMappings.Count)
		{
			DataColumnMappingCollection columnMappings = dataTableMapping.ColumnMappings;
			foreach (ICloneable columnMapping in ColumnMappings)
			{
				columnMappings.Add(columnMapping.Clone());
			}
		}
		return dataTableMapping;
	}

	/// <summary>Returns a <see cref="T:System.Data.DataColumn" /> object for a given column name.</summary>
	/// <returns>A <see cref="T:System.Data.DataColumn" /> object.</returns>
	/// <param name="sourceColumn">The name of the <see cref="T:System.Data.DataColumn" />. </param>
	/// <param name="dataType">The data type for <paramref name="sourceColumn" />.</param>
	/// <param name="dataTable">The table name from a <see cref="T:System.Data.DataSet" /> to map to. </param>
	/// <param name="mappingAction">One of the <see cref="T:System.Data.MissingMappingAction" /> values. </param>
	/// <param name="schemaAction">One of the <see cref="T:System.Data.MissingSchemaAction" /> values. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public DataColumn GetDataColumn(string sourceColumn, Type dataType, DataTable dataTable, MissingMappingAction mappingAction, MissingSchemaAction schemaAction)
	{
		return DataColumnMappingCollection.GetDataColumn(_columnMappings, sourceColumn, dataType, dataTable, mappingAction, schemaAction);
	}

	/// <summary>Gets a <see cref="T:System.Data.DataColumn" /> from the specified <see cref="T:System.Data.DataTable" /> using the specified <see cref="T:System.Data.MissingMappingAction" /> value and the name of the <see cref="T:System.Data.DataColumn" />.</summary>
	/// <returns>A data column.</returns>
	/// <param name="sourceColumn">The name of the <see cref="T:System.Data.DataColumn" />. </param>
	/// <param name="mappingAction">One of the <see cref="T:System.Data.MissingMappingAction" /> values. </param>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="mappingAction" /> parameter was set to Error, and no mapping was specified. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public DataColumnMapping GetColumnMappingBySchemaAction(string sourceColumn, MissingMappingAction mappingAction)
	{
		return DataColumnMappingCollection.GetColumnMappingBySchemaAction(_columnMappings, sourceColumn, mappingAction);
	}

	/// <summary>Gets the current <see cref="T:System.Data.DataTable" /> for the specified <see cref="T:System.Data.DataSet" /> using the specified <see cref="T:System.Data.MissingSchemaAction" /> value.</summary>
	/// <returns>A data table.</returns>
	/// <param name="dataSet">The <see cref="T:System.Data.DataSet" /> from which to get the <see cref="T:System.Data.DataTable" />. </param>
	/// <param name="schemaAction">One of the <see cref="T:System.Data.MissingSchemaAction" /> values. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public DataTable GetDataTableBySchemaAction(DataSet dataSet, MissingSchemaAction schemaAction)
	{
		if (dataSet == null)
		{
			throw ADP.ArgumentNull("dataSet");
		}
		string dataSetTable = DataSetTable;
		if (string.IsNullOrEmpty(dataSetTable))
		{
			return null;
		}
		DataTableCollection tables = dataSet.Tables;
		int num = tables.IndexOf(dataSetTable);
		if (0 <= num && num < tables.Count)
		{
			return tables[num];
		}
		switch (schemaAction)
		{
		case MissingSchemaAction.Add:
		case MissingSchemaAction.AddWithKey:
			return new DataTable(dataSetTable);
		case MissingSchemaAction.Ignore:
			return null;
		case MissingSchemaAction.Error:
			throw ADP.MissingTableSchema(dataSetTable, SourceTable);
		default:
			throw ADP.InvalidMissingSchemaAction(schemaAction);
		}
	}

	/// <summary>Converts the current <see cref="P:System.Data.Common.DataTableMapping.SourceTable" /> name to a string.</summary>
	/// <returns>The current <see cref="P:System.Data.Common.DataTableMapping.SourceTable" /> name, as a string.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		return SourceTable;
	}
}
