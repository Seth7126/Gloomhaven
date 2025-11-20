using System.Data.Common;

namespace System.Data.Odbc;

internal sealed class TypeMap
{
	private static readonly TypeMap s_bigInt = new TypeMap(OdbcType.BigInt, DbType.Int64, typeof(long), ODBC32.SQL_TYPE.BIGINT, ODBC32.SQL_C.SBIGINT, ODBC32.SQL_C.SBIGINT, 8, 20, signType: true);

	private static readonly TypeMap s_binary = new TypeMap(OdbcType.Binary, DbType.Binary, typeof(byte[]), ODBC32.SQL_TYPE.BINARY, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, signType: false);

	private static readonly TypeMap s_bit = new TypeMap(OdbcType.Bit, DbType.Boolean, typeof(bool), ODBC32.SQL_TYPE.BIT, ODBC32.SQL_C.BIT, ODBC32.SQL_C.BIT, 1, 1, signType: false);

	internal static readonly TypeMap _Char = new TypeMap(OdbcType.Char, DbType.AnsiStringFixedLength, typeof(string), ODBC32.SQL_TYPE.CHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.CHAR, -1, -1, signType: false);

	private static readonly TypeMap s_dateTime = new TypeMap(OdbcType.DateTime, DbType.DateTime, typeof(DateTime), ODBC32.SQL_TYPE.TYPE_TIMESTAMP, ODBC32.SQL_C.TYPE_TIMESTAMP, ODBC32.SQL_C.TYPE_TIMESTAMP, 16, 23, signType: false);

	private static readonly TypeMap s_date = new TypeMap(OdbcType.Date, DbType.Date, typeof(DateTime), ODBC32.SQL_TYPE.TYPE_DATE, ODBC32.SQL_C.TYPE_DATE, ODBC32.SQL_C.TYPE_DATE, 6, 10, signType: false);

	private static readonly TypeMap s_time = new TypeMap(OdbcType.Time, DbType.Time, typeof(TimeSpan), ODBC32.SQL_TYPE.TYPE_TIME, ODBC32.SQL_C.TYPE_TIME, ODBC32.SQL_C.TYPE_TIME, 6, 12, signType: false);

	private static readonly TypeMap s_decimal = new TypeMap(OdbcType.Decimal, DbType.Decimal, typeof(decimal), ODBC32.SQL_TYPE.DECIMAL, ODBC32.SQL_C.NUMERIC, ODBC32.SQL_C.NUMERIC, 19, 28, signType: false);

	private static readonly TypeMap s_double = new TypeMap(OdbcType.Double, DbType.Double, typeof(double), ODBC32.SQL_TYPE.DOUBLE, ODBC32.SQL_C.DOUBLE, ODBC32.SQL_C.DOUBLE, 8, 15, signType: false);

	internal static readonly TypeMap _Image = new TypeMap(OdbcType.Image, DbType.Binary, typeof(byte[]), ODBC32.SQL_TYPE.LONGVARBINARY, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, signType: false);

	private static readonly TypeMap s_int = new TypeMap(OdbcType.Int, DbType.Int32, typeof(int), ODBC32.SQL_TYPE.INTEGER, ODBC32.SQL_C.SLONG, ODBC32.SQL_C.SLONG, 4, 10, signType: true);

	private static readonly TypeMap s_NChar = new TypeMap(OdbcType.NChar, DbType.StringFixedLength, typeof(string), ODBC32.SQL_TYPE.WCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.WCHAR, -1, -1, signType: false);

	internal static readonly TypeMap _NText = new TypeMap(OdbcType.NText, DbType.String, typeof(string), ODBC32.SQL_TYPE.WLONGVARCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.WCHAR, -1, -1, signType: false);

	private static readonly TypeMap s_numeric = new TypeMap(OdbcType.Numeric, DbType.Decimal, typeof(decimal), ODBC32.SQL_TYPE.NUMERIC, ODBC32.SQL_C.NUMERIC, ODBC32.SQL_C.NUMERIC, 19, 28, signType: false);

	internal static readonly TypeMap _NVarChar = new TypeMap(OdbcType.NVarChar, DbType.String, typeof(string), ODBC32.SQL_TYPE.WVARCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.WCHAR, -1, -1, signType: false);

	private static readonly TypeMap s_real = new TypeMap(OdbcType.Real, DbType.Single, typeof(float), ODBC32.SQL_TYPE.REAL, ODBC32.SQL_C.REAL, ODBC32.SQL_C.REAL, 4, 7, signType: false);

	private static readonly TypeMap s_uniqueId = new TypeMap(OdbcType.UniqueIdentifier, DbType.Guid, typeof(Guid), ODBC32.SQL_TYPE.GUID, ODBC32.SQL_C.GUID, ODBC32.SQL_C.GUID, 16, 36, signType: false);

	private static readonly TypeMap s_smallDT = new TypeMap(OdbcType.SmallDateTime, DbType.DateTime, typeof(DateTime), ODBC32.SQL_TYPE.TYPE_TIMESTAMP, ODBC32.SQL_C.TYPE_TIMESTAMP, ODBC32.SQL_C.TYPE_TIMESTAMP, 16, 23, signType: false);

