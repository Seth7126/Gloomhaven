using System.Data.Common;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes;

/// <summary>Represents a currency value ranging from -2 63 (or -922,337,203,685,477.5808) to 2 63 -1 (or +922,337,203,685,477.5807) with an accuracy to a ten-thousandth of currency unit to be stored in or retrieved from a database.</summary>
[Serializable]
[XmlSchemaProvider("GetXsdType")]
public struct SqlMoney : INullable, IComparable, IXmlSerializable
{
	private bool _fNotNull;

	private long _value;

	internal static readonly int s_iMoneyScale = 4;

	private static readonly long s_lTickBase = 10000L;

	private static readonly double s_dTickBase = s_lTickBase;

	private static readonly long s_minLong = long.MinValue / s_lTickBase;

	private static readonly long s_maxLong = long.MaxValue / s_lTickBase;

	/// <summary>Represents a <see cref="T:System.DBNull" /> that can be assigned to this instance of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> class.</summary>
	public static readonly SqlMoney Null = new SqlMoney(fNull: true);

	/// <summary>Represents the zero value that can be assigned to the <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property of an instance of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> class.</summary>
	public static readonly SqlMoney Zero = new SqlMoney(0);

	/// <summary>Represents the minimum value that can be assigned to <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property of an instance of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> class.</summary>
	public static readonly SqlMoney MinValue = new SqlMoney(long.MinValue, 0);

	/// <summary>Represents the maximum value that can be assigned to the <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property of an instance of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> class.</summary>
	public static readonly SqlMoney MaxValue = new SqlMoney(long.MaxValue, 0);

	/// <summary>Returns a Boolean value that indicates whether this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure is null.</summary>
	/// <returns>true if null. Otherwise, false.</returns>
	public bool IsNull => !_fNotNull;

	/// <summary>Gets the monetary value of an instance of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. This property is read-only.</summary>
	/// <returns>The monetary value of an instance of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure.</returns>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The property is set to null. </exception>
	public decimal Value
	{
		get
		{
			if (_fNotNull)
			{
				return ToDecimal();
			}
			throw new SqlNullValueException();
		}
	}

	private SqlMoney(bool fNull)
	{
		_fNotNull = false;
		_value = 0L;
	}

