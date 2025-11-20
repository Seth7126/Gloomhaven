using System.Data.Common;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes;

/// <summary>Represents a 64-bit signed integer to be stored in or retrieved from a database.</summary>
[Serializable]
[XmlSchemaProvider("GetXsdType")]
public struct SqlInt64 : INullable, IComparable, IXmlSerializable
{
	private bool m_fNotNull;

	private long m_value;

	private static readonly long s_lLowIntMask = 4294967295L;

	private static readonly long s_lHighIntMask = -4294967296L;

	/// <summary>Represents a <see cref="T:System.DBNull" /> that can be assigned to this instance of the <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure.</summary>
	public static readonly SqlInt64 Null = new SqlInt64(fNull: true);

	/// <summary>Represents a zero value that can be assigned to the <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property of an instance of the <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure.</summary>
	public static readonly SqlInt64 Zero = new SqlInt64(0L);

	/// <summary>A constant representing the smallest possible value for a <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure.</summary>
	public static readonly SqlInt64 MinValue = new SqlInt64(long.MinValue);

	/// <summary>A constant representing the largest possible value for a <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure.</summary>
	public static readonly SqlInt64 MaxValue = new SqlInt64(long.MaxValue);

	/// <summary>Gets a Boolean value that indicates whether this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure is null.</summary>
	/// <returns>true if null. Otherwise, false.</returns>
	public bool IsNull => !m_fNotNull;

	/// <summary>Gets the value of this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. This property is read-only.</summary>
	/// <returns>A long integer representing the value of this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure.</returns>
	public long Value
	{
		get
		{
			if (m_fNotNull)
			{
				return m_value;
			}
			throw new SqlNullValueException();
		}
	}

	private SqlInt64(bool fNull)
	{
		m_fNotNull = false;
		m_value = 0L;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure using the supplied long integer.</summary>
	/// <param name="value">A long integer. </param>
	public SqlInt64(long value)
	{
		m_value = value;
		m_fNotNull = true;
	}

	/// <summary>Converts the long parameter to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> equals the value of the long parameter.</returns>
	/// <param name="x">A long integer value. </param>
	public static implicit operator SqlInt64(long x)
	{
		return new SqlInt64(x);
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter to long.</summary>
	/// <returns>A new long value equal to the <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlInt64" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static explicit operator long(SqlInt64 x)
	{
		return x.Value;
	}

	/// <summary>Converts this instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> to <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> representing the value of this <see cref="T:System.Data.SqlTypes.SqlInt64" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		if (!IsNull)
		{
			return m_value.ToString((IFormatProvider)null);
		}
		return SQLResource.NullString;
	}

	/// <summary>Converts the <see cref="T:System.String" /> representation of a number to its 64-bit signed integer equivalent.</summary>
	/// <returns>A 64-bit signed integer equivalent to the value that is contained in the specified <see cref="T:System.String" />.</returns>
	/// <param name="s">The <see cref="T:System.String" /> to be parsed. </param>
	public static SqlInt64 Parse(string s)
	{
		if (s == SQLResource.NullString)
		{
			return Null;
		}
		return new SqlInt64(long.Parse(s, null));
	}

	/// <summary>The unary minus operator negates the <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlInt64" /> operand.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> is equal to the negated <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 operator -(SqlInt64 x)
	{
		if (!x.IsNull)
		{
			return new SqlInt64(-x.m_value);
		}
		return Null;
	}

	/// <summary>Performs a bitwise one's complement operation on its <see cref="T:System.Data.SqlTypes.SqlInt64" /> operand.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> is equal to the ones complement of the <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 operator ~(SqlInt64 x)
	{
		if (!x.IsNull)
		{
			return new SqlInt64(~x.m_value);
		}
		return Null;
	}

	/// <summary>Computes the sum of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> is equal to the sum of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 operator +(SqlInt64 x, SqlInt64 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		long num = x.m_value + y.m_value;
		if (SameSignLong(x.m_value, y.m_value) && !SameSignLong(x.m_value, num))
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt64(num);
	}

