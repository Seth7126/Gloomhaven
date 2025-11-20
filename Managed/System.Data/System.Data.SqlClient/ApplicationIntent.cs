namespace System.Data.SqlClient;

/// <summary>Specifies a value for <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.ApplicationIntent" />. Possible values are ReadWrite and ReadOnly.</summary>
public enum ApplicationIntent
{
	/// <summary>The application workload type when connecting to a server is read write.</summary>
	ReadWrite,
	/// <summary>The application workload type when connecting to a server is read only.</summary>
	ReadOnly
}
