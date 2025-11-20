using System.Data.Common;

namespace System.Data.Odbc;

/// <summary>Provides data for the <see cref="E:System.Data.Odbc.OdbcDataAdapter.RowUpdating" /> event.</summary>
/// <filterpriority>2</filterpriority>
public sealed class OdbcRowUpdatingEventArgs : RowUpdatingEventArgs
{
	/// <summary>Gets or sets the <see cref="T:System.Data.Odbc.OdbcCommand" /> to execute when <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> is called.</summary>
	/// <returns>The <see cref="T:System.Data.Odbc.OdbcCommand" /> to execute when <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> is called.</returns>
	/// <filterpriority>2</filterpriority>
	public new OdbcCommand Command
	{
		get
		{
			return base.Command as OdbcCommand;
		}
		set
		{
			base.Command = value;
		}
	}

	protected override IDbCommand BaseCommand
	{
		get
		{
			return base.BaseCommand;
		}
		set
		{
			base.BaseCommand = value as OdbcCommand;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcRowUpdatingEventArgs" /> class.</summary>
	/// <param name="row">The <see cref="T:System.Data.DataRow" /> to update. </param>
	/// <param name="command">The <see cref="T:System.Data.Odbc.OdbcCommand" /> to execute during the update operation. </param>
	/// <param name="statementType">One of the <see cref="T:System.Data.StatementType" /> values that specifies the type of query executed. </param>
	/// <param name="tableMapping">The <see cref="T:System.Data.Common.DataTableMapping" /> sent through <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />. </param>
	public OdbcRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		: base(row, command, statementType, tableMapping)
	{
	}
}
