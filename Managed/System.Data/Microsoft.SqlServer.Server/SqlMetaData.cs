using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;

namespace Microsoft.SqlServer.Server;

/// <summary>Specifies and retrieves metadata information from parameters and columns of <see cref="T:Microsoft.SqlServer.Server.SqlDataRecord" /> objects. This class cannot be inherited.</summary>
public sealed class SqlMetaData
{
	private string _strName;

	private long _lMaxLength;

	private SqlDbType _sqlDbType;

	private byte _bPrecision;

	private byte _bScale;

	private long _lLocale;

	private SqlCompareOptions _eCompareOptions;

	private string _xmlSchemaCollectionDatabase;

	private string _xmlSchemaCollectionOwningSchema;

	private string _xmlSchemaCollectionName;

	private string _serverTypeName;

	private bool _bPartialLength;

	private Type _udtType;

	private bool _useServerDefault;

	private bool _isUniqueKey;

	private SortOrder _columnSortOrder;

	private int _sortOrdinal;

	private const long x_lMax = -1L;

	private const long x_lServerMaxUnicode = 4000L;

	private const long x_lServerMaxANSI = 8000L;

	private const long x_lServerMaxBinary = 8000L;

	private const bool x_defaultUseServerDefault = false;

	private const bool x_defaultIsUniqueKey = false;

	private const SortOrder x_defaultColumnSortOrder = SortOrder.Unspecified;

	private const int x_defaultSortOrdinal = -1;

	private const SqlCompareOptions x_eDefaultStringCompareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;

	private static byte[] s_maxLenFromPrecision = new byte[38]
	{
		5, 5, 5, 5, 5, 5, 5, 5, 5, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 13,
		13, 13, 13, 13, 13, 13, 13, 13, 17, 17,
		17, 17, 17, 17, 17, 17, 17, 17
	};

	private const byte MaxTimeScale = 7;

	private static byte[] s_maxVarTimeLenOffsetFromScale = new byte[8] { 2, 2, 2, 1, 1, 0, 0, 0 };

	private static readonly DateTime s_dtSmallMax = new DateTime(2079, 6, 6, 23, 59, 29, 998);

	private static readonly DateTime s_dtSmallMin = new DateTime(1899, 12, 31, 23, 59, 29, 999);

	private static readonly SqlMoney s_smSmallMax = new SqlMoney(214748.3647m);

	private static readonly SqlMoney s_smSmallMin = new SqlMoney(-214748.3648m);

	private static readonly TimeSpan s_timeMin = TimeSpan.Zero;

	private static readonly TimeSpan s_timeMax = new TimeSpan(863999999999L);

	private static readonly long[] s_unitTicksFromScale = new long[8] { 10000000L, 1000000L, 100000L, 10000L, 1000L, 100L, 10L, 1L };

	private static DbType[] sxm_rgSqlDbTypeToDbType = new DbType[35]
	{
		DbType.Int64,
		DbType.Binary,
		DbType.Boolean,
		DbType.AnsiString,
		DbType.DateTime,
		DbType.Decimal,
		DbType.Double,
		DbType.Binary,
		DbType.Int32,
		DbType.Currency,
		DbType.String,
		DbType.String,
		DbType.String,
		DbType.Single,
		DbType.Guid,
		DbType.DateTime,
		DbType.Int16,
		DbType.Currency,
		DbType.AnsiString,
		DbType.Binary,
		DbType.Byte,
		DbType.Binary,
		DbType.AnsiString,
		DbType.Object,
		DbType.Object,
		DbType.Xml,
		DbType.String,
		DbType.String,
		DbType.String,
		DbType.Object,
		DbType.Object,
		DbType.Date,
		DbType.Time,
		DbType.DateTime2,
		DbType.DateTimeOffset
	};

