namespace System.Data.Odbc;

/// <summary>Specifies the data type of a field, property, for use in an <see cref="T:System.Data.Odbc.OdbcParameter" />.</summary>
/// <filterpriority>2</filterpriority>
public enum OdbcType
{
	/// <summary>Exact numeric value with precision 19 (if signed) or 20 (if unsigned) and scale 0 (signed: –2[63] &lt;= n &lt;= 2[63] – 1, unsigned:0 &lt;= n &lt;= 2[64] – 1) (SQL_BIGINT). This maps to <see cref="T:System.Int64" />.</summary>
	BigInt = 1,
	/// <summary>A stream of binary data (SQL_BINARY). This maps to an <see cref="T:System.Array" /> of type <see cref="T:System.Byte" />.</summary>
	Binary,
	/// <summary>Single bit binary data (SQL_BIT). This maps to <see cref="T:System.Boolean" />.</summary>
	Bit,
	/// <summary>A fixed-length character string (SQL_CHAR). This maps to <see cref="T:System.String" />.</summary>
	Char,
	/// <summary>Date data in the format yyyymmddhhmmss (SQL_TYPE_TIMESTAMP). This maps to <see cref="T:System.DateTime" />.</summary>
	DateTime,
	/// <summary>Signed, exact, numeric value with a precision of at least p and scale s, where 1 &lt;= p &lt;= 15 and s &lt;= p. The maximum precision is driver-specific (SQL_DECIMAL). This maps to <see cref="T:System.Decimal" />.</summary>
	Decimal,
	/// <summary>Signed, exact, numeric value with a precision p and scale s, where 1 &lt;= p &lt;= 15, and s &lt;= p (SQL_NUMERIC). This maps to <see cref="T:System.Decimal" />.</summary>
	Numeric,
	/// <summary>Signed, approximate, numeric value with a binary precision 53 (zero or absolute value 10[–308] to 10[308]) (SQL_DOUBLE). This maps to <see cref="T:System.Double" />.</summary>
	Double,
	/// <summary>Variable length binary data. Maximum length is data source–dependent (SQL_LONGVARBINARY). This maps to an <see cref="T:System.Array" /> of type <see cref="T:System.Byte" />.</summary>
	Image,
	/// <summary>Exact numeric value with precision 10 and scale 0 (signed: –2[31] &lt;= n &lt;= 2[31] – 1, unsigned:0 &lt;= n &lt;= 2[32] – 1) (SQL_INTEGER). This maps to <see cref="T:System.Int32" />.</summary>
	Int,
	/// <summary>Unicode character string of fixed string length (SQL_WCHAR). This maps to <see cref="T:System.String" />.</summary>
	NChar,
	/// <summary>Unicode variable-length character data. Maximum length is data source–dependent. (SQL_WLONGVARCHAR). This maps to <see cref="T:System.String" />.</summary>
	NText,
	/// <summary>A variable-length stream of Unicode characters (SQL_WVARCHAR). This maps to <see cref="T:System.String" />.</summary>
	NVarChar,
	/// <summary>Signed, approximate, numeric value with a binary precision 24 (zero or absolute value 10[–38] to 10[38]).(SQL_REAL). This maps to <see cref="T:System.Single" />.</summary>
	Real,
	/// <summary>A fixed-length GUID (SQL_GUID). This maps to <see cref="T:System.Guid" />.</summary>
	UniqueIdentifier,
	/// <summary>Data and time data in the format yyyymmddhhmmss (SQL_TYPE_TIMESTAMP). This maps to <see cref="T:System.DateTime" />.</summary>
	SmallDateTime,
	/// <summary>Exact numeric value with precision 5 and scale 0 (signed: –32,768 &lt;= n &lt;= 32,767, unsigned: 0 &lt;= n &lt;= 65,535) (SQL_SMALLINT). This maps to <see cref="T:System.Int16" />.</summary>
	SmallInt,
	/// <summary>Variable length character data. Maximum length is data source–dependent (SQL_LONGVARCHAR). This maps to <see cref="T:System.String" />.</summary>
	Text,
	/// <summary>A stream of binary data (SQL_BINARY). This maps to an <see cref="T:System.Array" /> of type <see cref="T:System.Byte" />.</summary>
	Timestamp,
	/// <summary>Exact numeric value with precision 3 and scale 0 (signed: –128 &lt;= n &lt;= 127, unsigned:0 &lt;= n &lt;= 255)(SQL_TINYINT). This maps to <see cref="T:System.Byte" />.</summary>
	TinyInt,
	/// <summary>Variable length binary. The maximum is set by the user (SQL_VARBINARY). This maps to an <see cref="T:System.Array" /> of type <see cref="T:System.Byte" />.</summary>
	VarBinary,
	/// <summary>A variable-length stream character string (SQL_CHAR). This maps to <see cref="T:System.String" />.</summary>
	VarChar,
	/// <summary>Date data in the format yyyymmdd (SQL_TYPE_DATE). This maps to <see cref="T:System.DateTime" />.</summary>
	Date,
	/// <summary>Date data in the format hhmmss (SQL_TYPE_TIMES). This maps to <see cref="T:System.DateTime" />.</summary>
	Time
}
