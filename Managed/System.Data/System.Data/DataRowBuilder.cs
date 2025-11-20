using Unity;

namespace System.Data;

/// <summary>The DataRowBuilder type supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
/// <filterpriority>2</filterpriority>
public sealed class DataRowBuilder
{
	internal readonly DataTable _table;

	internal int _record;

	internal DataRowBuilder(DataTable table, int record)
	{
		_table = table;
		_record = record;
	}

	internal DataRowBuilder()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
