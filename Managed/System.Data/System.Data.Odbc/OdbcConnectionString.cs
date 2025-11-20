using System.Data.Common;

namespace System.Data.Odbc;

internal sealed class OdbcConnectionString : DbConnectionOptions
{
	private readonly string _expandedConnectionString;

	internal OdbcConnectionString(string connectionString, bool validate)
		: base(connectionString, null, useOdbcRules: true)
	{
		if (!validate)
		{
			string filename = null;
			int position = 0;
			_expandedConnectionString = ExpandDataDirectories(ref filename, ref position);
		}
		if ((validate || _expandedConnectionString == null) && connectionString != null && 1024 < connectionString.Length)
		{
			throw ODBC.ConnectionStringTooLong();
		}
	}
}
