using System.Data.Common;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes;

/// <summary>Represents an 8-bit unsigned integer, in the range of 0 through 255, to be stored in or retrieved from a database. </summary>
[Serializable]
[XmlSchemaProvider("GetXsdType")]
public struct SqlByte : INullable, IComparable, IXmlSerializable
{
	private bool m_fNotNull;

	private byte m_value;

	private static readonly int s_iBitNotByteMax = -256;

	/// <summary>Represents a <see cref="T:System.DBNull" /> that can be assigned to this instance of the <see cref="T:System.Data.SqlTypes.SqlByte" /> structure.</summary>
	public static readonly SqlByte Null = new SqlByte(fNull: true);

	/// <summary>Represents a zero value that can be assigned to the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property of an instance of the <see cref="T:System.Data.SqlTypes.SqlByte" /> structure.</summary>
	public static readonly SqlByte Zero = new SqlByte(0);

	/// <summary>A constant representing the smallest possible value of a <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	public static readonly SqlByte MinValue = new SqlByte(0);

	/// <summary>A constant representing the largest possible value of a <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	public static readonly SqlByte MaxValue = new SqlByte(byte.MaxValue);

	/// <summary>Gets a Boolean value that indicates whether this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure is null.</summary>
	/// <returns>true if null. Otherwise, false.</returns>
	public bool IsNull => !m_fNotNull;

	/// <summary>Gets the value of the <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. This property is read-only </summary>
	/// <returns>The value of the <see cref="T:System.Data.SqlTypes.SqlByte" /> structure.</returns>
	public byte Value
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

