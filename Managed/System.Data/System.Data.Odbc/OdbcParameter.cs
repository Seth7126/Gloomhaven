using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.Odbc;

/// <summary>Represents a parameter to an <see cref="T:System.Data.Odbc.OdbcCommand" /> and optionally, its mapping to a <see cref="T:System.Data.DataColumn" />. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
public sealed class OdbcParameter : DbParameter, ICloneable, IDataParameter, IDbDataParameter
{
	private bool _hasChanged;

	private bool _userSpecifiedType;

	private TypeMap _typemap;

	private TypeMap _bindtype;

	private string _parameterName;

	private byte _precision;

	private byte _scale;

	private bool _hasScale;

	private ODBC32.SQL_C _boundSqlCType;

	private ODBC32.SQL_TYPE _boundParameterType;

	private int _boundSize;

	private int _boundScale;

	private IntPtr _boundBuffer;

	private IntPtr _boundIntbuffer;

	private TypeMap _originalbindtype;

	private byte _internalPrecision;

	private bool _internalShouldSerializeSize;

	private int _internalSize;

	private ParameterDirection _internalDirection;

	private byte _internalScale;

	private int _internalOffset;

	internal bool _internalUserSpecifiedType;

	private object _internalValue;

	private int _preparedOffset;

	private int _preparedSize;

	private int _preparedBufferSize;

	private object _preparedValue;

	private int _preparedIntOffset;

	private int _preparedValueOffset;

	private ODBC32.SQL_C _prepared_Sql_C_Type;

	private object _value;

	private object _parent;

	private ParameterDirection _direction;

	private int _size;

	private int _offset;

	private string _sourceColumn;

	private DataRowVersion _sourceVersion;

	private bool _sourceColumnNullMapping;

	private bool _isNullable;

	private object _coercedValue;

