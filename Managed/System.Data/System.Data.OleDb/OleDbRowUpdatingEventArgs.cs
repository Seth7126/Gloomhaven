using System.Data.Common;

namespace System.Data.OleDb;

/// <summary>Provides data for the <see cref="E:System.Data.OleDb.OleDbDataAdapter.RowUpdating" /> event. </summary>
/// <filterpriority>2</filterpriority>
[System.MonoTODO("OleDb is not implemented.")]
public sealed class OleDbRowUpdatingEventArgs : RowUpdatingEventArgs
{
	protected override IDbCommand BaseCommand
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.OleDb.OleDbCommand" /> to execute when performing the <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />.</summary>
	/// <returns>The <see cref="T:System.Data.OleDb.OleDbCommand" /> to execute when performing the <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />.</returns>
	/// <filterpriority>2</filterpriority>
	public new OleDbCommand Command
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbRowUpdatingEventArgs" /> class.</summary>
	/// <param name="dataRow">The <see cref="T:System.Data.DataRow" /> to <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />. </param>
	/// <param name="command">The <see cref="T:System.Data.IDbCommand" /> to execute during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />. </param>
	/// <param name="statementType">One of the <see cref="T:System.Data.StatementType" /> values that specifies the type of query executed. </param>
	/// <param name="tableMapping">The <see cref="T:System.Data.Common.DataTableMapping" /> sent through an <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />. </param>
	public OleDbRowUpdatingEventArgs(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		: base(null, null, StatementType.Select, null)
	{
		throw ADP.OleDb();
	}
}
