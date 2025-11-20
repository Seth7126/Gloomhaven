using System.Data.Common;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes;

/// <summary>Represents a 32-bit signed integer to be stored in or retrieved from a database.</summary>
[Serializable]
[XmlSchemaProvider("GetXsdType")]
public struct SqlInt32 : INullable, IComparable, IXmlSerializable
{
	private bool m_fNotNull;

	private int m_value;

	private static readonly long s_iIntMin = -2147483648L;

	private static readonly long s_lBitNotIntMax = -2147483648L;

	/// <summary>Represents a <see cref="T:System.DBNull" /> that can be assigned to this instance of the <see cref="T:System.Data.SqlTypes.SqlInt32" /> class.</summary>
	public static readonly SqlInt32 Null = new SqlInt32(fNull: true);

	/// <summary>Represents a zero value that can be assigned to the <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property of an instance of the <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure.</summary>
	public static readonly SqlInt32 Zero = new SqlInt32(0);

	/// <summary>A constant representing the smallest possible value of a <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	public static readonly SqlInt32 MinValue = new SqlInt32(int.MinValue);

	/// <summary>A constant representing the largest possible value of a <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	public static readonly SqlInt32 MaxValue = new SqlInt32(int.MaxValue);

	/// <summary>Indicates whether this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure is null.</summary>
	/// <returns>This property is true if <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> is null. Otherwise, false.</returns>
	public bool IsNull => !m_fNotNull;

	/// <summary>Gets the value of this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. This property is read-only.</summary>
	/// <returns>An integer representing the value of this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure.</returns>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The property contains <see cref="F:System.Data.SqlTypes.SqlInt32.Null" />. </exception>
	public int Value
	{
		get
		{
			if (IsNull)
			{
				throw new SqlNullValueException();
			}
			return m_value;
		}
	}

	private SqlInt32(bool fNull)
	{
		m_fNotNull = false;
		m_value = 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure using the supplied integer value.</summary>
	/// <param name="value">The integer to be converted. </param>
	public SqlInt32(int value)
	{
		m_value = value;
		m_fNotNull = true;
	}

	/// <summary>Converts the supplied integer to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose Value property is equal to the integer parameter.</returns>
	/// <param name="x">An integer value. </param>
	public static implicit operator SqlInt32(int x)
	{
		return new SqlInt32(x);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to an integer.</summary>
	/// <returns>The converted integer value.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static explicit operator int(SqlInt32 x)
	{
		return x.Value;
	}

	/// <summary>Converts a <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> structure equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt32" />.</returns>
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

	/// <summary>Converts the <see cref="T:System.String" /> representation of a number to its 32-bit signed integer equivalent.</summary>
	/// <returns>A 32-bit signed integer equivalent to the value that is contained in the specified <see cref="T:System.String" />.</returns>
	/// <param name="s">The <see cref="T:System.String" /> to be parsed. </param>
	public static SqlInt32 Parse(string s)
	{
		if (s == SQLResource.NullString)
		{
			return Null;
		}
		return new SqlInt32(int.Parse(s, null));
	}

	/// <summary>Negates the <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlInt32" /> operand.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure that contains the negated value.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 operator -(SqlInt32 x)
	{
		if (!x.IsNull)
		{
			return new SqlInt32(-x.m_value);
		}
		return Null;
	}

	/// <summary>Performs a bitwise one's complement operation on the specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure that contains the results of the one's complement operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 operator ~(SqlInt32 x)
	{
		if (!x.IsNull)
		{
			return new SqlInt32(~x.m_value);
		}
		return Null;
	}

	/// <summary>Computes the sum of the two specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> structures.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property contains the sum of the specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> structures.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 operator +(SqlInt32 x, SqlInt32 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		int num = x.m_value + y.m_value;
		if (SameSignInt(x.m_value, y.m_value) && !SameSignInt(x.m_value, num))
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt32(num);
	}

	/// <summary>Subtracts the second <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter from the first.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property contains the results of the subtraction.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 operator -(SqlInt32 x, SqlInt32 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		int num = x.m_value - y.m_value;
		if (!SameSignInt(x.m_value, y.m_value) && SameSignInt(y.m_value, num))
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt32(num);
	}