	private SqlByte(bool fNull)
	{
		m_fNotNull = false;
		m_value = 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlByte" /> structure using the specified byte value.</summary>
	/// <param name="value">A byte value to be stored in the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property of the new <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public SqlByte(byte value)
	{
		m_value = value;
		m_fNotNull = true;
	}

	/// <summary>Converts the supplied byte value to a <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property is equal to the supplied parameter.</returns>
	/// <param name="x">A byte value to be converted to <see cref="T:System.Data.SqlTypes.SqlByte" />. </param>
	public static implicit operator SqlByte(byte x)
	{
		return new SqlByte(x);
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to a byte.</summary>
	/// <returns>A byte whose value equals the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlByte" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to be converted to a byte. </param>
	public static explicit operator byte(SqlByte x)
	{
		return x.Value;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to a <see cref="T:System.String" />.</summary>
	/// <returns>A string that contains the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlByte" />. If the Value is null, the String will be a null string.</returns>
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

	/// <summary>Converts the <see cref="T:System.String" /> representation of a number to its 8-bit unsigned integer equivalent.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure that contains the 8-bit number represented by the String parameter.</returns>
	/// <param name="s">The String to be parsed. </param>
	public static SqlByte Parse(string s)
	{
		if (s == SQLResource.NullString)
		{
			return Null;
		}
		return new SqlByte(byte.Parse(s, null));
	}

	/// <summary>The ones complement operator performs a bitwise one's complement operation on its <see cref="T:System.Data.SqlTypes.SqlByte" /> operand.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property contains the ones complement of the <see cref="T:System.Data.SqlTypes.SqlByte" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte operator ~(SqlByte x)
	{
		if (!x.IsNull)
		{
			return new SqlByte((byte)(~x.m_value));
		}
		return Null;
	}

	/// <summary>Computes the sum of the two specified <see cref="T:System.Data.SqlTypes.SqlByte" /> structures.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property contains the sum of the two operands.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte operator +(SqlByte x, SqlByte y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		int num = x.m_value + y.m_value;
		if ((num & s_iBitNotByteMax) != 0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlByte((byte)num);
	}

	/// <summary>Subtracts the second <see cref="T:System.Data.SqlTypes.SqlByte" /> operand from the first.</summary>
	/// <returns>The results of subtracting the second <see cref="T:System.Data.SqlTypes.SqlByte" /> operand from the first.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte operator -(SqlByte x, SqlByte y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		int num = x.m_value - y.m_value;
		if ((num & s_iBitNotByteMax) != 0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlByte((byte)num);
	}

	/// <summary>Computes the product of the two <see cref="T:System.Data.SqlTypes.SqlByte" /> operands.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property contains the product of the multiplication.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte operator *(SqlByte x, SqlByte y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		int num = x.m_value * y.m_value;
		if ((num & s_iBitNotByteMax) != 0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		return new SqlByte((byte)num);
	}

	/// <summary>Divides its first <see cref="T:System.Data.SqlTypes.SqlByte" /> operand by its second.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property contains the results of the division.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte operator /(SqlByte x, SqlByte y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		if (y.m_value != 0)
		{
			return new SqlByte((byte)(x.m_value / y.m_value));
		}
		throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
	}

	/// <summary>Computes the remainder after dividing its first <see cref="T:System.Data.SqlTypes.SqlByte" /> operand by its second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> contains the remainder.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte operator %(SqlByte x, SqlByte y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		if (y.m_value != 0)
		{
			return new SqlByte((byte)(x.m_value % y.m_value));
		}
		throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
	}

	/// <summary>Computes the bitwise AND of its <see cref="T:System.Data.SqlTypes.SqlByte" /> operands.</summary>
	/// <returns>The results of the bitwise AND operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte operator &(SqlByte x, SqlByte y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlByte((byte)(x.m_value & y.m_value));
		}
		return Null;
	}

	/// <summary>Computes the bitwise OR of its two <see cref="T:System.Data.SqlTypes.SqlByte" /> operands.</summary>
	/// <returns>The results of the bitwise OR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte operator |(SqlByte x, SqlByte y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlByte((byte)(x.m_value | y.m_value));
		}
		return Null;
	}

	/// <summary>Performs a bitwise exclusive-OR operation on the supplied parameters.</summary>
	/// <returns>The results of the bitwise XOR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte operator ^(SqlByte x, SqlByte y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlByte((byte)(x.m_value ^ y.m_value));
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property equals the <see cref="P:System.Data.SqlTypes.SqlBoolean.ByteValue" /> of the supplied <see cref="T:System.Data.SqlTypes.SqlBoolean" /> parameter.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlBoolean" /> parameter to be converted to a <see cref="T:System.Data.SqlTypes.SqlByte" />. </param>
	public static explicit operator SqlByte(SqlBoolean x)
	{
		if (!x.IsNull)
		{
			return new SqlByte(x.ByteValue);
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter.</returns>
	/// <param name="x">A SqlMoney structure. </param>
	public static explicit operator SqlByte(SqlMoney x)
	{
		if (!x.IsNull)
		{
			return new SqlByte(checked((byte)x.ToInt32()));
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure. </param>
	public static explicit operator SqlByte(SqlInt16 x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		if (x.Value > 255 || x.Value < 0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		if (!x.IsNull)
		{
			return new SqlByte((byte)x.Value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt32" /> to <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure. </param>
	public static explicit operator SqlByte(SqlInt32 x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		if (x.Value > 255 || x.Value < 0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		if (!x.IsNull)
		{
			return new SqlByte((byte)x.Value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlInt64" /> to <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> of the SqlInt64 parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> structure. </param>
	public static explicit operator SqlByte(SqlInt64 x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		if (x.Value > 255 || x.Value < 0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		if (!x.IsNull)
		{
			return new SqlByte((byte)x.Value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure to <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlSingle.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlSingle" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure. </param>
	public static explicit operator SqlByte(SqlSingle x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		if (x.Value > 255f || x.Value < 0f)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		if (!x.IsNull)
		{
			return new SqlByte((byte)x.Value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlDouble" /> to <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlDouble.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlDouble" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure. </param>
	public static explicit operator SqlByte(SqlDouble x)
	{
		if (x.IsNull)
		{
			return Null;
		}
		if (x.Value > 255.0 || x.Value < 0.0)
		{
			throw new OverflowException(SQLResource.ArithOverflowMessage);
		}
		if (!x.IsNull)
		{
			return new SqlByte((byte)x.Value);
		}
		return Null;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlDecimal" /> to <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property is equal to the <see cref="P:System.Data.SqlTypes.SqlDecimal.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlDecimal" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure. </param>
	public static explicit operator SqlByte(SqlDecimal x)
	{
		return (SqlByte)(SqlInt32)x;
	}

	/// <summary>Converts the supplied <see cref="T:System.Data.SqlTypes.SqlString" /> to <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property is equal to the numeric value represented by the <see cref="T:System.Data.SqlTypes.SqlString" />.</returns>
	/// <param name="x">An instance of the SqlString class. </param>
	public static explicit operator SqlByte(SqlString x)
	{
		if (!x.IsNull)
		{
			return new SqlByte(byte.Parse(x.Value, null));
		}
		return Null;
	}

	/// <summary>Performs a logical comparison of two <see cref="T:System.Data.SqlTypes.SqlByte" /> structures to determine whether they are equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are not equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean operator ==(SqlByte x, SqlByte y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value == y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlByte" /> to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean operator !=(SqlByte x, SqlByte y)
	{
		return !(x == y);
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlByte" /> to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean operator <(SqlByte x, SqlByte y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value < y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlByte" /> to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean operator >(SqlByte x, SqlByte y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value > y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlByte" /> to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean operator <=(SqlByte x, SqlByte y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value <= y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlByte" /> to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the SqlBoolean will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean operator >=(SqlByte x, SqlByte y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value >= y.m_value);
		}
		return SqlBoolean.Null;
	}

	/// <summary>The ones complement operator performs a bitwise one's complement operation on its <see cref="T:System.Data.SqlTypes.SqlByte" /> operand.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property contains the ones complement of the <see cref="T:System.Data.SqlTypes.SqlByte" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte OnesComplement(SqlByte x)
	{
		return ~x;
	}

	/// <summary>Computes the sum of the two specified <see cref="T:System.Data.SqlTypes.SqlByte" /> structures.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose Value property contains the results of the addition.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte Add(SqlByte x, SqlByte y)
	{
		return x + y;
	}

	/// <summary>Subtracts the second <see cref="T:System.Data.SqlTypes.SqlByte" /> operand from the first.</summary>
	/// <returns>The results of subtracting the second <see cref="T:System.Data.SqlTypes.SqlByte" /> operand from the first.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte Subtract(SqlByte x, SqlByte y)
	{
		return x - y;
	}

	/// <summary>Computes the product of the two <see cref="T:System.Data.SqlTypes.SqlByte" /> operands.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property contains the product of the multiplication.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte Multiply(SqlByte x, SqlByte y)
	{
		return x * y;
	}

	/// <summary>Divides its first <see cref="T:System.Data.SqlTypes.SqlByte" /> operand by its second.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property contains the results of the division.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte Divide(SqlByte x, SqlByte y)
	{
		return x / y;
	}

	/// <summary>Computes the remainder after dividing its first <see cref="T:System.Data.SqlTypes.SqlByte" /> operand by its second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> contains the remainder.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte Mod(SqlByte x, SqlByte y)
	{
		return x % y;
	}

	/// <summary>Divides two <see cref="T:System.Data.SqlTypes.SqlByte" /> values and returns the remainder.</summary>
	/// <returns>The remainder left after division is performed on <paramref name="x" /> and <paramref name="y" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" />.</param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" />.</param>
	public static SqlByte Modulus(SqlByte x, SqlByte y)
	{
		return x % y;
	}

	/// <summary>Computes the bitwise AND of its <see cref="T:System.Data.SqlTypes.SqlByte" /> operands.</summary>
	/// <returns>The results of the bitwise AND operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte BitwiseAnd(SqlByte x, SqlByte y)
	{
		return x & y;
	}

	/// <summary>Computes the bitwise OR of its two <see cref="T:System.Data.SqlTypes.SqlByte" /> operands.</summary>
	/// <returns>The results of the bitwise OR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte BitwiseOr(SqlByte x, SqlByte y)
	{
		return x | y;
	}

	/// <summary>Performs a bitwise exclusive-OR operation on the supplied parameters.</summary>
	/// <returns>The results of the XOR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlByte Xor(SqlByte x, SqlByte y)
	{
		return x ^ y;
	}

	/// <summary>Performs a logical comparison of two <see cref="T:System.Data.SqlTypes.SqlByte" /> structures to determine whether they are equal.</summary>
	/// <returns>true if the two values are equal. Otherwise, false. If either instance is null, then the SqlByte will be null.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean Equals(SqlByte x, SqlByte y)
	{
		return x == y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlByte" /> to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean NotEquals(SqlByte x, SqlByte y)
	{
		return x != y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlByte" /> to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean LessThan(SqlByte x, SqlByte y)
	{
		return x < y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlByte" /> to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean GreaterThan(SqlByte x, SqlByte y)
	{
		return x > y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlByte" /> to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean LessThanOrEqual(SqlByte x, SqlByte y)
	{
		return x <= y;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlByte" /> structures to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise, <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlByte" /> structure. </param>
	public static SqlBoolean GreaterThanOrEqual(SqlByte x, SqlByte y)
	{
		return x >= y;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</summary>
	/// <returns>true if the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> is non-zero; false if zero; otherwise Null.</returns>
	public SqlBoolean ToSqlBoolean()
	{
		return (SqlBoolean)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to <see cref="T:System.Data.SqlTypes.SqlDouble" />.</summary>
	/// <returns>A SqlDouble structure with the same value as this <see cref="T:System.Data.SqlTypes.SqlByte" />.</returns>
	public SqlDouble ToSqlDouble()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A SqlInt16 structure with the same value as this <see cref="T:System.Data.SqlTypes.SqlByte" />.</returns>
	public SqlInt16 ToSqlInt16()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlByte" /> to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A SqlInt32 structure with the same value as this <see cref="T:System.Data.SqlTypes.SqlByte" />.</returns>
	public SqlInt32 ToSqlInt32()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A SqlInt64 structure who <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> equals the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> of this <see cref="T:System.Data.SqlTypes.SqlByte" />.</returns>
	public SqlInt64 ToSqlInt64()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A SqlMoney structure whose <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> equals the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> of this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure.</returns>
	public SqlMoney ToSqlMoney()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to <see cref="T:System.Data.SqlTypes.SqlDecimal" />.</summary>
	/// <returns>A SqlDecimal structure whose <see cref="P:System.Data.SqlTypes.SqlDecimal.Value" /> equals the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> of this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure.</returns>
	public SqlDecimal ToSqlDecimal()
	{
		return this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure to <see cref="T:System.Data.SqlTypes.SqlSingle" />.</summary>
	/// <returns>A SqlSingle structure that has the same <see cref="P:System.Data.SqlTypes.SqlSingle.Value" /> as this <see cref="T:System.Data.SqlTypes.SqlByte" /> structure.</returns>
	public SqlSingle ToSqlSingle()
	{
		return this;
	}

	/// <summary>Converts this instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> to <see cref="T:System.Data.SqlTypes.SqlString" />.</summary>
	/// <returns>A SqlString that contains the string representation of the <see cref="T:System.Data.SqlTypes.SqlByte" /> structure's <see cref="P:System.Data.SqlTypes.SqlByte.Value" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SqlString ToSqlString()
	{
		return (SqlString)this;
	}

	/// <summary>Compares this instance to the supplied <see cref="T:System.Object" /> and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return Value Condition Less than zero This instance is less than the object. Zero This instance is the same as the object. Greater than zero This instance is greater than the object -or- The object is a null reference (Nothing in Visual Basic) </returns>
	/// <param name="value">The <see cref="T:System.Object" /> to be compared.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int CompareTo(object value)
	{
		if (value is SqlByte value2)
		{
			return CompareTo(value2);
		}
		throw ADP.WrongType(value.GetType(), typeof(SqlByte));
	}

	/// <summary>Compares this instance to the supplied <see cref="T:System.Data.SqlTypes.SqlByte" /> object and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and the object.Return Value Condition Less than zero This instance is less than the object. Zero This instance is the same as the object. Greater than zero This instance is greater than the object -or- The object is a null reference (Nothing in Visual Basic) </returns>
	/// <param name="value">The <see cref="T:System.Data.SqlTypes.SqlByte" /> object to be compared.</param>
	public int CompareTo(SqlByte value)
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

	/// <summary>Compares the supplied <see cref="T:System.Object" /> parameter to the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlByte" /> object.</summary>
	/// <returns>true if object is an instance of <see cref="T:System.Data.SqlTypes.SqlByte" /> and the two are equal; otherwise false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to be compared. </param>
	public override bool Equals(object value)
	{
		if (!(value is SqlByte sqlByte))
		{
			return false;
		}
		if (sqlByte.IsNull || IsNull)
		{
			if (sqlByte.IsNull)
			{
				return IsNull;
			}
			return false;
		}
		return (this == sqlByte).Value;
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
			m_value = XmlConvert.ToByte(reader.ReadElementString());
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
	/// <returns>A string value that indicates the XSD of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</returns>
	/// <param name="schemaSet">A <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
	{
		return new XmlQualifiedName("unsignedByte", "http://www.w3.org/2001/XMLSchema");
	}
}