	private static readonly TypeMap s_smallInt = new TypeMap(OdbcType.SmallInt, DbType.Int16, typeof(short), ODBC32.SQL_TYPE.SMALLINT, ODBC32.SQL_C.SSHORT, ODBC32.SQL_C.SSHORT, 2, 5, signType: true);

	internal static readonly TypeMap _Text = new TypeMap(OdbcType.Text, DbType.AnsiString, typeof(string), ODBC32.SQL_TYPE.LONGVARCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.CHAR, -1, -1, signType: false);

	private static readonly TypeMap s_timestamp = new TypeMap(OdbcType.Timestamp, DbType.Binary, typeof(byte[]), ODBC32.SQL_TYPE.BINARY, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, signType: false);

	private static readonly TypeMap s_tinyInt = new TypeMap(OdbcType.TinyInt, DbType.Byte, typeof(byte), ODBC32.SQL_TYPE.TINYINT, ODBC32.SQL_C.UTINYINT, ODBC32.SQL_C.UTINYINT, 1, 3, signType: true);

	private static readonly TypeMap s_varBinary = new TypeMap(OdbcType.VarBinary, DbType.Binary, typeof(byte[]), ODBC32.SQL_TYPE.VARBINARY, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, signType: false);

	internal static readonly TypeMap _VarChar = new TypeMap(OdbcType.VarChar, DbType.AnsiString, typeof(string), ODBC32.SQL_TYPE.VARCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.CHAR, -1, -1, signType: false);

	private static readonly TypeMap s_variant = new TypeMap(OdbcType.Binary, DbType.Binary, typeof(object), ODBC32.SQL_TYPE.SS_VARIANT, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, signType: false);

	private static readonly TypeMap s_UDT = new TypeMap(OdbcType.Binary, DbType.Binary, typeof(object), ODBC32.SQL_TYPE.SS_UDT, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, signType: false);

	private static readonly TypeMap s_XML = new TypeMap(OdbcType.Text, DbType.AnsiString, typeof(string), ODBC32.SQL_TYPE.LONGVARCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.CHAR, -1, -1, signType: false);

	internal readonly OdbcType _odbcType;

	internal readonly DbType _dbType;

	internal readonly Type _type;

	internal readonly ODBC32.SQL_TYPE _sql_type;

	internal readonly ODBC32.SQL_C _sql_c;

	internal readonly ODBC32.SQL_C _param_sql_c;

	internal readonly int _bufferSize;

	internal readonly int _columnSize;

	internal readonly bool _signType;

	private TypeMap(OdbcType odbcType, DbType dbType, Type type, ODBC32.SQL_TYPE sql_type, ODBC32.SQL_C sql_c, ODBC32.SQL_C param_sql_c, int bsize, int csize, bool signType)
	{
		_odbcType = odbcType;
		_dbType = dbType;
		_type = type;
		_sql_type = sql_type;
		_sql_c = sql_c;
		_param_sql_c = param_sql_c;
		_bufferSize = bsize;
		_columnSize = csize;
		_signType = signType;
	}

	internal static TypeMap FromOdbcType(OdbcType odbcType)
	{
		return odbcType switch
		{
			OdbcType.BigInt => s_bigInt, 
			OdbcType.Binary => s_binary, 
			OdbcType.Bit => s_bit, 
			OdbcType.Char => _Char, 
			OdbcType.DateTime => s_dateTime, 
			OdbcType.Date => s_date, 
			OdbcType.Time => s_time, 
			OdbcType.Double => s_double, 
			OdbcType.Decimal => s_decimal, 
			OdbcType.Image => _Image, 
			OdbcType.Int => s_int, 
			OdbcType.NChar => s_NChar, 
			OdbcType.NText => _NText, 
			OdbcType.Numeric => s_numeric, 
			OdbcType.NVarChar => _NVarChar, 
			OdbcType.Real => s_real, 
			OdbcType.UniqueIdentifier => s_uniqueId, 
			OdbcType.SmallDateTime => s_smallDT, 
			OdbcType.SmallInt => s_smallInt, 
			OdbcType.Text => _Text, 
			OdbcType.Timestamp => s_timestamp, 
			OdbcType.TinyInt => s_tinyInt, 
			OdbcType.VarBinary => s_varBinary, 
			OdbcType.VarChar => _VarChar, 
			_ => throw ODBC.UnknownOdbcType(odbcType), 
		};
	}

	internal static TypeMap FromDbType(DbType dbType)
	{
		return dbType switch
		{
			DbType.AnsiString => _VarChar, 
			DbType.AnsiStringFixedLength => _Char, 
			DbType.Binary => s_varBinary, 
			DbType.Byte => s_tinyInt, 
			DbType.Boolean => s_bit, 
			DbType.Currency => s_decimal, 
			DbType.Date => s_date, 
			DbType.Time => s_time, 
			DbType.DateTime => s_dateTime, 
			DbType.Decimal => s_decimal, 
			DbType.Double => s_double, 
			DbType.Guid => s_uniqueId, 
			DbType.Int16 => s_smallInt, 
			DbType.Int32 => s_int, 
			DbType.Int64 => s_bigInt, 
			DbType.Single => s_real, 
			DbType.String => _NVarChar, 
			DbType.StringFixedLength => s_NChar, 
			_ => throw ADP.DbTypeNotSupported(dbType, typeof(OdbcType)), 
		};
	}

