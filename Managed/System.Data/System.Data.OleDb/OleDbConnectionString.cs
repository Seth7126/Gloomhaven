using System.Data.Common;

namespace System.Data.OleDb;

[System.MonoTODO("OleDb is not implemented.")]
internal sealed class OleDbConnectionString : DbConnectionOptions
{
	internal OleDbConnectionString(string connectionString, bool validate)
		: base(connectionString, null)
	{
	}
}