	/// <summary>Gets or sets the <see cref="T:System.Data.DbType" /> of the parameter.</summary>
	/// <returns>One of the <see cref="T:System.Data.DbType" /> values. The default is <see cref="T:System.String" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property was not set to a valid <see cref="T:System.Data.DbType" />. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override DbType DbType
	{
		get
		{
			if (_userSpecifiedType)
			{
				return _typemap._dbType;
			}
			return TypeMap._NVarChar._dbType;
		}
		set
		{
			if (_typemap == null || _typemap._dbType != value)
			{
				PropertyTypeChanging();
				_typemap = TypeMap.FromDbType(value);
				_userSpecifiedType = true;
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.Odbc.OdbcType" /> of the parameter.</summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcType" /> value that is the <see cref="T:System.Data.Odbc.OdbcType" /> of the parameter. The default is Nchar.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DefaultValue(OdbcType.NChar)]
	[DbProviderSpecificTypeProperty(true)]
	public OdbcType OdbcType
	{
		get
		{
			if (_userSpecifiedType)
			{
				return _typemap._odbcType;
			}
			return TypeMap._NVarChar._odbcType;
		}
		set
		{
			if (_typemap == null || _typemap._odbcType != value)
			{
				PropertyTypeChanging();
				_typemap = TypeMap.FromOdbcType(value);
				_userSpecifiedType = true;
			}
		}
	}

	internal bool HasChanged
	{
		set
		{
			_hasChanged = value;
		}
	}

	internal bool UserSpecifiedType => _userSpecifiedType;

	/// <summary>Gets or sets the name of the <see cref="T:System.Data.Odbc.OdbcParameter" />.</summary>
	/// <returns>The name of the <see cref="T:System.Data.Odbc.OdbcParameter" />. The default is an empty string ("").</returns>
	public override string ParameterName
	{
		get
		{
			string parameterName = _parameterName;
			if (parameterName == null)
			{
				return ADP.StrEmpty;
			}
			return parameterName;
		}
		set
		{
			if (_parameterName != value)
			{
				PropertyChanging();
				_parameterName = value;
			}
		}
	}

	/// <summary>Gets or sets the number of digits used to represent the <see cref="P:System.Data.Odbc.OdbcParameter.Value" /> property.</summary>
	/// <returns>The maximum number of digits used to represent the <see cref="P:System.Data.Odbc.OdbcParameter.Value" /> property. The default value is 0, which indicates that the data provider sets the precision for <see cref="P:System.Data.Odbc.OdbcParameter.Value" />. </returns>
	/// <filterpriority>2</filterpriority>
	public new byte Precision
	{
		get
		{
			return PrecisionInternal;
		}
		set
		{
			PrecisionInternal = value;
		}
	}

	internal byte PrecisionInternal
	{
		get
		{
			byte b = _precision;
			if (b == 0)
			{
				b = ValuePrecision(Value);
			}
			return b;
		}
		set
		{
			if (_precision != value)
			{
				PropertyChanging();
				_precision = value;
			}
		}
	}

	/// <summary>Gets or sets the number of decimal places to which <see cref="P:System.Data.Odbc.OdbcParameter.Value" /> is resolved.</summary>
	/// <returns>The number of decimal places to which <see cref="P:System.Data.Odbc.OdbcParameter.Value" /> is resolved. The default is 0.</returns>
	/// <filterpriority>2</filterpriority>
	public new byte Scale
	{
		get
		{
			return ScaleInternal;
		}
		set
		{
			ScaleInternal = value;
		}
	}

	internal byte ScaleInternal
	{
		get
		{
			byte b = _scale;
			if (!ShouldSerializeScale(b))
			{
				b = ValueScale(Value);
			}
			return b;
		}
		set
		{
			if (_scale != value || !_hasScale)
			{
				PropertyChanging();
				_scale = value;
				_hasScale = true;
			}
		}
	}

	/// <summary>Gets or sets the value of the parameter.</summary>
	/// <returns>An <see cref="T:System.Object" /> that is the value of the parameter. The default value is null.</returns>
	public override object Value
	{
		get
		{
			return _value;
		}
		set
		{
			_coercedValue = null;
			_value = value;
		}
	}

	private object CoercedValue
	{
		get
		{
			return _coercedValue;
		}
		set
		{
			_coercedValue = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.</summary>
	/// <returns>One of the <see cref="T:System.Data.ParameterDirection" /> values. The default is Input.</returns>
	/// <exception cref="T:System.ArgumentException">The property was not set to one of the valid <see cref="T:System.Data.ParameterDirection" /> values.</exception>
	public override ParameterDirection Direction
	{
		get
		{
			ParameterDirection direction = _direction;
			if (direction == (ParameterDirection)0)
			{
				return ParameterDirection.Input;
			}
			return direction;
		}
		set
		{
			if (_direction != value)
			{
				if ((uint)(value - 1) > 2u && value != ParameterDirection.ReturnValue)
				{
					throw ADP.InvalidParameterDirection(value);
				}
				PropertyChanging();
				_direction = value;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether the parameter accepts null values.</summary>
	/// <returns>true if null values are accepted; otherwise false. The default is false.</returns>
	public override bool IsNullable
	{
		get
		{
			return _isNullable;
		}
		set
		{
			_isNullable = value;
		}
	}

	public int Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			if (value < 0)
			{
				throw ADP.InvalidOffsetValue(value);
			}
			_offset = value;
		}
	}

	/// <summary>Gets or sets the maximum size of the data within the column.</summary>
	/// <returns>The maximum size of the data within the column. The default value is inferred from the parameter value.</returns>
	public override int Size
	{
		get
		{
			int num = _size;
			if (num == 0)
			{
				num = ValueSize(Value);
			}
			return num;
		}
		set
		{
			if (_size != value)
			{
				if (value < -1)
				{
					throw ADP.InvalidSizeValue(value);
				}
				PropertyChanging();
				_size = value;
			}
		}
	}

	/// <summary>Gets or sets the name of the source column mapped to the <see cref="T:System.Data.DataSet" /> and used for loading or returning the <see cref="P:System.Data.Odbc.OdbcParameter.Value" />.</summary>
	/// <returns>The name of the source column that will be used to set the value of this parameter. The default is an empty string ("").</returns>
	public override string SourceColumn
	{
		get
		{
			string sourceColumn = _sourceColumn;
			if (sourceColumn == null)
			{
				return ADP.StrEmpty;
			}
			return sourceColumn;
		}
		set
		{
			_sourceColumn = value;
		}
	}

	/// <summary>Sets or gets a value which indicates whether the source column is nullable. This lets <see cref="T:System.Data.Common.DbCommandBuilder" /> correctly generate Update statements for nullable columns.</summary>
	/// <returns>true if the source column is nullable; false if it is not.</returns>
	public override bool SourceColumnNullMapping
	{
		get
		{
			return _sourceColumnNullMapping;
		}
		set
		{
			_sourceColumnNullMapping = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.DataRowVersion" /> to use when you load <see cref="P:System.Data.Odbc.OdbcParameter.Value" />.</summary>
	/// <returns>One of the <see cref="T:System.Data.DataRowVersion" /> values. The default is Current.</returns>
	public override DataRowVersion SourceVersion
	{
		get
		{
			DataRowVersion sourceVersion = _sourceVersion;
			if (sourceVersion == (DataRowVersion)0)
			{
				return DataRowVersion.Current;
			}
			return sourceVersion;
		}
		set
		{
			switch (value)
			{
			case DataRowVersion.Original:
			case DataRowVersion.Current:
			case DataRowVersion.Proposed:
			case DataRowVersion.Default:
				_sourceVersion = value;
				break;
			default:
				throw ADP.InvalidDataRowVersion(value);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcParameter" /> class.</summary>
	public OdbcParameter()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcParameter" /> class that uses the parameter name and an <see cref="T:System.Data.Odbc.OdbcParameter" /> object.</summary>
	/// <param name="name">The name of the parameter. </param>
	/// <param name="value">An <see cref="T:System.Data.Odbc.OdbcParameter" /> object. </param>
	public OdbcParameter(string name, object value)
		: this()
	{
		ParameterName = name;
		Value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcParameter" /> class that uses the parameter name and data type.</summary>
	/// <param name="name">The name of the parameter. </param>
	/// <param name="type">One of the <see cref="T:System.Data.Odbc.OdbcType" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The value supplied in the <paramref name="type" /> parameter is an invalid back-end data type. </exception>
	public OdbcParameter(string name, OdbcType type)
		: this()
	{
		ParameterName = name;
		OdbcType = type;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcParameter" /> class that uses the parameter name, data type, and length.</summary>
	/// <param name="name">The name of the parameter. </param>
	/// <param name="type">One of the <see cref="T:System.Data.Odbc.OdbcType" /> values. </param>
	/// <param name="size">The length of the parameter. </param>
	/// <exception cref="T:System.ArgumentException">The value supplied in the <paramref name="type" /> parameter is an invalid back-end data type. </exception>
	public OdbcParameter(string name, OdbcType type, int size)
		: this()
	{
		ParameterName = name;
		OdbcType = type;
		Size = size;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcParameter" /> class that uses the parameter name, data type, length, and source column name.</summary>
	/// <param name="name">The name of the parameter. </param>
	/// <param name="type">One of the <see cref="T:System.Data.Odbc.OdbcType" /> values. </param>
	/// <param name="size">The length of the parameter. </param>
	/// <param name="sourcecolumn">The name of the source column. </param>
	/// <exception cref="T:System.ArgumentException">The value supplied in the <paramref name="type" /> parameter is an invalid back-end data type. </exception>
	public OdbcParameter(string name, OdbcType type, int size, string sourcecolumn)
		: this()
	{
		ParameterName = name;
		OdbcType = type;
		Size = size;
		SourceColumn = sourcecolumn;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcParameter" /> class that uses the parameter name, data type, length, source column name, parameter direction, numeric precision, and other properties.</summary>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="odbcType">One of the <see cref="T:System.Data.Odbc.OdbcType" /> values. </param>
	/// <param name="size">The length of the parameter. </param>
	/// <param name="parameterDirection">One of the <see cref="T:System.Data.ParameterDirection" /> values. </param>
	/// <param name="isNullable">true if the value of the field can be null; otherwise false. </param>
	/// <param name="precision">The total number of digits to the left and right of the decimal point to which <see cref="P:System.Data.Odbc.OdbcParameter.Value" /> is resolved. </param>
	/// <param name="scale">The total number of decimal places to which <see cref="P:System.Data.Odbc.OdbcParameter.Value" /> is resolved. </param>
	/// <param name="srcColumn">The name of the source column. </param>
	/// <param name="srcVersion">One of the <see cref="T:System.Data.DataRowVersion" /> values. </param>
	/// <param name="value">An <see cref="T:System.Object" /> that is the value of the <see cref="T:System.Data.Odbc.OdbcParameter" />. </param>
	/// <exception cref="T:System.ArgumentException">The value supplied in the <paramref name="type" /> parameter is an invalid back-end data type. </exception>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public OdbcParameter(string parameterName, OdbcType odbcType, int size, ParameterDirection parameterDirection, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value)
		: this()
	{
		ParameterName = parameterName;
		OdbcType = odbcType;
		Size = size;
		Direction = parameterDirection;
		IsNullable = isNullable;
		PrecisionInternal = precision;
		ScaleInternal = scale;
		SourceColumn = srcColumn;
		SourceVersion = srcVersion;
		Value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcParameter" /> class that uses the parameter name, data type, length, source column name, parameter direction, numeric precision, and other properties.</summary>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="odbcType">One of the <see cref="P:System.Data.Odbc.OdbcParameter.OdbcType" /> values. </param>
	/// <param name="size">The length of the parameter. </param>
	/// <param name="parameterDirection">One of the <see cref="T:System.Data.ParameterDirection" /> values. </param>
	/// <param name="precision">The total number of digits to the left and right of the decimal point to which <see cref="P:System.Data.Odbc.OdbcParameter.Value" /> is resolved. </param>
	/// <param name="scale">The total number of decimal places to which <see cref="P:System.Data.Odbc.OdbcParameter.Value" /> is resolved. </param>
	/// <param name="sourceColumn">The name of the source column. </param>
	/// <param name="sourceVersion">One of the <see cref="T:System.Data.DataRowVersion" /> values. </param>
	/// <param name="sourceColumnNullMapping">true if the corresponding source column is nullable; false if it is not.</param>
	/// <param name="value">An <see cref="T:System.Object" /> that is the value of the <see cref="T:System.Data.Odbc.OdbcParameter" />. </param>
	/// <exception cref="T:System.ArgumentException">The value supplied in the <paramref name="type" /> parameter is an invalid back-end data type. </exception>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public OdbcParameter(string parameterName, OdbcType odbcType, int size, ParameterDirection parameterDirection, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping, object value)
		: this()
	{
		ParameterName = parameterName;
		OdbcType = odbcType;
		Size = size;
		Direction = parameterDirection;
		PrecisionInternal = precision;
		ScaleInternal = scale;
		SourceColumn = sourceColumn;
		SourceVersion = sourceVersion;
		SourceColumnNullMapping = sourceColumnNullMapping;
		Value = value;
	}

	/// <summary>Resets the type associated with this <see cref="T:System.Data.Odbc.OdbcParameter" />.</summary>
	/// <filterpriority>2</filterpriority>
	public override void ResetDbType()
	{
		ResetOdbcType();
	}

	/// <summary>Resets the type associated with this <see cref="T:System.Data.Odbc.OdbcParameter" />.</summary>
	/// <filterpriority>2</filterpriority>
	public void ResetOdbcType()
	{
		PropertyTypeChanging();
		_typemap = null;
		_userSpecifiedType = false;
	}

	private bool ShouldSerializePrecision()
	{
		return _precision != 0;
	}

	private bool ShouldSerializeScale()
	{
		return ShouldSerializeScale(_scale);
	}

	private bool ShouldSerializeScale(byte scale)
	{
		if (_hasScale)
		{
			if (scale == 0)
			{
				return ShouldSerializePrecision();
			}
			return true;
		}
		return false;
	}

	private int GetColumnSize(object value, int offset, int ordinal)
	{
		if (ODBC32.SQL_C.NUMERIC == _bindtype._sql_c && _internalPrecision != 0)
		{
			return Math.Min((int)_internalPrecision, 29);
		}
		int num = _bindtype._columnSize;
		if (0 >= num)
		{
			if (ODBC32.SQL_C.NUMERIC == _typemap._sql_c)
			{
				num = 62;
			}
			else
			{
				num = _internalSize;
				if (!_internalShouldSerializeSize || 1073741823 <= num || num < 0)
				{
					if (!_internalShouldSerializeSize && (ParameterDirection.Output & _internalDirection) != 0)
					{
						throw ADP.UninitializedParameterSize(ordinal, _bindtype._type);
					}
					if (value == null || Convert.IsDBNull(value))
					{
						num = 0;
					}
					else if (value is string)
					{
						num = ((string)value).Length - offset;
						if ((ParameterDirection.Output & _internalDirection) != 0 && 1073741823 <= _internalSize)
						{
							num = Math.Max(num, 4096);
						}
						if (ODBC32.SQL_TYPE.CHAR == _bindtype._sql_type || ODBC32.SQL_TYPE.VARCHAR == _bindtype._sql_type || ODBC32.SQL_TYPE.LONGVARCHAR == _bindtype._sql_type)
						{
							num = Encoding.Default.GetMaxByteCount(num);
						}
					}
					else if (value is char[])
					{
						num = ((char[])value).Length - offset;
						if ((ParameterDirection.Output & _internalDirection) != 0 && 1073741823 <= _internalSize)
						{
							num = Math.Max(num, 4096);
						}
						if (ODBC32.SQL_TYPE.CHAR == _bindtype._sql_type || ODBC32.SQL_TYPE.VARCHAR == _bindtype._sql_type || ODBC32.SQL_TYPE.LONGVARCHAR == _bindtype._sql_type)
						{
							num = Encoding.Default.GetMaxByteCount(num);
						}
					}
					else if (value is byte[])
					{
						num = ((byte[])value).Length - offset;
						if ((ParameterDirection.Output & _internalDirection) != 0 && 1073741823 <= _internalSize)
						{
							num = Math.Max(num, 8192);
						}
					}
					num = Math.Max(2, num);
				}
			}
		}
		return num;
	}

	private int GetValueSize(object value, int offset)
	{
		if (ODBC32.SQL_C.NUMERIC == _bindtype._sql_c && _internalPrecision != 0)
		{
			return Math.Min((int)_internalPrecision, 29);
		}
		int num = _bindtype._columnSize;
		if (0 >= num)
		{
			bool flag = false;
			if (value is string)
			{
				num = ((string)value).Length - offset;
				flag = true;
			}
			else if (!(value is char[]))
			{
				num = ((value is byte[]) ? (((byte[])value).Length - offset) : 0);
			}
			else
			{
				num = ((char[])value).Length - offset;
				flag = true;
			}
			if (_internalShouldSerializeSize && _internalSize >= 0 && _internalSize < num && _bindtype == _originalbindtype)
			{
				num = _internalSize;
			}
			if (flag)
			{
				num *= 2;
			}
		}
		return num;
	}

	private int GetParameterSize(object value, int offset, int ordinal)
	{
		int num = _bindtype._bufferSize;
		if (0 >= num)
		{
			if (ODBC32.SQL_C.NUMERIC == _typemap._sql_c)
			{
				num = 518;
			}
			else
			{
				num = _internalSize;
				if (!_internalShouldSerializeSize || 1073741823 <= num || num < 0)
				{
					if (num <= 0 && (ParameterDirection.Output & _internalDirection) != 0)
					{
						throw ADP.UninitializedParameterSize(ordinal, _bindtype._type);
					}
					if (value == null || Convert.IsDBNull(value))
					{
						num = ((_bindtype._sql_c == ODBC32.SQL_C.WCHAR) ? 2 : 0);
					}
					else if (value is string)
					{
						num = (((string)value).Length - offset) * 2 + 2;
					}
					else if (value is char[])
					{
						num = (((char[])value).Length - offset) * 2 + 2;
					}
					else if (value is byte[])
					{
						num = ((byte[])value).Length - offset;
					}
					if ((ParameterDirection.Output & _internalDirection) != 0 && 1073741823 <= _internalSize)
					{
						num = Math.Max(num, 8192);
					}
				}
				else if (ODBC32.SQL_C.WCHAR == _bindtype._sql_c)
				{
					if (value is string && num < ((string)value).Length && _bindtype == _originalbindtype)
					{
						num = ((string)value).Length;
					}
					num = num * 2 + 2;
				}
				else if (value is byte[] && num < ((byte[])value).Length && _bindtype == _originalbindtype)
				{
					num = ((byte[])value).Length;
				}
			}
		}
		return num;
	}

	private byte GetParameterPrecision(object value)
	{
		if (_internalPrecision != 0 && value is decimal)
		{
			if (_internalPrecision < 29)
			{
				if (_internalPrecision != 0)
				{
					byte precision = ((SqlDecimal)(decimal)value).Precision;
					_internalPrecision = Math.Max(_internalPrecision, precision);
				}
				return _internalPrecision;
			}
			return 29;
		}
		if (value == null || value is decimal || Convert.IsDBNull(value))
		{
			return 28;
		}
		return 0;
	}

	private byte GetParameterScale(object value)
	{
		if (!(value is decimal))
		{
			return _internalScale;
		}
		byte b = (byte)((decimal.GetBits((decimal)value)[3] & 0xFF0000) >> 16);
		if (_internalScale > 0 && _internalScale < b)
		{
			return _internalScale;
		}
		return b;
	}

	/// <summary>For a description of this member, see <see cref="M:System.ICloneable.Clone" />.</summary>
	/// <returns>A new <see cref="T:System.Object" /> that is a copy of this instance.</returns>
	object ICloneable.Clone()
	{
		return new OdbcParameter(this);
	}

	private void CopyParameterInternal()
	{
		_internalValue = Value;
		_internalPrecision = (ShouldSerializePrecision() ? PrecisionInternal : ValuePrecision(_internalValue));
		_internalShouldSerializeSize = ShouldSerializeSize();
		_internalSize = (_internalShouldSerializeSize ? Size : ValueSize(_internalValue));
		_internalDirection = Direction;
		_internalScale = (ShouldSerializeScale() ? ScaleInternal : ValueScale(_internalValue));
		_internalOffset = Offset;
		_internalUserSpecifiedType = UserSpecifiedType;
	}

	private void CloneHelper(OdbcParameter destination)
	{
		CloneHelperCore(destination);
		destination._userSpecifiedType = _userSpecifiedType;
		destination._typemap = _typemap;
		destination._parameterName = _parameterName;
		destination._precision = _precision;
		destination._scale = _scale;
		destination._hasScale = _hasScale;
	}

	internal void ClearBinding()
	{
		if (!_userSpecifiedType)
		{
			_typemap = null;
		}
		_bindtype = null;
	}

	internal void PrepareForBind(OdbcCommand command, short ordinal, ref int parameterBufferSize)
	{
		CopyParameterInternal();
		object obj = ProcessAndGetParameterValue();
		int num = _internalOffset;
		int num2 = _internalSize;
		if (num > 0)
		{
			if (obj is string)
			{
				if (num > ((string)obj).Length)
				{
					throw ADP.OffsetOutOfRangeException();
				}
			}
			else if (obj is char[])
			{
				if (num > ((char[])obj).Length)
				{
					throw ADP.OffsetOutOfRangeException();
				}
			}
			else if (obj is byte[])
			{
				if (num > ((byte[])obj).Length)
				{
					throw ADP.OffsetOutOfRangeException();
				}
			}
			else
			{
				num = 0;
			}
		}
		switch (_bindtype._sql_type)
		{
		case ODBC32.SQL_TYPE.NUMERIC:
		case ODBC32.SQL_TYPE.DECIMAL:
			if (!command.Connection.IsV3Driver || !command.Connection.TestTypeSupport(ODBC32.SQL_TYPE.NUMERIC) || command.Connection.TestRestrictedSqlBindType(_bindtype._sql_type))
			{
				_bindtype = TypeMap._VarChar;
				if (obj != null && !Convert.IsDBNull(obj))
				{
					obj = ((decimal)obj).ToString(CultureInfo.CurrentCulture);
					num2 = ((string)obj).Length;
					num = 0;
				}
			}
			break;
		case ODBC32.SQL_TYPE.BIGINT:
			if (!command.Connection.IsV3Driver)
			{
				_bindtype = TypeMap._VarChar;
				if (obj != null && !Convert.IsDBNull(obj))
				{
					obj = ((long)obj).ToString(CultureInfo.CurrentCulture);
					num2 = ((string)obj).Length;
					num = 0;
				}
			}
			break;
		case ODBC32.SQL_TYPE.WLONGVARCHAR:
		case ODBC32.SQL_TYPE.WVARCHAR:
		case ODBC32.SQL_TYPE.WCHAR:
			if (obj is char)
			{
				obj = obj.ToString();
				num2 = ((string)obj).Length;
				num = 0;
			}
			if (!command.Connection.TestTypeSupport(_bindtype._sql_type))
			{
				if (ODBC32.SQL_TYPE.WCHAR == _bindtype._sql_type)
				{
					_bindtype = TypeMap._Char;
				}
				else if (ODBC32.SQL_TYPE.WVARCHAR == _bindtype._sql_type)
				{
					_bindtype = TypeMap._VarChar;
				}
				else if (ODBC32.SQL_TYPE.WLONGVARCHAR == _bindtype._sql_type)
				{
					_bindtype = TypeMap._Text;
				}
			}
			break;
		}
		ODBC32.SQL_C sQL_C = _bindtype._sql_c;
		if (!command.Connection.IsV3Driver && sQL_C == ODBC32.SQL_C.WCHAR)
		{
			sQL_C = ODBC32.SQL_C.CHAR;
			if (obj != null && !Convert.IsDBNull(obj) && obj is string)
			{
				obj = Encoding.GetEncoding(new CultureInfo(CultureInfo.CurrentCulture.LCID).TextInfo.ANSICodePage).GetBytes(obj.ToString());
				num2 = ((byte[])obj).Length;
			}
		}
		int parameterSize = GetParameterSize(obj, num, ordinal);
		switch (_bindtype._sql_type)
		{
		case ODBC32.SQL_TYPE.VARBINARY:
			if (num2 > 8000)
			{
				_bindtype = TypeMap._Image;
			}
			break;
		case ODBC32.SQL_TYPE.VARCHAR:
			if (num2 > 8000)
			{
				_bindtype = TypeMap._Text;
			}
			break;
		case ODBC32.SQL_TYPE.WVARCHAR:
			if (num2 > 4000)
			{
				_bindtype = TypeMap._NText;
			}
			break;
		}
		_prepared_Sql_C_Type = sQL_C;
		_preparedOffset = num;
		_preparedSize = num2;
		_preparedValue = obj;
		_preparedBufferSize = parameterSize;
		_preparedIntOffset = parameterBufferSize;
		_preparedValueOffset = _preparedIntOffset + IntPtr.Size;
		parameterBufferSize += parameterSize + IntPtr.Size;
	}

	internal void Bind(OdbcStatementHandle hstmt, OdbcCommand command, short ordinal, CNativeBuffer parameterBuffer, bool allowReentrance)
	{
		ODBC32.SQL_C prepared_Sql_C_Type = _prepared_Sql_C_Type;
		ODBC32.SQL_PARAM sQL_PARAM = SqlDirectionFromParameterDirection();
		int preparedOffset = _preparedOffset;
		int preparedSize = _preparedSize;
		object obj = _preparedValue;
		int valueSize = GetValueSize(obj, preparedOffset);
		int columnSize = GetColumnSize(obj, preparedOffset, ordinal);
		byte parameterPrecision = GetParameterPrecision(obj);
		byte b = GetParameterScale(obj);
		HandleRef handleRef = parameterBuffer.PtrOffset(_preparedValueOffset, _preparedBufferSize);
		HandleRef intbuffer = parameterBuffer.PtrOffset(_preparedIntOffset, IntPtr.Size);
		if (ODBC32.SQL_C.NUMERIC == prepared_Sql_C_Type)
		{
			if (ODBC32.SQL_PARAM.INPUT_OUTPUT == sQL_PARAM && obj is decimal && b < _internalScale)
			{
				while (b < _internalScale)
				{
					obj = (decimal)obj * 10m;
					b++;
				}
			}
			SetInputValue(obj, prepared_Sql_C_Type, valueSize, parameterPrecision, 0, parameterBuffer);
			if (ODBC32.SQL_PARAM.INPUT != sQL_PARAM)
			{
				parameterBuffer.WriteInt16(_preparedValueOffset, (short)((b << 8) | parameterPrecision));
			}
		}
		else
		{
			SetInputValue(obj, prepared_Sql_C_Type, valueSize, preparedSize, preparedOffset, parameterBuffer);
		}
		if (!_hasChanged && _boundSqlCType == prepared_Sql_C_Type && _boundParameterType == _bindtype._sql_type && _boundSize == columnSize && _boundScale == b && _boundBuffer == handleRef.Handle && _boundIntbuffer == intbuffer.Handle)
		{
			return;
		}
		ODBC32.RetCode retCode = hstmt.BindParameter(ordinal, (short)sQL_PARAM, prepared_Sql_C_Type, _bindtype._sql_type, (IntPtr)columnSize, (IntPtr)b, handleRef, (IntPtr)_preparedBufferSize, intbuffer);
		if (retCode != ODBC32.RetCode.SUCCESS)
		{
			if ("07006" == command.GetDiagSqlState())
			{
				command.Connection.FlagRestrictedSqlBindType(_bindtype._sql_type);
				if (allowReentrance)
				{
					Bind(hstmt, command, ordinal, parameterBuffer, allowReentrance: false);
					return;
				}
			}
			command.Connection.HandleError(hstmt, retCode);
		}
		_hasChanged = false;
		_boundSqlCType = prepared_Sql_C_Type;
		_boundParameterType = _bindtype._sql_type;
		_boundSize = columnSize;
		_boundScale = b;
		_boundBuffer = handleRef.Handle;
		_boundIntbuffer = intbuffer.Handle;
		if (ODBC32.SQL_C.NUMERIC == prepared_Sql_C_Type)
		{
			OdbcDescriptorHandle descriptorHandle = command.GetDescriptorHandle(ODBC32.SQL_ATTR.APP_PARAM_DESC);
			retCode = descriptorHandle.SetDescriptionField1(ordinal, ODBC32.SQL_DESC.TYPE, (IntPtr)2);
			if (retCode != ODBC32.RetCode.SUCCESS)
			{
				command.Connection.HandleError(hstmt, retCode);
			}
			int num = parameterPrecision;
			retCode = descriptorHandle.SetDescriptionField1(ordinal, ODBC32.SQL_DESC.PRECISION, (IntPtr)num);
			if (retCode != ODBC32.RetCode.SUCCESS)
			{
				command.Connection.HandleError(hstmt, retCode);
			}
			num = b;
			retCode = descriptorHandle.SetDescriptionField1(ordinal, ODBC32.SQL_DESC.SCALE, (IntPtr)num);
			if (retCode != ODBC32.RetCode.SUCCESS)
			{
				command.Connection.HandleError(hstmt, retCode);
			}
			retCode = descriptorHandle.SetDescriptionField2(ordinal, ODBC32.SQL_DESC.DATA_PTR, handleRef);
			if (retCode != ODBC32.RetCode.SUCCESS)
			{
				command.Connection.HandleError(hstmt, retCode);
			}
		}
	}

	internal void GetOutputValue(CNativeBuffer parameterBuffer)
	{
		if (_hasChanged || _bindtype == null || _internalDirection == ParameterDirection.Input)
		{
			return;
		}
		TypeMap bindtype = _bindtype;
		_bindtype = null;
		int num = (int)parameterBuffer.ReadIntPtr(_preparedIntOffset);
		if (-1 == num)
		{
			Value = DBNull.Value;
		}
		else if (0 <= num || num == -3)
		{
			Value = parameterBuffer.MarshalToManaged(_preparedValueOffset, _boundSqlCType, num);
			if (_boundSqlCType == ODBC32.SQL_C.CHAR && Value != null && !Convert.IsDBNull(Value))
			{
				Encoding encoding = Encoding.GetEncoding(new CultureInfo(CultureInfo.CurrentCulture.LCID).TextInfo.ANSICodePage);
				Value = encoding.GetString((byte[])Value);
			}
			if (bindtype != _typemap && Value != null && !Convert.IsDBNull(Value) && Value.GetType() != _typemap._type)
			{
				Value = decimal.Parse((string)Value, CultureInfo.CurrentCulture);
			}
		}
	}

	private object ProcessAndGetParameterValue()
	{
		object obj = _internalValue;
		if (_internalUserSpecifiedType)
		{
			if (obj != null && !Convert.IsDBNull(obj))
			{
				Type type = obj.GetType();
				if (!type.IsArray)
				{
					if (type != _typemap._type)
					{
						try
						{
							obj = Convert.ChangeType(obj, _typemap._type, null);
						}
						catch (Exception ex)
						{
							if (!ADP.IsCatchableExceptionType(ex))
							{
								throw;
							}
							throw ADP.ParameterConversionFailed(obj, _typemap._type, ex);
						}
					}
				}
				else if (type == typeof(char[]))
				{
					obj = new string((char[])obj);
				}
			}
		}
		else if (_typemap == null)
		{
			if (obj == null || Convert.IsDBNull(obj))
			{
				_typemap = TypeMap._NVarChar;
			}
			else
			{
				Type type2 = obj.GetType();
				_typemap = TypeMap.FromSystemType(type2);
			}
		}
		_originalbindtype = (_bindtype = _typemap);
		return obj;
	}

	private void PropertyChanging()
	{
		_hasChanged = true;
	}

	private void PropertyTypeChanging()
	{
		PropertyChanging();
	}

	internal void SetInputValue(object value, ODBC32.SQL_C sql_c_type, int cbsize, int sizeorprecision, int offset, CNativeBuffer parameterBuffer)
	{
		if (ParameterDirection.Input == _internalDirection || ParameterDirection.InputOutput == _internalDirection)
		{
			if (value == null)
			{
				parameterBuffer.WriteIntPtr(_preparedIntOffset, (IntPtr)(-5));
				return;
			}
			if (Convert.IsDBNull(value))
			{
				parameterBuffer.WriteIntPtr(_preparedIntOffset, (IntPtr)(-1));
				return;
			}
			if (sql_c_type == ODBC32.SQL_C.WCHAR || sql_c_type == ODBC32.SQL_C.BINARY || sql_c_type == ODBC32.SQL_C.CHAR)
			{
				parameterBuffer.WriteIntPtr(_preparedIntOffset, (IntPtr)cbsize);
			}
			else
			{
				parameterBuffer.WriteIntPtr(_preparedIntOffset, IntPtr.Zero);
			}
			parameterBuffer.MarshalToNative(_preparedValueOffset, value, sql_c_type, sizeorprecision, offset);
		}
		else
		{
			_internalValue = null;
			parameterBuffer.WriteIntPtr(_preparedIntOffset, (IntPtr)(-1));
		}
	}

	private ODBC32.SQL_PARAM SqlDirectionFromParameterDirection()
	{
		switch (_internalDirection)
		{
		case ParameterDirection.Input:
			return ODBC32.SQL_PARAM.INPUT;
		case ParameterDirection.Output:
		case ParameterDirection.ReturnValue:
			return ODBC32.SQL_PARAM.OUTPUT;
		case ParameterDirection.InputOutput:
			return ODBC32.SQL_PARAM.INPUT_OUTPUT;
		default:
			return ODBC32.SQL_PARAM.INPUT;
		}
	}

	private byte ValuePrecision(object value)
	{
		return ValuePrecisionCore(value);
	}

	private byte ValueScale(object value)
	{
		return ValueScaleCore(value);
	}

	private int ValueSize(object value)
	{
		return ValueSizeCore(value);
	}

	private OdbcParameter(OdbcParameter source)
		: this()
	{
		ADP.CheckArgumentNull(source, "source");
		source.CloneHelper(this);
		if (_value is ICloneable cloneable)
		{
			_value = cloneable.Clone();
		}
	}

	private bool ShouldSerializeSize()
	{
		return _size != 0;
	}

	private void CloneHelperCore(OdbcParameter destination)
	{
		destination._value = _value;
		destination._direction = _direction;
		destination._size = _size;
		destination._offset = _offset;
		destination._sourceColumn = _sourceColumn;
		destination._sourceVersion = _sourceVersion;
		destination._sourceColumnNullMapping = _sourceColumnNullMapping;
		destination._isNullable = _isNullable;
	}

	internal object CompareExchangeParent(object value, object comparand)
	{
		object parent = _parent;
		if (comparand == parent)
		{
			_parent = value;
		}
		return parent;
	}

	internal void ResetParent()
	{
		_parent = null;
	}

	/// <summary>Gets a string that contains the <see cref="P:System.Data.Odbc.OdbcParameter.ParameterName" />.</summary>
	/// <returns>A string that contains the <see cref="P:System.Data.Odbc.OdbcParameter.ParameterName" />.</returns>
	public override string ToString()
	{
		return ParameterName;
	}

	private byte ValuePrecisionCore(object value)
	{
		if (value is decimal)
		{
			return ((SqlDecimal)(decimal)value).Precision;
		}
		return 0;
	}

	private byte ValueScaleCore(object value)
	{
		if (value is decimal)
		{
			return (byte)((decimal.GetBits((decimal)value)[3] & 0xFF0000) >> 16);
		}
		return 0;
	}

	private int ValueSizeCore(object value)
	{
		if (!ADP.IsNull(value))
		{
			if (value is string text)
			{
				return text.Length;
			}
			if (value is byte[] array)
			{
				return array.Length;
			}
			if (value is char[] array2)
			{
				return array2.Length;
			}
			if (value is byte || value is char)
			{
				return 1;
			}
		}
		return 0;
	}
}
