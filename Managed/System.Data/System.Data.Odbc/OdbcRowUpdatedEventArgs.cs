using System.Data.Common;

namespace System.Data.Odbc;

/// <summary>Provides data for the <see cref="E:System.Data.Odbc.OdbcDataAdapter.RowUpdated" /> event.</summary>
/// <filterpriority>2</filterpriority>
public sealed class OdbcRowUpdatedEventArgs : RowUpdatedEventArgs
{
	/// <summary>Gets the <see cref="T:System.Data.Odbc.OdbcCommand" /> executed when <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> is called.</summary>
	/// <returns>The <see cref="T:System.Data.Odbc.OdbcCommand" /> executed when <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> is called.</returns>
	/// <filterpriority>2</filterpriority>
	public new OdbcCommand Command => (OdbcCommand)base.Command;

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcRowUpdatedEventArgs" /> class.</summary>
	/// <param name="row">The DataRow sent through an update operation. </param>
	/// <param name="command">The <see cref="T:System.Data.Odbc.OdbcCommand" /> executed when <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> is called. </param>
	/// <param name="statementType">One of the <see cref="T:System.Data.StatementType" /> values that specifies the type of query executed. </param>
	/// <param name="tableMapping">The <see cref="T:System.Data.Common.DataTableMapping" /> sent through <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />. </param>
	public OdbcRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		: base(row, command, statementType, tableMapping)
	{
	}
}