	/// <summary>Subtracts the second <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter from the first.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property equals the results of the subtraction operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 operator -(SqlInt64 x, SqlInt64 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		long num = x.m_value - y.m_value;
		if (!SameSignLong(x.m_value, y.m_value) && SameSignLong(y.m_value, num))
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt64(num);
	}

	/// <summary>Computes the product of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> is equal to the product of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 operator *(SqlInt64 x, SqlInt64 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		bool flag = false;
		long num = x.m_value;
		long num2 = y.m_value;
		long num3 = 0L;
		if (num < 0)
		{
			flag = true;
			num = -num;
		}
		if (num2 < 0)
		{
			flag = !flag;
			num2 = -num2;
		}
		long num4 = num & s_lLowIntMask;
		long num5 = (num >> 32) & s_lLowIntMask;
		long num6 = num2 & s_lLowIntMask;
		long num7 = (num2 >> 32) & s_lLowIntMask;
		if (num5 != 0L && num7 != 0L)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		long num8 = num4 * num6;
		if (num8 < 0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		if (num5 != 0L)
		{
			num3 = num5 * num6;
			if (num3 < 0 || num3 > long.MaxValue)
			{
				throw new OverflowException(SQLResource.ArithOverflowMessage);
			}
		}
		else if (num7 != 0L)
		{
			num3 = num4 * num7;
			if (num3 < 0 || num3 > long.MaxValue)
			{
				throw new OverflowException(SQLResource.ArithOverflowMessage);
			}
		}
		num8 += num3 << 32;
		if (num8 < 0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		if (flag)
		{
			num8 = -num8;
		}
		return new SqlInt64(num8);
	}

	/// <summary>Divides the first <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter by the second.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property contains the results of the division operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 operator /(SqlInt64 x, SqlInt64 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		if (y.m_value != 0L)
		{
			if (x.m_value == long.MinValue && y.m_value == -1)
			{
				throw new OverflowException(SQLResource.ArithOverflowMessage);
			}
			return new SqlInt64(x.m_value / y.m_value);
		}
		throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
	}

	/// <summary>Computes the remainder after dividing the first <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter by the second.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property contains the remainder.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 operator %(SqlInt64 x, SqlInt64 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		if (y.m_value != 0L)
		{
			if (x.m_value == long.MinValue && y.m_value == -1)
			{
				throw new OverflowException(SQLResource.ArithOverflowMessage);
			}
			return new SqlInt64(x.m_value % y.m_value);
		}
		throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
	}

	/// <summary>Computes the bitwise AND of its <see cref="T:System.Data.SqlTypes.SqlInt64" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure that contains the results of the bitwise AND operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 operator &(SqlInt64 x, SqlInt64 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlInt64(x.m_value & y.m_value);
		}
		return Null;
	}

	/// <summary>Computes the bitwise OR of its two <see cref="T:System.Data.SqlTypes.SqlInt64" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure that contains the results of the bitwise OR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 operator |(SqlInt64 x, SqlInt64 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlInt64(x.m_value | y.m_value);
		}
		return Null;
	}

	/// <summary>Performs a bitwise exclusive-OR operation on the supplied parameters.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure that contains the results of the bitwise XOR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 operator ^(SqlInt64 x, SqlInt64 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlInt64(x.m_value ^ y.m_value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlBoolean" /> parameter to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlBoolean.ByteValue" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to be converted. </param>
	public static explicit operator SqlInt64(SqlBoolean x)
	{
		if (!x.IsNull)
		{
			return new SqlInt64(x.ByteValue);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlByte" /> parameter to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlByte" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to be converted. </param>
	public static implicit operator SqlInt64(SqlByte x)
	{
		if (!x.IsNull)
		{
			return new SqlInt64(x.Value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to be converted. </param>
	public static implicit operator SqlInt64(SqlInt16 x)
	{
		if (!x.IsNull)
		{
			return new SqlInt64(x.Value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to be converted. </param>
	public static implicit operator SqlInt64(SqlInt32 x)
	{
		if (!x.IsNull)
		{
			return new SqlInt64(x.Value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlSingle" /> parameter to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property contains the integer part of the <see cref="T:System.Data.SqlTypes.SqlSingle" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure to be converted. </param>
	public static explicit operator SqlInt64(SqlSingle x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		float value = x.Value;
		if (value > 9.223372E+18f || value < -9.223372E+18f)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt64((long)value);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property equals the integer part of the <see cref="T:System.Data.SqlTypes.SqlDouble" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure to be converted. </param>
	public static explicit operator SqlInt64(SqlDouble x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		double value = x.Value;
		if (value > 9.223372036854776E+18 || value < -9.223372036854776E+18)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt64((long)value);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property equals the integer part of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to be converted. </param>
	public static explicit operator SqlInt64(SqlMoney x)
	{
		if (!x.IsNull)
		{
			return new SqlInt64(x.ToInt64());
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlDecimal" /> parameter to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> is equal to the integer part of the <see cref="T:System.Data.SqlTypes.SqlDecimal" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure to be converted. </param>
	public static explicit operator SqlInt64(SqlDecimal x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		SqlDecimal sqlDecimal = x;
		sqlDecimal.AdjustScale(-sqlDecimal._bScale, fRound: false);
		if (sqlDecimal._bLen > 2)
		{
			throw new OverflowException(SQLResource.ConversionOverflowMessage);
		}
		long num2;
		if (sqlDecimal._bLen == 2)
		{
			ulong num = SqlDecimal.DWL(sqlDecimal._data1, sqlDecimal._data2);
			if (num > SqlDecimal.s_llMax && (sqlDecimal.IsPositive || num != 1 + SqlDecimal.s_llMax))
			{
				throw new OverflowException(SQLResource.ConversionOverflowMessage);
			}
			num2 = (long)num;
		}
		else
		{
			num2 = sqlDecimal._data1;
		}
		if (!sqlDecimal.IsPositive)
		{
			num2 = -num2;
		}
		return new SqlInt64(num2);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlString" /> parameter to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> is equal to the value represented by the <see cref="T:System.Data.SqlTypes.SqlString" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlString" /> object to be converted. </param>
	public static explicit operator SqlInt64(SqlString x)
	{
		if (!x.IsNull)
		{
			return new SqlInt64(long.Parse(x.Value, null));
		}
		return Null;
	}

	private static bool SameSignLong(long x, long y)
	{
		return ((x ^ y) & long.MinValue) == 0;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters to determine whether they are equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are not equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean operator ==(SqlInt64 x, SqlInt64 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value == y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a logical comparison on the two SqlInt64 parameters to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean operator !=(SqlInt64 x, SqlInt64 y)
	{
		return !(x == y);
	}

	/// <summary>Performs a logical comparison on the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean operator <(SqlInt64 x, SqlInt64 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value < y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean operator >(SqlInt64 x, SqlInt64 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value > y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a logical comparison on the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean operator <=(SqlInt64 x, SqlInt64 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value <= y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean operator >=(SqlInt64 x, SqlInt64 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value >= y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a bitwise one's complement operation on its <see cref="T:System.Data.SqlTypes.SqlInt64" /> operand.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> is equal to the ones complement of the <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 OnesComplement(SqlInt64 x)
	{
		return ~x;
	}

	/// <summary>Computes the sum of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> is equal to the sum of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 Add(SqlInt64 x, SqlInt64 y)
	{
		return x + y;
	}

	/// <summary>Subtracts the second <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter from the first.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property equals the results of the subtraction operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 Subtract(SqlInt64 x, SqlInt64 y)
	{
		return x - y;
	}

	/// <summary>Computes the product of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> is equal to the product of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 Multiply(SqlInt64 x, SqlInt64 y)
	{
		return x * y;
	}

	/// <summary>Divides the first <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter by the second.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property contains the results of the division operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 Divide(SqlInt64 x, SqlInt64 y)
	{
		return x / y;
	}

	/// <summary>Computes the remainder after dividing the first <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter by the second.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property contains the remainder.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 Mod(SqlInt64 x, SqlInt64 y)
	{
		return x % y;
	}

	/// <summary>Divides two <see cref="T:System.Data.SqlTypes.SqlInt64" /> values and returns the remainder.</summary>
	/// <returns>The remainder left after division is performed on <paramref name="x" /> and <paramref name="y" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> value.</param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> value.</param>
	public static SqlInt64 Modulus(SqlInt64 x, SqlInt64 y)
	{
		return x % y;
	}

	/// <summary>Computes the bitwise AND of its <see cref="T:System.Data.SqlTypes.SqlInt64" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure that contains the results of the bitwise AND operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 BitwiseAnd(SqlInt64 x, SqlInt64 y)
	{
		return x & y;
	}

	/// <summary>Computes the bitwise OR of its two <see cref="T:System.Data.SqlTypes.SqlInt64" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure that contains the results of the bitwise OR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 BitwiseOr(SqlInt64 x, SqlInt64 y)
	{
		return x | y;
	}

	/// <summary>Performs a bitwise exclusive-OR operation on the supplied parameters.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure that contains the results of the bitwise XOR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlInt64 Xor(SqlInt64 x, SqlInt64 y)
	{
		return x ^ y;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters to determine whether they are equal.</summary>
	/// <returns>true if the two values are equal. Otherwise, false. If either instance is null, then the SqlInt64 will be null.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean Equals(SqlInt64 x, SqlInt64 y)
	{
		return x == y;
	}

	/// <summary>Performs a logical comparison on the two SqlInt64 parameters to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean NotEquals(SqlInt64 x, SqlInt64 y)
	{
		return x != y;
	}

	/// <summary>Performs a logical comparison on the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean LessThan(SqlInt64 x, SqlInt64 y)
	{
		return x < y;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean GreaterThan(SqlInt64 x, SqlInt64 y)
	{
		return x > y;
	}

	/// <summary>Performs a logical comparison on the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean LessThanOrEqual(SqlInt64 x, SqlInt64 y)
	{
		return x <= y;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameters to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static SqlBoolean GreaterThanOrEqual(SqlInt64 x, SqlInt64 y)
	{
		return x >= y;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure to <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</summary>
	/// <returns>true if the <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> is non-zero; false if zero; otherwise Null.</returns>
	public SqlBoolean ToSqlBoolean()
	{
		return (SqlBoolean)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure to <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose Value equals the Value of this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </returns>
	public SqlByte ToSqlByte()
	{
		return (SqlByte)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure to <see cref="T:System.Data.SqlTypes.SqlDouble" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlDouble" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt64" />.</returns>
	public SqlDouble ToSqlDouble()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt64" />.</returns>
	public SqlInt16 ToSqlInt16()
	{
		return (SqlInt16)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt64" />.</returns>
	public SqlInt32 ToSqlInt32()
	{
		return (SqlInt32)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt64" />.</returns>
	public SqlMoney ToSqlMoney()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure to <see cref="T:System.Data.SqlTypes.SqlDecimal" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlDecimal" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt64" />.</returns>
	public SqlDecimal ToSqlDecimal()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure to <see cref="T:System.Data.SqlTypes.SqlSingle" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlSingle" /> equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt64" />.</returns>
	public SqlSingle ToSqlSingle()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure to <see cref="T:System.Data.SqlTypes.SqlString" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlString" /> representing the value of this <see cref="T:System.Data.SqlTypes.SqlInt64" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SqlString ToSqlString()
	{
		return (SqlString)this;
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlInt64" /> instance to the supplied <see cref="T:System.Object" /> and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return value Condition Less than zero This instance is less than the object. Zero This instance is the same as the object. Greater than zero This instance is greater than the object -or- The object is a null reference (Nothing in Visual Basic). </returns>
	/// <param name="value">The <see cref="T:System.Object" /> to be compared. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int CompareTo(object value)
	{
		if (value is SqlInt64 value2)
		{
			return CompareTo(value2);
		}
		throw ADP.WrongType(value.GetType(), typeof(SqlInt64));
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlInt64" /> instance to the supplied <see cref="T:System.Data.SqlTypes.SqlInt64" /> and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return value Condition Less than zero This instance is less than the object. Zero This instance is the same as the object. Greater than zero This instance is greater than the object -or- The object is a null reference (Nothing in Visual Basic). </returns>
	/// <param name="value">The <see cref="T:System.Data.SqlTypes.SqlInt64" /> to be compared.</param>
	public int CompareTo(SqlInt64 value)
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

	/// <summary>Compares the supplied object parameter to the <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlInt64" /> object.</summary>
	/// <returns>true if object is an instance of <see cref="T:System.Data.SqlTypes.SqlInt64" /> and the two are equal; otherwise false.</returns>
	/// <param name="value">The object to be compared. </param>
	public override bool Equals(object value)
	{
		if (!(value is SqlInt64 sqlInt))
		{
			return false;
		}
		if (sqlInt.IsNull || IsNull)
		{
			if (sqlInt.IsNull)
			{
				return IsNull;
			}
			return false;
		}
		return (this == sqlInt).Value;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		if (!IsNull)
		{
			return Value.GetHashCode();
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
			m_fNotNull = false;
		}
		else
		{
			m_value = XmlConvert.ToInt64(reader.ReadElementString());
			m_fNotNull = true;
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
			writer.WriteString(XmlConvert.ToString(m_value));
		}
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>A string that indicates the XSD of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</returns>
	/// <param name="schemaSet">An <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
	{
		return new XmlQualifiedName("long", "http://www.w3.org/2001/XMLSchema");
	}
}
