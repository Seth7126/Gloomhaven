using System.Data.Common;

namespace System.Data.SqlClient;

/// <summary>Defines the mapping between a column in a <see cref="T:System.Data.SqlClient.SqlBulkCopy" /> instance's data source and a column in the instance's destination table. </summary>
/// <filterpriority>2</filterpriority>
public sealed class SqlBulkCopyColumnMapping
{
	internal string _destinationColumnName;

	internal int _destinationColumnOrdinal;

	internal string _sourceColumnName;

	internal int _sourceColumnOrdinal;

	internal int _internalDestinationColumnOrdinal;

	internal int _internalSourceColumnOrdinal;

	/// <summary>Name of the column being mapped in the destination database table.</summary>
	/// <returns>The string value of the <see cref="P:System.Data.SqlClient.SqlBulkCopyColumnMapping.DestinationColumn" /> property.</returns>
	/// <filterpriority>1</filterpriority>
	public string DestinationColumn
	{
		get
		{
			if (_destinationColumnName != null)
			{
				return _destinationColumnName;
			}
			return string.Empty;
		}
		set
		{
			_destinationColumnOrdinal = (_internalDestinationColumnOrdinal = -1);
			_destinationColumnName = value;
		}
	}

	/// <summary>Ordinal value of the destination column within the destination table.</summary>
	/// <returns>The integer value of the <see cref="P:System.Data.SqlClient.SqlBulkCopyColumnMapping.DestinationOrdinal" /> property, or -1 if the property has not been set.</returns>
	/// <filterpriority>1</filterpriority>
	public int DestinationOrdinal
	{
		get
		{
			return _destinationColumnOrdinal;
		}
		set
		{
			if (value >= 0)
			{
				_destinationColumnName = null;
				_destinationColumnOrdinal = (_internalDestinationColumnOrdinal = value);
				return;
			}
			throw ADP.IndexOutOfRange(value);
		}
	}

	/// <summary>Name of the column being mapped in the data source.</summary>
	/// <returns>The string value of the <see cref="P:System.Data.SqlClient.SqlBulkCopyColumnMapping.SourceColumn" /> property.</returns>
	/// <filterpriority>1</filterpriority>
	public string SourceColumn
	{
		get
		{
			if (_sourceColumnName != null)
			{
				return _sourceColumnName;
			}
			return string.Empty;
		}
		set
		{
			_sourceColumnOrdinal = (_internalSourceColumnOrdinal = -1);
			_sourceColumnName = value;
		}
	}

	/// <summary>The ordinal position of the source column within the data source.</summary>
	/// <returns>The integer value of the <see cref="P:System.Data.SqlClient.SqlBulkCopyColumnMapping.SourceOrdinal" /> property.</returns>
	/// <filterpriority>1</filterpriority>
	public int SourceOrdinal
	{
		get
		{
			return _sourceColumnOrdinal;
		}
		set
		{
			if (value >= 0)
			{
				_sourceColumnName = null;
				_sourceColumnOrdinal = (_internalSourceColumnOrdinal = value);
				return;
			}
			throw ADP.IndexOutOfRange(value);
		}
	}

	/// <summary>Default constructor that initializes a new <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object.</summary>
	public SqlBulkCopyColumnMapping()
	{
		_internalSourceColumnOrdinal = -1;
	}

	/// <summary>Creates a new column mapping, using column names to refer to source and destination columns.</summary>
	/// <param name="sourceColumn">The name of the source column within the data source.</param>
	/// <param name="destinationColumn">The name of the destination column within the destination table.</param>
	public SqlBulkCopyColumnMapping(string sourceColumn, string destinationColumn)
	{
		SourceColumn = sourceColumn;
		DestinationColumn = destinationColumn;
	}

	/// <summary>Creates a new column mapping, using a column ordinal to refer to the source column and a column name for the target column.</summary>
	/// <param name="sourceColumnOrdinal">The ordinal position of the source column within the data source.</param>
	/// <param name="destinationColumn">The name of the destination column within the destination table.</param>
	public SqlBulkCopyColumnMapping(int sourceColumnOrdinal, string destinationColumn)
	{
		SourceOrdinal = sourceColumnOrdinal;
		DestinationColumn = destinationColumn;
	}

	/// <summary>Creates a new column mapping, using a column name to refer to the source column and a column ordinal for the target column.</summary>
	/// <param name="sourceColumn">The name of the source column within the data source.</param>
	/// <param name="destinationOrdinal">The ordinal position of the destination column within the destination table.</param>
	public SqlBulkCopyColumnMapping(string sourceColumn, int destinationOrdinal)
	{
		SourceColumn = sourceColumn;
		DestinationOrdinal = destinationOrdinal;
	}

	/// <summary>Creates a new column mapping, using column ordinals to refer to source and destination columns.</summary>
	/// <param name="sourceColumnOrdinal">The ordinal position of the source column within the data source.</param>
	/// <param name="destinationOrdinal">The ordinal position of the destination column within the destination table.</param>
	public SqlBulkCopyColumnMapping(int sourceColumnOrdinal, int destinationOrdinal)
	{
		SourceOrdinal = sourceColumnOrdinal;
		DestinationOrdinal = destinationOrdinal;
	}
}
