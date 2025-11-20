using System.Data.Common;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes;

/// <summary>Represents a 16-bit signed integer to be stored in or retrieved from a database.</summary>
[Serializable]
[XmlSchemaProvider("GetXsdType")]
public struct SqlInt16 : INullable, IComparable, IXmlSerializable
{
	private bool m_fNotNull;

	private short m_value;

	private static readonly int s_MASKI2 = -32768;

	/// <summary>Represents a <see cref="T:System.DBNull" /> that can be assigned to this instance of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure.</summary>
	public static readonly SqlInt16 Null = new SqlInt16(fNull: true);

	/// <summary>Represents a zero value that can be assigned to the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property of an instance of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure.</summary>
	public static readonly SqlInt16 Zero = new SqlInt16(0);

	/// <summary>A constant representing the smallest possible value of a <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	public static readonly SqlInt16 MinValue = new SqlInt16(short.MinValue);

	/// <summary>A constant representing the largest possible value of a <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	public static readonly SqlInt16 MaxValue = new SqlInt16(short.MaxValue);

	/// <summary>Indicates whether this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure is null.</summary>
	/// <returns>true if null. Otherwise, false. For more information, see Handling Null Values (ADO.NET).</returns>
	public bool IsNull => !m_fNotNull;

	/// <summary>Gets the value of this instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. This property is read-only.</summary>
	/// <returns>A short integer representing the value of this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure.</returns>
	public short Value
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

	private SqlInt16(bool fNull)
	{
		m_fNotNull = false;
		m_value = 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure using the supplied short integer parameter.</summary>
	/// <param name="value">A short integer. </param>
	public SqlInt16(short value)
	{
		m_value = value;
		m_fNotNull = true;
	}

	/// <summary>Converts the supplied short integer to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure with the same value as the specified short integer.</returns>
	/// <param name="x">A short integer value. </param>
	public static implicit operator SqlInt16(short x)
	{
		return new SqlInt16(x);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to a short integer.</summary>
	/// <returns>A short integer whose value is the Value of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static explicit operator short(SqlInt16 x)
	{
		return x.Value;
	}

	/// <summary>Converts a <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> object representing the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> of this instance of <see cref="T:System.Data.SqlTypes.SqlInt16" />.</returns>
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

	/// <summary>Converts the <see cref="T:System.String" /> representation of a number to its 16-bit signed integer equivalent.</summary>
	/// <returns>A 16-bit signed integer equivalent to the value that is contained in the specified <see cref="T:System.String" />.</returns>
	/// <param name="s">The String to be parsed. </param>
	public static SqlInt16 Parse(string s)
	{
		if (s == SQLResource.NullString)
		{
			return Null;
		}
		return new SqlInt16(short.Parse(s, null));
	}

	/// <summary>The unary minus operator negates the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> operand.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure that contains the negated value.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 operator -(SqlInt16 x)
	{
		if (!x.IsNull)
		{
			return new SqlInt16((short)(-x.m_value));
		}
		return Null;
	}

	/// <summary>The ~ operator performs a bitwise one's complement operation on its <see cref="T:System.Data.SqlTypes.SqlByte" /> operand.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the complement of the specified <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 operator ~(SqlInt16 x)
	{
		if (!x.IsNull)
		{
			return new SqlInt16((short)(~x.m_value));
		}
		return Null;
	}

	/// <summary>Computes the sum of the two <see cref="T:System.Data.SqlTypes.SqlInt16" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the sum of the two <see cref="T:System.Data.SqlTypes.SqlInt16" /> operands.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 operator +(SqlInt16 x, SqlInt16 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		int num = x.m_value + y.m_value;
		if ((((num >> 15) ^ (num >> 16)) & 1) != 0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt16((short)num);
	}

	/// <summary>Subtracts the second <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter from the first.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the results of the subtraction.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 operator -(SqlInt16 x, SqlInt16 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		int num = x.m_value - y.m_value;
		if ((((num >> 15) ^ (num >> 16)) & 1) != 0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt16((short)num);
	}

	/// <summary>Computes the product of the two <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameters.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> contains the product of the two parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 operator *(SqlInt16 x, SqlInt16 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		int num = x.m_value * y.m_value;
		int num2 = num & s_MASKI2;
		if (num2 != 0 && num2 != s_MASKI2)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt16((short)num);
	}

	/// <summary>Divides the first <see cref="T:System.Data.SqlTypes.SqlInt16" /> operand by the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the results of the division.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 operator /(SqlInt16 x, SqlInt16 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		if (y.m_value != 0)
		{
			if (x.m_value == short.MinValue && y.m_value == -1)
			{
				throw new OverflowException(SQLResource.ArithOverflowMessage);
			}
			return new SqlInt16((short)(x.m_value / y.m_value));
		}
		throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
	}

