using System.Data.Common;
using System.Globalization;

namespace System.Data.Odbc;

internal static class ODBC
{
	internal const string Pwd = "pwd";

	internal static Exception ConnectionClosed()
	{
		return ADP.InvalidOperation(global::SR.GetString("The connection is closed."));
	}

	internal static Exception OpenConnectionNoOwner()
	{
		return ADP.InvalidOperation(global::SR.GetString("An internal connection does not have an owner."));
	}

	internal static Exception UnknownSQLType(ODBC32.SQL_TYPE sqltype)
	{
		return ADP.Argument(global::SR.GetString("Unknown SQL type - {0}.", sqltype.ToString()));
	}

	internal static Exception ConnectionStringTooLong()
	{
		return ADP.Argument(global::SR.GetString("Connection string exceeds maximum allowed length of {0}.", 1024));
	}

	internal static ArgumentException GetSchemaRestrictionRequired()
	{
		return ADP.Argument(global::SR.GetString("The ODBC managed provider requires that the TABLE_NAME restriction be specified and non-null for the GetSchema indexes collection."));
	}

	internal static ArgumentOutOfRangeException NotSupportedEnumerationValue(Type type, int value)
	{
		return ADP.ArgumentOutOfRange(global::SR.GetString("The {0} enumeration value, {1}, is not supported by the .Net Framework Odbc Data Provider.", type.Name, value.ToString(CultureInfo.InvariantCulture)), type.Name);
	}

	internal static ArgumentOutOfRangeException NotSupportedCommandType(CommandType value)
	{
		return NotSupportedEnumerationValue(typeof(CommandType), (int)value);
	}

	internal static ArgumentOutOfRangeException NotSupportedIsolationLevel(IsolationLevel value)
	{
		return NotSupportedEnumerationValue(typeof(IsolationLevel), (int)value);
	}

	internal static InvalidOperationException NoMappingForSqlTransactionLevel(int value)
	{
		return ADP.DataAdapter(global::SR.GetString("No valid mapping for a SQL_TRANSACTION '{0}' to a System.Data.IsolationLevel enumeration value.", value.ToString(CultureInfo.InvariantCulture)));
	}

	internal static Exception NegativeArgument()
	{
		return ADP.Argument(global::SR.GetString("Invalid negative argument!"));
	}

	internal static Exception CantSetPropertyOnOpenConnection()
	{
		return ADP.InvalidOperation(global::SR.GetString("Can't set property on an open connection."));
	}

	internal static Exception CantEnableConnectionpooling(ODBC32.RetCode retcode)
	{
		return ADP.DataAdapter(global::SR.GetString("{0} - unable to enable connection pooling...", ODBC32.RetcodeToString(retcode)));
	}

	internal static Exception CantAllocateEnvironmentHandle(ODBC32.RetCode retcode)
	{
		return ADP.DataAdapter(global::SR.GetString("{0} - unable to allocate an environment handle.", ODBC32.RetcodeToString(retcode)));
	}

	internal static Exception FailedToGetDescriptorHandle(ODBC32.RetCode retcode)
	{
		return ADP.DataAdapter(global::SR.GetString("{0} - unable to get descriptor handle.", ODBC32.RetcodeToString(retcode)));
	}

	internal static Exception NotInTransaction()
	{
		return ADP.InvalidOperation(global::SR.GetString("Not in a transaction"));
	}

	internal static Exception UnknownOdbcType(OdbcType odbctype)
	{
		return ADP.InvalidEnumerationValue(typeof(OdbcType), (int)odbctype);
	}

	internal static void TraceODBC(int level, string method, ODBC32.RetCode retcode)
	{
	}

	internal static short ShortStringLength(string inputString)
	{
		return checked((short)ADP.StringLength(inputString));
	}
}
