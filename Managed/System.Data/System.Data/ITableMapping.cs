namespace System.Data;

/// <summary>Associates a source table with a table in a <see cref="T:System.Data.DataSet" />, and is implemented by the <see cref="T:System.Data.Common.DataTableMapping" /> class, which is used in common by .NET Framework data providers.</summary>
/// <filterpriority>2</filterpriority>
public interface ITableMapping
{
	/// <summary>Gets the derived <see cref="T:System.Data.Common.DataColumnMappingCollection" /> for the <see cref="T:System.Data.DataTable" />.</summary>
	/// <returns>A collection of data column mappings.</returns>
	/// <filterpriority>2</filterpriority>
	IColumnMappingCollection ColumnMappings { get; }

	/// <summary>Gets or sets the case-insensitive name of the table within the <see cref="T:System.Data.DataSet" />.</summary>
	/// <returns>The case-insensitive name of the table within the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>2</filterpriority>
	string DataSetTable { get; set; }

	/// <summary>Gets or sets the case-sensitive name of the source table.</summary>
	/// <returns>The case-sensitive name of the source table.</returns>
	/// <filterpriority>2</filterpriority>
	string SourceTable { get; set; }
}