	internal static TypeMap FromSystemType(Type dataType)
	{
		switch (Type.GetTypeCode(dataType))
		{
		case TypeCode.Empty:
			throw ADP.InvalidDataType(TypeCode.Empty);
		case TypeCode.Object:
			if (dataType == typeof(byte[]))
			{
				return s_varBinary;
			}
			if (dataType == typeof(Guid))
			{
				return s_uniqueId;
			}
			if (dataType == typeof(TimeSpan))
			{
				return s_time;
			}
			if (dataType == typeof(char[]))
			{
				return _NVarChar;
			}
			throw ADP.UnknownDataType(dataType);
		case TypeCode.DBNull:
			throw ADP.InvalidDataType(TypeCode.DBNull);
		case TypeCode.Boolean:
			return s_bit;
		case TypeCode.SByte:
			return s_smallInt;
		case TypeCode.Byte:
			return s_tinyInt;
		case TypeCode.Int16:
			return s_smallInt;
		case TypeCode.UInt16:
			return s_int;
		case TypeCode.Int32:
			return s_int;
		case TypeCode.UInt32:
			return s_bigInt;
		case TypeCode.Int64:
			return s_bigInt;
		case TypeCode.UInt64:
			return s_numeric;
		case TypeCode.Single:
			return s_real;
		case TypeCode.Double:
			return s_double;
		case TypeCode.Decimal:
			return s_numeric;
		case TypeCode.DateTime:
			return s_dateTime;
		case TypeCode.Char:
		case TypeCode.String:
			return _NVarChar;
		default:
			throw ADP.UnknownDataTypeCode(dataType, Type.GetTypeCode(dataType));
		}
	}

	internal static TypeMap FromSqlType(ODBC32.SQL_TYPE sqltype)
	{
		switch (sqltype)
		{
		case ODBC32.SQL_TYPE.CHAR:
			return _Char;
		case ODBC32.SQL_TYPE.VARCHAR:
			return _VarChar;
		case ODBC32.SQL_TYPE.LONGVARCHAR:
			return _Text;
		case ODBC32.SQL_TYPE.WCHAR:
			return s_NChar;
		case ODBC32.SQL_TYPE.WVARCHAR:
			return _NVarChar;
		case ODBC32.SQL_TYPE.WLONGVARCHAR:
			return _NText;
		case ODBC32.SQL_TYPE.DECIMAL:
			return s_decimal;
		case ODBC32.SQL_TYPE.NUMERIC:
			return s_numeric;
		case ODBC32.SQL_TYPE.SMALLINT:
			return s_smallInt;
		case ODBC32.SQL_TYPE.INTEGER:
			return s_int;
		case ODBC32.SQL_TYPE.REAL:
			return s_real;
		case ODBC32.SQL_TYPE.FLOAT:
			return s_double;
		case ODBC32.SQL_TYPE.DOUBLE:
			return s_double;
		case ODBC32.SQL_TYPE.BIT:
			return s_bit;
		case ODBC32.SQL_TYPE.TINYINT:
			return s_tinyInt;
		case ODBC32.SQL_TYPE.BIGINT:
			return s_bigInt;
		case ODBC32.SQL_TYPE.BINARY:
			return s_binary;
		case ODBC32.SQL_TYPE.VARBINARY:
			return s_varBinary;
		case ODBC32.SQL_TYPE.LONGVARBINARY:
			return _Image;
		case ODBC32.SQL_TYPE.TYPE_DATE:
			return s_date;
		case ODBC32.SQL_TYPE.TYPE_TIME:
			return s_time;
		case ODBC32.SQL_TYPE.TIMESTAMP:
		case ODBC32.SQL_TYPE.TYPE_TIMESTAMP:
			return s_dateTime;
		case ODBC32.SQL_TYPE.GUID:
			return s_uniqueId;
		case ODBC32.SQL_TYPE.SS_VARIANT:
			return s_variant;
		case ODBC32.SQL_TYPE.SS_UDT:
			return s_UDT;
		case ODBC32.SQL_TYPE.SS_XML:
			return s_XML;
		case ODBC32.SQL_TYPE.SS_TIME_EX:
		case ODBC32.SQL_TYPE.SS_UTCDATETIME:
			throw ODBC.UnknownSQLType(sqltype);
		default:
			throw ODBC.UnknownSQLType(sqltype);
		}
	}

	internal static TypeMap UpgradeSignedType(TypeMap typeMap, bool unsigned)
	{
		if (unsigned)
		{
			return typeMap._dbType switch
			{
				DbType.Int64 => s_decimal, 
				DbType.Int32 => s_bigInt, 
				DbType.Int16 => s_int, 
				_ => typeMap, 
			};
		}
		if (typeMap._dbType == DbType.Byte)
		{
			return s_smallInt;
		}
		return typeMap;
	}
}
