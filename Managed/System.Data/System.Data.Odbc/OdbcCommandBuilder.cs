using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;

namespace System.Data.Odbc;

/// <summary>Automatically generates single-table commands that are used to reconcile changes made to a <see cref="T:System.Data.DataSet" /> with the associated data source. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
public sealed class OdbcCommandBuilder : DbCommandBuilder
{
	/// <summary>Gets or sets an <see cref="T:System.Data.Odbc.OdbcDataAdapter" /> object for which this <see cref="T:System.Data.Odbc.OdbcCommandBuilder" /> object will generate SQL statements.</summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcDataAdapter" /> object that is associated with this <see cref="T:System.Data.Odbc.OdbcCommandBuilder" />.</returns>
	/// <filterpriority>2</filterpriority>
	public new OdbcDataAdapter DataAdapter
	{
		get
		{
			return base.DataAdapter as OdbcDataAdapter;
		}
		set
		{
			base.DataAdapter = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcCommandBuilder" /> class.</summary>
	public OdbcCommandBuilder()
	{
		GC.SuppressFinalize(this);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcCommandBuilder" /> class with the associated <see cref="T:System.Data.Odbc.OdbcDataAdapter" /> object.</summary>
	/// <param name="adapter">An <see cref="T:System.Data.Odbc.OdbcDataAdapter" /> object to associate with this <see cref="T:System.Data.Odbc.OdbcCommandBuilder" />.</param>
	public OdbcCommandBuilder(OdbcDataAdapter adapter)
		: this()
	{
		DataAdapter = adapter;
	}

	private void OdbcRowUpdatingHandler(object sender, OdbcRowUpdatingEventArgs ruevent)
	{
		RowUpdatingHandler(ruevent);
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform insertions at the data source.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform insertions.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public new OdbcCommand GetInsertCommand()
	{
		return (OdbcCommand)base.GetInsertCommand();
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform insertions at the data source.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform insertions.</returns>
	/// <param name="useColumnsForParameterNames">If true, generate parameter names matching column names, if it is possible. If false, generate @p1, @p2, and so on.</param>
	/// <filterpriority>1</filterpriority>
	public new OdbcCommand GetInsertCommand(bool useColumnsForParameterNames)
	{
		return (OdbcCommand)base.GetInsertCommand(useColumnsForParameterNames);
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform updates at the data source.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform updates.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public new OdbcCommand GetUpdateCommand()
	{
		return (OdbcCommand)base.GetUpdateCommand();
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform updates at the data source.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform updates.</returns>
	/// <param name="useColumnsForParameterNames">If true, generate parameter names matching column names, if it is possible. If false, generate @p1, @p2, and so on.</param>
	/// <filterpriority>1</filterpriority>
	public new OdbcCommand GetUpdateCommand(bool useColumnsForParameterNames)
	{
		return (OdbcCommand)base.GetUpdateCommand(useColumnsForParameterNames);
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform deletions at the data source.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform deletions.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public new OdbcCommand GetDeleteCommand()
	{
		return (OdbcCommand)base.GetDeleteCommand();
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform deletions at the data source.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.Odbc.OdbcCommand" /> object required to perform deletions.</returns>
	/// <param name="useColumnsForParameterNames">If true, generate parameter names matching column names, if it is possible. If false, generate @p1, @p2, and so on.</param>
	/// <filterpriority>1</filterpriority>
	public new OdbcCommand GetDeleteCommand(bool useColumnsForParameterNames)
	{
		return (OdbcCommand)base.GetDeleteCommand(useColumnsForParameterNames);
	}

	protected override string GetParameterName(int parameterOrdinal)
	{
		return "p" + parameterOrdinal.ToString(CultureInfo.InvariantCulture);
	}

	protected override string GetParameterName(string parameterName)
	{
		return parameterName;
	}

	protected override string GetParameterPlaceholder(int parameterOrdinal)
	{
		return "?";
	}

	protected override void ApplyParameterInfo(DbParameter parameter, DataRow datarow, StatementType statementType, bool whereClause)
	{
		OdbcParameter odbcParameter = (OdbcParameter)parameter;
		object obj = datarow[SchemaTableColumn.ProviderType];
		odbcParameter.OdbcType = (OdbcType)obj;
		object obj2 = datarow[SchemaTableColumn.NumericPrecision];
		if (DBNull.Value != obj2)
		{
			byte b = (byte)(short)obj2;
			odbcParameter.PrecisionInternal = (byte)((byte.MaxValue != b) ? b : 0);
		}
		obj2 = datarow[SchemaTableColumn.NumericScale];
		if (DBNull.Value != obj2)
		{
			byte b2 = (byte)(short)obj2;
			odbcParameter.ScaleInternal = (byte)((byte.MaxValue != b2) ? b2 : 0);
		}
	}

	/// <summary>Retrieves parameter information from the stored procedure specified in the <see cref="T:System.Data.Odbc.OdbcCommand" /> and populates the <see cref="P:System.Data.Odbc.OdbcCommand.Parameters" /> collection of the specified <see cref="T:System.Data.Odbc.OdbcCommand" /> object.</summary>
	/// <param name="command">The <see cref="T:System.Data.Odbc.OdbcCommand" /> referencing the stored procedure from which the parameter information is to be derived. The derived parameters are added to the <see cref="P:System.Data.Odbc.OdbcCommand.Parameters" /> collection of the <see cref="T:System.Data.Odbc.OdbcCommand" />. </param>
	/// <exception cref="T:System.InvalidOperationException">The underlying ODBC driver does not support returning stored procedure parameter information, or the command text is not a valid stored procedure name, or the <see cref="T:System.Data.CommandType" /> specified was not CommandType.StoredProcedure. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void DeriveParameters(OdbcCommand command)
	{
		if (command == null)
		{
			throw ADP.ArgumentNull("command");
		}
		switch (command.CommandType)
		{
		case CommandType.Text:
			throw ADP.DeriveParametersNotSupported(command);
		case CommandType.TableDirect:
			throw ADP.DeriveParametersNotSupported(command);
		default:
			throw ADP.InvalidCommandType(command.CommandType);
		case CommandType.StoredProcedure:
		{
			if (string.IsNullOrEmpty(command.CommandText))
			{
				throw ADP.CommandTextRequired("DeriveParameters");
			}
			OdbcConnection connection = command.Connection;
			if (connection == null)
			{
				throw ADP.ConnectionRequired("DeriveParameters");
			}
			ConnectionState state = connection.State;
			if (ConnectionState.Open != state)
			{
				throw ADP.OpenConnectionRequired("DeriveParameters", state);
			}
			OdbcParameter[] array = DeriveParametersFromStoredProcedure(connection, command);
			OdbcParameterCollection parameters = command.Parameters;
			parameters.Clear();
			int num = array.Length;
			if (0 < num)
			{
				for (int i = 0; i < array.Length; i++)
				{
					parameters.Add(array[i]);
				}
			}
			break;
		}
		}
	}

	private static OdbcParameter[] DeriveParametersFromStoredProcedure(OdbcConnection connection, OdbcCommand command)
	{
		List<OdbcParameter> list = new List<OdbcParameter>();
		CMDWrapper statementHandle = command.GetStatementHandle();
		OdbcStatementHandle statementHandle2 = statementHandle.StatementHandle;
		string text = connection.QuoteChar("DeriveParameters");
		string[] array = MultipartIdentifier.ParseMultipartIdentifier(command.CommandText, text, text, '.', 4, removequotes: true, "OdbcCommandBuilder.DeriveParameters failed because the OdbcCommand.CommandText property value is an invalid multipart name", ThrowOnEmptyMultipartName: false);
		if (array[3] == null)
		{
			array[3] = command.CommandText;
		}
		ODBC32.RetCode retCode = statementHandle2.ProcedureColumns(array[1], array[2], array[3], null);
		if (retCode != ODBC32.RetCode.SUCCESS)
		{
			connection.HandleError(statementHandle2, retCode);
		}
		using (OdbcDataReader odbcDataReader = new OdbcDataReader(command, statementHandle, CommandBehavior.Default))
		{
			odbcDataReader.FirstResult();
			_ = odbcDataReader.FieldCount;
			while (odbcDataReader.Read())
			{
				OdbcParameter odbcParameter = new OdbcParameter();
				odbcParameter.ParameterName = odbcDataReader.GetString(3);
				switch ((ODBC32.SQL_PARAM)odbcDataReader.GetInt16(4))
				{
				case ODBC32.SQL_PARAM.INPUT:
					odbcParameter.Direction = ParameterDirection.Input;
					break;
				case ODBC32.SQL_PARAM.OUTPUT:
					odbcParameter.Direction = ParameterDirection.Output;
					break;
				case ODBC32.SQL_PARAM.INPUT_OUTPUT:
					odbcParameter.Direction = ParameterDirection.InputOutput;
					break;
				case ODBC32.SQL_PARAM.RETURN_VALUE:
					odbcParameter.Direction = ParameterDirection.ReturnValue;
					break;
				}
				odbcParameter.OdbcType = TypeMap.FromSqlType((ODBC32.SQL_TYPE)odbcDataReader.GetInt16(5))._odbcType;
				odbcParameter.Size = odbcDataReader.GetInt32(7);
				OdbcType odbcType = odbcParameter.OdbcType;
				if ((uint)(odbcType - 6) <= 1u)
				{
					odbcParameter.ScaleInternal = (byte)odbcDataReader.GetInt16(9);
					odbcParameter.PrecisionInternal = (byte)odbcDataReader.GetInt16(10);
				}
				list.Add(odbcParameter);
			}
		}
		retCode = statementHandle2.CloseCursor();
		return list.ToArray();
	}

	/// <summary>Given an unquoted identifier in the correct catalog case, returns the correct quoted form of that identifier. This includes correctly escaping any embedded quotes in the identifier.</summary>
	/// <returns>The quoted version of the identifier. Embedded quotes within the identifier are correctly escaped.</returns>
	/// <param name="unquotedIdentifier">The original unquoted identifier.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string QuoteIdentifier(string unquotedIdentifier)
	{
		return QuoteIdentifier(unquotedIdentifier, null);
	}

	/// <summary>Given an unquoted identifier in the correct catalog case, returns the correct quoted form of that identifier. This includes correctly escaping any embedded quotes in the identifier.</summary>
	/// <returns>The quoted version of the identifier. Embedded quotes within the identifier are correctly escaped.</returns>
	/// <param name="unquotedIdentifier">The original unquoted identifier.</param>
	/// <param name="connection">When a connection is passed, causes the managed wrapper to get the quote character from the ODBC driver, calling SQLGetInfo(SQL_IDENTIFIER_QUOTE_CHAR). When no connection is passed, the string is quoted using values from <see cref="P:System.Data.Common.DbCommandBuilder.QuotePrefix" /> and <see cref="P:System.Data.Common.DbCommandBuilder.QuoteSuffix" />.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public string QuoteIdentifier(string unquotedIdentifier, OdbcConnection connection)
	{
		ADP.CheckArgumentNull(unquotedIdentifier, "unquotedIdentifier");
		string text = QuotePrefix;
		string quoteSuffix = QuoteSuffix;
		if (string.IsNullOrEmpty(text))
		{
			if (connection == null)
			{
				connection = DataAdapter?.SelectCommand?.Connection;
				if (connection == null)
				{
					throw ADP.QuotePrefixNotSet("QuoteIdentifier");
				}
			}
			text = connection.QuoteChar("QuoteIdentifier");
			quoteSuffix = text;
		}
		if (!string.IsNullOrEmpty(text) && text != " ")
		{
			return ADP.BuildQuotedString(text, quoteSuffix, unquotedIdentifier);
		}
		return unquotedIdentifier;
	}

	protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
	{
		if (adapter == base.DataAdapter)
		{
			((OdbcDataAdapter)adapter).RowUpdating -= OdbcRowUpdatingHandler;
		}
		else
		{
			((OdbcDataAdapter)adapter).RowUpdating += OdbcRowUpdatingHandler;
		}
	}

	/// <summary>Given a quoted identifier, returns the correct unquoted form of that identifier, including correctly unescaping any embedded quotes in the identifier.</summary>
	/// <returns>The unquoted identifier, with embedded quotes correctly unescaped.</returns>
	/// <param name="quotedIdentifier">The identifier that will have its embedded quotes removed.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string UnquoteIdentifier(string quotedIdentifier)
	{
		return UnquoteIdentifier(quotedIdentifier, null);
	}

	/// <summary>Given a quoted identifier, returns the correct unquoted form of that identifier, including correctly unescaping any embedded quotes in the identifier.</summary>
	/// <returns>The unquoted identifier, with embedded quotes correctly unescaped.</returns>
	/// <param name="quotedIdentifier">The identifier that will have its embedded quotes removed.</param>
	/// <param name="connection">The <see cref="T:System.Data.Odbc.OdbcConnection" />.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public string UnquoteIdentifier(string quotedIdentifier, OdbcConnection connection)
	{
		ADP.CheckArgumentNull(quotedIdentifier, "quotedIdentifier");
		string text = QuotePrefix;
		string quoteSuffix = QuoteSuffix;
		if (string.IsNullOrEmpty(text))
		{
			if (connection == null)
			{
				connection = DataAdapter?.SelectCommand?.Connection;
				if (connection == null)
				{
					throw ADP.QuotePrefixNotSet("UnquoteIdentifier");
				}
			}
			text = connection.QuoteChar("UnquoteIdentifier");
			quoteSuffix = text;
		}
		if (!string.IsNullOrEmpty(text) || text != " ")
		{
			ADP.RemoveStringQuotes(text, quoteSuffix, quotedIdentifier, out var unquotedString);
			return unquotedString;
		}
		return quotedIdentifier;
	}
}