	internal static SqlMetaData[] sxm_rgDefaults = new SqlMetaData[35]
	{
		new SqlMetaData("bigint", SqlDbType.BigInt, 8L, 19, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("binary", SqlDbType.Binary, 1L, 0, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("bit", SqlDbType.Bit, 1L, 1, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("char", SqlDbType.Char, 1L, 0, 0, 0L, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth, partialLength: false),
		new SqlMetaData("datetime", SqlDbType.DateTime, 8L, 23, 3, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("decimal", SqlDbType.Decimal, 9L, 18, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("float", SqlDbType.Float, 8L, 53, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("image", SqlDbType.Image, -1L, 0, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("int", SqlDbType.Int, 4L, 10, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("money", SqlDbType.Money, 8L, 19, 4, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("nchar", SqlDbType.NChar, 1L, 0, 0, 0L, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth, partialLength: false),
		new SqlMetaData("ntext", SqlDbType.NText, -1L, 0, 0, 0L, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth, partialLength: false),
		new SqlMetaData("nvarchar", SqlDbType.NVarChar, 4000L, 0, 0, 0L, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth, partialLength: false),
		new SqlMetaData("real", SqlDbType.Real, 4L, 24, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("uniqueidentifier", SqlDbType.UniqueIdentifier, 16L, 0, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("smalldatetime", SqlDbType.SmallDateTime, 4L, 16, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("smallint", SqlDbType.SmallInt, 2L, 5, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("smallmoney", SqlDbType.SmallMoney, 4L, 10, 4, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("text", SqlDbType.Text, -1L, 0, 0, 0L, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth, partialLength: false),
		new SqlMetaData("timestamp", SqlDbType.Timestamp, 8L, 0, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("tinyint", SqlDbType.TinyInt, 1L, 3, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("varbinary", SqlDbType.VarBinary, 8000L, 0, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("varchar", SqlDbType.VarChar, 8000L, 0, 0, 0L, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth, partialLength: false),
		new SqlMetaData("sql_variant", SqlDbType.Variant, 8016L, 0, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("nvarchar", SqlDbType.NVarChar, 1L, 0, 0, 0L, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth, partialLength: false),
		new SqlMetaData("xml", SqlDbType.Xml, -1L, 0, 0, 0L, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth, partialLength: true),
		new SqlMetaData("nvarchar", SqlDbType.NVarChar, 1L, 0, 0, 0L, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth, partialLength: false),
		new SqlMetaData("nvarchar", SqlDbType.NVarChar, 4000L, 0, 0, 0L, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth, partialLength: false),
		new SqlMetaData("nvarchar", SqlDbType.NVarChar, 4000L, 0, 0, 0L, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth, partialLength: false),
		new SqlMetaData("udt", SqlDbType.Udt, 0L, 0, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("table", SqlDbType.Structured, 0L, 0, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("date", SqlDbType.Date, 3L, 10, 0, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("time", SqlDbType.Time, 5L, 0, 7, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("datetime2", SqlDbType.DateTime2, 8L, 0, 7, 0L, SqlCompareOptions.None, partialLength: false),
		new SqlMetaData("datetimeoffset", SqlDbType.DateTimeOffset, 10L, 0, 7, 0L, SqlCompareOptions.None, partialLength: false)
	};

	/// <summary>Gets the comparison rules used for the column or parameter.</summary>
	/// <returns>The comparison rules used for the column or parameter as a <see cref="T:System.Data.SqlTypes.SqlCompareOptions" />.</returns>
	public SqlCompareOptions CompareOptions => _eCompareOptions;

	/// <summary>Gets the data type of the column or parameter.</summary>
	/// <returns>The data type of the column or parameter as a <see cref="T:System.Data.DbType" />.</returns>
	public DbType DbType => sxm_rgSqlDbTypeToDbType[(int)_sqlDbType];

	/// <summary>Indicates if the column in the table-valued parameter is unique.</summary>
	/// <returns>A Boolean value.</returns>
	public bool IsUniqueKey => _isUniqueKey;

	/// <summary>Gets the locale ID of the column or parameter.</summary>
	/// <returns>The locale ID of the column or parameter as a <see cref="T:System.Int64" />.</returns>
	public long LocaleId => _lLocale;

	/// <summary>Gets the length of text, ntext, and image data types. </summary>
	/// <returns>The length of text, ntext, and image data types.</returns>
	public static long Max => -1L;

	/// <summary>Gets the maximum length of the column or parameter.</summary>
	/// <returns>The maximum length of the column or parameter as a <see cref="T:System.Int64" />.</returns>
	public long MaxLength => _lMaxLength;

	/// <summary>Gets the name of the column or parameter.</summary>
	/// <returns>The name of the column or parameter as a <see cref="T:System.String" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="Name" /> specified in the constructor is longer than 128 characters. </exception>
	public string Name => _strName;

	/// <summary>Gets the precision of the column or parameter.</summary>
	/// <returns>The precision of the column or parameter as a <see cref="T:System.Byte" />.</returns>
	public byte Precision => _bPrecision;

	/// <summary>Gets the scale of the column or parameter.</summary>
	/// <returns>The scale of the column or parameter.</returns>
	public byte Scale => _bScale;

	/// <summary>Returns the sort order for a column.</summary>
	/// <returns>A <see cref="T:System.Data.SqlClient.SortOrder" /> object.</returns>
	public SortOrder SortOrder => _columnSortOrder;

	/// <summary>Returns the ordinal of the sort column.</summary>
	/// <returns>The ordinal of the sort column.</returns>
	public int SortOrdinal => _sortOrdinal;

	/// <summary>Gets the data type of the column or parameter.</summary>
	/// <returns>The data type of the column or parameter as a <see cref="T:System.Data.DbType" />.</returns>
	public SqlDbType SqlDbType => _sqlDbType;

	/// <summary>Gets the common language runtime (CLR) type of a user-defined type (UDT).</summary>
	/// <returns>The CLR type name of a user-defined type as a <see cref="T:System.Type" />.</returns>
	public Type Type => _udtType;

	/// <summary>Gets the three-part name of the user-defined type (UDT) or the SQL Server type represented by the instance.</summary>
	/// <returns>The name of the UDT or SQL Server type as a <see cref="T:System.String" />.</returns>
	public string TypeName
	{
		get
		{
			if (_serverTypeName != null)
			{
				return _serverTypeName;
			}
			if (SqlDbType == SqlDbType.Udt)
			{
				return UdtTypeName;
			}
			return sxm_rgDefaults[(int)SqlDbType].Name;
		}
	}

	internal string ServerTypeName => _serverTypeName;

	/// <summary>Reports whether this column should use the default server value.</summary>
	/// <returns>A Boolean value.</returns>
	public bool UseServerDefault => _useServerDefault;

	/// <summary>Gets the name of the database where the schema collection for this XML instance is located.</summary>
	/// <returns>The name of the database where the schema collection for this XML instance is located as a <see cref="T:System.String" />.</returns>
	public string XmlSchemaCollectionDatabase => _xmlSchemaCollectionDatabase;

	/// <summary>Gets the name of the schema collection for this XML instance.</summary>
	/// <returns>The name of the schema collection for this XML instance as a <see cref="T:System.String" />.</returns>
	public string XmlSchemaCollectionName => _xmlSchemaCollectionName;

	/// <summary>Gets the owning relational schema where the schema collection for this XML instance is located.</summary>
	/// <returns>The owning relational schema where the schema collection for this XML instance is located as a <see cref="T:System.String" />.</returns>
	public string XmlSchemaCollectionOwningSchema => _xmlSchemaCollectionOwningSchema;

	internal bool IsPartialLength => _bPartialLength;

	internal string UdtTypeName
	{
		get
		{
			if (SqlDbType != SqlDbType.Udt)
			{
				return null;
			}
			if (_udtType == null)
			{
				return null;
			}
			return _udtType.FullName;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name and type.</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="Name" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">A SqlDbType that is not allowed was passed to the constructor as <paramref name="dbType" />.</exception>
	public SqlMetaData(string name, SqlDbType dbType)
	{
		Construct(name, dbType, useServerDefault: false, isUniqueKey: false, SortOrder.Unspecified, -1);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, and default server. This form of the constructor supports table-valued parameters by allowing you to specify if the column is unique in the table-valued parameter, the sort order for the column, and the ordinal of the sort column.</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="useServerDefault">Specifes whether this column should use the default server value.</param>
	/// <param name="isUniqueKey">Specifies if the column in the table-valued parameter is unique.</param>
	/// <param name="columnSortOrder">Specifies the sort order for a column.</param>
	/// <param name="sortOrdinal">Specifies the ordinal of the sort column.</param>
	public SqlMetaData(string name, SqlDbType dbType, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		Construct(name, dbType, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, type, and maximum length.</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="maxLength">The maximum length of the specified type.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="Name" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">A SqlDbType that is not allowed was passed to the constructor as <paramref name="dbType" />.</exception>
	public SqlMetaData(string name, SqlDbType dbType, long maxLength)
	{
		Construct(name, dbType, maxLength, useServerDefault: false, isUniqueKey: false, SortOrder.Unspecified, -1);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, type, maximum length, and server default. This form of the constructor supports table-valued parameters by allowing you to specify if the column is unique in the table-valued parameter, the sort order for the column, and the ordinal of the sort column.</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="maxLength">The maximum length of the specified type.</param>
	/// <param name="useServerDefault">Specifes whether this column should use the default server value.</param>
	/// <param name="isUniqueKey">Specifies if the column in the table-valued parameter is unique.</param>
	/// <param name="columnSortOrder">Specifies the sort order for a column.</param>
	/// <param name="sortOrdinal">Specifies the ordinal of the sort column.</param>
	public SqlMetaData(string name, SqlDbType dbType, long maxLength, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		Construct(name, dbType, maxLength, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, type, and user-defined type (UDT).</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="userDefinedType">A <see cref="T:System.Type" /> instance that points to the UDT.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="Name" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">A SqlDbType that is not allowed was passed to the constructor as <paramref name="dbType" />, or <paramref name="userDefinedType" /> points to a type that does not have <see cref="T:Microsoft.SqlServer.Server.SqlUserDefinedTypeAttribute" /> declared. </exception>
	public SqlMetaData(string name, SqlDbType dbType, Type userDefinedType)
	{
		Construct(name, dbType, userDefinedType, null, useServerDefault: false, isUniqueKey: false, SortOrder.Unspecified, -1);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, user-defined type (UDT), and SQLServer type.</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="userDefinedType">A <see cref="T:System.Type" /> instance that points to the UDT.</param>
	/// <param name="serverTypeName">The SQL Server type name for <paramref name="userDefinedType" />.</param>
	public SqlMetaData(string name, SqlDbType dbType, Type userDefinedType, string serverTypeName)
	{
		Construct(name, dbType, userDefinedType, serverTypeName, useServerDefault: false, isUniqueKey: false, SortOrder.Unspecified, -1);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, type, user-defined type, SQL Server type, and server default. This form of the constructor supports table-valued parameters by allowing you to specify if the column is unique in the table-valued parameter, the sort order for the column, and the ordinal of the sort column.</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="userDefinedType">A <see cref="T:System.Type" /> instance that points to the UDT.</param>
	/// <param name="serverTypeName">The SQL Server type name for <paramref name="userDefinedType" />.</param>
	/// <param name="useServerDefault">Specifes whether this column should use the default server value.</param>
	/// <param name="isUniqueKey">Specifies if the column in the table-valued parameter is unique.</param>
	/// <param name="columnSortOrder">Specifies the sort order for a column.</param>
	/// <param name="sortOrdinal">Specifies the ordinal of the sort column.</param>
	public SqlMetaData(string name, SqlDbType dbType, Type userDefinedType, string serverTypeName, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		Construct(name, dbType, userDefinedType, serverTypeName, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, type, precision, and scale.</summary>
	/// <param name="name">The name of the parameter or column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="precision">The precision of the parameter or column.</param>
	/// <param name="scale">The scale of the parameter or column.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="Name" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">A SqlDbType that is not allowed was passed to the constructor as <paramref name="dbType" />, or <paramref name="scale" /> was greater than <paramref name="precision" />. </exception>
	public SqlMetaData(string name, SqlDbType dbType, byte precision, byte scale)
	{
		Construct(name, dbType, precision, scale, useServerDefault: false, isUniqueKey: false, SortOrder.Unspecified, -1);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, type, precision, scale, and server default. This form of the constructor supports table-valued parameters by allowing you to specify if the column is unique in the table-valued parameter, the sort order for the column, and the ordinal of the sort column.</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="precision">The precision of the parameter or column.</param>
	/// <param name="scale">The scale of the parameter or column.</param>
	/// <param name="useServerDefault">Specifes whether this column should use the default server value.</param>
	/// <param name="isUniqueKey">Specifies if the column in the table-valued parameter is unique.</param>
	/// <param name="columnSortOrder">Specifies the sort order for a column.</param>
	/// <param name="sortOrdinal">Specifies the ordinal of the sort column.</param>
	public SqlMetaData(string name, SqlDbType dbType, byte precision, byte scale, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		Construct(name, dbType, precision, scale, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, type, maximum length, locale, and compare options.</summary>
	/// <param name="name">The name of the parameter or column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="maxLength">The maximum length of the specified type. </param>
	/// <param name="locale">The locale ID of the parameter or column.</param>
	/// <param name="compareOptions">The comparison rules of the parameter or column.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="Name" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">A SqlDbType that is not allowed was passed to the constructor as <paramref name="dbType" />.</exception>
	public SqlMetaData(string name, SqlDbType dbType, long maxLength, long locale, SqlCompareOptions compareOptions)
	{
		Construct(name, dbType, maxLength, locale, compareOptions, useServerDefault: false, isUniqueKey: false, SortOrder.Unspecified, -1);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, type, maximum length, locale, compare options, and server default. This form of the constructor supports table-valued parameters by allowing you to specify if the column is unique in the table-valued parameter, the sort order for the column, and the ordinal of the sort column.</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="maxLength">The maximum length of the specified type.</param>
	/// <param name="locale">The locale ID of the parameter or column.</param>
	/// <param name="compareOptions">The comparison rules of the parameter or column.</param>
	/// <param name="useServerDefault">Specifes whether this column should use the default server value.</param>
	/// <param name="isUniqueKey">Specifies if the column in the table-valued parameter is unique.</param>
	/// <param name="columnSortOrder">Specifies the sort order for a column.</param>
	/// <param name="sortOrdinal">Specifies the ordinal of the sort column.</param>
	public SqlMetaData(string name, SqlDbType dbType, long maxLength, long locale, SqlCompareOptions compareOptions, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		Construct(name, dbType, maxLength, locale, compareOptions, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, database name, owning schema, object name, and default server. This form of the constructor supports table-valued parameters by allowing you to specify if the column is unique in the table-valued parameter, the sort order for the column, and the ordinal of the sort column.</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="database">The database name of the XML schema collection of a typed XML instance.</param>
	/// <param name="owningSchema">The relational schema name of the XML schema collection of a typed XML instance.</param>
	/// <param name="objectName">The name of the XML schema collection of a typed XML instance.</param>
	/// <param name="useServerDefault">Specifes whether this column should use the default server value.</param>
	/// <param name="isUniqueKey">Specifies if the column in the table-valued parameter is unique.</param>
	/// <param name="columnSortOrder">Specifies the sort order for a column.</param>
	/// <param name="sortOrdinal">Specifies the ordinal of the sort column.</param>
	public SqlMetaData(string name, SqlDbType dbType, string database, string owningSchema, string objectName, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		Construct(name, dbType, database, owningSchema, objectName, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, type, maximum length, precision, scale, locale ID, compare options, and user-defined type (UDT).</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="maxLength">The maximum length of the specified type.</param>
	/// <param name="precision">The precision of the parameter or column.</param>
	/// <param name="scale">The scale of the parameter or column.</param>
	/// <param name="locale">The locale ID of the parameter or column.</param>
	/// <param name="compareOptions">The comparison rules of the parameter or column.</param>
	/// <param name="userDefinedType">A <see cref="T:System.Type" /> instance that points to the UDT.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="Name" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">A SqlDbType that is not allowed was passed to the constructor as <paramref name="dbType" />, or <paramref name="userDefinedType" /> points to a type that does not have <see cref="T:Microsoft.SqlServer.Server.SqlUserDefinedTypeAttribute" /> declared.</exception>
	public SqlMetaData(string name, SqlDbType dbType, long maxLength, byte precision, byte scale, long locale, SqlCompareOptions compareOptions, Type userDefinedType)
		: this(name, dbType, maxLength, precision, scale, locale, compareOptions, userDefinedType, useServerDefault: false, isUniqueKey: false, SortOrder.Unspecified, -1)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, type, maximum length, precision, scale, locale ID, compare options, and user-defined type (UDT). This form of the constructor supports table-valued parameters by allowing you to specify if the column is unique in the table-valued parameter, the sort order for the column, and the ordinal of the sort column.</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="maxLength">The maximum length of the specified type.</param>
	/// <param name="precision">The precision of the parameter or column.</param>
	/// <param name="scale">The scale of the parameter or column.</param>
	/// <param name="localeId">The locale ID of the parameter or column.</param>
	/// <param name="compareOptions">The comparison rules of the parameter or column.</param>
	/// <param name="userDefinedType">A <see cref="T:System.Type" /> instance that points to the UDT.</param>
	/// <param name="useServerDefault">Specifes whether this column should use the default server value.</param>
	/// <param name="isUniqueKey">Specifies if the column in the table-valued parameter is unique.</param>
	/// <param name="columnSortOrder">Specifies the sort order for a column.</param>
	/// <param name="sortOrdinal">Specifies the ordinal of the sort column.</param>
	public SqlMetaData(string name, SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, Type userDefinedType, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		switch (dbType)
		{
		case SqlDbType.BigInt:
		case SqlDbType.Bit:
		case SqlDbType.DateTime:
		case SqlDbType.Float:
		case SqlDbType.Image:
		case SqlDbType.Int:
		case SqlDbType.Money:
		case SqlDbType.Real:
		case SqlDbType.UniqueIdentifier:
		case SqlDbType.SmallDateTime:
		case SqlDbType.SmallInt:
		case SqlDbType.SmallMoney:
		case SqlDbType.Timestamp:
		case SqlDbType.TinyInt:
		case SqlDbType.Xml:
		case SqlDbType.Date:
			Construct(name, dbType, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
			break;
		case SqlDbType.Binary:
		case SqlDbType.VarBinary:
			Construct(name, dbType, maxLength, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
			break;
		case SqlDbType.Char:
		case SqlDbType.NChar:
		case SqlDbType.NVarChar:
		case SqlDbType.VarChar:
			Construct(name, dbType, maxLength, localeId, compareOptions, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
			break;
		case SqlDbType.NText:
		case SqlDbType.Text:
			Construct(name, dbType, Max, localeId, compareOptions, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
			break;
		case SqlDbType.Decimal:
		case SqlDbType.Time:
		case SqlDbType.DateTime2:
		case SqlDbType.DateTimeOffset:
			Construct(name, dbType, precision, scale, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
			break;
		case SqlDbType.Variant:
			Construct(name, dbType, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
			break;
		case SqlDbType.Udt:
			Construct(name, dbType, userDefinedType, "", useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
			break;
		default:
			SQL.InvalidSqlDbTypeForConstructor(dbType);
			break;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> class with the specified column name, type, database name, owning schema, and object name.</summary>
	/// <param name="name">The name of the column.</param>
	/// <param name="dbType">The SQL Server type of the parameter or column.</param>
	/// <param name="database">The database name of the XML schema collection of a typed XML instance.</param>
	/// <param name="owningSchema">The relational schema name of the XML schema collection of a typed XML instance.</param>
	/// <param name="objectName">The name of the XML schema collection of a typed XML instance.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="Name" /> is null, or <paramref name="objectName" /> is null when <paramref name="database" /> and <paramref name="owningSchema" /> are non-null.</exception>
	/// <exception cref="T:System.ArgumentException">A SqlDbType that is not allowed was passed to the constructor as <paramref name="dbType" />.</exception>
	public SqlMetaData(string name, SqlDbType dbType, string database, string owningSchema, string objectName)
	{
		Construct(name, dbType, database, owningSchema, objectName, useServerDefault: false, isUniqueKey: false, SortOrder.Unspecified, -1);
	}

	internal SqlMetaData(string name, SqlDbType sqlDBType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, string xmlSchemaCollectionDatabase, string xmlSchemaCollectionOwningSchema, string xmlSchemaCollectionName, bool partialLength, Type udtType)
	{
		AssertNameIsValid(name);
		_strName = name;
		_sqlDbType = sqlDBType;
		_lMaxLength = maxLength;
		_bPrecision = precision;
		_bScale = scale;
		_lLocale = localeId;
		_eCompareOptions = compareOptions;
		_xmlSchemaCollectionDatabase = xmlSchemaCollectionDatabase;
		_xmlSchemaCollectionOwningSchema = xmlSchemaCollectionOwningSchema;
		_xmlSchemaCollectionName = xmlSchemaCollectionName;
		_bPartialLength = partialLength;
		_udtType = udtType;
	}

	private SqlMetaData(string name, SqlDbType sqlDbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, bool partialLength)
	{
		AssertNameIsValid(name);
		_strName = name;
		_sqlDbType = sqlDbType;
		_lMaxLength = maxLength;
		_bPrecision = precision;
		_bScale = scale;
		_lLocale = localeId;
		_eCompareOptions = compareOptions;
		_bPartialLength = partialLength;
		_udtType = null;
	}

	private void Construct(string name, SqlDbType dbType, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		AssertNameIsValid(name);
		ValidateSortOrder(columnSortOrder, sortOrdinal);
		if (dbType != SqlDbType.BigInt && SqlDbType.Bit != dbType && SqlDbType.DateTime != dbType && SqlDbType.Date != dbType && SqlDbType.DateTime2 != dbType && SqlDbType.DateTimeOffset != dbType && SqlDbType.Decimal != dbType && SqlDbType.Float != dbType && SqlDbType.Image != dbType && SqlDbType.Int != dbType && SqlDbType.Money != dbType && SqlDbType.NText != dbType && SqlDbType.Real != dbType && SqlDbType.SmallDateTime != dbType && SqlDbType.SmallInt != dbType && SqlDbType.SmallMoney != dbType && SqlDbType.Text != dbType && SqlDbType.Time != dbType && SqlDbType.Timestamp != dbType && SqlDbType.TinyInt != dbType && SqlDbType.UniqueIdentifier != dbType && SqlDbType.Variant != dbType && SqlDbType.Xml != dbType)
		{
			throw SQL.InvalidSqlDbTypeForConstructor(dbType);
		}
		SetDefaultsForType(dbType);
		if (SqlDbType.NText == dbType || SqlDbType.Text == dbType)
		{
			_lLocale = CultureInfo.CurrentCulture.LCID;
		}
		_strName = name;
		_useServerDefault = useServerDefault;
		_isUniqueKey = isUniqueKey;
		_columnSortOrder = columnSortOrder;
		_sortOrdinal = sortOrdinal;
	}

	private void Construct(string name, SqlDbType dbType, long maxLength, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		AssertNameIsValid(name);
		ValidateSortOrder(columnSortOrder, sortOrdinal);
		long lLocale = 0L;
		if (SqlDbType.Char == dbType)
		{
			if (maxLength > 8000 || maxLength < 0)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
			lLocale = CultureInfo.CurrentCulture.LCID;
		}
		else if (SqlDbType.VarChar == dbType)
		{
			if ((maxLength > 8000 || maxLength < 0) && maxLength != Max)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
			lLocale = CultureInfo.CurrentCulture.LCID;
		}
		else if (SqlDbType.NChar == dbType)
		{
			if (maxLength > 4000 || maxLength < 0)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
			lLocale = CultureInfo.CurrentCulture.LCID;
		}
		else if (SqlDbType.NVarChar == dbType)
		{
			if ((maxLength > 4000 || maxLength < 0) && maxLength != Max)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
			lLocale = CultureInfo.CurrentCulture.LCID;
		}
		else if (SqlDbType.NText == dbType || SqlDbType.Text == dbType)
		{
			if (Max != maxLength)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
			lLocale = CultureInfo.CurrentCulture.LCID;
		}
		else if (SqlDbType.Binary == dbType)
		{
			if (maxLength > 8000 || maxLength < 0)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
		}
		else if (SqlDbType.VarBinary == dbType)
		{
			if ((maxLength > 8000 || maxLength < 0) && maxLength != Max)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
		}
		else
		{
			if (SqlDbType.Image != dbType)
			{
				throw SQL.InvalidSqlDbTypeForConstructor(dbType);
			}
			if (Max != maxLength)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
		}
		SetDefaultsForType(dbType);
		_strName = name;
		_lMaxLength = maxLength;
		_lLocale = lLocale;
		_useServerDefault = useServerDefault;
		_isUniqueKey = isUniqueKey;
		_columnSortOrder = columnSortOrder;
		_sortOrdinal = sortOrdinal;
	}

	private void Construct(string name, SqlDbType dbType, long maxLength, long locale, SqlCompareOptions compareOptions, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		AssertNameIsValid(name);
		ValidateSortOrder(columnSortOrder, sortOrdinal);
		if (SqlDbType.Char == dbType)
		{
			if (maxLength > 8000 || maxLength < 0)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
		}
		else if (SqlDbType.VarChar == dbType)
		{
			if ((maxLength > 8000 || maxLength < 0) && maxLength != Max)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
		}
		else if (SqlDbType.NChar == dbType)
		{
			if (maxLength > 4000 || maxLength < 0)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
		}
		else if (SqlDbType.NVarChar == dbType)
		{
			if ((maxLength > 4000 || maxLength < 0) && maxLength != Max)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
		}
		else
		{
			if (SqlDbType.NText != dbType && SqlDbType.Text != dbType)
			{
				throw SQL.InvalidSqlDbTypeForConstructor(dbType);
			}
			if (Max != maxLength)
			{
				throw ADP.Argument(global::SR.GetString("Specified length '{0}' is out of range.", maxLength.ToString(CultureInfo.InvariantCulture)), "maxLength");
			}
		}
		if (SqlCompareOptions.BinarySort != compareOptions && (~(SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreNonSpace | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth) & compareOptions) != SqlCompareOptions.None)
		{
			throw ADP.InvalidEnumerationValue(typeof(SqlCompareOptions), (int)compareOptions);
		}
		SetDefaultsForType(dbType);
		_strName = name;
		_lMaxLength = maxLength;
		_lLocale = locale;
		_eCompareOptions = compareOptions;
		_useServerDefault = useServerDefault;
		_isUniqueKey = isUniqueKey;
		_columnSortOrder = columnSortOrder;
		_sortOrdinal = sortOrdinal;
	}

	private void Construct(string name, SqlDbType dbType, byte precision, byte scale, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		AssertNameIsValid(name);
		ValidateSortOrder(columnSortOrder, sortOrdinal);
		if (SqlDbType.Decimal == dbType)
		{
			if (precision > SqlDecimal.MaxPrecision || scale > precision)
			{
				throw SQL.PrecisionValueOutOfRange(precision);
			}
			if (scale > SqlDecimal.MaxScale)
			{
				throw SQL.ScaleValueOutOfRange(scale);
			}
		}
		else
		{
			if (SqlDbType.Time != dbType && SqlDbType.DateTime2 != dbType && SqlDbType.DateTimeOffset != dbType)
			{
				throw SQL.InvalidSqlDbTypeForConstructor(dbType);
			}
			if (scale > 7)
			{
				throw SQL.TimeScaleValueOutOfRange(scale);
			}
		}
		SetDefaultsForType(dbType);
		_strName = name;
		_bPrecision = precision;
		_bScale = scale;
		if (SqlDbType.Decimal == dbType)
		{
			_lMaxLength = s_maxLenFromPrecision[precision - 1];
		}
		else
		{
			_lMaxLength -= s_maxVarTimeLenOffsetFromScale[scale];
		}
		_useServerDefault = useServerDefault;
		_isUniqueKey = isUniqueKey;
		_columnSortOrder = columnSortOrder;
		_sortOrdinal = sortOrdinal;
	}

	private void Construct(string name, SqlDbType dbType, Type userDefinedType, string serverTypeName, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		AssertNameIsValid(name);
		ValidateSortOrder(columnSortOrder, sortOrdinal);
		if (SqlDbType.Udt != dbType)
		{
			throw SQL.InvalidSqlDbTypeForConstructor(dbType);
		}
		if (null == userDefinedType)
		{
			throw ADP.ArgumentNull("userDefinedType");
		}
		SetDefaultsForType(SqlDbType.Udt);
		_strName = name;
		_lMaxLength = SerializationHelperSql9.GetUdtMaxLength(userDefinedType);
		_udtType = userDefinedType;
		_serverTypeName = serverTypeName;
		_useServerDefault = useServerDefault;
		_isUniqueKey = isUniqueKey;
		_columnSortOrder = columnSortOrder;
		_sortOrdinal = sortOrdinal;
	}

	private void Construct(string name, SqlDbType dbType, string database, string owningSchema, string objectName, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
	{
		AssertNameIsValid(name);
		ValidateSortOrder(columnSortOrder, sortOrdinal);
		if (SqlDbType.Xml != dbType)
		{
			throw SQL.InvalidSqlDbTypeForConstructor(dbType);
		}
		if ((database != null || owningSchema != null) && objectName == null)
		{
			throw ADP.ArgumentNull("objectName");
		}
		SetDefaultsForType(SqlDbType.Xml);
		_strName = name;
		_xmlSchemaCollectionDatabase = database;
		_xmlSchemaCollectionOwningSchema = owningSchema;
		_xmlSchemaCollectionName = objectName;
		_useServerDefault = useServerDefault;
		_isUniqueKey = isUniqueKey;
		_columnSortOrder = columnSortOrder;
		_sortOrdinal = sortOrdinal;
	}

	private void AssertNameIsValid(string name)
	{
		if (name == null)
		{
			throw ADP.ArgumentNull("name");
		}
		if (128L < (long)name.Length)
		{
			throw SQL.NameTooLong("name");
		}
	}

	private void ValidateSortOrder(SortOrder columnSortOrder, int sortOrdinal)
	{
		if (SortOrder.Unspecified != columnSortOrder && columnSortOrder != SortOrder.Ascending && SortOrder.Descending != columnSortOrder)
		{
			throw SQL.InvalidSortOrder(columnSortOrder);
		}
		if (SortOrder.Unspecified == columnSortOrder != (-1 == sortOrdinal))
		{
			throw SQL.MustSpecifyBothSortOrderAndOrdinal(columnSortOrder, sortOrdinal);
		}
	}

	/// <summary>Validates the specified <see cref="T:System.Int16" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Int16" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public short Adjust(short value)
	{
		if (SqlDbType.SmallInt != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Int32" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Int32" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public int Adjust(int value)
	{
		if (SqlDbType.Int != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Int64" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Int64" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public long Adjust(long value)
	{
		if (SqlDbType != SqlDbType.BigInt)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Single" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Single" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public float Adjust(float value)
	{
		if (SqlDbType.Real != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Double" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Double" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public double Adjust(double value)
	{
		if (SqlDbType.Float != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.String" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.String" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public string Adjust(string value)
	{
		if (SqlDbType.Char == SqlDbType || SqlDbType.NChar == SqlDbType)
		{
			if (value != null && value.Length < MaxLength)
			{
				value = value.PadRight((int)MaxLength);
			}
		}
		else if (SqlDbType.VarChar != SqlDbType && SqlDbType.NVarChar != SqlDbType && SqlDbType.Text != SqlDbType && SqlDbType.NText != SqlDbType)
		{
			ThrowInvalidType();
		}
		if (value == null)
		{
			return null;
		}
		if (value.Length > MaxLength && Max != MaxLength)
		{
			value = value.Remove((int)MaxLength, (int)(value.Length - MaxLength));
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Decimal" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Decimal" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public decimal Adjust(decimal value)
	{
		if (SqlDbType.Decimal != SqlDbType && SqlDbType.Money != SqlDbType && SqlDbType.SmallMoney != SqlDbType)
		{
			ThrowInvalidType();
		}
		if (SqlDbType.Decimal != SqlDbType)
		{
			VerifyMoneyRange(new SqlMoney(value));
			return value;
		}
		return InternalAdjustSqlDecimal(new SqlDecimal(value)).Value;
	}

	/// <summary>Validates the specified <see cref="T:System.DateTime" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.DateTime" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public DateTime Adjust(DateTime value)
	{
		if (SqlDbType.DateTime == SqlDbType || SqlDbType.SmallDateTime == SqlDbType)
		{
			VerifyDateTimeRange(value);
		}
		else
		{
			if (SqlDbType.DateTime2 == SqlDbType)
			{
				return new DateTime(InternalAdjustTimeTicks(value.Ticks));
			}
			if (SqlDbType.Date == SqlDbType)
			{
				return value.Date;
			}
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Guid" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Guid" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public Guid Adjust(Guid value)
	{
		if (SqlDbType.UniqueIdentifier != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlBoolean" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlBoolean Adjust(SqlBoolean value)
	{
		if (SqlDbType.Bit != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlByte" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlByte" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlByte Adjust(SqlByte value)
	{
		if (SqlDbType.TinyInt != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlInt16" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlInt16" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlInt16 Adjust(SqlInt16 value)
	{
		if (SqlDbType.SmallInt != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlInt32" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlInt32 Adjust(SqlInt32 value)
	{
		if (SqlDbType.Int != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlInt64" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlInt64" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlInt64 Adjust(SqlInt64 value)
	{
		if (SqlDbType != SqlDbType.BigInt)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlSingle" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlSingle" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlSingle Adjust(SqlSingle value)
	{
		if (SqlDbType.Real != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlDouble" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlDouble" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlDouble Adjust(SqlDouble value)
	{
		if (SqlDbType.Float != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlMoney" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlMoney" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlMoney Adjust(SqlMoney value)
	{
		if (SqlDbType.Money != SqlDbType && SqlDbType.SmallMoney != SqlDbType)
		{
			ThrowInvalidType();
		}
		if (!value.IsNull)
		{
			VerifyMoneyRange(value);
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlDateTime" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlDateTime" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlDateTime Adjust(SqlDateTime value)
	{
		if (SqlDbType.DateTime != SqlDbType && SqlDbType.SmallDateTime != SqlDbType)
		{
			ThrowInvalidType();
		}
		if (!value.IsNull)
		{
			VerifyDateTimeRange(value.Value);
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlDecimal" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlDecimal" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlDecimal Adjust(SqlDecimal value)
	{
		if (SqlDbType.Decimal != SqlDbType)
		{
			ThrowInvalidType();
		}
		return InternalAdjustSqlDecimal(value);
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlString" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlString" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlString Adjust(SqlString value)
	{
		if (SqlDbType.Char == SqlDbType || SqlDbType.NChar == SqlDbType)
		{
			if (!value.IsNull && value.Value.Length < MaxLength)
			{
				return new SqlString(value.Value.PadRight((int)MaxLength));
			}
		}
		else if (SqlDbType.VarChar != SqlDbType && SqlDbType.NVarChar != SqlDbType && SqlDbType.Text != SqlDbType && SqlDbType.NText != SqlDbType)
		{
			ThrowInvalidType();
		}
		if (value.IsNull)
		{
			return value;
		}
		if (value.Value.Length > MaxLength && Max != MaxLength)
		{
			value = new SqlString(value.Value.Remove((int)MaxLength, (int)(value.Value.Length - MaxLength)));
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlBinary" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlBinary" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlBinary Adjust(SqlBinary value)
	{
		if (SqlDbType.Binary == SqlDbType || SqlDbType.Timestamp == SqlDbType)
		{
			if (!value.IsNull && value.Length < MaxLength)
			{
				byte[] value2 = value.Value;
				byte[] array = new byte[MaxLength];
				Buffer.BlockCopy(value2, 0, array, 0, value2.Length);
				Array.Clear(array, value2.Length, array.Length - value2.Length);
				return new SqlBinary(array);
			}
		}
		else if (SqlDbType.VarBinary != SqlDbType && SqlDbType.Image != SqlDbType)
		{
			ThrowInvalidType();
		}
		if (value.IsNull)
		{
			return value;
		}
		if (value.Length > MaxLength && Max != MaxLength)
		{
			byte[] value3 = value.Value;
			byte[] array2 = new byte[MaxLength];
			Buffer.BlockCopy(value3, 0, array2, 0, (int)MaxLength);
			value = new SqlBinary(array2);
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlGuid" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlGuid" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlGuid Adjust(SqlGuid value)
	{
		if (SqlDbType.UniqueIdentifier != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlChars" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlChars" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlChars Adjust(SqlChars value)
	{
		if (SqlDbType.Char == SqlDbType || SqlDbType.NChar == SqlDbType)
		{
			if (value != null && !value.IsNull)
			{
				long length = value.Length;
				if (length < MaxLength)
				{
					if (value.MaxLength < MaxLength)
					{
						char[] array = new char[(int)MaxLength];
						Buffer.BlockCopy(value.Buffer, 0, array, 0, (int)length);
						value = new SqlChars(array);
					}
					char[] buffer = value.Buffer;
					for (long num = length; num < MaxLength; num++)
					{
						buffer[num] = ' ';
					}
					value.SetLength(MaxLength);
					return value;
				}
			}
		}
		else if (SqlDbType.VarChar != SqlDbType && SqlDbType.NVarChar != SqlDbType && SqlDbType.Text != SqlDbType && SqlDbType.NText != SqlDbType)
		{
			ThrowInvalidType();
		}
		if (value == null || value.IsNull)
		{
			return value;
		}
		if (value.Length > MaxLength && Max != MaxLength)
		{
			value.SetLength(MaxLength);
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlBytes" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlBytes" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlBytes Adjust(SqlBytes value)
	{
		if (SqlDbType.Binary == SqlDbType || SqlDbType.Timestamp == SqlDbType)
		{
			if (value != null && !value.IsNull)
			{
				int num = (int)value.Length;
				if (num < MaxLength)
				{
					if (value.MaxLength < MaxLength)
					{
						byte[] array = new byte[MaxLength];
						Buffer.BlockCopy(value.Buffer, 0, array, 0, num);
						value = new SqlBytes(array);
					}
					byte[] buffer = value.Buffer;
					Array.Clear(buffer, num, buffer.Length - num);
					value.SetLength(MaxLength);
					return value;
				}
			}
		}
		else if (SqlDbType.VarBinary != SqlDbType && SqlDbType.Image != SqlDbType)
		{
			ThrowInvalidType();
		}
		if (value == null || value.IsNull)
		{
			return value;
		}
		if (value.Length > MaxLength && Max != MaxLength)
		{
			value.SetLength(MaxLength);
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Data.SqlTypes.SqlXml" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Data.SqlTypes.SqlXml" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public SqlXml Adjust(SqlXml value)
	{
		if (SqlDbType.Xml != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.TimeSpan" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as an array of <see cref="T:System.TimeSpan" /> values.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public TimeSpan Adjust(TimeSpan value)
	{
		if (SqlDbType.Time != SqlDbType)
		{
			ThrowInvalidType();
		}
		VerifyTimeRange(value);
		return new TimeSpan(InternalAdjustTimeTicks(value.Ticks));
	}

	/// <summary>Validates the specified <see cref="T:System.DateTimeOffset" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as an array of <see cref="T:System.DateTimeOffset" /> values.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public DateTimeOffset Adjust(DateTimeOffset value)
	{
		if (SqlDbType.DateTimeOffset != SqlDbType)
		{
			ThrowInvalidType();
		}
		return new DateTimeOffset(InternalAdjustTimeTicks(value.Ticks), value.Offset);
	}

	/// <summary>Validates the specified <see cref="T:System.Object" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Object" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public object Adjust(object value)
	{
		if (value == null)
		{
			return null;
		}
		Type type = value.GetType();
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.Boolean:
			value = Adjust((bool)value);
			break;
		case TypeCode.Byte:
			value = Adjust((byte)value);
			break;
		case TypeCode.Char:
			value = Adjust((char)value);
			break;
		case TypeCode.DateTime:
			value = Adjust((DateTime)value);
			break;
		case TypeCode.Decimal:
			value = Adjust((decimal)value);
			break;
		case TypeCode.Double:
			value = Adjust((double)value);
			break;
		case TypeCode.Empty:
			throw ADP.InvalidDataType(TypeCode.Empty);
		case TypeCode.Int16:
			value = Adjust((short)value);
			break;
		case TypeCode.Int32:
			value = Adjust((int)value);
			break;
		case TypeCode.Int64:
			value = Adjust((long)value);
			break;
		case TypeCode.SByte:
			throw ADP.InvalidDataType(TypeCode.SByte);
		case TypeCode.Single:
			value = Adjust((float)value);
			break;
		case TypeCode.String:
			value = Adjust((string)value);
			break;
		case TypeCode.UInt16:
			throw ADP.InvalidDataType(TypeCode.UInt16);
		case TypeCode.UInt32:
			throw ADP.InvalidDataType(TypeCode.UInt32);
		case TypeCode.UInt64:
			throw ADP.InvalidDataType(TypeCode.UInt64);
		case TypeCode.Object:
			if (type == typeof(byte[]))
			{
				value = Adjust((byte[])value);
				break;
			}
			if (type == typeof(char[]))
			{
				value = Adjust((char[])value);
				break;
			}
			if (type == typeof(Guid))
			{
				value = Adjust((Guid)value);
				break;
			}
			if (type == typeof(object))
			{
				throw ADP.InvalidDataType(TypeCode.UInt64);
			}
			if (type == typeof(SqlBinary))
			{
				value = Adjust((SqlBinary)value);
				break;
			}
			if (type == typeof(SqlBoolean))
			{
				value = Adjust((SqlBoolean)value);
				break;
			}
			if (type == typeof(SqlByte))
			{
				value = Adjust((SqlByte)value);
				break;
			}
			if (type == typeof(SqlDateTime))
			{
				value = Adjust((SqlDateTime)value);
				break;
			}
			if (type == typeof(SqlDouble))
			{
				value = Adjust((SqlDouble)value);
				break;
			}
			if (type == typeof(SqlGuid))
			{
				value = Adjust((SqlGuid)value);
				break;
			}
			if (type == typeof(SqlInt16))
			{
				value = Adjust((SqlInt16)value);
				break;
			}
			if (type == typeof(SqlInt32))
			{
				value = Adjust((SqlInt32)value);
				break;
			}
			if (type == typeof(SqlInt64))
			{
				value = Adjust((SqlInt64)value);
				break;
			}
			if (type == typeof(SqlMoney))
			{
				value = Adjust((SqlMoney)value);
				break;
			}
			if (type == typeof(SqlDecimal))
			{
				value = Adjust((SqlDecimal)value);
				break;
			}
			if (type == typeof(SqlSingle))
			{
				value = Adjust((SqlSingle)value);
				break;
			}
			if (type == typeof(SqlString))
			{
				value = Adjust((SqlString)value);
				break;
			}
			if (type == typeof(SqlChars))
			{
				value = Adjust((SqlChars)value);
				break;
			}
			if (type == typeof(SqlBytes))
			{
				value = Adjust((SqlBytes)value);
				break;
			}
			if (type == typeof(SqlXml))
			{
				value = Adjust((SqlXml)value);
				break;
			}
			if (type == typeof(TimeSpan))
			{
				value = Adjust((TimeSpan)value);
				break;
			}
			if (type == typeof(DateTimeOffset))
			{
				value = Adjust((DateTimeOffset)value);
				break;
			}
			throw ADP.UnknownDataType(type);
		default:
			throw ADP.UnknownDataTypeCode(type, Type.GetTypeCode(type));
		case TypeCode.DBNull:
			break;
		}
		return value;
	}

	/// <summary>Infers the metadata from the specified object and returns it as a <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</summary>
	/// <returns>The inferred metadata as a <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</returns>
	/// <param name="value">The object used from which the metadata is inferred.</param>
	/// <param name="name">The name assigned to the returned <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentNullException">The v<paramref name="alue" /> is null. </exception>
	public static SqlMetaData InferFromValue(object value, string name)
	{
		if (value == null)
		{
			throw ADP.ArgumentNull("value");
		}
		SqlMetaData sqlMetaData = null;
		Type type = value.GetType();
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.Boolean:
			return new SqlMetaData(name, SqlDbType.Bit);
		case TypeCode.Byte:
			return new SqlMetaData(name, SqlDbType.TinyInt);
		case TypeCode.Char:
			return new SqlMetaData(name, SqlDbType.NVarChar, 1L);
		case TypeCode.DateTime:
			return new SqlMetaData(name, SqlDbType.DateTime);
		case TypeCode.DBNull:
			throw ADP.InvalidDataType(TypeCode.DBNull);
		case TypeCode.Decimal:
		{
			SqlDecimal sqlDecimal2 = new SqlDecimal((decimal)value);
			return new SqlMetaData(name, SqlDbType.Decimal, sqlDecimal2.Precision, sqlDecimal2.Scale);
		}
		case TypeCode.Double:
			return new SqlMetaData(name, SqlDbType.Float);
		case TypeCode.Empty:
			throw ADP.InvalidDataType(TypeCode.Empty);
		case TypeCode.Int16:
			return new SqlMetaData(name, SqlDbType.SmallInt);
		case TypeCode.Int32:
			return new SqlMetaData(name, SqlDbType.Int);
		case TypeCode.Int64:
			return new SqlMetaData(name, SqlDbType.BigInt);
		case TypeCode.SByte:
			throw ADP.InvalidDataType(TypeCode.SByte);
		case TypeCode.Single:
			return new SqlMetaData(name, SqlDbType.Real);
		case TypeCode.String:
		{
			long num7 = ((string)value).Length;
			if (num7 < 1)
			{
				num7 = 1L;
			}
			if (4000 < num7)
			{
				num7 = Max;
			}
			return new SqlMetaData(name, SqlDbType.NVarChar, num7);
		}
		case TypeCode.UInt16:
			throw ADP.InvalidDataType(TypeCode.UInt16);
		case TypeCode.UInt32:
			throw ADP.InvalidDataType(TypeCode.UInt32);
		case TypeCode.UInt64:
			throw ADP.InvalidDataType(TypeCode.UInt64);
		case TypeCode.Object:
			if (type == typeof(byte[]))
			{
				long num = ((byte[])value).Length;
				if (num < 1)
				{
					num = 1L;
				}
				if (8000 < num)
				{
					num = Max;
				}
				return new SqlMetaData(name, SqlDbType.VarBinary, num);
			}
			if (type == typeof(char[]))
			{
				long num2 = ((char[])value).Length;
				if (num2 < 1)
				{
					num2 = 1L;
				}
				if (4000 < num2)
				{
					num2 = Max;
				}
				return new SqlMetaData(name, SqlDbType.NVarChar, num2);
			}
			if (type == typeof(Guid))
			{
				return new SqlMetaData(name, SqlDbType.UniqueIdentifier);
			}
			if (type == typeof(object))
			{
				return new SqlMetaData(name, SqlDbType.Variant);
			}
			if (type == typeof(SqlBinary))
			{
				SqlBinary sqlBinary = (SqlBinary)value;
				long num3;
				if (!sqlBinary.IsNull)
				{
					num3 = sqlBinary.Length;
					if (num3 < 1)
					{
						num3 = 1L;
					}
					if (8000 < num3)
					{
						num3 = Max;
					}
				}
				else
				{
					num3 = sxm_rgDefaults[21].MaxLength;
				}
				return new SqlMetaData(name, SqlDbType.VarBinary, num3);
			}
			if (type == typeof(SqlBoolean))
			{
				return new SqlMetaData(name, SqlDbType.Bit);
			}
			if (type == typeof(SqlByte))
			{
				return new SqlMetaData(name, SqlDbType.TinyInt);
			}
			if (type == typeof(SqlDateTime))
			{
				return new SqlMetaData(name, SqlDbType.DateTime);
			}
			if (type == typeof(SqlDouble))
			{
				return new SqlMetaData(name, SqlDbType.Float);
			}
			if (type == typeof(SqlGuid))
			{
				return new SqlMetaData(name, SqlDbType.UniqueIdentifier);
			}
			if (type == typeof(SqlInt16))
			{
				return new SqlMetaData(name, SqlDbType.SmallInt);
			}
			if (type == typeof(SqlInt32))
			{
				return new SqlMetaData(name, SqlDbType.Int);
			}
			if (type == typeof(SqlInt64))
			{
				return new SqlMetaData(name, SqlDbType.BigInt);
			}
			if (type == typeof(SqlMoney))
			{
				return new SqlMetaData(name, SqlDbType.Money);
			}
			if (type == typeof(SqlDecimal))
			{
				SqlDecimal sqlDecimal = (SqlDecimal)value;
				byte precision;
				byte scale;
				if (!sqlDecimal.IsNull)
				{
					precision = sqlDecimal.Precision;
					scale = sqlDecimal.Scale;
				}
				else
				{
					precision = sxm_rgDefaults[5].Precision;
					scale = sxm_rgDefaults[5].Scale;
				}
				return new SqlMetaData(name, SqlDbType.Decimal, precision, scale);
			}
			if (type == typeof(SqlSingle))
			{
				return new SqlMetaData(name, SqlDbType.Real);
			}
			if (type == typeof(SqlString))
			{
				SqlString sqlString = (SqlString)value;
				if (!sqlString.IsNull)
				{
					long num4 = sqlString.Value.Length;
					if (num4 < 1)
					{
						num4 = 1L;
					}
					if (num4 > 4000)
					{
						num4 = Max;
					}
					return new SqlMetaData(name, SqlDbType.NVarChar, num4, sqlString.LCID, sqlString.SqlCompareOptions);
				}
				return new SqlMetaData(name, SqlDbType.NVarChar, sxm_rgDefaults[12].MaxLength);
			}
			if (type == typeof(SqlChars))
			{
				SqlChars sqlChars = (SqlChars)value;
				long num5;
				if (!sqlChars.IsNull)
				{
					num5 = sqlChars.Length;
					if (num5 < 1)
					{
						num5 = 1L;
					}
					if (num5 > 4000)
					{
						num5 = Max;
					}
				}
				else
				{
					num5 = sxm_rgDefaults[12].MaxLength;
				}
				return new SqlMetaData(name, SqlDbType.NVarChar, num5);
			}
			if (type == typeof(SqlBytes))
			{
				SqlBytes sqlBytes = (SqlBytes)value;
				long num6;
				if (!sqlBytes.IsNull)
				{
					num6 = sqlBytes.Length;
					if (num6 < 1)
					{
						num6 = 1L;
					}
					else if (8000 < num6)
					{
						num6 = Max;
					}
				}
				else
				{
					num6 = sxm_rgDefaults[21].MaxLength;
				}
				return new SqlMetaData(name, SqlDbType.VarBinary, num6);
			}
			if (type == typeof(SqlXml))
			{
				return new SqlMetaData(name, SqlDbType.Xml);
			}
			if (type == typeof(TimeSpan))
			{
				return new SqlMetaData(name, SqlDbType.Time, 0, InferScaleFromTimeTicks(((TimeSpan)value).Ticks));
			}
			if (type == typeof(DateTimeOffset))
			{
				return new SqlMetaData(name, SqlDbType.DateTimeOffset, 0, InferScaleFromTimeTicks(((DateTimeOffset)value).Ticks));
			}
			throw ADP.UnknownDataType(type);
		default:
			throw ADP.UnknownDataTypeCode(type, Type.GetTypeCode(type));
		}
	}

	/// <summary>Validates the specified <see cref="T:System.Boolean" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Boolean" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public bool Adjust(bool value)
	{
		if (SqlDbType.Bit != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Byte" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Byte" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public byte Adjust(byte value)
	{
		if (SqlDbType.TinyInt != SqlDbType)
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified array of <see cref="T:System.Byte" /> values against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as an array of <see cref="T:System.Byte" /> values.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public byte[] Adjust(byte[] value)
	{
		if (SqlDbType.Binary == SqlDbType || SqlDbType.Timestamp == SqlDbType)
		{
			if (value != null && value.Length < MaxLength)
			{
				byte[] array = new byte[MaxLength];
				Buffer.BlockCopy(value, 0, array, 0, value.Length);
				Array.Clear(array, value.Length, array.Length - value.Length);
				return array;
			}
		}
		else if (SqlDbType.VarBinary != SqlDbType && SqlDbType.Image != SqlDbType)
		{
			ThrowInvalidType();
		}
		if (value == null)
		{
			return null;
		}
		if (value.Length > MaxLength && Max != MaxLength)
		{
			byte[] array2 = new byte[MaxLength];
			Buffer.BlockCopy(value, 0, array2, 0, (int)MaxLength);
			value = array2;
		}
		return value;
	}

	/// <summary>Validates the specified <see cref="T:System.Char" /> value against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as a <see cref="T:System.Char" />.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public char Adjust(char value)
	{
		if (SqlDbType.Char == SqlDbType || SqlDbType.NChar == SqlDbType)
		{
			if (1 != MaxLength)
			{
				ThrowInvalidType();
			}
		}
		else if (1 > MaxLength || (SqlDbType.VarChar != SqlDbType && SqlDbType.NVarChar != SqlDbType && SqlDbType.Text != SqlDbType && SqlDbType.NText != SqlDbType))
		{
			ThrowInvalidType();
		}
		return value;
	}

	/// <summary>Validates the specified array of <see cref="T:System.Char" /> values against the metadata, and adjusts the value if necessary.</summary>
	/// <returns>The adjusted value as an array <see cref="T:System.Char" /> values.</returns>
	/// <param name="value">The value to validate against the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Value" /> does not match the <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> type, or <paramref name="value" /> could not be adjusted. </exception>
	public char[] Adjust(char[] value)
	{
		if (SqlDbType.Char == SqlDbType || SqlDbType.NChar == SqlDbType)
		{
			if (value != null)
			{
				long num = value.Length;
				if (num < MaxLength)
				{
					char[] array = new char[(int)MaxLength];
					Buffer.BlockCopy(value, 0, array, 0, (int)num);
					for (long num2 = num; num2 < array.Length; num2++)
					{
						array[num2] = ' ';
					}
					return array;
				}
			}
		}
		else if (SqlDbType.VarChar != SqlDbType && SqlDbType.NVarChar != SqlDbType && SqlDbType.Text != SqlDbType && SqlDbType.NText != SqlDbType)
		{
			ThrowInvalidType();
		}
		if (value == null)
		{
			return null;
		}
		if (value.Length > MaxLength && Max != MaxLength)
		{
			char[] array2 = new char[MaxLength];
			Buffer.BlockCopy(value, 0, array2, 0, (int)MaxLength);
			value = array2;
		}
		return value;
	}

	internal static SqlMetaData GetPartialLengthMetaData(SqlMetaData md)
	{
		if (md.IsPartialLength)
		{
			return md;
		}
		if (md.SqlDbType == SqlDbType.Xml)
		{
			ThrowInvalidType();
		}
		if (md.SqlDbType == SqlDbType.NVarChar || md.SqlDbType == SqlDbType.VarChar || md.SqlDbType == SqlDbType.VarBinary)
		{
			return new SqlMetaData(md.Name, md.SqlDbType, Max, 0, 0, md.LocaleId, md.CompareOptions, null, null, null, partialLength: true, md.Type);
		}
		return md;
	}

	private static void ThrowInvalidType()
	{
		throw ADP.InvalidMetaDataValue();
	}

	private void VerifyDateTimeRange(DateTime value)
	{
		if (SqlDbType.SmallDateTime == SqlDbType && (s_dtSmallMax < value || s_dtSmallMin > value))
		{
			ThrowInvalidType();
		}
	}

	private void VerifyMoneyRange(SqlMoney value)
	{
		if (SqlDbType.SmallMoney == SqlDbType && ((s_smSmallMax < value).Value || (s_smSmallMin > value).Value))
		{
			ThrowInvalidType();
		}
	}

	private SqlDecimal InternalAdjustSqlDecimal(SqlDecimal value)
	{
		if (!value.IsNull && (value.Precision != Precision || value.Scale != Scale))
		{
			if (value.Scale != Scale)
			{
				value = SqlDecimal.AdjustScale(value, Scale - value.Scale, fRound: false);
			}
			return SqlDecimal.ConvertToPrecScale(value, Precision, Scale);
		}
		return value;
	}

	private void VerifyTimeRange(TimeSpan value)
	{
		if (SqlDbType.Time == SqlDbType && (s_timeMin > value || value > s_timeMax))
		{
			ThrowInvalidType();
		}
	}

	private long InternalAdjustTimeTicks(long ticks)
	{
		return ticks / s_unitTicksFromScale[Scale] * s_unitTicksFromScale[Scale];
	}

	private static byte InferScaleFromTimeTicks(long ticks)
	{
		for (byte b = 0; b < 7; b++)
		{
			if (ticks / s_unitTicksFromScale[b] * s_unitTicksFromScale[b] == ticks)
			{
				return b;
			}
		}
		return 7;
	}

	private void SetDefaultsForType(SqlDbType dbType)
	{
		if (SqlDbType.BigInt <= dbType && SqlDbType.DateTimeOffset >= dbType)
		{
			SqlMetaData sqlMetaData = sxm_rgDefaults[(int)dbType];
			_sqlDbType = dbType;
			_lMaxLength = sqlMetaData.MaxLength;
			_bPrecision = sqlMetaData.Precision;
			_bScale = sqlMetaData.Scale;
			_lLocale = sqlMetaData.LocaleId;
			_eCompareOptions = sqlMetaData.CompareOptions;
		}
	}
}