	/// <summary>Computes the product of the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> contains the product of the two parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 operator *(SqlInt32 x, SqlInt32 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		long num = (long)x.m_value * (long)y.m_value;
		long num2 = num & s_lBitNotIntMax;
		if (num2 != 0L && num2 != s_lBitNotIntMax)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt32((int)num);
	}

	/// <summary>Divides the first <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter from the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property contains the results of the division.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 operator /(SqlInt32 x, SqlInt32 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		if (y.m_value != 0)
		{
			if (x.m_value == s_iIntMin && y.m_value == -1)
			{
				throw new OverflowException(SQLResource.ArithOverflowMessage);
			}
			return new SqlInt32(x.m_value / y.m_value);
		}
		throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
	}

	/// <summary>Computes the remainder after dividing the first <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter by the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> contains the remainder.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 operator %(SqlInt32 x, SqlInt32 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		if (y.m_value != 0)
		{
			if (x.m_value == s_iIntMin && y.m_value == -1)
			{
				throw new OverflowException(SQLResource.ArithOverflowMessage);
			}
			return new SqlInt32(x.m_value % y.m_value);
		}
		throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
	}

	/// <summary>Computes the bitwise AND of its <see cref="T:System.Data.SqlTypes.SqlInt32" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure that contains the results of the bitwise AND operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 operator &(SqlInt32 x, SqlInt32 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlInt32(x.m_value & y.m_value);
		}
		return Null;
	}

	/// <summary>Computes the bitwise OR of the specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> structures.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure that contains the results of the bitwise OR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 operator |(SqlInt32 x, SqlInt32 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlInt32(x.m_value | y.m_value);
		}
		return Null;
	}

	/// <summary>Performs a bitwise exclusive-OR operation on the specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> structures.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure that contains the results of the bitwise XOR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 operator ^(SqlInt32 x, SqlInt32 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlInt32(x.m_value ^ y.m_value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlBoolean.ByteValue" /> property of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static explicit operator SqlInt32(SqlBoolean x)
	{
		if (!x.IsNull)
		{
			return new SqlInt32(x.ByteValue);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlByte" /> property to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlByte" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static implicit operator SqlInt32(SqlByte x)
	{
		if (!x.IsNull)
		{
			return new SqlInt32(x.Value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt16" /> to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static implicit operator SqlInt32(SqlInt16 x)
	{
		if (!x.IsNull)
		{
			return new SqlInt32(x.Value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt64" /> to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property of the SqlInt64 parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static explicit operator SqlInt32(SqlInt64 x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		long value = x.Value;
		if (value > int.MaxValue || value < int.MinValue)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt32((int)value);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlSingle" /> to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property equals the integer part of the <see cref="T:System.Data.SqlTypes.SqlSingle" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure. </param>
	public static explicit operator SqlInt32(SqlSingle x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		float value = x.Value;
		if (value > 2.1474836E+09f || value < -2.1474836E+09f)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt32((int)value);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlDouble" /> to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property equals the integer part of the <see cref="T:System.Data.SqlTypes.SqlDouble" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure. </param>
	public static explicit operator SqlInt32(SqlDouble x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		double value = x.Value;
		if (value > 2147483647.0 || value < -2147483648.0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt32((int)value);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static explicit operator SqlInt32(SqlMoney x)
	{
		if (!x.IsNull)
		{
			return new SqlInt32(x.ToInt32());
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlDecimal.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlDecimal" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure. </param>
	public static explicit operator SqlInt32(SqlDecimal x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		x.AdjustScale(-x.Scale, fRound: true);
		long num = x._data1;
		if (!x.IsPositive)
		{
			num = -num;
		}
		if (x._bLen > 1 || num > int.MaxValue || num < int.MinValue)
		{
			throw new OverflowException(SQLResource.ConversionOverflowMessage);
		}
		return new SqlInt32((int)num);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlString" /> object to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property equals the value represented by the <see cref="T:System.Data.SqlTypes.SqlString" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlString" /> object. </param>
	public static explicit operator SqlInt32(SqlString x)
	{
		if (!x.IsNull)
		{
			return new SqlInt32(int.Parse(x.Value, null));
		}
		return Null;
	}

	private static bool SameSignInt(int x, int y)
	{
		return ((x ^ y) & 0x80000000u) == 0;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether they are equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are not equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean operator ==(SqlInt32 x, SqlInt32 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value == y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performa a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean operator !=(SqlInt32 x, SqlInt32 y)
	{
		return !(x == y);
	}

	/// <summary>Compares the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean operator <(SqlInt32 x, SqlInt32 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value < y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean operator >(SqlInt32 x, SqlInt32 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value > y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean operator <=(SqlInt32 x, SqlInt32 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value <= y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean operator >=(SqlInt32 x, SqlInt32 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value >= y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a bitwise one's complement operation on the specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure that contains the results of the one's complement operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 OnesComplement(SqlInt32 x)
	{
		return ~x;
	}

	/// <summary>Computes the sum of the two specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> structures.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property contains the sum of the specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> structures.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 Add(SqlInt32 x, SqlInt32 y)
	{
		return x + y;
	}

	/// <summary>Subtracts the second <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter from the first.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property contains the results of the subtraction.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 Subtract(SqlInt32 x, SqlInt32 y)
	{
		return x - y;
	}

	/// <summary>Computes the product of the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> contains the product of the two parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 Multiply(SqlInt32 x, SqlInt32 y)
	{
		return x * y;
	}

	/// <summary>Divides the first <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter from the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property contains the results of the division.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 Divide(SqlInt32 x, SqlInt32 y)
	{
		return x / y;
	}

	/// <summary>Computes the remainder after dividing the first <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter by the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> contains the remainder.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 Mod(SqlInt32 x, SqlInt32 y)
	{
		return x % y;
	}

	/// <summary>Divides two <see cref="T:System.Data.SqlTypes.SqlInt32" /> values and returns the remainder.</summary>
	/// <returns>The remainder left after division is performed on <paramref name="x" /> and <paramref name="y" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> value.</param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> value.</param>
	public static SqlInt32 Modulus(SqlInt32 x, SqlInt32 y)
	{
		return x % y;
	}

	/// <summary>Computes the bitwise AND of its <see cref="T:System.Data.SqlTypes.SqlInt32" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure that contains the results of the bitwise AND operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 BitwiseAnd(SqlInt32 x, SqlInt32 y)
	{
		return x & y;
	}

	/// <summary>Computes the bitwise OR of the specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> structures.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure that contains the results of the bitwise OR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 BitwiseOr(SqlInt32 x, SqlInt32 y)
	{
		return x | y;
	}

	/// <summary>Performs a bitwise exclusive-OR operation on the specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> structures.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure that contains the results of the bitwise XOR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlInt32 Xor(SqlInt32 x, SqlInt32 y)
	{
		return x ^ y;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether they are equal.</summary>
	/// <returns>true if the two values are equal. Otherwise, false. If either instance is null, then the SqlInt32 will be null.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean Equals(SqlInt32 x, SqlInt32 y)
	{
		return x == y;
	}

	/// <summary>Performs a logical comparison of the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean NotEquals(SqlInt32 x, SqlInt32 y)
	{
		return x != y;
	}

	/// <summary>Compares the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean LessThan(SqlInt32 x, SqlInt32 y)
	{
		return x < y;
	}

	/// <summary>Compares the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean GreaterThan(SqlInt32 x, SqlInt32 y)
	{
		return x > y;
	}

	/// <summary>Compares the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean LessThanOrEqual(SqlInt32 x, SqlInt32 y)
	{
		return x <= y;
	}

	/// <summary>Compares the two <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameters to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static SqlBoolean GreaterThanOrEqual(SqlInt32 x, SqlInt32 y)
	{
		return x >= y;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</summary>
	/// <returns>true if the <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> is non-zero; false if zero; otherwise Null.</returns>
	public SqlBoolean ToSqlBoolean()
	{
		return (SqlBoolean)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose Value equals the Value of this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. If the value of the SqlInt32 is less than 0 or greater than 255, an <see cref="T:System.OverflowException" /> occurs.</returns>
	public SqlByte ToSqlByte()
	{
		return (SqlByte)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to <see cref="T:System.Data.SqlTypes.SqlDouble" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt32" />.</returns>
	public SqlDouble ToSqlDouble()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt32" />.</returns>
	public SqlInt16 ToSqlInt16()
	{
		return (SqlInt16)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt32" />.</returns>
	public SqlInt64 ToSqlInt64()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt32" />.</returns>
	public SqlMoney ToSqlMoney()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to <see cref="T:System.Data.SqlTypes.SqlDecimal" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt32" />.</returns>
	public SqlDecimal ToSqlDecimal()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to <see cref="T:System.Data.SqlTypes.SqlSingle" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt32" />.</returns>
	public SqlSingle ToSqlSingle()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to <see cref="T:System.Data.SqlTypes.SqlString" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlString" /> structure equal to the value of this <see cref="T:System.Data.SqlTypes.SqlInt32" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SqlString ToSqlString()
	{
		return (SqlString)this;
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlInt32" /> instance to the supplied <see cref="T:System.Object" /> and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return value Condition Less than zero This instance is less than the object. Zero This instance is the same as the object. Greater than zero This instance is greater than the object -or- The object is a null reference (Nothing in Visual Basic). </returns>
	/// <param name="value">The <see cref="T:System.Object" /> to be compared. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int CompareTo(object value)
	{
		if (value is SqlInt32 value2)
		{
			return CompareTo(value2);
		}
		throw ADP.WrongType(value.GetType(), typeof(SqlInt32));
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlInt32" /> instance to the supplied <see cref="T:System.Data.SqlTypes.SqlInt32" /> and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return value Condition Less than zero This instance is less than the object. Zero This instance is the same as the object. Greater than zero This instance is greater than the object -or- The object is a null reference (Nothing in Visual Basic) </returns>
	/// <param name="value">The <see cref="T:System.Data.SqlTypes.SqlInt32" /> to be compared.</param>
	public int CompareTo(SqlInt32 value)
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

	/// <summary>Compares the supplied object parameter to the <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlInt32" /> object.</summary>
	/// <returns>true if object is an instance of <see cref="T:System.Data.SqlTypes.SqlInt32" /> and the two are equal; otherwise false.</returns>
	/// <param name="value">The object to be compared. </param>
	public override bool Equals(object value)
	{
		if (!(value is SqlInt32 sqlInt))
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
	/// <param name="reader">XmlReader </param>
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
			m_value = XmlConvert.ToInt32(reader.ReadElementString());
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
	/// <returns>A string value that indicates the XSD of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</returns>
	/// <param name="schemaSet">An <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
	{
		return new XmlQualifiedName("int", "http://www.w3.org/2001/XMLSchema");
	}
}
