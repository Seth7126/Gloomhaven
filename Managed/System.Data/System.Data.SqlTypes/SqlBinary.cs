using System.Data.Common;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes;

/// <summary>Represents a variable-length stream of binary data to be stored in or retrieved from a database. </summary>
[Serializable]
[XmlSchemaProvider("GetXsdType")]
public struct SqlBinary : INullable, IComparable, IXmlSerializable
{
	private byte[] _value;

	/// <summary>Represents a <see cref="T:System.DBNull" /> that can be assigned to this instance of the <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure.</summary>
	public static readonly SqlBinary Null = new SqlBinary(fNull: true);

	/// <summary>Indicates whether this <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure is null. This property is read-only.</summary>
	/// <returns>true if null. Otherwise false.</returns>
	public bool IsNull => _value == null;

	/// <summary>Gets the value of the <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. This property is read-only.</summary>
	/// <returns>The value of the <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure.</returns>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The <see cref="P:System.Data.SqlTypes.SqlBinary.Value" /> property is read when the property contains <see cref="F:System.Data.SqlTypes.SqlBinary.Null" />. </exception>
	public byte[] Value
	{
		get
		{
			if (IsNull)
			{
				throw new SqlNullValueException();
			}
			byte[] array = new byte[_value.Length];
			_value.CopyTo(array, 0);
			return array;
		}
	}

	/// <summary>Gets the single byte from the <see cref="P:System.Data.SqlTypes.SqlBinary.Value" /> property located at the position indicated by the integer parameter, <paramref name="index" />. If <paramref name="index" /> indicates a position beyond the end of the byte array, a <see cref="T:System.Data.SqlTypes.SqlNullValueException" /> will be raised. This property is read-only.</summary>
	/// <returns>The byte located at the position indicated by the integer parameter.</returns>
	/// <param name="index">The position of the byte to be retrieved. </param>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The property is read when the <see cref="P:System.Data.SqlTypes.SqlBinary.Value" /> property contains <see cref="F:System.Data.SqlTypes.SqlBinary.Null" />- or - The <paramref name="index" /> parameter indicates a position byond the length of the byte array as indicated by the <see cref="P:System.Data.SqlTypes.SqlBinary.Length" /> property. </exception>
	public byte this[int index]
	{
		get
		{
			if (IsNull)
			{
				throw new SqlNullValueException();
			}
			return _value[index];
		}
	}

	/// <summary>Gets the length in bytes of the <see cref="P:System.Data.SqlTypes.SqlBinary.Value" /> property. This property is read-only.</summary>
	/// <returns>The length of the binary data in the <see cref="P:System.Data.SqlTypes.SqlBinary.Value" /> property.</returns>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The <see cref="P:System.Data.SqlTypes.SqlBinary.Length" /> property is read when the <see cref="P:System.Data.SqlTypes.SqlBinary.Value" /> property contains <see cref="F:System.Data.SqlTypes.SqlBinary.Null" />. </exception>
	public int Length
	{
		get
		{
			if (!IsNull)
			{
				return _value.Length;
			}
			throw new SqlNullValueException();
		}
	}

