namespace System.Data;

/// <summary>Represents a parameter to a Command object, and optionally, its mapping to <see cref="T:System.Data.DataSet" /> columns; and is implemented by .NET Framework data providers that access data sources.</summary>
/// <filterpriority>2</filterpriority>
public interface IDataParameter
{
	/// <summary>Gets or sets the <see cref="T:System.Data.DbType" /> of the parameter.</summary>
	/// <returns>One of the <see cref="T:System.Data.DbType" /> values. The default is <see cref="F:System.Data.DbType.String" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property was not set to a valid <see cref="T:System.Data.DbType" />. </exception>
	/// <filterpriority>2</filterpriority>
	DbType DbType { get; set; }

	/// <summary>Gets or sets a value indicating whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.</summary>
	/// <returns>One of the <see cref="T:System.Data.ParameterDirection" /> values. The default is Input.</returns>
	/// <exception cref="T:System.ArgumentException">The property was not set to one of the valid <see cref="T:System.Data.ParameterDirection" /> values. </exception>
	/// <filterpriority>2</filterpriority>
	ParameterDirection Direction { get; set; }

	/// <summary>Gets a value indicating whether the parameter accepts null values.</summary>
	/// <returns>true if null values are accepted; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	bool IsNullable { get; }

	/// <summary>Gets or sets the name of the <see cref="T:System.Data.IDataParameter" />.</summary>
	/// <returns>The name of the <see cref="T:System.Data.IDataParameter" />. The default is an empty string.</returns>
	/// <filterpriority>2</filterpriority>
	string ParameterName { get; set; }

	/// <summary>Gets or sets the name of the source column that is mapped to the <see cref="T:System.Data.DataSet" /> and used for loading or returning the <see cref="P:System.Data.IDataParameter.Value" />.</summary>
	/// <returns>The name of the source column that is mapped to the <see cref="T:System.Data.DataSet" />. The default is an empty string.</returns>
	/// <filterpriority>2</filterpriority>
	string SourceColumn { get; set; }

	/// <summary>Gets or sets the <see cref="T:System.Data.DataRowVersion" /> to use when loading <see cref="P:System.Data.IDataParameter.Value" />.</summary>
	/// <returns>One of the <see cref="T:System.Data.DataRowVersion" /> values. The default is Current.</returns>
	/// <exception cref="T:System.ArgumentException">The property was not set one of the <see cref="T:System.Data.DataRowVersion" /> values. </exception>
	/// <filterpriority>2</filterpriority>
	DataRowVersion SourceVersion { get; set; }

	/// <summary>Gets or sets the value of the parameter.</summary>
	/// <returns>An <see cref="T:System.Object" /> that is the value of the parameter. The default value is null.</returns>
	/// <filterpriority>2</filterpriority>
	object Value { get; set; }
}