	/// <summary>Computes the remainder after dividing its first <see cref="T:System.Data.SqlTypes.SqlInt16" /> operand by its second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> contains the remainder.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 operator %(SqlInt16 x, SqlInt16 y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		if (y.m_value != 0)
		{
			if (x.m_value == short.MinValue && y.m_value == -1)
			{
				throw new OverflowException(SQLResource.ArithOverflowMessage);
			}
			return new SqlInt16((short)(x.m_value % y.m_value));
		}
		throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
	}

	/// <summary>Computes the bitwise AND of its <see cref="T:System.Data.SqlTypes.SqlInt16" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the results of the bitwise AND.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 operator &(SqlInt16 x, SqlInt16 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlInt16((short)(x.m_value & y.m_value));
		}
		return Null;
	}

	/// <summary>Computes the bitwise OR of its two <see cref="T:System.Data.SqlTypes.SqlInt16" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the results of the bitwise OR.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 operator |(SqlInt16 x, SqlInt16 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlInt16((short)((ushort)x.m_value | (ushort)y.m_value));
		}
		return Null;
	}

	/// <summary>Performs a bitwise exclusive-OR operation on the supplied parameters.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the results of the bitwise XOR.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 operator ^(SqlInt16 x, SqlInt16 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlInt16((short)(x.m_value ^ y.m_value));
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlBoolean.ByteValue" /> property of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static explicit operator SqlInt16(SqlBoolean x)
	{
		if (!x.IsNull)
		{
			return new SqlInt16(x.ByteValue);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlByte" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static implicit operator SqlInt16(SqlByte x)
	{
		if (!x.IsNull)
		{
			return new SqlInt16(x.Value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> of the supplied <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static explicit operator SqlInt16(SqlInt32 x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		int value = x.Value;
		if (value > 32767 || value < -32768)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt16((short)value);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static explicit operator SqlInt16(SqlInt64 x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		long value = x.Value;
		if (value > 32767 || value < -32768)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt16((short)value);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property is equal to the integer part of the <see cref="T:System.Data.SqlTypes.SqlSingle" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure. </param>
	public static explicit operator SqlInt16(SqlSingle x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		float value = x.Value;
		if (value < -32768f || value > 32767f)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt16((short)value);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property is equal to the integer part of the <see cref="T:System.Data.SqlTypes.SqlDouble" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure. </param>
	public static explicit operator SqlInt16(SqlDouble x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		double value = x.Value;
		if (value < -32768.0 || value > 32767.0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlInt16((short)value);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure. </param>
	public static explicit operator SqlInt16(SqlMoney x)
	{
		if (!x.IsNull)
		{
			return new SqlInt16(checked((short)x.ToInt32()));
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlDecimal.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlDecimal" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure. </param>
	public static explicit operator SqlInt16(SqlDecimal x)
	{
		return (SqlInt16)(SqlInt32)x;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlString" /> object to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property is equal to the value represented by the <see cref="T:System.Data.SqlTypes.SqlString" /> object parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlString" /> object. </param>
	public static explicit operator SqlInt16(SqlString x)
	{
		if (!x.IsNull)
		{
			return new SqlInt16(short.Parse(x.Value, null));
		}
		return Null;
	}

	/// <summary>Performs a logical comparison of two <see cref="T:System.Data.SqlTypes.SqlInt16" /> structures to determine whether they are equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are not equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean operator ==(SqlInt16 x, SqlInt16 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value == y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Performs a logical comparison of two <see cref="T:System.Data.SqlTypes.SqlInt16" /> structures to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean operator !=(SqlInt16 x, SqlInt16 y)
	{
		return !(x == y);
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlInt16" /> to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean operator <(SqlInt16 x, SqlInt16 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value < y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlInt16" /> to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean operator >(SqlInt16 x, SqlInt16 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value > y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlInt16" /> structures to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean operator <=(SqlInt16 x, SqlInt16 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value <= y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlInt16" /> structures to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean operator >=(SqlInt16 x, SqlInt16 y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value >= y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>The ~ operator performs a bitwise one's complement operation on its <see cref="T:System.Data.SqlTypes.SqlByte" /> operand.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the complement of the specified <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 OnesComplement(SqlInt16 x)
	{
		return ~x;
	}

	/// <summary>Computes the sum of the two <see cref="T:System.Data.SqlTypes.SqlInt16" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the sum of the two <see cref="T:System.Data.SqlTypes.SqlInt16" /> operands.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 Add(SqlInt16 x, SqlInt16 y)
	{
		return x + y;
	}

	/// <summary>Subtracts the second <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter from the first.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the results of the subtraction.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 Subtract(SqlInt16 x, SqlInt16 y)
	{
		return x - y;
	}

	/// <summary>Computes the product of the two <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameters.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> contains the product of the two parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 Multiply(SqlInt16 x, SqlInt16 y)
	{
		return x * y;
	}

	/// <summary>Divides the first <see cref="T:System.Data.SqlTypes.SqlInt16" /> operand by the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the results of the division.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 Divide(SqlInt16 x, SqlInt16 y)
	{
		return x / y;
	}

	/// <summary>Computes the remainder after dividing its first <see cref="T:System.Data.SqlTypes.SqlInt16" /> operand by its second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> contains the remainder.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 Mod(SqlInt16 x, SqlInt16 y)
	{
		return x % y;
	}

	/// <summary>Divides two <see cref="T:System.Data.SqlTypes.SqlInt16" /> values and returns the remainder.</summary>
	/// <returns>The remainder left after division is performed on <paramref name="x" /> and <paramref name="y" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> value.</param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> value.</param>
	public static SqlInt16 Modulus(SqlInt16 x, SqlInt16 y)
	{
		return x % y;
	}

	/// <summary>Computes the bitwise AND of its <see cref="T:System.Data.SqlTypes.SqlInt16" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the results of the bitwise AND.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 BitwiseAnd(SqlInt16 x, SqlInt16 y)
	{
		return x & y;
	}

	/// <summary>Computes the bitwise OR of its two <see cref="T:System.Data.SqlTypes.SqlInt16" /> operands.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property contains the results of the bitwise OR.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 BitwiseOr(SqlInt16 x, SqlInt16 y)
	{
		return x | y;
	}

	/// <summary>Performs a bitwise exclusive-OR operation on the supplied parameters.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure that contains the results of the XOR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlInt16 Xor(SqlInt16 x, SqlInt16 y)
	{
		return x ^ y;
	}

	/// <summary>Performs a logical comparison of two <see cref="T:System.Data.SqlTypes.SqlInt16" /> structures to determine whether they are equal.</summary>
	/// <returns>true if the two values are equal. Otherwise, false. If either instance is null, then the SqlInt16 will be null.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean Equals(SqlInt16 x, SqlInt16 y)
	{
		return x == y;
	}

	/// <summary>Performs a logical comparison of two <see cref="T:System.Data.SqlTypes.SqlInt16" /> structures to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean NotEquals(SqlInt16 x, SqlInt16 y)
	{
		return x != y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlInt16" /> to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean LessThan(SqlInt16 x, SqlInt16 y)
	{
		return x < y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlInt16" /> to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean GreaterThan(SqlInt16 x, SqlInt16 y)
	{
		return x > y;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlInt16" /> structures to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean LessThanOrEqual(SqlInt16 x, SqlInt16 y)
	{
		return x <= y;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlInt16" /> structures to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static SqlBoolean GreaterThanOrEqual(SqlInt16 x, SqlInt16 y)
	{
		return x >= y;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</summary>
	/// <returns>true if the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> is non-zero; false if zero; otherwise Null.</returns>
	public SqlBoolean ToSqlBoolean()
	{
		return (SqlBoolean)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> equals the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> of this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. If the value of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> is less than 0 or greater than 255, an <see cref="T:System.OverflowException" /> occurs.</returns>
	public SqlByte ToSqlByte()
	{
		return (SqlByte)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to <see cref="T:System.Data.SqlTypes.SqlDouble" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure whose Value equals the value of this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure.</returns>
	public SqlDouble ToSqlDouble()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure whose Value equals the value of this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure.</returns>
	public SqlInt32 ToSqlInt32()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure whose Value equals the value of this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure.</returns>
	public SqlInt64 ToSqlInt64()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose Value equals the value of this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure.</returns>
	public SqlMoney ToSqlMoney()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to <see cref="T:System.Data.SqlTypes.SqlDecimal" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure whose Value equals the value of this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure.</returns>
	public SqlDecimal ToSqlDecimal()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to <see cref="T:System.Data.SqlTypes.SqlSingle" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure whose Value equals the value of this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure.</returns>
	public SqlSingle ToSqlSingle()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure to <see cref="T:System.Data.SqlTypes.SqlString" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlString" /> representing the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> of this instance of <see cref="T:System.Data.SqlTypes.SqlInt16" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SqlString ToSqlString()
	{
		return (SqlString)this;
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlInt16" /> instance to the supplied <see cref="T:System.Object" /> and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return value Condition Less than zero This instance is less than the object. Zero This instance is the same as the object. Greater than zero This instance is greater than the object -or- object is a null reference (Nothing in Visual Basic) </returns>
	/// <param name="value">The <see cref="T:System.Object" /> to be compared. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int CompareTo(object value)
	{
		if (value is SqlInt16 value2)
		{
			return CompareTo(value2);
		}
		throw ADP.WrongType(value.GetType(), typeof(SqlInt16));
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlInt16" /> instance to the supplied <see cref="T:System.Data.SqlTypes.SqlInt16" /> and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return value Condition Less than zero This instance is less than the object. Zero This instance is the same as the object. Greater than zero This instance is greater than the object -or- The object is a null reference (Nothing in Visual Basic) </returns>
	/// <param name="value">The <see cref="T:System.Data.SqlTypes.SqlInt16" /> to be compared.</param>
	public int CompareTo(SqlInt16 value)
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

	/// <summary>Compares the specified object to the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> object.</summary>
	/// <returns>true if object is an instance of <see cref="T:System.Data.SqlTypes.SqlInt16" /> and the two are equal; otherwise false.</returns>
	/// <param name="value">The object to be compared. </param>
	public override bool Equals(object value)
	{
		if (!(value is SqlInt16 sqlInt))
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
	/// <returns>An XML schema consumed by .NET Framework.</returns>
	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code. </summary>
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
			m_value = XmlConvert.ToInt16(reader.ReadElementString());
			m_fNotNull = true;
		}
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="writer">XmlWriter </param>
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
	/// <returns>A <see cref="T:System.String" /> value that indicates the XSD of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</returns>
	/// <param name="schemaSet">An <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
	{
		return new XmlQualifiedName("short", "http://www.w3.org/2001/XMLSchema");
	}
}