	private SqlBinary(bool fNull)
	{
		_value = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure, setting the <see cref="P:System.Data.SqlTypes.SqlBinary.Value" /> property to the contents of the supplied byte array.</summary>
	/// <param name="value">The byte array to be stored or retrieved. </param>
	public SqlBinary(byte[] value)
	{
		if (value == null)
		{
			_value = null;
			return;
		}
		_value = new byte[value.Length];
		value.CopyTo(_value, 0);
	}

	internal SqlBinary(byte[] value, bool ignored)
	{
		_value = value;
	}

	/// <summary>Converts an array of bytes to a <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure that represents the converted array of bytes.</returns>
	/// <param name="x">The array of bytes to be converted. </param>
	public static implicit operator SqlBinary(byte[] x)
	{
		return new SqlBinary(x);
	}

	/// <summary>Converts a <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure to a <see cref="T:System.Byte" /> array.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array.</returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure to be converted. </param>
	public static explicit operator byte[](SqlBinary x)
	{
		return x.Value;
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlBinary" /> object to a string.</summary>
	/// <returns>A string that contains the <see cref="P:System.Data.SqlTypes.SqlBinary.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBinary" />. If the <see cref="P:System.Data.SqlTypes.SqlBinary.Value" /> is null the string will contain "null".</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		if (!IsNull)
		{
			return "SqlBinary(" + _value.Length.ToString(CultureInfo.InvariantCulture) + ")";
		}
		return SQLResource.NullString;
	}

	/// <summary>Concatenates the two <see cref="T:System.Data.SqlTypes.SqlBinary" /> parameters to create a new <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure.</summary>
	/// <returns>The concatenated values of the <paramref name="x" /> and <paramref name="y" /> parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	public static SqlBinary operator +(SqlBinary x, SqlBinary y)
	{
		if (x.IsNull || y.IsNull)
		{
			return Null;
		}
		byte[] array = new byte[x.Value.Length + y.Value.Length];
		x.Value.CopyTo(array, 0);
		y.Value.CopyTo(array, x.Value.Length);
		return new SqlBinary(array);
	}

	private static EComparison PerformCompareByte(byte[] x, byte[] y)
	{
		int num = ((x.Length < y.Length) ? x.Length : y.Length);
		for (int i = 0; i < num; i++)
		{
			if (x[i] != y[i])
			{
				if (x[i] < y[i])
				{
					return EComparison.LT;
				}
				return EComparison.GT;
			}
		}
		if (x.Length == y.Length)
		{
			return EComparison.EQ;
		}
		byte b = 0;
		if (x.Length < y.Length)
		{
			for (int i = num; i < y.Length; i++)
			{
				if (y[i] != b)
				{
					return EComparison.LT;
				}
			}
		}
		else
		{
			for (int i = num; i < x.Length; i++)
			{
				if (x[i] != b)
				{
					return EComparison.GT;
				}
			}
		}
		return EComparison.EQ;
	}

	/// <summary>Converts a <see cref="T:System.Data.SqlTypes.SqlGuid" /> structure to a <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure.</summary>
	/// <returns>The <see cref="T:System.Data.SqlTypes.SqlGuid" /> structure to be converted. </returns>
	/// <param name="x">The <see cref="T:System.Data.SqlTypes.SqlGuid" /> structure to be converted. </param>
	public static explicit operator SqlBinary(SqlGuid x)
	{
		if (!x.IsNull)
		{
			return new SqlBinary(x.ToByteArray());
		}
		return Null;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to determine whether they are equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are not equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	public static SqlBoolean operator ==(SqlBinary x, SqlBinary y)
	{
		if (x.IsNull || y.IsNull)
		{
			return SqlBoolean.Null;
		}
		return new SqlBoolean(PerformCompareByte(x.Value, y.Value) == EComparison.EQ);
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	public static SqlBoolean operator !=(SqlBinary x, SqlBinary y)
	{
		return !(x == y);
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	public static SqlBoolean operator <(SqlBinary x, SqlBinary y)
	{
		if (x.IsNull || y.IsNull)
		{
			return SqlBoolean.Null;
		}
		return new SqlBoolean(PerformCompareByte(x.Value, y.Value) == EComparison.LT);
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	public static SqlBoolean operator >(SqlBinary x, SqlBinary y)
	{
		if (x.IsNull || y.IsNull)
		{
			return SqlBoolean.Null;
		}
		return new SqlBoolean(PerformCompareByte(x.Value, y.Value) == EComparison.GT);
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	public static SqlBoolean operator <=(SqlBinary x, SqlBinary y)
	{
		if (x.IsNull || y.IsNull)
		{
			return SqlBoolean.Null;
		}
		EComparison eComparison = PerformCompareByte(x.Value, y.Value);
		return new SqlBoolean(eComparison == EComparison.LT || eComparison == EComparison.EQ);
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structues to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> object. </param>
	public static SqlBoolean operator >=(SqlBinary x, SqlBinary y)
	{
		if (x.IsNull || y.IsNull)
		{
			return SqlBoolean.Null;
		}
		EComparison eComparison = PerformCompareByte(x.Value, y.Value);
		return new SqlBoolean(eComparison == EComparison.GT || eComparison == EComparison.EQ);
	}

	/// <summary>Concatenates two specified <see cref="T:System.Data.SqlTypes.SqlBinary" /> values to create a new <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBinary" /> that is the concatenated value of x and y.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" />. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" />. </param>
	public static SqlBinary Add(SqlBinary x, SqlBinary y)
	{
		return x + y;
	}

	/// <summary>Concatenates two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to create a new <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure.</summary>
	/// <returns>The concatenated values of the <paramref name="x" /> and <paramref name="y" /> parameters.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	public static SqlBinary Concat(SqlBinary x, SqlBinary y)
	{
		return x + y;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to determine whether they are equal.</summary>
	/// <returns>true if the two values are equal. Otherwise, false. If either instance is null, then the SqlBinary will be null.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	public static SqlBoolean Equals(SqlBinary x, SqlBinary y)
	{
		return x == y;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to determine whether they are not equal.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the two instances are not equal or <see cref="F:System.Data.SqlTypes.SqlBoolean.False" /> if the two instances are equal. If either instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	public static SqlBoolean NotEquals(SqlBinary x, SqlBinary y)
	{
		return x != y;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to determine whether the first is less than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than the second instance. Otherwise <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	public static SqlBoolean LessThan(SqlBinary x, SqlBinary y)
	{
		return x < y;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to determine whether the first is greater than the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than the second instance. Otherwise <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	public static SqlBoolean GreaterThan(SqlBinary x, SqlBinary y)
	{
		return x > y;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to determine whether the first is less than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is less than or equal to the second instance. Otherwise <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	public static SqlBoolean LessThanOrEqual(SqlBinary x, SqlBinary y)
	{
		return x <= y;
	}

	/// <summary>Compares two <see cref="T:System.Data.SqlTypes.SqlBinary" /> structures to determine whether the first is greater than or equal to the second.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBoolean" /> that is <see cref="F:System.Data.SqlTypes.SqlBoolean.True" /> if the first instance is greater than or equal to the second instance. Otherwise <see cref="F:System.Data.SqlTypes.SqlBoolean.False" />. If either instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> is null, the <see cref="P:System.Data.SqlTypes.SqlBoolean.Value" /> of the <see cref="T:System.Data.SqlTypes.SqlBoolean" /> will be <see cref="F:System.Data.SqlTypes.SqlBoolean.Null" />.</returns>
	/// <param name="x">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	/// <param name="y">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	public static SqlBoolean GreaterThanOrEqual(SqlBinary x, SqlBinary y)
	{
		return x >= y;
	}

	/// <summary>Converts this instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> to <see cref="T:System.Data.SqlTypes.SqlGuid" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlGuid" /> structure.</returns>
	public SqlGuid ToSqlGuid()
	{
		return (SqlGuid)this;
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlBinary" /> object to the supplied object and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of this <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure and the object.Return value Condition Less than zero The value of this <see cref="T:System.Data.SqlTypes.SqlBinary" /> object is less than the object. Zero This <see cref="T:System.Data.SqlTypes.SqlBinary" /> object is the same as object. Greater than zero This <see cref="T:System.Data.SqlTypes.SqlBinary" /> object is greater than object.-or- The object is a null reference. </returns>
	/// <param name="value">The object to be compared to this <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int CompareTo(object value)
	{
		if (value is SqlBinary value2)
		{
			return CompareTo(value2);
		}
		throw ADP.WrongType(value.GetType(), typeof(SqlBinary));
	}

	/// <summary>Compares this <see cref="T:System.Data.SqlTypes.SqlBinary" /> object to the supplied <see cref="T:System.Data.SqlTypes.SqlBinary" /> object and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of this <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure and the object.Return value Condition Less than zero The value of this <see cref="T:System.Data.SqlTypes.SqlBinary" /> object is less than the object. Zero This <see cref="T:System.Data.SqlTypes.SqlBinary" /> object is the same as object. Greater than zero This <see cref="T:System.Data.SqlTypes.SqlBinary" /> object is greater than object.-or- The object is a null reference. </returns>
	/// <param name="value">The <see cref="T:System.Data.SqlTypes.SqlBinary" /> object to be compared to this <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure. </param>
	public int CompareTo(SqlBinary value)
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

	/// <summary>Compares the supplied object parameter to the <see cref="P:System.Data.SqlTypes.SqlBinary.Value" /> property of the <see cref="T:System.Data.SqlTypes.SqlBinary" /> object.</summary>
	/// <returns>true if object is an instance of <see cref="T:System.Data.SqlTypes.SqlBinary" /> and the two are equal; otherwise false.</returns>
	/// <param name="value">The object to be compared. </param>
	public override bool Equals(object value)
	{
		if (!(value is SqlBinary sqlBinary))
		{
			return false;
		}
		if (sqlBinary.IsNull || IsNull)
		{
			if (sqlBinary.IsNull)
			{
				return IsNull;
			}
			return false;
		}
		return (this == sqlBinary).Value;
	}

	internal static int HashByteArray(byte[] rgbValue, int length)
	{
		if (length <= 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < length; i++)
		{
			int num2 = (num >> 28) & 0xFF;
			num <<= 4;
			num = num ^ rgbValue[i] ^ num2;
		}
		return num;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		if (IsNull)
		{
			return 0;
		}
		int num = _value.Length;
		while (num > 0 && _value[num - 1] == 0)
		{
			num--;
		}
		return HashByteArray(_value, num);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.Serialization.IXmlSerializable.GetSchema" />.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XMLSchema" /> instance.</returns>
	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />.</summary>
	/// <param name="reader">A <see cref="T:System.Xml.XmlReader" />.</param>
	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		string attribute = reader.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
		if (attribute != null && XmlConvert.ToBoolean(attribute))
		{
			reader.ReadElementString();
			_value = null;
			return;
		}
		string text = reader.ReadElementString();
		if (text == null)
		{
			_value = Array.Empty<byte>();
			return;
		}
		text = text.Trim();
		if (text.Length == 0)
		{
			_value = Array.Empty<byte>();
		}
		else
		{
			_value = Convert.FromBase64String(text);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" />.</summary>
	/// <param name="writer">A <see cref="T:System.Xml.XmlWriter" />.</param>
	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		if (IsNull)
		{
			writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
		}
		else
		{
			writer.WriteString(Convert.ToBase64String(_value));
		}
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />. </summary>
	/// <returns>A string that indicates the XSD of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</returns>
	/// <param name="schemaSet">An <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
	{
		return new XmlQualifiedName("base64Binary", "http://www.w3.org/2001/XMLSchema");
	}
}
