using System.Data.Common;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes;

/// <summary>Represents an integer value that is either 1 or 0 to be stored in or retrieved from a database.</summary>
[Serializable]
[XmlSchemaProvider("GetXsdType")]
public struct SqlBoolean : INullable, IComparable, IXmlSerializable
{
	private byte m_value;

	private const byte x_Null = 0;

	private const byte x_False = 1;

	private const byte x_True = 2;

	/// <summary>Represents a true value that can be assigned to the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> property of an instance of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	public static readonly SqlBoolean True = new SqlBoolean(value: true);

	/// <summary>Represents a false value that can be assigned to the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> property of an instance of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	public static readonly SqlBoolean False = new SqlBoolean(value: false);

	/// <summary>Represents <see cref="T:System.DBNull" /> that can be assigned to this instance of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	public static readonly SqlBoolean Null = new SqlBoolean(0, fNull: true);

	/// <summary>Represents a zero value that can be assigned to the <see cref="P:System.Data.SqlTypes.SqlBoolean.ByteValue" /> property of an instance of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	public static readonly SqlBoolean Zero = new SqlBoolean(0);

	/// <summary>Represents a one value that can be assigned to the <see cref="P:System.Data.SqlTypes.SqlBoolean.ByteValue" /> property of an instance of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	public static readonly SqlBoolean One = new SqlBoolean(1);

	/// <summary>Indicates whether this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure is null.</summary>
	/// <returns>true if the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure is null; otherwise false.</returns>
	public bool IsNull => m_value == 0;

	/// <summary>Gets the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value. This property is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" />; otherwise false.</returns>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The property is set to null. </exception>
	public bool Value => m_value switch
	{
		2 => true, 
		1 => false, 
		_ => throw new SqlNullValueException(), 
	};

	/// <summary>Indicates whether the current <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" />.</summary>
	/// <returns>true if Value is True; otherwise, false.</returns>
	public bool IsTrue => m_value == 2;

	/// <summary>Indicates whether the current <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> is <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />.</summary>
	/// <returns>true if Value is False; otherwise, false.</returns>
	public bool IsFalse => m_value == 1;

