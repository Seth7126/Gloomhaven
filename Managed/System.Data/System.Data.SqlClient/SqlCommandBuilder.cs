using System.Data.Common;
using System.Data.Sql;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Data.SqlClient;

/// <summary>Automatically generates single-table commands that are used to reconcile changes made to a <see cref="T:System.Data.DataSet" /> with the associated SQL Server database. This class cannot be inherited. </summary>
/// <filterpriority>2</filterpriority>
public sealed class SqlCommandBuilder : DbCommandBuilder
{
	/// <summary>Sets or gets the <see cref="T:System.Data.Common.CatalogLocation" /> for an instance of the <see cref="T:System.Data.SqlClient.SqlCommandBuilder" /> class.</summary>
	/// <returns>A <see cref="T:System.Data.Common.CatalogLocation" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override CatalogLocation CatalogLocation
	{
		get
		{
			return CatalogLocation.Start;
		}
		set
		{
			if (CatalogLocation.Start != value)
			{
				throw ADP.SingleValuedProperty("CatalogLocation", "Start");
			}
		}
	}

	/// <summary>Sets or gets a string used as the catalog separator for an instance of the <see cref="T:System.Data.SqlClient.SqlCommandBuilder" /> class.</summary>
	/// <returns>A string that indicates the catalog separator for use with an instance of the <see cref="T:System.Data.SqlClient.SqlCommandBuilder" /> class.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string CatalogSeparator
	{
		get
		{
			return ".";
		}
		set
		{
			if ("." != value)
			{
				throw ADP.SingleValuedProperty("CatalogSeparator", ".");
			}
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Data.SqlClient.SqlDataAdapter" /> object for which Transact-SQL statements are automatically generated.</summary>
	/// <returns>A <see cref="T:System.Data.SqlClient.SqlDataAdapter" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public new SqlDataAdapter DataAdapter
	{
		get
		{
			return (SqlDataAdapter)base.DataAdapter;
		}
		set
		{
			base.DataAdapter = value;
		}
	}

	/// <summary>Gets or sets the starting character or characters to use when specifying SQL Server database objects, such as tables or columns, whose names contain characters such as spaces or reserved tokens.</summary>
	/// <returns>The starting character or characters to use. The default is an empty string.</returns>
	/// <exception cref="T:System.InvalidOperationException">This property cannot be changed after an INSERT, UPDATE, or DELETE command has been generated. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string QuotePrefix
	{
		get
		{
			return base.QuotePrefix;
		}
		set
		{
			if ("[" != value && "\"" != value)
			{
				throw ADP.DoubleValuedProperty("QuotePrefix", "[", "\"");
			}
			base.QuotePrefix = value;
		}
	}

	/// <summary>Gets or sets the ending character or characters to use when specifying SQL Server database objects, such as tables or columns, whose names contain characters such as spaces or reserved tokens.</summary>
	/// <returns>The ending character or characters to use. The default is an empty string.</returns>
	/// <exception cref="T:System.InvalidOperationException">This property cannot be changed after an insert, update, or delete command has been generated. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string QuoteSuffix
	{
		get
		{
			return base.QuoteSuffix;
		}
		set
		{
			if ("]" != value && "\"" != value)
			{
				throw ADP.DoubleValuedProperty("QuoteSuffix", "]", "\"");
			}
			base.QuoteSuffix = value;
		}
	}

	/// <summary>Gets or sets the character to be used for the separator between the schema identifier and any other identifiers.</summary>
	/// <returns>The character to be used as the schema separator.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string SchemaSeparator
	{
		get
		{
			return ".";
		}
		set
		{
			if ("." != value)
			{
				throw ADP.SingleValuedProperty("SchemaSeparator", ".");
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlCommandBuilder" /> class.</summary>
	public SqlCommandBuilder()
	{
		GC.SuppressFinalize(this);
		base.QuotePrefix = "[";
		base.QuoteSuffix = "]";
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlCommandBuilder" /> class with the associated <see cref="T:System.Data.SqlClient.SqlDataAdapter" /> object.</summary>
	/// <param name="adapter">The name of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />. </param>
	public SqlCommandBuilder(SqlDataAdapter adapter)
		: this()
	{
		DataAdapter = adapter;
	}

	private void SqlRowUpdatingHandler(object sender, SqlRowUpdatingEventArgs ruevent)
	{
		RowUpdatingHandler(ruevent);
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object required to perform insertions on the database.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object required to perform insertions.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public new SqlCommand GetInsertCommand()
	{
		return (SqlCommand)base.GetInsertCommand();
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object that is required to perform insertions on the database.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object that is required to perform insertions.</returns>
	/// <param name="useColumnsForParameterNames">If true, generate parameter names matching column names if possible. If false, generate @p1, @p2, and so on.</param>
	/// <filterpriority>1</filterpriority>
	public new SqlCommand GetInsertCommand(bool useColumnsForParameterNames)
	{
		return (SqlCommand)base.GetInsertCommand(useColumnsForParameterNames);
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object required to perform updates on the database.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object that is required to perform updates.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public new SqlCommand GetUpdateCommand()
	{
		return (SqlCommand)base.GetUpdateCommand();
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object required to perform updates on the database.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object required to perform updates.</returns>
	/// <param name="useColumnsForParameterNames">If true, generate parameter names matching column names if possible. If false, generate @p1, @p2, and so on.</param>
	/// <filterpriority>1</filterpriority>
	public new SqlCommand GetUpdateCommand(bool useColumnsForParameterNames)
	{
		return (SqlCommand)base.GetUpdateCommand(useColumnsForParameterNames);
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object required to perform deletions on the database.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object required to perform deletions.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public new SqlCommand GetDeleteCommand()
	{
		return (SqlCommand)base.GetDeleteCommand();
	}

	/// <summary>Gets the automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object that is required to perform deletions on the database.</summary>
	/// <returns>The automatically generated <see cref="T:System.Data.SqlClient.SqlCommand" /> object that is required to perform deletions.</returns>
	/// <param name="useColumnsForParameterNames">If true, generate parameter names matching column names if possible. If false, generate @p1, @p2, and so on.</param>
	/// <filterpriority>1</filterpriority>
	public new SqlCommand GetDeleteCommand(bool useColumnsForParameterNames)
	{
		return (SqlCommand)base.GetDeleteCommand(useColumnsForParameterNames);
	}

	protected override void ApplyParameterInfo(DbParameter parameter, DataRow datarow, StatementType statementType, bool whereClause)
	{
		SqlParameter sqlParameter = (SqlParameter)parameter;
		object obj = datarow[SchemaTableColumn.ProviderType];
		sqlParameter.SqlDbType = (SqlDbType)obj;
		sqlParameter.Offset = 0;
		if (sqlParameter.SqlDbType == SqlDbType.Udt && !sqlParameter.SourceColumnNullMapping)
		{
			sqlParameter.UdtTypeName = datarow["DataTypeName"] as string;
		}
		else
		{
			sqlParameter.UdtTypeName = string.Empty;
		}
		object obj2 = datarow[SchemaTableColumn.NumericPrecision];
		if (DBNull.Value != obj2)
		{
			byte b = (byte)(short)obj2;
			sqlParameter.PrecisionInternal = (byte)((byte.MaxValue != b) ? b : 0);
		}
		obj2 = datarow[SchemaTableColumn.NumericScale];
		if (DBNull.Value != obj2)
		{
			byte b2 = (byte)(short)obj2;
			sqlParameter.ScaleInternal = (byte)((byte.MaxValue != b2) ? b2 : 0);
		}
	}

	protected override string GetParameterName(int parameterOrdinal)
	{
		return "@p" + parameterOrdinal.ToString(CultureInfo.InvariantCulture);
	}

	protected override string GetParameterName(string parameterName)
	{
		return "@" + parameterName;
	}

	protected override string GetParameterPlaceholder(int parameterOrdinal)
	{
		return "@p" + parameterOrdinal.ToString(CultureInfo.InvariantCulture);
	}

	private void ConsistentQuoteDelimiters(string quotePrefix, string quoteSuffix)
	{
		if (("\"" == quotePrefix && "\"" != quoteSuffix) || ("[" == quotePrefix && "]" != quoteSuffix))
		{
			throw ADP.InvalidPrefixSuffix();
		}
	}

	/// <summary>Retrieves parameter information from the stored procedure specified in the <see cref="T:System.Data.SqlClient.SqlCommand" /> and populates the <see cref="P:System.Data.SqlClient.SqlCommand.Parameters" /> collection of the specified <see cref="T:System.Data.SqlClient.SqlCommand" /> object.</summary>
	/// <param name="command">The <see cref="T:System.Data.SqlClient.SqlCommand" /> referencing the stored procedure from which the parameter information is to be derived. The derived parameters are added to the <see cref="P:System.Data.SqlClient.SqlCommand.Parameters" /> collection of the <see cref="T:System.Data.SqlClient.SqlCommand" />. </param>
	/// <exception cref="T:System.InvalidOperationException">The command text is not a valid stored procedure name. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy, ControlAppDomain" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Data.SqlClient.SqlClientPermission, System.Data, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void DeriveParameters(SqlCommand command)
	{
		if (command == null)
		{
			throw ADP.ArgumentNull("command");
		}
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			command.DeriveParameters();
		}
		catch (OutOfMemoryException e)
		{
			command?.Connection?.Abort(e);
			throw;
		}
		catch (StackOverflowException e2)
		{
			command?.Connection?.Abort(e2);
			throw;
		}
		catch (ThreadAbortException e3)
		{
			command?.Connection?.Abort(e3);
			throw;
		}
	}

	protected override DataTable GetSchemaTable(DbCommand srcCommand)
	{
		SqlCommand sqlCommand = srcCommand as SqlCommand;
		SqlNotificationRequest notification = sqlCommand.Notification;
		sqlCommand.Notification = null;
		try
		{
			using SqlDataReader sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
			return sqlDataReader.GetSchemaTable();
		}
		finally
		{
			sqlCommand.Notification = notification;
		}
	}

	protected override DbCommand InitializeCommand(DbCommand command)
	{
		return (SqlCommand)base.InitializeCommand(command);
	}

	/// <summary>Given an unquoted identifier in the correct catalog case, returns the correct quoted form of that identifier. This includes correctly escaping any embedded quotes in the identifier.</summary>
	/// <returns>The quoted version of the identifier. Embedded quotes within the identifier are correctly escaped.</returns>
	/// <param name="unquotedIdentifier">The original unquoted identifier.</param>
	/// <filterpriority>1</filterpriority>
	public override string QuoteIdentifier(string unquotedIdentifier)
	{
		ADP.CheckArgumentNull(unquotedIdentifier, "unquotedIdentifier");
		string quoteSuffix = QuoteSuffix;
		string quotePrefix = QuotePrefix;
		ConsistentQuoteDelimiters(quotePrefix, quoteSuffix);
		return ADP.BuildQuotedString(quotePrefix, quoteSuffix, unquotedIdentifier);
	}

	protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
	{
		if (adapter == base.DataAdapter)
		{
			((SqlDataAdapter)adapter).RowUpdating -= SqlRowUpdatingHandler;
		}
		else
		{
			((SqlDataAdapter)adapter).RowUpdating += SqlRowUpdatingHandler;
		}
	}

	/// <summary>Given a quoted identifier, returns the correct unquoted form of that identifier. This includes correctly unescaping any embedded quotes in the identifier.</summary>
	/// <returns>The unquoted identifier, with embedded quotes properly unescaped.</returns>
	/// <param name="quotedIdentifier">The identifier that will have its embedded quotes removed.</param>
	/// <filterpriority>1</filterpriority>
	public override string UnquoteIdentifier(string quotedIdentifier)
	{
		ADP.CheckArgumentNull(quotedIdentifier, "quotedIdentifier");
		string quoteSuffix = QuoteSuffix;
		string quotePrefix = QuotePrefix;
		ConsistentQuoteDelimiters(quotePrefix, quoteSuffix);
		ADP.RemoveStringQuotes(quotePrefix, quoteSuffix, quotedIdentifier, out var unquotedString);
		return unquotedString;
	}
}
