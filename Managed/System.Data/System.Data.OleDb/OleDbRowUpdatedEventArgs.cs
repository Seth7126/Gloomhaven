using System.Data.Common;

namespace System.Data.OleDb;

/// <summary>Provides data for the <see cref="E:System.Data.OleDb.OleDbDataAdapter.RowUpdated" /> event.</summary>
/// <filterpriority>2</filterpriority>
[System.MonoTODO("OleDb is not implemented.")]
public sealed class OleDbRowUpdatedEventArgs : RowUpdatedEventArgs
{
	/// <summary>Gets the <see cref="T:System.Data.OleDb.OleDbCommand" /> executed when <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> is called.</summary>
	/// <returns>The <see cref="T:System.Data.OleDb.OleDbCommand" /> executed when <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> is called.</returns>
	/// <filterpriority>2</filterpriority>
	public new OleDbCommand Command
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbRowUpdatedEventArgs" /> class.</summary>
	/// <param name="dataRow">The <see cref="T:System.Data.DataRow" /> sent through an <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />. </param>
	/// <param name="command">The <see cref="T:System.Data.IDbCommand" /> executed when <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> is called. </param>
	/// <param name="statementType">One of the <see cref="T:System.Data.StatementType" /> values that specifies the type of query executed. </param>
	/// <param name="tableMapping">The <see cref="T:System.Data.Common.DataTableMapping" /> sent through an <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />. </param>
	public OleDbRowUpdatedEventArgs(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		: base(null, null, StatementType.Select, null)
	{
		throw ADP.OleDb();
	}
}