	/// <summary>Gets the value of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure as a byte.</summary>
	/// <returns>A byte representing the value of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</returns>
	public byte ByteValue
	{
		get
		{
			if (!IsNull)
			{
				if (m_value != 2)
				{
					return 0;
				}
				return 1;
			}
			throw new SqlNullValueException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure using the supplied Boolean value.</summary>
	/// <param name="value">The value for the new <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure; either true or false. </param>
	public SqlBoolean(bool value)
	{
		m_value = (byte)((!value) ? 1 : 2);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure using the specified integer value.</summary>
	/// <param name="value">The integer whose value is to be used for the new <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public SqlBoolean(int value)
		: this(value, fNull: false)
	{
	}

	private SqlBoolean(int value, bool fNull)
	{
		if (fNull)
		{
			m_value = 0;
		}
		else
		{
			m_value = (byte)((value == 0) ? 1 : 2);
		}
	}

	/// <summary>Converts the supplied byte value to a <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> value that contains 0 or 1.</returns>
	/// <param name="x">A byte value to be converted to <see cref="T:System.Data.SqlTypes.SqlBoolean" />. </param>
	public static implicit operator SqlBoolean(bool x)
	{
		return new SqlBoolean(x);
	}

	/// <summary>Converts a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to a Boolean.</summary>
	/// <returns>A Boolean set to the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to convert. </param>
	public static explicit operator bool(SqlBoolean x)
	{
		return x.Value;
	}

	/// <summary>Performs a NOT operation on a <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> with the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /><see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if argument was true, <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" /> if argument was null, and <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> otherwise.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlBoolean" /> on which the NOT operation will be performed. </param>
	public static SqlBoolean operator !(SqlBoolean x)
	{
		return x.m_value switch
		{
			2 => False, 
			1 => True, 
			_ => Null, 
		};
	}

	/// <summary>The true operator can be used to test the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to determine whether it is true.</summary>
	/// <returns>Returns true if the supplied parameter is <see cref="T:System.Data.SqlTypes.SqlBoolean" /> is true, false otherwise.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to be tested. </param>
	public static bool operator true(SqlBoolean x)
	{
		return x.IsTrue;
	}

	/// <summary>The false operator can be used to test the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to determine whether it is false.</summary>
	/// <returns>Returns true if the supplied parameter is <see cref="T:System.Data.SqlTypes.SqlBoolean" /> is false, false otherwise.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to be tested. </param>
	public static bool operator false(SqlBoolean x)
	{
		return x.IsFalse;
	}

	/// <summary>Computes the bitwise AND operation of two specified <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structures.</summary>
	/// <returns>The result of the logical AND operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static SqlBoolean operator &(SqlBoolean x, SqlBoolean y)
	{
		if (x.m_value == 1 || y.m_value == 1)
		{
			return False;
		}
		if (x.m_value == 2 && y.m_value == 2)
		{
			return True;
		}
		return Null;
	}

	/// <summary>Computes the bitwise OR of its operands.</summary>
	/// <returns>The results of the logical OR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static SqlBoolean operator |(SqlBoolean x, SqlBoolean y)
	{
		if (x.m_value == 2 || y.m_value == 2)
		{
			return True;
		}
		if (x.m_value == 1 && y.m_value == 1)
		{
			return False;
		}
		return Null;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to a string.</summary>
	/// <returns>A string that contains the value of the <see cref="T:System.Data.SqlTypes.SqlBoolean" />. If the value is null, the string will contain "null".</returns>
	public override string ToString()
	{
		if (!IsNull)
		{
			return Value.ToString();
		}
		return SQLResource.NullString;
	}

	/// <summary>Converts the specified <see cref="T:System.String" /> representation of a logical value to its <see cref="T:System.Data.SqlTypes.SqlBoolean" /> equivalent.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure that contains the parsed value.</returns>
	/// <param name="s">The <see cref="T:System.String" /> to be converted. </param>
	public static SqlBoolean Parse(string s)
	{
		if (s == null)
		{
			return new SqlBoolean(bool.Parse(s));
		}
		if (s == SQLResource.NullString)
		{
			return Null;
		}
		s = s.TrimStart();
		char c = s[0];
		if (char.IsNumber(c) || '-' == c || '+' == c)
		{
			return new SqlBoolean(int.Parse(s, null));
		}
		return new SqlBoolean(bool.Parse(s));
	}

	/// <summary>Performs a one's complement operation on the supplied <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structures.</summary>
	/// <returns>The one's complement of the supplied <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static SqlBoolean operator ~(SqlBoolean x)
	{
		return !x;
	}

	/// <summary>Performs a bitwise exclusive-OR (XOR) operation on the supplied parameters.</summary>
	/// <returns>The result of the logical XOR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static SqlBoolean operator ^(SqlBoolean x, SqlBoolean y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value != y.m_value);
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlByte" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure whose value equals the <see cref="P:System.Data.SqlTypes.SqlByte.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlByte" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlByte" /> to be converted to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static explicit operator SqlBoolean(SqlByte x)
	{
		if (!x.IsNull)
		{
			return new SqlBoolean(x.Value != 0);
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure whose value equals the <see cref="P:System.Data.SqlTypes.SqlInt16.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlInt16" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt16" /> to be converted to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static explicit operator SqlBoolean(SqlInt16 x)
	{
		if (!x.IsNull)
		{
			return new SqlBoolean(x.Value != 0);
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure whose value equals the <see cref="P:System.Data.SqlTypes.SqlInt32.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlInt32" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt32" /> to be converted to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static explicit operator SqlBoolean(SqlInt32 x)
	{
		if (!x.IsNull)
		{
			return new SqlBoolean(x.Value != 0);
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure whose value equals the <see cref="P:System.Data.SqlTypes.SqlInt64.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlInt64" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlInt64" /> to be converted to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static explicit operator SqlBoolean(SqlInt64 x)
	{
		if (!x.IsNull)
		{
			return new SqlBoolean(x.Value != 0);
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlDouble" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure whose value equals the <see cref="P:System.Data.SqlTypes.SqlDouble.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlDouble" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDouble" /> to be converted to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static explicit operator SqlBoolean(SqlDouble x)
	{
		if (!x.IsNull)
		{
			return new SqlBoolean(x.Value != 0.0);
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlSingle" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure whose value equals the <see cref="P:System.Data.SqlTypes.SqlSingle.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlSingle" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlSingle" /> to be converted to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static explicit operator SqlBoolean(SqlSingle x)
	{
		if (!x.IsNull)
		{
			return new SqlBoolean((double)x.Value != 0.0);
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose value equals the <see cref="P:System.Data.SqlTypes.SqlMoney.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlMoney" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlMoney" /> to be converted to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static explicit operator SqlBoolean(SqlMoney x)
	{
		if (!x.IsNull)
		{
			return x != SqlMoney.Zero;
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlDecimal" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose value equals the <see cref="P:System.Data.SqlTypes.SqlDecimal.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlDecimal" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlDecimal" /> to be converted to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static explicit operator SqlBoolean(SqlDecimal x)
	{
		if (!x.IsNull)
		{
			return new SqlBoolean(x._data1 != 0 || x._data2 != 0 || x._data3 != 0 || x._data4 != 0);
		}
		return Null;
	}

	/// <summary>Converts the <see cref="T:System.Data.SqlTypes.SqlString" /> parameter to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose value equals the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> parameter.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlString" /> to be converted to a <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static explicit operator SqlBoolean(SqlString x)
	{
		if (!x.IsNull)
		{
			return Parse(x.Value);
		}
		return Null;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> for equality.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are not equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" />. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" />. </param>
	public static SqlBoolean operator ==(SqlBoolean x, SqlBoolean y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value == y.m_value);
		}
		return Null;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" />. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" />. </param>
	public static SqlBoolean operator !=(SqlBoolean x, SqlBoolean y)
	{
		return !(x == y);
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is true if the first instance is less than the second instance; otherwise, false.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	public static SqlBoolean operator <(SqlBoolean x, SqlBoolean y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value < y.m_value);
		}
		return Null;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structures to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is true if the first instance is greater than the second instance; otherwise, false. </returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> object. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> object. </param>
	public static SqlBoolean operator >(SqlBoolean x, SqlBoolean y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value > y.m_value);
		}
		return Null;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is true if the first instance is less than or equal to the second instance; otherwise, false.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	public static SqlBoolean operator <=(SqlBoolean x, SqlBoolean y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value <= y.m_value);
		}
		return Null;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structures to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is true if the first instance is greater than or equal to the second instance; otherwise, false. </returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	public static SqlBoolean operator >=(SqlBoolean x, SqlBoolean y)
	{
		if (!x.IsNull && !y.IsNull)
		{
			return new SqlBoolean(x.m_value >= y.m_value);
		}
		return Null;
	}

	/// <summary>Performs a one's complement operation on the supplied <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structures.</summary>
	/// <returns>The one's complement of the supplied <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static SqlBoolean OnesComplement(SqlBoolean x)
	{
		return ~x;
	}

	/// <summary>Computes the bitwise AND operation of two specified <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structures.</summary>
	/// <returns>The result of the logical AND operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static SqlBoolean And(SqlBoolean x, SqlBoolean y)
	{
		return x & y;
	}

	/// <summary>Performs a bitwise OR operation on the two specified <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structures.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure whose Value is the result of the bitwise OR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static SqlBoolean Or(SqlBoolean x, SqlBoolean y)
	{
		return x | y;
	}

	/// <summary>Performs a bitwise exclusive-OR operation on the supplied parameters.</summary>
	/// <returns>The result of the logical XOR operation.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static SqlBoolean Xor(SqlBoolean x, SqlBoolean y)
	{
		return x ^ y;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structures to determine whether they are equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are not equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static SqlBoolean Equals(SqlBoolean x, SqlBoolean y)
	{
		return x == y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> for equality.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure. </param>
	public static SqlBoolean NotEquals(SqlBoolean x, SqlBoolean y)
	{
		return x != y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is true if the first instance is greater than the second instance; otherwise false. </returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	public static SqlBoolean GreaterThan(SqlBoolean x, SqlBoolean y)
	{
		return x > y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is true if the first instance is less than the second instance; otherwise, false.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	public static SqlBoolean LessThan(SqlBoolean x, SqlBoolean y)
	{
		return x < y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is true if the first instance is greater than or equal to the second instance; otherwise false.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	public static SqlBoolean GreaterThanOrEquals(SqlBoolean x, SqlBoolean y)
	{
		return x >= y;
	}

	/// <summary>Compares two instances of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is true if the first instance is less than or equal to the second instance; otherwise, false.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure.</param>
	public static SqlBoolean LessThanOrEquals(SqlBoolean x, SqlBoolean y)
	{
		return x <= y;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlByte" /> structure whose value is 1 or 0. If the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value equals true, the new <see cref="T:System.Data.SqlTypes.SqlByte" /> structure's value is 1. Otherwise, the new <see cref="T:System.Data.SqlTypes.SqlByte" /> structure's value is 0.</returns>
	public SqlByte ToSqlByte()
	{
		return (SqlByte)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to <see cref="T:System.Data.SqlTypes.SqlDouble" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure whose value is 1 or 0. If the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value equals true then the new <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure's value is 1. Otherwise, the new <see cref="T:System.Data.SqlTypes.SqlDouble" /> structure's value is 0.</returns>
	public SqlDouble ToSqlDouble()
	{
		return (SqlDouble)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>A new SqlInt16 structure whose value is 1 or 0. If the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value equals true then the new <see cref="T:System.Data.SqlTypes.SqlInt16" /> structure's value is 1. Otherwise, the new SqlInt16 structure's value is 0.</returns>
	public SqlInt16 ToSqlInt16()
	{
		return (SqlInt16)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>A new SqlInt32 structure whose value is 1 or 0. If the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value equals true, the new <see cref="T:System.Data.SqlTypes.SqlInt32" /> structure's value is 1. Otherwise, the new SqlInt32 structure's value is 0.</returns>
	public SqlInt32 ToSqlInt32()
	{
		return (SqlInt32)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>A new SqlInt64 structure whose value is 1 or 0. If the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value equals true, the new <see cref="T:System.Data.SqlTypes.SqlInt64" />  structure's value is 1. Otherwise, the new SqlInt64 structure's value is 0.</returns>
	public SqlInt64 ToSqlInt64()
	{
		return (SqlInt64)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlMoney" /> structure whose value is 1 or 0. If the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value equals true, the new <see cref="T:System.Data.SqlTypes.SqlMoney" /> value is 1. If the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value equals false, the new <see cref="T:System.Data.SqlTypes.SqlMoney" /> value is 0. If <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value is neither 1 nor 0, the new <see cref="T:System.Data.SqlTypes.SqlMoney" /> value is <see cref="F:System.Data.SqlTypes.SqlMoney.Null" />.</returns>
	public SqlMoney ToSqlMoney()
	{
		return (SqlMoney)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to <see cref="T:System.Data.SqlTypes.SqlDecimal" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure whose value is 1 or 0. If the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value equals true then the new <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure's value is 1. Otherwise, the new <see cref="T:System.Data.SqlTypes.SqlDecimal" /> structure's value is 0.</returns>
	public SqlDecimal ToSqlDecimal()
	{
		return (SqlDecimal)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to <see cref="T:System.Data.SqlTypes.SqlSingle" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure whose value is 1 or 0.If the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value equals true, the new <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure's value is 1; otherwise the new <see cref="T:System.Data.SqlTypes.SqlSingle" /> structure's value is 0.</returns>
	public SqlSingle ToSqlSingle()
	{
		return (SqlSingle)this;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to <see cref="T:System.Data.SqlTypes.SqlString" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlTypes.SqlString" /> structure whose value is 1 or 0. If the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure's value equals true then <see cref="T:System.Data.SqlTypes.SqlString" /> structure's value is 1. Otherwise the new <see cref="T:System.Data.SqlTypes.SqlString" /> structure's value is 0.</returns>
	public SqlString ToSqlString()
	{
		return (SqlString)this;
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> structure to a specified object and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and value.Value Description A negative integer This instance is less than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. A positive integer This instance is greater than <paramref name="value" />.-or- <paramref name="value" /> is a null reference (Nothing in Visual Basic). </returns>
	/// <param name="value">An object to compare, or a null reference (Nothing in Visual Basic). </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int CompareTo(object value)
	{
		if (value is SqlBoolean value2)
		{
			return CompareTo(value2);
		}
		throw ADP.WrongType(value.GetType(), typeof(SqlBoolean));
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlBoolean" /> object to the supplied <see cref="T:System.Data.SqlTypes.SqlBoolean" /> object and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of the instance and value.Value Description A negative integer This instance is less than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. A positive integer This instance is greater than <paramref name="value" />.-or- <paramref name="value" /> is a null reference (Nothing in Visual Basic). </returns>
	/// <param name="value">A <see cref="T:System.Data.SqlTypes.SqlBoolean" /><see cref="T:System.Data.SqlTypes.SqlBoolean" /> object to compare, or a null reference (Nothing in Visual Basic).  </param>
	public int CompareTo(SqlBoolean value)
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
		if (ByteValue < value.ByteValue)
		{
			return -1;
		}
		if (ByteValue > value.ByteValue)
		{
			return 1;
		}
		return 0;
	}

	/// <summary>Compares the supplied object parameter to the <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</summary>
	/// <returns>true if object is an instance of <see cref="T:System.Data.SqlTypes.SqlBoolean" /> and the two are equal; otherwise, false.</returns>
	/// <param name="value">The object to be compared. </param>
	public override bool Equals(object value)
	{
		if (!(value is SqlBoolean sqlBoolean))
		{
			return false;
		}
		if (sqlBoolean.IsNull || IsNull)
		{
			if (sqlBoolean.IsNull)
			{
				return IsNull;
			}
			return false;
		}
		return (this == sqlBoolean).Value;
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
			m_value = 0;
		}
		else
		{
			m_value = (byte)((!XmlConvert.ToBoolean(reader.ReadElementString())) ? 1 : 2);
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
			writer.WriteString((m_value == 2) ? "true" : "false");
		}
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>A string value that indicates the XSD of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</returns>
	/// <param name="schemaSet">A <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
	{
		return new XmlQualifiedName("boolean", "http://www.w3.org/2001/XMLSchema");
	}
}
