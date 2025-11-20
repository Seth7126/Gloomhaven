using System.Diagnostics;
using System.Globalization;

namespace System.Numerics;

/// <summary>Represents an arbitrarily large signed integer.</summary>
[Serializable]
public readonly struct BigInteger : IFormattable, IComparable, IComparable<BigInteger>, IEquatable<BigInteger>
{
	private enum GetBytesMode
	{
		AllocateArray,
		Count,
		Span
	}

	private const int knMaskHighBit = int.MinValue;

	private const uint kuMaskHighBit = 2147483648u;

	private const int kcbitUint = 32;

	private const int kcbitUlong = 64;

	private const int DecimalScaleFactorMask = 16711680;

	private const int DecimalSignMask = int.MinValue;

	internal readonly int _sign;

	internal readonly uint[] _bits;

	private static readonly BigInteger s_bnMinInt = new BigInteger(-1, new uint[1] { 2147483648u });

	private static readonly BigInteger s_bnOneInt = new BigInteger(1);

	private static readonly BigInteger s_bnZeroInt = new BigInteger(0);

	private static readonly BigInteger s_bnMinusOneInt = new BigInteger(-1);

	private static readonly byte[] s_success = Array.Empty<byte>();

	/// <summary>Gets a value that represents the number 0 (zero).</summary>
	/// <returns>An integer whose value is 0 (zero).</returns>
	public static BigInteger Zero => s_bnZeroInt;

	/// <summary>Gets a value that represents the number one (1).</summary>
	/// <returns>An object whose value is one (1).</returns>
	public static BigInteger One => s_bnOneInt;

	/// <summary>Gets a value that represents the number negative one (-1).</summary>
	/// <returns>An integer whose value is negative one (-1).</returns>
	public static BigInteger MinusOne => s_bnMinusOneInt;

	/// <summary>Indicates whether the value of the current <see cref="T:System.Numerics.BigInteger" /> object is a power of two.</summary>
	/// <returns>true if the value of the <see cref="T:System.Numerics.BigInteger" /> object is a power of two; otherwise, false.</returns>
	public bool IsPowerOfTwo
	{
		get
		{
			if (_bits == null)
			{
				if ((_sign & (_sign - 1)) == 0)
				{
					return _sign != 0;
				}
				return false;
			}
			if (_sign != 1)
			{
				return false;
			}
			int num = _bits.Length - 1;
			if ((_bits[num] & (_bits[num] - 1)) != 0)
			{
				return false;
			}
			while (--num >= 0)
			{
				if (_bits[num] != 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	/// <summary>Indicates whether the value of the current <see cref="T:System.Numerics.BigInteger" /> object is <see cref="P:System.Numerics.BigInteger.Zero" />.</summary>
	/// <returns>true if the value of the <see cref="T:System.Numerics.BigInteger" /> object is <see cref="P:System.Numerics.BigInteger.Zero" />; otherwise, false.</returns>
	public bool IsZero => _sign == 0;

	/// <summary>Indicates whether the value of the current <see cref="T:System.Numerics.BigInteger" /> object is <see cref="P:System.Numerics.BigInteger.One" />.</summary>
	/// <returns>true if the value of the <see cref="T:System.Numerics.BigInteger" /> object is <see cref="P:System.Numerics.BigInteger.One" />; otherwise, false.</returns>
	public bool IsOne
	{
		get
		{
			if (_sign == 1)
			{
				return _bits == null;
			}
			return false;
		}
	}

	/// <summary>Indicates whether the value of the current <see cref="T:System.Numerics.BigInteger" /> object is an even number.</summary>
	/// <returns>true if the value of the <see cref="T:System.Numerics.BigInteger" /> object is an even number; otherwise, false.</returns>
	public bool IsEven
	{
		get
		{
			if (_bits != null)
			{
				return (_bits[0] & 1) == 0;
			}
			return (_sign & 1) == 0;
		}
	}

	/// <summary>Gets a number that indicates the sign (negative, positive, or zero) of the current <see cref="T:System.Numerics.BigInteger" /> object.</summary>
	/// <returns>A number that indicates the sign of the <see cref="T:System.Numerics.BigInteger" /> object, as shown in the following table.NumberDescription-1The value of this object is negative.0The value of this object is 0 (zero).1The value of this object is positive.</returns>
	public int Sign => (_sign >> 31) - (-_sign >> 31);

	/// <summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a 32-bit signed integer value.</summary>
	/// <param name="value">A 32-bit signed integer.</param>
	public BigInteger(int value)
	{
		if (value == int.MinValue)
		{
			this = s_bnMinInt;
			return;
		}
		_sign = value;
		_bits = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using an unsigned 32-bit integer value.</summary>
	/// <param name="value">An unsigned 32-bit integer value.</param>
	[CLSCompliant(false)]
	public BigInteger(uint value)
	{
		if (value <= int.MaxValue)
		{
			_sign = (int)value;
			_bits = null;
		}
		else
		{
			_sign = 1;
			_bits = new uint[1];
			_bits[0] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a 64-bit signed integer value.</summary>
	/// <param name="value">A 64-bit signed integer.</param>
	public BigInteger(long value)
	{
		if (int.MinValue < value && value <= int.MaxValue)
		{
			_sign = (int)value;
			_bits = null;
			return;
		}
		if (value == int.MinValue)
		{
			this = s_bnMinInt;
			return;
		}
		ulong num = 0uL;
		if (value < 0)
		{
			num = (ulong)(-value);
			_sign = -1;
		}
		else
		{
			num = (ulong)value;
			_sign = 1;
		}
		if (num <= uint.MaxValue)
		{
			_bits = new uint[1];
			_bits[0] = (uint)num;
		}
		else
		{
			_bits = new uint[2];
			_bits[0] = (uint)num;
			_bits[1] = (uint)(num >> 32);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure with an unsigned 64-bit integer value.</summary>
	/// <param name="value">An unsigned 64-bit integer.</param>
	[CLSCompliant(false)]
	public BigInteger(ulong value)
	{
		if (value <= int.MaxValue)
		{
			_sign = (int)value;
			_bits = null;
		}
		else if (value <= uint.MaxValue)
		{
			_sign = 1;
			_bits = new uint[1];
			_bits[0] = (uint)value;
		}
		else
		{
			_sign = 1;
			_bits = new uint[2];
			_bits[0] = (uint)value;
			_bits[1] = (uint)(value >> 32);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a single-precision floating-point value.</summary>
	/// <param name="value">A single-precision floating-point value.</param>
	/// <exception cref="T:System.OverflowException">The value of <paramref name="value" /> is <see cref="F:System.Single.NaN" />.-or-The value of <paramref name="value" /> is <see cref="F:System.Single.NegativeInfinity" />.-or-The value of <paramref name="value" /> is <see cref="F:System.Single.PositiveInfinity" />.</exception>
	public BigInteger(float value)
		: this((double)value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a double-precision floating-point value.</summary>
	/// <param name="value">A double-precision floating-point value.</param>
	/// <exception cref="T:System.OverflowException">The value of <paramref name="value" /> is <see cref="F:System.Double.NaN" />.-or-The value of <paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />.-or-The value of <paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />.</exception>
	public BigInteger(double value)
	{
		if (!double.IsFinite(value))
		{
			if (double.IsInfinity(value))
			{
				throw new OverflowException("BigInteger cannot represent infinity.");
			}
			throw new OverflowException("The value is not a number.");
		}
		_sign = 0;
		_bits = null;
		NumericsHelpers.GetDoubleParts(value, out var sign, out var exp, out var man, out var _);
		if (man == 0L)
		{
			this = Zero;
			return;
		}
		if (exp <= 0)
		{
			if (exp <= -64)
			{
				this = Zero;
				return;
			}
			this = man >> -exp;
			if (sign < 0)
			{
				_sign = -_sign;
			}
			return;
		}
		if (exp <= 11)
		{
			this = man << exp;
			if (sign < 0)
			{
				_sign = -_sign;
			}
			return;
		}
		man <<= 11;
		exp -= 11;
		int num = (exp - 1) / 32 + 1;
		int num2 = num * 32 - exp;
		_bits = new uint[num + 2];
		_bits[num + 1] = (uint)(man >> num2 + 32);
		_bits[num] = (uint)(man >> num2);
		if (num2 > 0)
		{
			_bits[num - 1] = (uint)((int)man << 32 - num2);
		}
		_sign = sign;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a <see cref="T:System.Decimal" /> value.</summary>
	/// <param name="value">A decimal number.</param>
	public BigInteger(decimal value)
	{
		int[] bits = decimal.GetBits(decimal.Truncate(value));
		int num = 3;
		while (num > 0 && bits[num - 1] == 0)
		{
			num--;
		}
		switch (num)
		{
		case 0:
			this = s_bnZeroInt;
			return;
		case 1:
			if (bits[0] > 0)
			{
				_sign = bits[0];
				_sign *= (((bits[3] & int.MinValue) == 0) ? 1 : (-1));
				_bits = null;
				return;
			}
			break;
		}
		_bits = new uint[num];
		_bits[0] = (uint)bits[0];
		if (num > 1)
		{
			_bits[1] = (uint)bits[1];
		}
		if (num > 2)
		{
			_bits[2] = (uint)bits[2];
		}
		_sign = (((bits[3] & int.MinValue) == 0) ? 1 : (-1));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using the values in a byte array.</summary>
	/// <param name="value">An array of byte values in little-endian order.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	[CLSCompliant(false)]
	public BigInteger(byte[] value)
		: this(new ReadOnlySpan<byte>(value ?? throw new ArgumentNullException("value")))
	{
	}

	public BigInteger(ReadOnlySpan<byte> value, bool isUnsigned = false, bool isBigEndian = false)
	{
		int num = value.Length;
		bool flag;
		if (num > 0)
		{
			byte num2 = (isBigEndian ? value[0] : value[num - 1]);
			flag = (num2 & 0x80) != 0 && !isUnsigned;
			if (num2 == 0)
			{
				if (isBigEndian)
				{
					int i;
					for (i = 1; i < num && value[i] == 0; i++)
					{
					}
					value = value.Slice(i);
					num = value.Length;
				}
				else
				{
					num -= 2;
					while (num >= 0 && value[num] == 0)
					{
						num--;
					}
					num++;
				}
			}
		}
		else
		{
			flag = false;
		}
		if (num == 0)
		{
			_sign = 0;
			_bits = null;
			return;
		}
		if (num <= 4)
		{
			_sign = (flag ? (-1) : 0);
			if (isBigEndian)
			{
				for (int j = 0; j < num; j++)
				{
					_sign = (_sign << 8) | value[j];
				}
			}
			else
			{
				for (int num3 = num - 1; num3 >= 0; num3--)
				{
					_sign = (_sign << 8) | value[num3];
				}
			}
			_bits = null;
			if (_sign < 0 && !flag)
			{
				_bits = new uint[1] { (uint)_sign };
				_sign = 1;
			}
			if (_sign == int.MinValue)
			{
				this = s_bnMinInt;
			}
			return;
		}
		int num4 = num % 4;
		int num5 = num / 4 + ((num4 != 0) ? 1 : 0);
		uint[] array = new uint[num5];
		int num6 = num - 1;
		int k;
		if (isBigEndian)
		{
			int num7 = num - 4;
			for (k = 0; k < num5 - ((num4 != 0) ? 1 : 0); k++)
			{
				for (int l = 0; l < 4; l++)
				{
					byte b = value[num7];
					array[k] = (array[k] << 8) | b;
					num7++;
				}
				num7 -= 8;
			}
		}
		else
		{
			int num7 = 3;
			for (k = 0; k < num5 - ((num4 != 0) ? 1 : 0); k++)
			{
				for (int m = 0; m < 4; m++)
				{
					byte b2 = value[num7];
					array[k] = (array[k] << 8) | b2;
					num7--;
				}
				num7 += 8;
			}
		}
		if (num4 != 0)
		{
			if (flag)
			{
				array[num5 - 1] = uint.MaxValue;
			}
			if (isBigEndian)
			{
				for (int num7 = 0; num7 < num4; num7++)
				{
					byte b3 = value[num7];
					array[k] = (array[k] << 8) | b3;
				}
			}
			else
			{
				for (int num7 = num6; num7 >= num - num4; num7--)
				{
					byte b4 = value[num7];
					array[k] = (array[k] << 8) | b4;
				}
			}
		}
		if (flag)
		{
			NumericsHelpers.DangerousMakeTwosComplement(array);
			int num8 = array.Length - 1;
			while (num8 >= 0 && array[num8] == 0)
			{
				num8--;
			}
			num8++;
			if (num8 == 1)
			{
				switch (array[0])
				{
				case 1u:
					this = s_bnMinusOneInt;
					return;
				case 2147483648u:
					this = s_bnMinInt;
					return;
				}
				if ((int)array[0] > 0)
				{
					_sign = -1 * (int)array[0];
					_bits = null;
					return;
				}
			}
			if (num8 != array.Length)
			{
				_sign = -1;
				_bits = new uint[num8];
				Array.Copy(array, 0, _bits, 0, num8);
			}
			else
			{
				_sign = -1;
				_bits = array;
			}
		}
		else
		{
			_sign = 1;
			_bits = array;
		}
	}

	internal BigInteger(int n, uint[] rgu)
	{
		_sign = n;
		_bits = rgu;
	}

	internal BigInteger(uint[] value, bool negative)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int num = value.Length;
		while (num > 0 && value[num - 1] == 0)
		{
			num--;
		}
		switch (num)
		{
		case 0:
			this = s_bnZeroInt;
			break;
		case 1:
			if (value[0] < 2147483648u)
			{
				_sign = (int)(negative ? (0 - value[0]) : value[0]);
				_bits = null;
				if (_sign == int.MinValue)
				{
					this = s_bnMinInt;
				}
				break;
			}
			goto default;
		default:
			_sign = ((!negative) ? 1 : (-1));
			_bits = new uint[num];
			Array.Copy(value, 0, _bits, 0, num);
			break;
		}
	}

	private BigInteger(uint[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int num = value.Length;
		bool flag = num > 0 && (value[num - 1] & 0x80000000u) == 2147483648u;
		while (num > 0 && value[num - 1] == 0)
		{
			num--;
		}
		switch (num)
		{
		case 0:
			this = s_bnZeroInt;
			return;
		case 1:
			if ((int)value[0] < 0 && !flag)
			{
				_bits = new uint[1];
				_bits[0] = value[0];
				_sign = 1;
			}
			else if (int.MinValue == (int)value[0])
			{
				this = s_bnMinInt;
			}
			else
			{
				_sign = (int)value[0];
				_bits = null;
			}
			return;
		}
		if (!flag)
		{
			if (num != value.Length)
			{
				_sign = 1;
				_bits = new uint[num];
				Array.Copy(value, 0, _bits, 0, num);
			}
			else
			{
				_sign = 1;
				_bits = value;
			}
			return;
		}
		NumericsHelpers.DangerousMakeTwosComplement(value);
		int num2 = value.Length;
		while (num2 > 0 && value[num2 - 1] == 0)
		{
			num2--;
		}
		if (num2 == 1 && (int)value[0] > 0)
		{
			if (value[0] == 1)
			{
				this = s_bnMinusOneInt;
				return;
			}
			if (value[0] == 2147483648u)
			{
				this = s_bnMinInt;
				return;
			}
			_sign = -1 * (int)value[0];
			_bits = null;
		}
		else if (num2 != value.Length)
		{
			_sign = -1;
			_bits = new uint[num2];
			Array.Copy(value, 0, _bits, 0, num2);
		}
		else
		{
			_sign = -1;
			_bits = value;
		}
	}

	/// <summary>Converts the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
	/// <returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
	/// <param name="value">A string that contains the number to convert.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in the correct format.</exception>
	public static BigInteger Parse(string value)
	{
		return Parse(value, NumberStyles.Integer);
	}

	/// <summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
	/// <returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
	/// <param name="value">A string that contains a number to convert. </param>
	/// <param name="style">A bitwise combination of the enumeration values that specify the permitted format of <paramref name="value" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value.-or-<paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not comply with the input pattern specified by <see cref="T:System.Globalization.NumberStyles" />.</exception>
	public static BigInteger Parse(string value, NumberStyles style)
	{
		return Parse(value, style, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the string representation of a number in a specified culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
	/// <returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
	/// <param name="value">A string that contains a number to convert.</param>
	/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="value" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in the correct format.</exception>
	public static BigInteger Parse(string value, IFormatProvider provider)
	{
		return Parse(value, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
	/// <returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
	/// <param name="value">A string that contains a number to convert.</param>
	/// <param name="style">A bitwise combination of the enumeration values that specify the permitted format of <paramref name="value" />.</param>
	/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="value" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value.-or-<paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not comply with the input pattern specified by <paramref name="style" />.</exception>
	public static BigInteger Parse(string value, NumberStyles style, IFormatProvider provider)
	{
		return BigNumber.ParseBigInteger(value, style, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Tries to convert the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	/// <returns>true if <paramref name="value" /> was converted successfully; otherwise, false.</returns>
	/// <param name="value">The string representation of a number.</param>
	/// <param name="result">When this method returns, contains the <see cref="T:System.Numerics.BigInteger" /> equivalent to the number that is contained in <paramref name="value" />, or zero (0) if the conversion fails. The conversion fails if the <paramref name="value" /> parameter is null or is not of the correct format. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public static bool TryParse(string value, out BigInteger result)
	{
		return TryParse(value, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
	}

	/// <summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	/// <returns>true if the <paramref name="value" /> parameter was converted successfully; otherwise, false.</returns>
	/// <param name="value">The string representation of a number. The string is interpreted using the style specified by <paramref name="style" />.</param>
	/// <param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="value" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="value" />.</param>
	/// <param name="result">When this method returns, contains the <see cref="T:System.Numerics.BigInteger" /> equivalent to the number that is contained in <paramref name="value" />, or <see cref="P:System.Numerics.BigInteger.Zero" /> if the conversion failed. The conversion fails if the <paramref name="value" /> parameter is null or is not in a format that is compliant with <paramref name="style" />. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value.-or-<paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value. </exception>
	public static bool TryParse(string value, NumberStyles style, IFormatProvider provider, out BigInteger result)
	{
		return BigNumber.TryParseBigInteger(value, style, NumberFormatInfo.GetInstance(provider), out result);
	}

	public static BigInteger Parse(ReadOnlySpan<char> value, NumberStyles style = NumberStyles.Integer, IFormatProvider provider = null)
	{
		return BigNumber.ParseBigInteger(value, style, NumberFormatInfo.GetInstance(provider));
	}

	public static bool TryParse(ReadOnlySpan<char> value, out BigInteger result)
	{
		return BigNumber.TryParseBigInteger(value, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
	}

	public static bool TryParse(ReadOnlySpan<char> value, NumberStyles style, IFormatProvider provider, out BigInteger result)
	{
		return BigNumber.TryParseBigInteger(value, style, NumberFormatInfo.GetInstance(provider), out result);
	}

	/// <summary>Compares two <see cref="T:System.Numerics.BigInteger" /> values and returns an integer that indicates whether the first value is less than, equal to, or greater than the second value.</summary>
	/// <returns>A signed integer that indicates the relative values of <paramref name="left" /> and <paramref name="right" />, as shown in the following table.ValueConditionLess than zero<paramref name="left" /> is less than <paramref name="right" />.Zero<paramref name="left" /> equals <paramref name="right" />.Greater than zero<paramref name="left" /> is greater than <paramref name="right" />.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static int Compare(BigInteger left, BigInteger right)
	{
		return left.CompareTo(right);
	}

	/// <summary>Gets the absolute value of a <see cref="T:System.Numerics.BigInteger" /> object.</summary>
	/// <returns>The absolute value of <paramref name="value" />.</returns>
	/// <param name="value">A number.</param>
	public static BigInteger Abs(BigInteger value)
	{
		if (!(value >= Zero))
		{
			return -value;
		}
		return value;
	}

	/// <summary>Adds two <see cref="T:System.Numerics.BigInteger" /> values and returns the result.</summary>
	/// <returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
	/// <param name="left">The first value to add.</param>
	/// <param name="right">The second value to add.</param>
	public static BigInteger Add(BigInteger left, BigInteger right)
	{
		return left + right;
	}

	/// <summary>Subtracts one <see cref="T:System.Numerics.BigInteger" /> value from another and returns the result.</summary>
	/// <returns>The result of subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
	/// <param name="left">The value to subtract from (the minuend).</param>
	/// <param name="right">The value to subtract (the subtrahend).</param>
	public static BigInteger Subtract(BigInteger left, BigInteger right)
	{
		return left - right;
	}

	/// <summary>Returns the product of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	/// <returns>The product of the <paramref name="left" /> and <paramref name="right" /> parameters.</returns>
	/// <param name="left">The first number to multiply.</param>
	/// <param name="right">The second number to multiply.</param>
	public static BigInteger Multiply(BigInteger left, BigInteger right)
	{
		return left * right;
	}

	/// <summary>Divides one <see cref="T:System.Numerics.BigInteger" /> value by another and returns the result.</summary>
	/// <returns>The quotient of the division.</returns>
	/// <param name="dividend">The value to be divided.</param>
	/// <param name="divisor">The value to divide by.</param>
	/// <exception cref="T:System.DivideByZeroException">
	///   <paramref name="divisor" /> is 0 (zero).</exception>
	public static BigInteger Divide(BigInteger dividend, BigInteger divisor)
	{
		return dividend / divisor;
	}

	/// <summary>Performs integer division on two <see cref="T:System.Numerics.BigInteger" /> values and returns the remainder.</summary>
	/// <returns>The remainder after dividing <paramref name="dividend" /> by <paramref name="divisor" />.</returns>
	/// <param name="dividend">The value to be divided.</param>
	/// <param name="divisor">The value to divide by.</param>
	/// <exception cref="T:System.DivideByZeroException">
	///   <paramref name="divisor" /> is 0 (zero).</exception>
	public static BigInteger Remainder(BigInteger dividend, BigInteger divisor)
	{
		return dividend % divisor;
	}

	/// <summary>Divides one <see cref="T:System.Numerics.BigInteger" /> value by another, returns the result, and returns the remainder in an output parameter.</summary>
	/// <returns>The quotient of the division.</returns>
	/// <param name="dividend">The value to be divided.</param>
	/// <param name="divisor">The value to divide by.</param>
	/// <param name="remainder">When this method returns, contains a <see cref="T:System.Numerics.BigInteger" /> value that represents the remainder from the division. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.DivideByZeroException">
	///   <paramref name="divisor" /> is 0 (zero).</exception>
	public static BigInteger DivRem(BigInteger dividend, BigInteger divisor, out BigInteger remainder)
	{
		bool flag = dividend._bits == null;
		bool flag2 = divisor._bits == null;
		if (flag && flag2)
		{
			remainder = dividend._sign % divisor._sign;
			return dividend._sign / divisor._sign;
		}
		if (flag)
		{
			remainder = dividend;
			return s_bnZeroInt;
		}
		if (flag2)
		{
			uint remainder2;
			uint[] value = BigIntegerCalculator.Divide(dividend._bits, NumericsHelpers.Abs(divisor._sign), out remainder2);
			remainder = ((dividend._sign < 0) ? (-1 * remainder2) : remainder2);
			return new BigInteger(value, (dividend._sign < 0) ^ (divisor._sign < 0));
		}
		if (dividend._bits.Length < divisor._bits.Length)
		{
			remainder = dividend;
			return s_bnZeroInt;
		}
		uint[] remainder3;
		uint[] value2 = BigIntegerCalculator.Divide(dividend._bits, divisor._bits, out remainder3);
		remainder = new BigInteger(remainder3, dividend._sign < 0);
		return new BigInteger(value2, (dividend._sign < 0) ^ (divisor._sign < 0));
	}

	/// <summary>Negates a specified <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>The result of the <paramref name="value" /> parameter multiplied by negative one (-1).</returns>
	/// <param name="value">The value to negate.</param>
	public static BigInteger Negate(BigInteger value)
	{
		return -value;
	}

	/// <summary>Returns the natural (base e) logarithm of a specified number.</summary>
	/// <returns>The natural (base e) logarithm of <paramref name="value" />, as shown in the table in the Remarks section.</returns>
	/// <param name="value">The number whose logarithm is to be found.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The natural log of <paramref name="value" /> is out of range of the <see cref="T:System.Double" /> data type.</exception>
	public static double Log(BigInteger value)
	{
		return Log(value, Math.E);
	}

	/// <summary>Returns the logarithm of a specified number in a specified base.</summary>
	/// <returns>The base <paramref name="baseValue" /> logarithm of <paramref name="value" />, as shown in the table in the Remarks section.</returns>
	/// <param name="value">A number whose logarithm is to be found.</param>
	/// <param name="baseValue">The base of the logarithm.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The log of <paramref name="value" /> is out of range of the <see cref="T:System.Double" /> data type.</exception>
	public static double Log(BigInteger value, double baseValue)
	{
		if (value._sign < 0 || baseValue == 1.0)
		{
			return double.NaN;
		}
		if (baseValue == double.PositiveInfinity)
		{
			if (!value.IsOne)
			{
				return double.NaN;
			}
			return 0.0;
		}
		if (baseValue == 0.0 && !value.IsOne)
		{
			return double.NaN;
		}
		if (value._bits == null)
		{
			return Math.Log(value._sign, baseValue);
		}
		long num = value._bits[value._bits.Length - 1];
		ulong num2 = ((value._bits.Length > 1) ? value._bits[value._bits.Length - 2] : 0u);
		ulong num3 = ((value._bits.Length > 2) ? value._bits[value._bits.Length - 3] : 0u);
		int num4 = NumericsHelpers.CbitHighZero((uint)num);
		long num5 = (long)value._bits.Length * 32L - num4;
		return Math.Log((ulong)(num << 32 + num4) | (num2 << num4) | (num3 >> 32 - num4), baseValue) + (double)(num5 - 64) / Math.Log(baseValue, 2.0);
	}

	/// <summary>Returns the base 10 logarithm of a specified number.</summary>
	/// <returns>The base 10 logarithm of <paramref name="value" />, as shown in the table in the Remarks section.</returns>
	/// <param name="value">A number whose logarithm is to be found.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The base 10 log of <paramref name="value" /> is out of range of the <see cref="T:System.Double" /> data type.</exception>
	public static double Log10(BigInteger value)
	{
		return Log(value, 10.0);
	}

	/// <summary>Finds the greatest common divisor of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	/// <returns>The greatest common divisor of <paramref name="left" /> and <paramref name="right" />.</returns>
	/// <param name="left">The first value.</param>
	/// <param name="right">The second value.</param>
	public static BigInteger GreatestCommonDivisor(BigInteger left, BigInteger right)
	{
		bool flag = left._bits == null;
		bool flag2 = right._bits == null;
		if (flag && flag2)
		{
			return BigIntegerCalculator.Gcd(NumericsHelpers.Abs(left._sign), NumericsHelpers.Abs(right._sign));
		}
		if (flag)
		{
			if (left._sign == 0)
			{
				return new BigInteger(right._bits, negative: false);
			}
			return BigIntegerCalculator.Gcd(right._bits, NumericsHelpers.Abs(left._sign));
		}
		if (flag2)
		{
			if (right._sign == 0)
			{
				return new BigInteger(left._bits, negative: false);
			}
			return BigIntegerCalculator.Gcd(left._bits, NumericsHelpers.Abs(right._sign));
		}
		if (BigIntegerCalculator.Compare(left._bits, right._bits) < 0)
		{
			return GreatestCommonDivisor(right._bits, left._bits);
		}
		return GreatestCommonDivisor(left._bits, right._bits);
	}

	private static BigInteger GreatestCommonDivisor(uint[] leftBits, uint[] rightBits)
	{
		if (rightBits.Length == 1)
		{
			uint right = BigIntegerCalculator.Remainder(leftBits, rightBits[0]);
			return BigIntegerCalculator.Gcd(rightBits[0], right);
		}
		if (rightBits.Length == 2)
		{
			uint[] array = BigIntegerCalculator.Remainder(leftBits, rightBits);
			ulong left = ((ulong)rightBits[1] << 32) | rightBits[0];
			ulong right2 = ((ulong)array[1] << 32) | array[0];
			return BigIntegerCalculator.Gcd(left, right2);
		}
		return new BigInteger(BigIntegerCalculator.Gcd(leftBits, rightBits), negative: false);
	}

	/// <summary>Returns the larger of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	/// <returns>The <paramref name="left" /> or <paramref name="right" /> parameter, whichever is larger.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static BigInteger Max(BigInteger left, BigInteger right)
	{
		if (left.CompareTo(right) < 0)
		{
			return right;
		}
		return left;
	}

	/// <summary>Returns the smaller of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	/// <returns>The <paramref name="left" /> or <paramref name="right" /> parameter, whichever is smaller.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static BigInteger Min(BigInteger left, BigInteger right)
	{
		if (left.CompareTo(right) <= 0)
		{
			return left;
		}
		return right;
	}

	/// <summary>Performs modulus division on a number raised to the power of another number.</summary>
	/// <returns>The remainder after dividing <paramref name="value" />exponent by <paramref name="modulus" />.</returns>
	/// <param name="value">The number to raise to the <paramref name="exponent" /> power.</param>
	/// <param name="exponent">The exponent to raise <paramref name="value" /> by.</param>
	/// <param name="modulus">The number by which to divide <paramref name="value" /> raised to the <paramref name="exponent" /> power.</param>
	/// <exception cref="T:System.DivideByZeroException">
	///   <paramref name="modulus" /> is zero.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="exponent" /> is negative.</exception>
	public static BigInteger ModPow(BigInteger value, BigInteger exponent, BigInteger modulus)
	{
		if (exponent.Sign < 0)
		{
			throw new ArgumentOutOfRangeException("exponent", "The number must be greater than or equal to zero.");
		}
		bool flag = value._bits == null;
		bool flag2 = exponent._bits == null;
		if (modulus._bits == null)
		{
			uint num = ((flag && flag2) ? BigIntegerCalculator.Pow(NumericsHelpers.Abs(value._sign), NumericsHelpers.Abs(exponent._sign), NumericsHelpers.Abs(modulus._sign)) : (flag ? BigIntegerCalculator.Pow(NumericsHelpers.Abs(value._sign), exponent._bits, NumericsHelpers.Abs(modulus._sign)) : (flag2 ? BigIntegerCalculator.Pow(value._bits, NumericsHelpers.Abs(exponent._sign), NumericsHelpers.Abs(modulus._sign)) : BigIntegerCalculator.Pow(value._bits, exponent._bits, NumericsHelpers.Abs(modulus._sign)))));
			return (value._sign < 0 && !exponent.IsEven) ? (-1 * num) : num;
		}
		return new BigInteger((flag && flag2) ? BigIntegerCalculator.Pow(NumericsHelpers.Abs(value._sign), NumericsHelpers.Abs(exponent._sign), modulus._bits) : (flag ? BigIntegerCalculator.Pow(NumericsHelpers.Abs(value._sign), exponent._bits, modulus._bits) : (flag2 ? BigIntegerCalculator.Pow(value._bits, NumericsHelpers.Abs(exponent._sign), modulus._bits) : BigIntegerCalculator.Pow(value._bits, exponent._bits, modulus._bits))), value._sign < 0 && !exponent.IsEven);
	}

	/// <summary>Raises a <see cref="T:System.Numerics.BigInteger" /> value to the power of a specified value.</summary>
	/// <returns>The result of raising <paramref name="value" /> to the <paramref name="exponent" /> power.</returns>
	/// <param name="value">The number to raise to the <paramref name="exponent" /> power.</param>
	/// <param name="exponent">The exponent to raise <paramref name="value" /> by.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of the <paramref name="exponent" /> parameter is negative.</exception>
	public static BigInteger Pow(BigInteger value, int exponent)
	{
		if (exponent < 0)
		{
			throw new ArgumentOutOfRangeException("exponent", "The number must be greater than or equal to zero.");
		}
		switch (exponent)
		{
		case 0:
			return s_bnOneInt;
		case 1:
			return value;
		default:
		{
			bool flag = value._bits == null;
			if (flag)
			{
				if (value._sign == 1)
				{
					return value;
				}
				if (value._sign == -1)
				{
					if ((exponent & 1) == 0)
					{
						return s_bnOneInt;
					}
					return value;
				}
				if (value._sign == 0)
				{
					return value;
				}
			}
			return new BigInteger(flag ? BigIntegerCalculator.Pow(NumericsHelpers.Abs(value._sign), NumericsHelpers.Abs(exponent)) : BigIntegerCalculator.Pow(value._bits, NumericsHelpers.Abs(exponent)), value._sign < 0 && (exponent & 1) != 0);
		}
		}
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Numerics.BigInteger" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		if (_bits == null)
		{
			return _sign;
		}
		int num = _sign;
		int num2 = _bits.Length;
		while (--num2 >= 0)
		{
			num = NumericsHelpers.CombineHash(num, (int)_bits[num2]);
		}
		return num;
	}

	/// <summary>Returns a value that indicates whether the current instance and a specified object have the same value.</summary>
	/// <returns>true if the <paramref name="obj" /> parameter is a <see cref="T:System.Numerics.BigInteger" /> object or a type capable of implicit conversion to a <see cref="T:System.Numerics.BigInteger" /> value, and its value is equal to the value of the current <see cref="T:System.Numerics.BigInteger" /> object; otherwise, false.</returns>
	/// <param name="obj">The object to compare. </param>
	public override bool Equals(object obj)
	{
		if (!(obj is BigInteger))
		{
			return false;
		}
		return Equals((BigInteger)obj);
	}

	/// <summary>Returns a value that indicates whether the current instance and a signed 64-bit integer have the same value.</summary>
	/// <returns>true if the signed 64-bit integer and the current instance have the same value; otherwise, false.</returns>
	/// <param name="other">The signed 64-bit integer value to compare.</param>
	public bool Equals(long other)
	{
		if (_bits == null)
		{
			return _sign == other;
		}
		int num;
		if ((_sign ^ other) < 0 || (num = _bits.Length) > 2)
		{
			return false;
		}
		ulong num2 = (ulong)((other < 0) ? (-other) : other);
		if (num == 1)
		{
			return _bits[0] == num2;
		}
		return NumericsHelpers.MakeUlong(_bits[1], _bits[0]) == num2;
	}

	/// <summary>Returns a value that indicates whether the current instance and an unsigned 64-bit integer have the same value.</summary>
	/// <returns>true if the current instance and the unsigned 64-bit integer have the same value; otherwise, false.</returns>
	/// <param name="other">The unsigned 64-bit integer to compare.</param>
	[CLSCompliant(false)]
	public bool Equals(ulong other)
	{
		if (_sign < 0)
		{
			return false;
		}
		if (_bits == null)
		{
			return (ulong)_sign == other;
		}
		int num = _bits.Length;
		if (num > 2)
		{
			return false;
		}
		if (num == 1)
		{
			return _bits[0] == other;
		}
		return NumericsHelpers.MakeUlong(_bits[1], _bits[0]) == other;
	}

	/// <summary>Returns a value that indicates whether the current instance and a specified <see cref="T:System.Numerics.BigInteger" /> object have the same value.</summary>
	/// <returns>true if this <see cref="T:System.Numerics.BigInteger" /> object and <paramref name="other" /> have the same value; otherwise, false.</returns>
	/// <param name="other">The object to compare.</param>
	public bool Equals(BigInteger other)
	{
		if (_sign != other._sign)
		{
			return false;
		}
		if (_bits == other._bits)
		{
			return true;
		}
		if (_bits == null || other._bits == null)
		{
			return false;
		}
		int num = _bits.Length;
		if (num != other._bits.Length)
		{
			return false;
		}
		return GetDiffLength(_bits, other._bits, num) == 0;
	}

	/// <summary>Compares this instance to a signed 64-bit integer and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the signed 64-bit integer.</summary>
	/// <returns>A signed integer value that indicates the relationship of this instance to <paramref name="other" />, as shown in the following table.Return valueDescriptionLess than zeroThe current instance is less than <paramref name="other" />.ZeroThe current instance equals <paramref name="other" />.Greater than zeroThe current instance is greater than <paramref name="other" />.</returns>
	/// <param name="other">The signed 64-bit integer to compare.</param>
	public int CompareTo(long other)
	{
		if (_bits == null)
		{
			return ((long)_sign).CompareTo(other);
		}
		int num;
		if ((_sign ^ other) < 0 || (num = _bits.Length) > 2)
		{
			return _sign;
		}
		ulong value = (ulong)((other < 0) ? (-other) : other);
		ulong num2 = ((num == 2) ? NumericsHelpers.MakeUlong(_bits[1], _bits[0]) : _bits[0]);
		return _sign * num2.CompareTo(value);
	}

	/// <summary>Compares this instance to an unsigned 64-bit integer and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the unsigned 64-bit integer.</summary>
	/// <returns>A signed integer that indicates the relative value of this instance and <paramref name="other" />, as shown in the following table.Return valueDescriptionLess than zeroThe current instance is less than <paramref name="other" />.ZeroThe current instance equals <paramref name="other" />.Greater than zeroThe current instance is greater than <paramref name="other" />.</returns>
	/// <param name="other">The unsigned 64-bit integer to compare.</param>
	[CLSCompliant(false)]
	public int CompareTo(ulong other)
	{
		if (_sign < 0)
		{
			return -1;
		}
		if (_bits == null)
		{
			return ((ulong)_sign).CompareTo(other);
		}
		int num = _bits.Length;
		if (num > 2)
		{
			return 1;
		}
		return ((num == 2) ? NumericsHelpers.MakeUlong(_bits[1], _bits[0]) : _bits[0]).CompareTo(other);
	}

	/// <summary>Compares this instance to a second <see cref="T:System.Numerics.BigInteger" /> and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
	/// <returns>A signed integer value that indicates the relationship of this instance to <paramref name="other" />, as shown in the following table.Return valueDescriptionLess than zeroThe current instance is less than <paramref name="other" />.ZeroThe current instance equals <paramref name="other" />.Greater than zeroThe current instance is greater than <paramref name="other" />.</returns>
	/// <param name="other">The object to compare.</param>
	public int CompareTo(BigInteger other)
	{
		if ((_sign ^ other._sign) < 0)
		{
			if (_sign >= 0)
			{
				return 1;
			}
			return -1;
		}
		if (_bits == null)
		{
			if (other._bits == null)
			{
				if (_sign >= other._sign)
				{
					if (_sign <= other._sign)
					{
						return 0;
					}
					return 1;
				}
				return -1;
			}
			return -other._sign;
		}
		int num;
		int num2;
		if (other._bits == null || (num = _bits.Length) > (num2 = other._bits.Length))
		{
			return _sign;
		}
		if (num < num2)
		{
			return -_sign;
		}
		int diffLength = GetDiffLength(_bits, other._bits, num);
		if (diffLength == 0)
		{
			return 0;
		}
		if (_bits[diffLength - 1] >= other._bits[diffLength - 1])
		{
			return _sign;
		}
		return -_sign;
	}

	/// <summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
	/// <returns>A signed integer that indicates the relationship of the current instance to the <paramref name="obj" /> parameter, as shown in the following table.Return valueDescriptionLess than zeroThe current instance is less than <paramref name="obj" />.ZeroThe current instance equals <paramref name="obj" />.Greater than zeroThe current instance is greater than <paramref name="obj" />, or the <paramref name="obj" /> parameter is null. </returns>
	/// <param name="obj">The object to compare.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="obj" /> is not a <see cref="T:System.Numerics.BigInteger" />. </exception>
	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (!(obj is BigInteger))
		{
			throw new ArgumentException("The parameter must be a BigInteger.", "obj");
		}
		return CompareTo((BigInteger)obj);
	}

	/// <summary>Converts a <see cref="T:System.Numerics.BigInteger" /> value to a byte array.</summary>
	/// <returns>The value of the current <see cref="T:System.Numerics.BigInteger" /> object converted to an array of bytes.</returns>
	public byte[] ToByteArray()
	{
		return ToByteArray(isUnsigned: false, isBigEndian: false);
	}

	public byte[] ToByteArray(bool isUnsigned = false, bool isBigEndian = false)
	{
		int bytesWritten = 0;
		return TryGetBytes(GetBytesMode.AllocateArray, default(Span<byte>), isUnsigned, isBigEndian, ref bytesWritten);
	}

	public bool TryWriteBytes(Span<byte> destination, out int bytesWritten, bool isUnsigned = false, bool isBigEndian = false)
	{
		bytesWritten = 0;
		if (TryGetBytes(GetBytesMode.Span, destination, isUnsigned, isBigEndian, ref bytesWritten) == null)
		{
			bytesWritten = 0;
			return false;
		}
		return true;
	}

	internal bool TryWriteOrCountBytes(Span<byte> destination, out int bytesWritten, bool isUnsigned = false, bool isBigEndian = false)
	{
		bytesWritten = 0;
		return TryGetBytes(GetBytesMode.Span, destination, isUnsigned, isBigEndian, ref bytesWritten) != null;
	}

	public int GetByteCount(bool isUnsigned = false)
	{
		int bytesWritten = 0;
		TryGetBytes(GetBytesMode.Count, default(Span<byte>), isUnsigned, isBigEndian: false, ref bytesWritten);
		return bytesWritten;
	}

	private byte[] TryGetBytes(GetBytesMode mode, Span<byte> destination, bool isUnsigned, bool isBigEndian, ref int bytesWritten)
	{
		int sign = _sign;
		if (sign == 0)
		{
			switch (mode)
			{
			case GetBytesMode.AllocateArray:
				return new byte[1];
			case GetBytesMode.Count:
				bytesWritten = 1;
				return null;
			default:
				bytesWritten = 1;
				if (destination.Length != 0)
				{
					destination[0] = 0;
					return s_success;
				}
				return null;
			}
		}
		if (isUnsigned && sign < 0)
		{
			throw new OverflowException("Negative values do not have an unsigned representation.");
		}
		int i = 0;
		uint[] bits = _bits;
		byte b;
		uint num;
		if (bits == null)
		{
			b = (byte)((sign < 0) ? 255u : 0u);
			num = (uint)sign;
		}
		else if (sign == -1)
		{
			b = byte.MaxValue;
			for (; bits[i] == 0; i++)
			{
			}
			num = ~bits[^1];
			if (bits.Length - 1 == i)
			{
				num++;
			}
		}
		else
		{
			b = 0;
			num = bits[^1];
		}
		byte b2;
		int num2;
		if ((b2 = (byte)(num >> 24)) != b)
		{
			num2 = 3;
		}
		else if ((b2 = (byte)(num >> 16)) != b)
		{
			num2 = 2;
		}
		else if ((b2 = (byte)(num >> 8)) != b)
		{
			num2 = 1;
		}
		else
		{
			b2 = (byte)num;
			num2 = 0;
		}
		bool flag = (b2 & 0x80) != (b & 0x80) && !isUnsigned;
		int num3 = num2 + 1 + (flag ? 1 : 0);
		if (bits != null)
		{
			num3 = checked(4 * (bits.Length - 1) + num3);
		}
		byte[] result;
		switch (mode)
		{
		case GetBytesMode.AllocateArray:
			destination = (result = new byte[num3]);
			break;
		case GetBytesMode.Count:
			bytesWritten = num3;
			return null;
		default:
			bytesWritten = num3;
			if (destination.Length < num3)
			{
				return null;
			}
			result = s_success;
			break;
		}
		int num4 = (isBigEndian ? (num3 - 1) : 0);
		int num5 = ((!isBigEndian) ? 1 : (-1));
		if (bits != null)
		{
			for (int j = 0; j < bits.Length - 1; j++)
			{
				uint num6 = bits[j];
				if (sign == -1)
				{
					num6 = ~num6;
					if (j <= i)
					{
						num6++;
					}
				}
				destination[num4] = (byte)num6;
				num4 += num5;
				destination[num4] = (byte)(num6 >> 8);
				num4 += num5;
				destination[num4] = (byte)(num6 >> 16);
				num4 += num5;
				destination[num4] = (byte)(num6 >> 24);
				num4 += num5;
			}
		}
		destination[num4] = (byte)num;
		if (num2 != 0)
		{
			num4 += num5;
			destination[num4] = (byte)(num >> 8);
			if (num2 != 1)
			{
				num4 += num5;
				destination[num4] = (byte)(num >> 16);
				if (num2 != 2)
				{
					num4 += num5;
					destination[num4] = (byte)(num >> 24);
				}
			}
		}
		if (flag)
		{
			num4 += num5;
			destination[num4] = b;
		}
		return result;
	}

	private uint[] ToUInt32Array()
	{
		if (_bits == null && _sign == 0)
		{
			return new uint[1];
		}
		uint[] array;
		uint num;
		if (_bits == null)
		{
			array = new uint[1] { (uint)_sign };
			num = ((_sign < 0) ? uint.MaxValue : 0u);
		}
		else if (_sign == -1)
		{
			array = (uint[])_bits.Clone();
			NumericsHelpers.DangerousMakeTwosComplement(array);
			num = uint.MaxValue;
		}
		else
		{
			array = _bits;
			num = 0u;
		}
		int num2 = array.Length - 1;
		while (num2 > 0 && array[num2] == num)
		{
			num2--;
		}
		bool flag = (array[num2] & 0x80000000u) != (num & 0x80000000u);
		uint[] array2 = new uint[num2 + 1 + (flag ? 1 : 0)];
		Array.Copy(array, 0, array2, 0, num2 + 1);
		if (flag)
		{
			array2[^1] = num;
		}
		return array2;
	}

	/// <summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation.</summary>
	/// <returns>The string representation of the current <see cref="T:System.Numerics.BigInteger" /> value.</returns>
	public override string ToString()
	{
		return BigNumber.FormatBigInteger(this, null, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of the current <see cref="T:System.Numerics.BigInteger" /> value in the format specified by the <paramref name="provider" /> parameter.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	public string ToString(IFormatProvider provider)
	{
		return BigNumber.FormatBigInteger(this, null, NumberFormatInfo.GetInstance(provider));
	}

	/// <summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified format.</summary>
	/// <returns>The string representation of the current <see cref="T:System.Numerics.BigInteger" /> value in the format specified by the <paramref name="format" /> parameter.</returns>
	/// <param name="format">A standard or custom numeric format string.</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid format string.</exception>
	public string ToString(string format)
	{
		return BigNumber.FormatBigInteger(this, format, NumberFormatInfo.CurrentInfo);
	}

	/// <summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified format and culture-specific format information.</summary>
	/// <returns>The string representation of the current <see cref="T:System.Numerics.BigInteger" /> value as specified by the <paramref name="format" /> and <paramref name="provider" /> parameters.</returns>
	/// <param name="format">A standard or custom numeric format string.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid format string.</exception>
	public string ToString(string format, IFormatProvider provider)
	{
		return BigNumber.FormatBigInteger(this, format, NumberFormatInfo.GetInstance(provider));
	}

	public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default(ReadOnlySpan<char>), IFormatProvider provider = null)
	{
		return BigNumber.TryFormatBigInteger(this, format, NumberFormatInfo.GetInstance(provider), destination, out charsWritten);
	}

	private static BigInteger Add(uint[] leftBits, int leftSign, uint[] rightBits, int rightSign)
	{
		bool flag = leftBits == null;
		bool flag2 = rightBits == null;
		if (flag && flag2)
		{
			return (long)leftSign + (long)rightSign;
		}
		if (flag)
		{
			return new BigInteger(BigIntegerCalculator.Add(rightBits, NumericsHelpers.Abs(leftSign)), leftSign < 0);
		}
		if (flag2)
		{
			return new BigInteger(BigIntegerCalculator.Add(leftBits, NumericsHelpers.Abs(rightSign)), leftSign < 0);
		}
		if (leftBits.Length < rightBits.Length)
		{
			return new BigInteger(BigIntegerCalculator.Add(rightBits, leftBits), leftSign < 0);
		}
		return new BigInteger(BigIntegerCalculator.Add(leftBits, rightBits), leftSign < 0);
	}

	/// <summary>Subtracts a <see cref="T:System.Numerics.BigInteger" /> value from another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>The result of subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
	/// <param name="left">The value to subtract from (the minuend).</param>
	/// <param name="right">The value to subtract (the subtrahend).</param>
	public static BigInteger operator -(BigInteger left, BigInteger right)
	{
		if (left._sign < 0 != right._sign < 0)
		{
			return Add(left._bits, left._sign, right._bits, -1 * right._sign);
		}
		return Subtract(left._bits, left._sign, right._bits, right._sign);
	}

	private static BigInteger Subtract(uint[] leftBits, int leftSign, uint[] rightBits, int rightSign)
	{
		bool flag = leftBits == null;
		bool flag2 = rightBits == null;
		if (flag && flag2)
		{
			return (long)leftSign - (long)rightSign;
		}
		if (flag)
		{
			return new BigInteger(BigIntegerCalculator.Subtract(rightBits, NumericsHelpers.Abs(leftSign)), leftSign >= 0);
		}
		if (flag2)
		{
			return new BigInteger(BigIntegerCalculator.Subtract(leftBits, NumericsHelpers.Abs(rightSign)), leftSign < 0);
		}
		if (BigIntegerCalculator.Compare(leftBits, rightBits) < 0)
		{
			return new BigInteger(BigIntegerCalculator.Subtract(rightBits, leftBits), leftSign >= 0);
		}
		return new BigInteger(BigIntegerCalculator.Subtract(leftBits, rightBits), leftSign < 0);
	}

	/// <summary>Defines an implicit conversion of an unsigned byte to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
	public static implicit operator BigInteger(byte value)
	{
		return new BigInteger(value);
	}

	/// <summary>Defines an implicit conversion of an 8-bit signed integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
	[CLSCompliant(false)]
	public static implicit operator BigInteger(sbyte value)
	{
		return new BigInteger(value);
	}

	/// <summary>Defines an implicit conversion of a signed 16-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
	public static implicit operator BigInteger(short value)
	{
		return new BigInteger(value);
	}

	/// <summary>Defines an implicit conversion of a 16-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
	[CLSCompliant(false)]
	public static implicit operator BigInteger(ushort value)
	{
		return new BigInteger(value);
	}

	/// <summary>Defines an implicit conversion of a signed 32-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
	public static implicit operator BigInteger(int value)
	{
		return new BigInteger(value);
	}

	/// <summary>Defines an implicit conversion of a 32-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
	[CLSCompliant(false)]
	public static implicit operator BigInteger(uint value)
	{
		return new BigInteger(value);
	}

	/// <summary>Defines an implicit conversion of a signed 64-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
	public static implicit operator BigInteger(long value)
	{
		return new BigInteger(value);
	}

	/// <summary>Defines an implicit conversion of a 64-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
	[CLSCompliant(false)]
	public static implicit operator BigInteger(ulong value)
	{
		return new BigInteger(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Single" /> object to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is <see cref="F:System.Single.NaN" />.-or-<paramref name="value" /> is <see cref="F:System.Single.PositiveInfinity" />.-or-<paramref name="value" /> is <see cref="F:System.Single.NegativeInfinity" />.</exception>
	public static explicit operator BigInteger(float value)
	{
		return new BigInteger(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Double" /> value to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is <see cref="F:System.Double.NaN" />.-or-<paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />.-or-<paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />.</exception>
	public static explicit operator BigInteger(double value)
	{
		return new BigInteger(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> object to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
	public static explicit operator BigInteger(decimal value)
	{
		return new BigInteger(value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned byte value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Byte" />.</param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Byte.MinValue" />. -or-<paramref name="value" /> is greater than <see cref="F:System.Byte.MaxValue" />.</exception>
	public static explicit operator byte(BigInteger value)
	{
		return checked((byte)(int)value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a signed 8-bit value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a signed 8-bit value.</param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.SByte.MinValue" />.-or-<paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" />.</exception>
	[CLSCompliant(false)]
	public static explicit operator sbyte(BigInteger value)
	{
		return checked((sbyte)(int)value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 16-bit signed integer value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a 16-bit signed integer.</param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Int16.MinValue" />.-or-<paramref name="value" /> is greater than <see cref="F:System.Int16.MaxValue" />.</exception>
	public static explicit operator short(BigInteger value)
	{
		return checked((short)(int)value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 16-bit integer value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter</returns>
	/// <param name="value">The value to convert to an unsigned 16-bit integer.</param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.UInt16.MinValue" />.-or-<paramref name="value" /> is greater than <see cref="F:System.UInt16.MaxValue" />. </exception>
	[CLSCompliant(false)]
	public static explicit operator ushort(BigInteger value)
	{
		return checked((ushort)(int)value);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 32-bit signed integer value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a 32-bit signed integer. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Int32.MinValue" />.-or-<paramref name="value" /> is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public static explicit operator int(BigInteger value)
	{
		if (value._bits == null)
		{
			return value._sign;
		}
		if (value._bits.Length > 1)
		{
			throw new OverflowException("Value was either too large or too small for an Int32.");
		}
		if (value._sign > 0)
		{
			return checked((int)value._bits[0]);
		}
		if (value._bits[0] > 2147483648u)
		{
			throw new OverflowException("Value was either too large or too small for an Int32.");
		}
		return (int)(0 - value._bits[0]);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 32-bit integer value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to an unsigned 32-bit integer.</param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.UInt32.MinValue" />.-or-<paramref name="value" /> is greater than <see cref="F:System.UInt32.MaxValue" />.</exception>
	[CLSCompliant(false)]
	public static explicit operator uint(BigInteger value)
	{
		if (value._bits == null)
		{
			return checked((uint)value._sign);
		}
		if (value._bits.Length > 1 || value._sign < 0)
		{
			throw new OverflowException("Value was either too large or too small for a UInt32.");
		}
		return value._bits[0];
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 64-bit signed integer value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a 64-bit signed integer.</param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Int64.MinValue" />.-or-<paramref name="value" /> is greater than <see cref="F:System.Int64.MaxValue" />.</exception>
	public static explicit operator long(BigInteger value)
	{
		if (value._bits == null)
		{
			return value._sign;
		}
		int num = value._bits.Length;
		if (num > 2)
		{
			throw new OverflowException("Value was either too large or too small for an Int64.");
		}
		ulong num2 = ((num <= 1) ? value._bits[0] : NumericsHelpers.MakeUlong(value._bits[1], value._bits[0]));
		long num3 = (long)((value._sign > 0) ? num2 : (0L - num2));
		if ((num3 > 0 && value._sign > 0) || (num3 < 0 && value._sign < 0))
		{
			return num3;
		}
		throw new OverflowException("Value was either too large or too small for an Int64.");
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 64-bit integer value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to an unsigned 64-bit integer.</param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.UInt64.MinValue" />.-or-<paramref name="value" /> is greater than <see cref="F:System.UInt64.MaxValue" />. </exception>
	[CLSCompliant(false)]
	public static explicit operator ulong(BigInteger value)
	{
		if (value._bits == null)
		{
			return checked((ulong)value._sign);
		}
		int num = value._bits.Length;
		if (num > 2 || value._sign < 0)
		{
			throw new OverflowException("Value was either too large or too small for a UInt64.");
		}
		if (num > 1)
		{
			return NumericsHelpers.MakeUlong(value._bits[1], value._bits[0]);
		}
		return value._bits[0];
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a single-precision floating-point value.</summary>
	/// <returns>An object that contains the closest possible representation of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a single-precision floating-point value.</param>
	public static explicit operator float(BigInteger value)
	{
		return (float)(double)value;
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a <see cref="T:System.Double" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Double" />.</param>
	public static explicit operator double(BigInteger value)
	{
		int sign = value._sign;
		uint[] bits = value._bits;
		if (bits == null)
		{
			return sign;
		}
		int num = bits.Length;
		if (num > 32)
		{
			if (sign == 1)
			{
				return double.PositiveInfinity;
			}
			return double.NegativeInfinity;
		}
		long num2 = bits[num - 1];
		ulong num3 = ((num > 1) ? bits[num - 2] : 0u);
		ulong num4 = ((num > 2) ? bits[num - 3] : 0u);
		int num5 = NumericsHelpers.CbitHighZero((uint)num2);
		int exp = (num - 2) * 32 - num5;
		ulong man = (ulong)(num2 << 32 + num5) | (num3 << num5) | (num4 >> 32 - num5);
		return NumericsHelpers.GetDoubleFromParts(sign, exp, man);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a <see cref="T:System.Decimal" /> value.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
	/// <param name="value">The value to convert to a <see cref="T:System.Decimal" />.</param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Decimal.MinValue" />.-or-<paramref name="value" /> is greater than <see cref="F:System.Decimal.MaxValue" />.</exception>
	public static explicit operator decimal(BigInteger value)
	{
		if (value._bits == null)
		{
			return value._sign;
		}
		int num = value._bits.Length;
		if (num > 3)
		{
			throw new OverflowException("Value was either too large or too small for a Decimal.");
		}
		int lo = 0;
		int mid = 0;
		int hi = 0;
		if (num > 2)
		{
			hi = (int)value._bits[2];
		}
		if (num > 1)
		{
			mid = (int)value._bits[1];
		}
		if (num > 0)
		{
			lo = (int)value._bits[0];
		}
		return new decimal(lo, mid, hi, value._sign < 0, 0);
	}

	/// <summary>Performs a bitwise And operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	/// <returns>The result of the bitwise And operation.</returns>
	/// <param name="left">The first value.</param>
	/// <param name="right">The second value.</param>
	public static BigInteger operator &(BigInteger left, BigInteger right)
	{
		if (left.IsZero || right.IsZero)
		{
			return Zero;
		}
		if (left._bits == null && right._bits == null)
		{
			return left._sign & right._sign;
		}
		uint[] array = left.ToUInt32Array();
		uint[] array2 = right.ToUInt32Array();
		uint[] array3 = new uint[Math.Max(array.Length, array2.Length)];
		uint num = ((left._sign < 0) ? uint.MaxValue : 0u);
		uint num2 = ((right._sign < 0) ? uint.MaxValue : 0u);
		for (int i = 0; i < array3.Length; i++)
		{
			uint num3 = ((i < array.Length) ? array[i] : num);
			uint num4 = ((i < array2.Length) ? array2[i] : num2);
			array3[i] = num3 & num4;
		}
		return new BigInteger(array3);
	}

	/// <summary>Performs a bitwise Or operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	/// <returns>The result of the bitwise Or operation.</returns>
	/// <param name="left">The first value.</param>
	/// <param name="right">The second value.</param>
	public static BigInteger operator |(BigInteger left, BigInteger right)
	{
		if (left.IsZero)
		{
			return right;
		}
		if (right.IsZero)
		{
			return left;
		}
		if (left._bits == null && right._bits == null)
		{
			return left._sign | right._sign;
		}
		uint[] array = left.ToUInt32Array();
		uint[] array2 = right.ToUInt32Array();
		uint[] array3 = new uint[Math.Max(array.Length, array2.Length)];
		uint num = ((left._sign < 0) ? uint.MaxValue : 0u);
		uint num2 = ((right._sign < 0) ? uint.MaxValue : 0u);
		for (int i = 0; i < array3.Length; i++)
		{
			uint num3 = ((i < array.Length) ? array[i] : num);
			uint num4 = ((i < array2.Length) ? array2[i] : num2);
			array3[i] = num3 | num4;
		}
		return new BigInteger(array3);
	}

	/// <summary>Performs a bitwise exclusive Or (XOr) operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	/// <returns>The result of the bitwise Or operation.</returns>
	/// <param name="left">The first value.</param>
	/// <param name="right">The second value.</param>
	public static BigInteger operator ^(BigInteger left, BigInteger right)
	{
		if (left._bits == null && right._bits == null)
		{
			return left._sign ^ right._sign;
		}
		uint[] array = left.ToUInt32Array();
		uint[] array2 = right.ToUInt32Array();
		uint[] array3 = new uint[Math.Max(array.Length, array2.Length)];
		uint num = ((left._sign < 0) ? uint.MaxValue : 0u);
		uint num2 = ((right._sign < 0) ? uint.MaxValue : 0u);
		for (int i = 0; i < array3.Length; i++)
		{
			uint num3 = ((i < array.Length) ? array[i] : num);
			uint num4 = ((i < array2.Length) ? array2[i] : num2);
			array3[i] = num3 ^ num4;
		}
		return new BigInteger(array3);
	}

	/// <summary>Shifts a <see cref="T:System.Numerics.BigInteger" /> value a specified number of bits to the left.</summary>
	/// <returns>A value that has been shifted to the left by the specified number of bits.</returns>
	/// <param name="value">The value whose bits are to be shifted.</param>
	/// <param name="shift">The number of bits to shift <paramref name="value" /> to the left.</param>
	public static BigInteger operator <<(BigInteger value, int shift)
	{
		if (shift == 0)
		{
			return value;
		}
		if (shift == int.MinValue)
		{
			return value >> int.MaxValue >> 1;
		}
		if (shift < 0)
		{
			return value >> -shift;
		}
		int num = shift / 32;
		int num2 = shift - num * 32;
		uint[] xd;
		int xl;
		bool partsForBitManipulation = GetPartsForBitManipulation(ref value, out xd, out xl);
		uint[] array = new uint[xl + num + 1];
		if (num2 == 0)
		{
			for (int i = 0; i < xl; i++)
			{
				array[i + num] = xd[i];
			}
		}
		else
		{
			int num3 = 32 - num2;
			uint num4 = 0u;
			int j;
			for (j = 0; j < xl; j++)
			{
				uint num5 = xd[j];
				array[j + num] = (num5 << num2) | num4;
				num4 = num5 >> num3;
			}
			array[j + num] = num4;
		}
		return new BigInteger(array, partsForBitManipulation);
	}

	/// <summary>Shifts a <see cref="T:System.Numerics.BigInteger" /> value a specified number of bits to the right.</summary>
	/// <returns>A value that has been shifted to the right by the specified number of bits.</returns>
	/// <param name="value">The value whose bits are to be shifted.</param>
	/// <param name="shift">The number of bits to shift <paramref name="value" /> to the right.</param>
	public static BigInteger operator >>(BigInteger value, int shift)
	{
		if (shift == 0)
		{
			return value;
		}
		if (shift == int.MinValue)
		{
			return value << int.MaxValue << 1;
		}
		if (shift < 0)
		{
			return value << -shift;
		}
		int num = shift / 32;
		int num2 = shift - num * 32;
		uint[] xd;
		int xl;
		bool partsForBitManipulation = GetPartsForBitManipulation(ref value, out xd, out xl);
		if (partsForBitManipulation)
		{
			if (shift >= 32 * xl)
			{
				return MinusOne;
			}
			uint[] array = new uint[xl];
			Array.Copy(xd, 0, array, 0, xl);
			xd = array;
			NumericsHelpers.DangerousMakeTwosComplement(xd);
		}
		int num3 = xl - num;
		if (num3 < 0)
		{
			num3 = 0;
		}
		uint[] array2 = new uint[num3];
		if (num2 == 0)
		{
			for (int num4 = xl - 1; num4 >= num; num4--)
			{
				array2[num4 - num] = xd[num4];
			}
		}
		else
		{
			int num5 = 32 - num2;
			uint num6 = 0u;
			for (int num7 = xl - 1; num7 >= num; num7--)
			{
				uint num8 = xd[num7];
				if (partsForBitManipulation && num7 == xl - 1)
				{
					array2[num7 - num] = (num8 >> num2) | (uint)(-1 << num5);
				}
				else
				{
					array2[num7 - num] = (num8 >> num2) | num6;
				}
				num6 = num8 << num5;
			}
		}
		if (partsForBitManipulation)
		{
			NumericsHelpers.DangerousMakeTwosComplement(array2);
		}
		return new BigInteger(array2, partsForBitManipulation);
	}

	/// <summary>Returns the bitwise one's complement of a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>The bitwise one's complement of <paramref name="value" />.</returns>
	/// <param name="value">An integer value.</param>
	public static BigInteger operator ~(BigInteger value)
	{
		return -(value + One);
	}

	/// <summary>Negates a specified BigInteger value. </summary>
	/// <returns>The result of the <paramref name="value" /> parameter multiplied by negative one (-1).</returns>
	/// <param name="value">The value to negate.</param>
	public static BigInteger operator -(BigInteger value)
	{
		return new BigInteger(-value._sign, value._bits);
	}

	/// <summary>Returns the value of the <see cref="T:System.Numerics.BigInteger" /> operand. (The sign of the operand is unchanged.)</summary>
	/// <returns>The value of the <paramref name="value" /> operand.</returns>
	/// <param name="value">An integer value.</param>
	public static BigInteger operator +(BigInteger value)
	{
		return value;
	}

	/// <summary>Increments a <see cref="T:System.Numerics.BigInteger" /> value by 1.</summary>
	/// <returns>The value of the <paramref name="value" /> parameter incremented by 1.</returns>
	/// <param name="value">The value to increment.</param>
	public static BigInteger operator ++(BigInteger value)
	{
		return value + One;
	}

	/// <summary>Decrements a <see cref="T:System.Numerics.BigInteger" /> value by 1.</summary>
	/// <returns>The value of the <paramref name="value" /> parameter decremented by 1.</returns>
	/// <param name="value">The value to decrement.</param>
	public static BigInteger operator --(BigInteger value)
	{
		return value - One;
	}

	/// <summary>Adds the values of two specified <see cref="T:System.Numerics.BigInteger" /> objects.</summary>
	/// <returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
	/// <param name="left">The first value to add.</param>
	/// <param name="right">The second value to add.</param>
	public static BigInteger operator +(BigInteger left, BigInteger right)
	{
		if (left._sign < 0 != right._sign < 0)
		{
			return Subtract(left._bits, left._sign, right._bits, -1 * right._sign);
		}
		return Add(left._bits, left._sign, right._bits, right._sign);
	}

	/// <summary>Multiplies two specified <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	/// <returns>The product of <paramref name="left" /> and <paramref name="right" />.</returns>
	/// <param name="left">The first value to multiply.</param>
	/// <param name="right">The second value to multiply.</param>
	public static BigInteger operator *(BigInteger left, BigInteger right)
	{
		bool flag = left._bits == null;
		bool flag2 = right._bits == null;
		if (flag && flag2)
		{
			return (long)left._sign * (long)right._sign;
		}
		if (flag)
		{
			return new BigInteger(BigIntegerCalculator.Multiply(right._bits, NumericsHelpers.Abs(left._sign)), (left._sign < 0) ^ (right._sign < 0));
		}
		if (flag2)
		{
			return new BigInteger(BigIntegerCalculator.Multiply(left._bits, NumericsHelpers.Abs(right._sign)), (left._sign < 0) ^ (right._sign < 0));
		}
		if (left._bits == right._bits)
		{
			return new BigInteger(BigIntegerCalculator.Square(left._bits), (left._sign < 0) ^ (right._sign < 0));
		}
		if (left._bits.Length < right._bits.Length)
		{
			return new BigInteger(BigIntegerCalculator.Multiply(right._bits, left._bits), (left._sign < 0) ^ (right._sign < 0));
		}
		return new BigInteger(BigIntegerCalculator.Multiply(left._bits, right._bits), (left._sign < 0) ^ (right._sign < 0));
	}

	/// <summary>Divides a specified <see cref="T:System.Numerics.BigInteger" /> value by another specified <see cref="T:System.Numerics.BigInteger" /> value by using integer division.</summary>
	/// <returns>The integral result of the division.</returns>
	/// <param name="dividend">The value to be divided.</param>
	/// <param name="divisor">The value to divide by.</param>
	/// <exception cref="T:System.DivideByZeroException">
	///   <paramref name="divisor" /> is 0 (zero).</exception>
	public static BigInteger operator /(BigInteger dividend, BigInteger divisor)
	{
		bool flag = dividend._bits == null;
		bool flag2 = divisor._bits == null;
		if (flag && flag2)
		{
			return dividend._sign / divisor._sign;
		}
		if (flag)
		{
			return s_bnZeroInt;
		}
		if (flag2)
		{
			return new BigInteger(BigIntegerCalculator.Divide(dividend._bits, NumericsHelpers.Abs(divisor._sign)), (dividend._sign < 0) ^ (divisor._sign < 0));
		}
		if (dividend._bits.Length < divisor._bits.Length)
		{
			return s_bnZeroInt;
		}
		return new BigInteger(BigIntegerCalculator.Divide(dividend._bits, divisor._bits), (dividend._sign < 0) ^ (divisor._sign < 0));
	}

	/// <summary>Returns the remainder that results from division with two specified <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	/// <returns>The remainder that results from the division.</returns>
	/// <param name="dividend">The value to be divided.</param>
	/// <param name="divisor">The value to divide by.</param>
	/// <exception cref="T:System.DivideByZeroException">
	///   <paramref name="divisor" /> is 0 (zero).</exception>
	public static BigInteger operator %(BigInteger dividend, BigInteger divisor)
	{
		bool flag = dividend._bits == null;
		bool flag2 = divisor._bits == null;
		if (flag && flag2)
		{
			return dividend._sign % divisor._sign;
		}
		if (flag)
		{
			return dividend;
		}
		if (flag2)
		{
			uint num = BigIntegerCalculator.Remainder(dividend._bits, NumericsHelpers.Abs(divisor._sign));
			return (dividend._sign < 0) ? (-1 * num) : num;
		}
		if (dividend._bits.Length < divisor._bits.Length)
		{
			return dividend;
		}
		return new BigInteger(BigIntegerCalculator.Remainder(dividend._bits, divisor._bits), dividend._sign < 0);
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is less than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator <(BigInteger left, BigInteger right)
	{
		return left.CompareTo(right) < 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator <=(BigInteger left, BigInteger right)
	{
		return left.CompareTo(right) <= 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator >(BigInteger left, BigInteger right)
	{
		return left.CompareTo(right) > 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator >=(BigInteger left, BigInteger right)
	{
		return left.CompareTo(right) >= 0;
	}

	/// <summary>Returns a value that indicates whether the values of two <see cref="T:System.Numerics.BigInteger" /> objects are equal.</summary>
	/// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator ==(BigInteger left, BigInteger right)
	{
		return left.Equals(right);
	}

	/// <summary>Returns a value that indicates whether two <see cref="T:System.Numerics.BigInteger" /> objects have different values.</summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator !=(BigInteger left, BigInteger right)
	{
		return !left.Equals(right);
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than a 64-bit signed integer.</summary>
	/// <returns>true if <paramref name="left" /> is less than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator <(BigInteger left, long right)
	{
		return left.CompareTo(right) < 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to a 64-bit signed integer.</summary>
	/// <returns>true if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator <=(BigInteger left, long right)
	{
		return left.CompareTo(right) <= 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> is greater than a 64-bit signed integer value.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator >(BigInteger left, long right)
	{
		return left.CompareTo(right) > 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to a 64-bit signed integer value.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator >=(BigInteger left, long right)
	{
		return left.CompareTo(right) >= 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a signed long integer value are equal.</summary>
	/// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator ==(BigInteger left, long right)
	{
		return left.Equals(right);
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a 64-bit signed integer are not equal.</summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator !=(BigInteger left, long right)
	{
		return !left.Equals(right);
	}

	/// <summary>Returns a value that indicates whether a 64-bit signed integer is less than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is less than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator <(long left, BigInteger right)
	{
		return right.CompareTo(left) > 0;
	}

	/// <summary>Returns a value that indicates whether a 64-bit signed integer is less than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator <=(long left, BigInteger right)
	{
		return right.CompareTo(left) >= 0;
	}

	/// <summary>Returns a value that indicates whether a 64-bit signed integer is greater than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator >(long left, BigInteger right)
	{
		return right.CompareTo(left) < 0;
	}

	/// <summary>Returns a value that indicates whether a 64-bit signed integer is greater than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator >=(long left, BigInteger right)
	{
		return right.CompareTo(left) <= 0;
	}

	/// <summary>Returns a value that indicates whether a signed long integer value and a <see cref="T:System.Numerics.BigInteger" /> value are equal.</summary>
	/// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator ==(long left, BigInteger right)
	{
		return right.Equals(left);
	}

	/// <summary>Returns a value that indicates whether a 64-bit signed integer and a <see cref="T:System.Numerics.BigInteger" /> value are not equal.</summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator !=(long left, BigInteger right)
	{
		return !right.Equals(left);
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than a 64-bit unsigned integer.</summary>
	/// <returns>true if <paramref name="left" /> is less than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator <(BigInteger left, ulong right)
	{
		return left.CompareTo(right) < 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to a 64-bit unsigned integer.</summary>
	/// <returns>true if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator <=(BigInteger left, ulong right)
	{
		return left.CompareTo(right) <= 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than a 64-bit unsigned integer.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator >(BigInteger left, ulong right)
	{
		return left.CompareTo(right) > 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to a 64-bit unsigned integer value.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator >=(BigInteger left, ulong right)
	{
		return left.CompareTo(right) >= 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and an unsigned long integer value are equal.</summary>
	/// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator ==(BigInteger left, ulong right)
	{
		return left.Equals(right);
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a 64-bit unsigned integer are not equal.</summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator !=(BigInteger left, ulong right)
	{
		return !left.Equals(right);
	}

	/// <summary>Returns a value that indicates whether a 64-bit unsigned integer is less than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is less than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator <(ulong left, BigInteger right)
	{
		return right.CompareTo(left) > 0;
	}

	/// <summary>Returns a value that indicates whether a 64-bit unsigned integer is less than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator <=(ulong left, BigInteger right)
	{
		return right.CompareTo(left) >= 0;
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than a 64-bit unsigned integer.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator >(ulong left, BigInteger right)
	{
		return right.CompareTo(left) < 0;
	}

	/// <summary>Returns a value that indicates whether a 64-bit unsigned integer is greater than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator >=(ulong left, BigInteger right)
	{
		return right.CompareTo(left) <= 0;
	}

	/// <summary>Returns a value that indicates whether an unsigned long integer value and a <see cref="T:System.Numerics.BigInteger" /> value are equal.</summary>
	/// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator ==(ulong left, BigInteger right)
	{
		return right.Equals(left);
	}

	/// <summary>Returns a value that indicates whether a 64-bit unsigned integer and a <see cref="T:System.Numerics.BigInteger" /> value are not equal.</summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	[CLSCompliant(false)]
	public static bool operator !=(ulong left, BigInteger right)
	{
		return !right.Equals(left);
	}

	private static bool GetPartsForBitManipulation(ref BigInteger x, out uint[] xd, out int xl)
	{
		if (x._bits == null)
		{
			if (x._sign < 0)
			{
				xd = new uint[1] { (uint)(-x._sign) };
			}
			else
			{
				xd = new uint[1] { (uint)x._sign };
			}
		}
		else
		{
			xd = x._bits;
		}
		xl = ((x._bits == null) ? 1 : x._bits.Length);
		return x._sign < 0;
	}

	internal static int GetDiffLength(uint[] rgu1, uint[] rgu2, int cu)
	{
		int num = cu;
		while (--num >= 0)
		{
			if (rgu1[num] != rgu2[num])
			{
				return num + 1;
			}
		}
		return 0;
	}

	[Conditional("DEBUG")]
	private void AssertValid()
	{
		_ = _bits;
	}
}
