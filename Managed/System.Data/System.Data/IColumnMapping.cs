namespace System.Data;

/// <summary>Associates a data source column with a <see cref="T:System.Data.DataSet" /> column, and is implemented by the <see cref="T:System.Data.Common.DataColumnMapping" /> class, which is used in common by .NET Framework data providers.</summary>
/// <filterpriority>2</filterpriority>
public interface IColumnMapping
{
	/// <summary>Gets or sets the name of the column within the <see cref="T:System.Data.DataSet" /> to map to.</summary>
	/// <returns>The name of the column within the <see cref="T:System.Data.DataSet" /> to map to. The name is not case sensitive.</returns>
	/// <filterpriority>2</filterpriority>
	string DataSetColumn { get; set; }

	/// <summary>Gets or sets the name of the column within the data source to map from. The name is case-sensitive.</summary>
	/// <returns>The case-sensitive name of the column in the data source.</returns>
	/// <filterpriority>2</filterpriority>
	string SourceColumn { get; set; }
}