	internal SqlMoney(long value, int ignored)
	{
		_value = value;
		_fNotNull = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> class with the specified integer value.</summary>
	/// <param name="value">The monetary value to initialize. </param>
	public SqlMoney(int value)
	{
		_value = value * s_lTickBase;
		_fNotNull = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> class with the specified long integer value.</summary>
	/// <param name="value">The monetary value to initialize. </param>
	public SqlMoney(long value)
	{
		if (value < s_minLong || value > s_maxLong)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		_value = value * s_lTickBase;
		_fNotNull = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> class with the specified <see cref="T:System.Decimal" /> value.</summary>
	/// <param name="value">The monetary value to initialize. </param>
	public SqlMoney(decimal value)
	{
		SqlDecimal sqlDecimal = new SqlDecimal(value);
		sqlDecimal.AdjustScale(s_iMoneyScale - sqlDecimal.Scale, fRound: true);
		if (sqlDecimal._data3 != 0 || sqlDecimal._data4 != 0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		bool isPositive = sqlDecimal.IsPositive;
		ulong num = sqlDecimal._data1 + ((ulong)sqlDecimal._data2 << 32);
		if ((isPositive && num > long.MaxValue) || (!isPositive && num > 9223372036854775808uL))
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		_value = (long)(isPositive ? num : (0L - num));
		_fNotNull = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> class with specified double value.</summary>
	/// <param name="value">The monetary value to initialize. </param>
	public SqlMoney(double value)
		: this(new decimal(value))
	{
	}

	/// <summary>Converts the Value of this instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> as a <see cref="T:System.Decimal" /> structure.</summary>
	/// <returns>A <see cref="T:System.Decimal" /> structure whose value equals the <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property of this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure.</returns>
	public decimal ToDecimal()
	{
		if (IsNull)
		{
			throw new SqlNullValueException();
		}
		bool isNegative = false;
		long num = _value;
		if (_value < 0)
		{
			isNegative = true;
			num = -_value;
		}
		return new decimal((int)num, (int)(num >> 32), 0, isNegative, (byte)s_iMoneyScale);
	}

	/// <summary>Converts the Value of this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to an <see cref="T:System.Int64" />.</summary>
	/// <returns>A 64-bit integer whose value equals the integer part of this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure.</returns>
	public long ToInt64()
	{
		if (IsNull)
		{
			throw new SqlNullValueException();
		}
		long num = _value / (s_lTickBase / 10);
		bool flag = num >= 0;
		long num2 = num % 10;
		num /= 10;
		if (num2 >= 5)
		{
			num = ((!flag) ? (num - 1) : (num + 1));
		}
		return num;
	}

	internal long ToSqlInternalRepresentation()
	{
		if (IsNull)
		{
			throw new SqlNullValueException();
		}
		return _value;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to an <see cref="T:System.Int32" />.</summary>
	/// <returns>A 32-bit integer whose value equals the integer part of this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure.</returns>
	public int ToInt32()
	{
		return checked((int)ToInt64());
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to a <see cref="T:System.Double" />.</summary>
	/// <returns>A double with a value equal to this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure.</returns>
	public double ToDouble()
	{
		return decimal.ToDouble(ToDecimal());
	}

	/// <summary>Converts the <see cref="T:System.Decimal" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> equals the value of the <see cref="T:System.Decimal" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Decimal" /> value to be converted. </param>
	public static implicit operator SqlMoney(decimal x)
	{
		return new SqlMoney(x);
	}

	/// <summary>This implicit operator converts the supplied <see cref="T:System.Data.SqlTypes.SqlDouble" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlBoolean.ByteValue" /> property of the <see cref="T:System.Data.SqlTypes.SqlDouble" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure to be converted. </param>
	public static explicit operator SqlMoney(double x)
	{
		return new SqlMoney(x);
	}

	/// <summary>This implicit operator converts the supplied <see cref="T:System.Int64" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property is equal to the value of the <see cref="T:System.Int64" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Int64" /> structure to be converted. </param>
	public static implicit operator SqlMoney(long x)
	{
		return new SqlMoney(new decimal(x));
	}

	/// <summary>Converts the specified <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Decimal" />.</summary>
	/// <returns>A new <see cref="T:System.Decimal" /> structure whose value equals the <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static explicit operator decimal(SqlMoney x)
	{
		return x.Value;
	}

	/// <summary>Converts this instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> to string.</summary>
	/// <returns>A string whose value is the string representation of the value of this <see cref="T:System.Data.SqlTypes.SqlMoney" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		if (IsNull)
		{
			return SQLResource.NullString;
		}
		return ToDecimal().ToString("#0.00##", null);
	}

	/// <summary>Converts the <see cref="T:System.String" /> representation of a number to its <see cref="T:System.Data.SqlTypes.SqlMoney" /> equivalent.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlMoney" /> equivalent to the value that is contained in the specified <see cref="T:System.String" />.</returns>
	/// <param name="s">The String to be parsed. </param>
	public static SqlMoney Parse(string s)
	{
		if (s == SQLResource.NullString)
		{
			return Null;
		}
		decimal result;
		return (!decimal.TryParse(s, NumberStyles.Integer | NumberStyles.AllowTrailingSign | NumberStyles.AllowParentheses | NumberStyles.AllowDecimalPoint | NumberStyles.AllowCurrencySymbol, NumberFormatInfo.InvariantInfo, out result)) ? new SqlMoney(decimal.Parse(s, NumberStyles.Currency, NumberFormatInfo.CurrentInfo)) : new SqlMoney(result);
	}

	/// <summary>The unary minus operator negates the <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> contains the results of the negation.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to be negated. </param>
	public static SqlMoney operator -(SqlMoney x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		if (x._value == s_minLong)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlMoney(-x._value, 0);
	}

	/// <summary>Calculates the sum of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> stucture whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> contains the sum of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlMoney operator +(SqlMoney x, SqlMoney y)
	{
		try
		{
			return (x.IsNull || y.IsNull) ? Null : new SqlMoney(checked(x._value + y._value), 0);
		}
		catch (OverflowException)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
	}

	/// <summary>The subtraction operator subtracts the second <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter from the first.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure that contains the results of the subtraction.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlMoney operator -(SqlMoney x, SqlMoney y)
	{
		try
		{
			return (x.IsNull || y.IsNull) ? Null : new SqlMoney(checked(x._value - y._value), 0);
		}
		catch (OverflowException)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
	}

	/// <summary>The multiplicaion operator calculates the product of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> contains the product of the multiplication.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlMoney operator *(SqlMoney x, SqlMoney y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlMoney(decimal.Multiply(x.ToDecimal(), y.ToDecimal()));
		}
		return Null;
	}

	/// <summary>The division operator divides the first <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter by the second.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> contains the results of the division.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlMoney operator /(SqlMoney x, SqlMoney y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlMoney(decimal.Divide(x.ToDecimal(), y.ToDecimal()));
		}
		return Null;
	}

	/// <summary>This implicit operator converts the supplied <see cref="T:System.Data.SqlTypes.SqlBoolean" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlBoolean.ByteValue" /> property of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to be converted. </param>
	public static explicit operator SqlMoney(SqlBoolean x)
	{
		if (!x.IsNull)
		{
			return new SqlMoney(x.ByteValue);
		}
		return Null;
	}

	/// <summary>This implicit operator converts the supplied <see cref="T:System.Data.SqlTypes.SqlByte" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlByte" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to be converted. </param>
	public static implicit operator SqlMoney(SqlByte x)
	{
		if (!x.IsNull)
		{
			return new SqlMoney(x.Value);
		}
		return Null;
	}

	/// <summary>This implicit operator converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to be converted. </param>
	public static implicit operator SqlMoney(SqlInt16 x)
	{
		if (!x.IsNull)
		{
			return new SqlMoney(x.Value);
		}
		return Null;
	}

	/// <summary>This implicit operator converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to be converted. </param>
	public static implicit operator SqlMoney(SqlInt32 x)
	{
		if (!x.IsNull)
		{
			return new SqlMoney(x.Value);
		}
		return Null;
	}

	/// <summary>This implicit operator converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure to be converted. </param>
	public static implicit operator SqlMoney(SqlInt64 x)
	{
		if (!x.IsNull)
		{
			return new SqlMoney(x.Value);
		}
		return Null;
	}

	/// <summary>This operator converts the supplied <see cref="T:System.Data.SqlTypes.SqlSingle" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlSingle.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlSingle" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure to be converted. </param>
	public static explicit operator SqlMoney(SqlSingle x)
	{
		if (!x.IsNull)
		{
			return new SqlMoney(x.Value);
		}
		return Null;
	}

	/// <summary>This operator converts the supplied <see cref="T:System.Data.SqlTypes.SqlDouble" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlDouble.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlDouble" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure to be converted. </param>
	public static explicit operator SqlMoney(SqlDouble x)
	{
		if (!x.IsNull)
		{
			return new SqlMoney(x.Value);
		}
		return Null;
	}

	/// <summary>This operator converts the supplied <see cref="T:System.Data.SqlTypes.SqlDecimal" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlDecimal.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlDecimal" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure to be converted. </param>
	public static explicit operator SqlMoney(SqlDecimal x)
	{
		if (!x.IsNull)
		{
			return new SqlMoney(x.Value);
		}
		return Null;
	}

	/// <summary>This operator converts the <see cref="T:System.Data.SqlTypes.SqlString" /> parameter to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property equals the value represented by the <see cref="T:System.Data.SqlTypes.SqlString" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlString" /> object to be converted. </param>
	public static explicit operator SqlMoney(SqlString x)
	{
		if (!x.IsNull)
		{
			return new SqlMoney(decimal.Parse(x.Value, NumberStyles.Currency, null));
		}
		return Null;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether they are equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are not equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean operator ==(SqlMoney x, SqlMoney y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x._value == y._value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean operator !=(SqlMoney x, SqlMoney y)
	{
		return !(x == y);
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean operator <(SqlMoney x, SqlMoney y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x._value < y._value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean operator >(SqlMoney x, SqlMoney y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x._value > y._value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean operator <=(SqlMoney x, SqlMoney y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x._value <= y._value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean operator >=(SqlMoney x, SqlMoney y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x._value >= y._value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Calculates the sum of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> stucture whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> contains the sum of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlMoney Add(SqlMoney x, SqlMoney y)
	{
		return x + y;
	}

	/// <summary>The subtraction operator subtracts the second <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter from the first.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure that contains the results of the subtraction.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlMoney Subtract(SqlMoney x, SqlMoney y)
	{
		return x - y;
	}

	/// <summary>The multiplicaion operator calculates the product of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> contains the product of the multiplication.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlMoney Multiply(SqlMoney x, SqlMoney y)
	{
		return x * y;
	}

	/// <summary>The division operator divides the first <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter by the second.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> contains the results of the division.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlMoney Divide(SqlMoney x, SqlMoney y)
	{
		return x / y;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether they are equal.</summary>
	/// <returns>true if the two values are equal. Otherwise, false. If either instance is null, then the SqlMoney will be null.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean Equals(SqlMoney x, SqlMoney y)
	{
		return x == y;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean NotEquals(SqlMoney x, SqlMoney y)
	{
		return x != y;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean LessThan(SqlMoney x, SqlMoney y)
	{
		return x < y;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean GreaterThan(SqlMoney x, SqlMoney y)
	{
		return x > y;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean LessThanOrEqual(SqlMoney x, SqlMoney y)
	{
		return x <= y;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameters to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static SqlBoolean GreaterThanOrEqual(SqlMoney x, SqlMoney y)
	{
		return x >= y;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. If the value of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure is zero, the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value will be <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.True" />.</returns>
	public SqlBoolean ToSqlBoolean()
	{
		return (SqlBoolean)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlMoney" />. </returns>
	public SqlByte ToSqlByte()
	{
		return (SqlByte)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Data.SqlTypes.SqlDouble" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlDouble" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlMoney" />.</returns>
	public SqlDouble ToSqlDouble()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlMoney" />.</returns>
	public SqlInt16 ToSqlInt16()
	{
		return (SqlInt16)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlMoney" />.</returns>
	public SqlInt32 ToSqlInt32()
	{
		return (SqlInt32)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlMoney" />.</returns>
	public SqlInt64 ToSqlInt64()
	{
		return (SqlInt64)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Data.SqlTypes.SqlDecimal" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlDecimal" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlMoney" />.</returns>
	public SqlDecimal ToSqlDecimal()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Data.SqlTypes.SqlSingle" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlSingle" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlMoney" />.</returns>
	public SqlSingle ToSqlSingle()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Data.SqlTypes.SqlString" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlString" /> structure whose value is a string representing the value of this <see cref="T:System.Data.SqlTypes.SqlMoney" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SqlString ToSqlString()
	{
		return (SqlString)this;
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlMoney" /> instance to the supplied <see cref="T:System.Object" /> and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return value Condition Less than zero This instance is less than the object. Zero This instance is the same as the object. Greater than zero This instance is greater than the object -or- The object is a null reference (Nothing in Visual Basic) </returns>
	/// <param name="value">The <see cref="T:System.Object" /> to be compared. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int CompareTo(object value)
	{
		if (value is SqlMoney value2)
		{
			return CompareTo(value2);
		}
		throw ADP.WrongType(value.GetType(), typeof(SqlMoney));
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlMoney" /> instance to the supplied <see cref="T:System.Data.SqlTypes.SqlMoney" /> and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return value Condition Less than zero This instance is less than the object. Zero This instance is the same as the object. Greater than zero This instance is greater than the object -or- The object is a null reference (Nothing in Visual Basic) </returns>
	/// <param name="value">The <see cref="T:System.Data.SqlTypes.SqlMoney" /> to be compared.</param>
	public int CompareTo(SqlMoney value)
	{
		if (IsNull)
		{
			if (!value.IsNull)
			{
				return -1;
			}
			return 0;
		}
		if (value.IsNull)
		{
			return 1;
		}
		if (this < value)
		{
			return -1;
		}
		if (this > value)
		{
			return 1;
		}
		return 0;
	}

	/// <summary>Compares the supplied object parameter to the <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> object.</summary>
	/// <returns>Equals will return true if the object is an instance of <see cref="T:System.Data.SqlTypes.SqlMoney" /> and the two are equal; otherwise false.</returns>
	/// <param name="value">The object to be compared. </param>
	public override bool Equals(object value)
	{
		if (!(value is SqlMoney sqlMoney))
		{
			return false;
		}
		if (sqlMoney.IsNull || IsNull)
		{
			if (sqlMoney.IsNull)
			{
				return IsNull;
			}
			return false;
		}
		return (this == sqlMoney).Value;
	}

	/// <summary>Gets the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		if (!IsNull)
		{
			return _value.GetHashCode();
		}
		return 0;
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>An XmlSchema.</returns>
	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="reader">XmlReader</param>
	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		string attribute = reader.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
		if (attribute != null && XmlConvert.ToBoolean(attribute))
		{
			reader.ReadElementString();
			_fNotNull = false;
		}
		else
		{
			SqlMoney sqlMoney = new SqlMoney(XmlConvert.ToDecimal(reader.ReadElementString()));
			_fNotNull = sqlMoney._fNotNull;
			_value = sqlMoney._value;
		}
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="writer">XmlWriter</param>
	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		if (IsNull)
		{
			writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
		}
		else
		{
			writer.WriteString(XmlConvert.ToString(ToDecimal()));
		}
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>A string that indicates the XSD of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</returns>
	/// <param name="schemaSet">An <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
	{
		return new XmlQualifiedName("decimal", "http://www.w3.org/2001/XMLSchema");
	}
}
