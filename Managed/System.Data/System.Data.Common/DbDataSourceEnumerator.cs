namespace System.Data.Common;

/// <summary>Provides a mechanism for enumerating all available instances of database servers within the local network. </summary>
/// <filterpriority>2</filterpriority>
public abstract class DbDataSourceEnumerator
{
	/// <summary>Creates a new instance of the <see cref="T:System.Data.Common.DbDataSourceEnumerator" /> class. </summary>
	protected DbDataSourceEnumerator()
	{
	}

	/// <summary>Retrieves a <see cref="T:System.Data.DataTable" /> containing information about all visible instances of the server represented by the strongly typed instance of this class.</summary>
	/// <returns>Returns a <see cref="T:System.Data.DataTable" /> containing information about the visible instances of the associated data source.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract DataTable GetDataSources();
}
