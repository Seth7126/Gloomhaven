using System.Data.Common;

namespace System.Data.SqlClient;

/// <summary>Provides data for the <see cref="E:System.Data.SqlClient.SqlDataAdapter.RowUpdating" /> event.</summary>
/// <filterpriority>2</filterpriority>
public sealed class SqlRowUpdatingEventArgs : RowUpdatingEventArgs
{
	/// <summary>Gets or sets the <see cref="T:System.Data.SqlClient.SqlCommand" /> to execute when performing the <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />.</summary>
	/// <returns>The <see cref="T:System.Data.SqlClient.SqlCommand" /> to execute when performing the <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />.</returns>
	/// <filterpriority>2</filterpriority>
	public new SqlCommand Command
	{
		get
		{
			return base.Command as SqlCommand;
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
			base.BaseCommand = value as SqlCommand;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlRowUpdatingEventArgs" /> class.</summary>
	/// <param name="row">The <see cref="T:System.Data.DataRow" /> to <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />. </param>
	/// <param name="command">The <see cref="T:System.Data.IDbCommand" /> to execute during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />. </param>
	/// <param name="statementType">One of the <see cref="T:System.Data.StatementType" /> values that specifies the type of query executed. </param>
	/// <param name="tableMapping">The <see cref="T:System.Data.Common.DataTableMapping" /> sent through an <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />. </param>
	public SqlRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		: base(row, command, statementType, tableMapping)
	{
	}
}
