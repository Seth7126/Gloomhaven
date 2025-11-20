using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;

namespace System;

/// <summary>Represents a globally unique identifier (GUID).</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[NonVersionable]
public struct Guid : IFormattable, IComparable, IComparable<Guid>, IEquatable<Guid>, ISpanFormattable
{
	[Flags]
	private enum GuidStyles
	{
		None = 0,
		AllowParenthesis = 1,
		AllowBraces = 2,
		AllowDashes = 4,
		AllowHexPrefix = 8,
		RequireParenthesis = 0x10,
		RequireBraces = 0x20,
		RequireDashes = 0x40,
		RequireHexPrefix = 0x80,
		HexFormat = 0xA0,
		NumberFormat = 0,
		DigitFormat = 0x40,
		BraceFormat = 0x60,
		ParenthesisFormat = 0x50,
		Any = 0xF
	}

	private enum GuidParseThrowStyle
	{
		None,
		All,
		AllButOverflow
	}

	private enum ParseFailureKind
	{
		None,
		ArgumentNull,
		Format,
		FormatWithParameter,
		NativeException,
		FormatWithInnerException
	}

	private struct GuidResult
	{
		internal Guid _parsedGuid;

		internal GuidParseThrowStyle _throwStyle;

		private ParseFailureKind _failure;

		private string _failureMessageID;

		private object _failureMessageFormatArgument;

		private string _failureArgumentName;

		private Exception _innerException;

		internal void Init(GuidParseThrowStyle canThrow)
		{
			_throwStyle = canThrow;
		}

		internal void SetFailure(Exception nativeException)
		{
			_failure = ParseFailureKind.NativeException;
			_innerException = nativeException;
		}

		internal void SetFailure(ParseFailureKind failure, string failureMessageID)
		{
			SetFailure(failure, failureMessageID, null, null, null);
		}

		internal void SetFailure(ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument)
		{
			SetFailure(failure, failureMessageID, failureMessageFormatArgument, null, null);
		}

		internal void SetFailure(ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument, string failureArgumentName, Exception innerException)
		{
			_failure = failure;
			_failureMessageID = failureMessageID;
			_failureMessageFormatArgument = failureMessageFormatArgument;
			_failureArgumentName = failureArgumentName;
			_innerException = innerException;
			if (_throwStyle != GuidParseThrowStyle.None)
			{
				throw GetGuidParseException();
			}
		}

		internal Exception GetGuidParseException()
		{
			return _failure switch
			{
				ParseFailureKind.ArgumentNull => new ArgumentNullException(_failureArgumentName, SR.GetResourceString(_failureMessageID)), 
				ParseFailureKind.FormatWithInnerException => new FormatException(SR.GetResourceString(_failureMessageID), _innerException), 
				ParseFailureKind.FormatWithParameter => new FormatException(SR.Format(SR.GetResourceString(_failureMessageID), _failureMessageFormatArgument)), 
				ParseFailureKind.Format => new FormatException(SR.GetResourceString(_failureMessageID)), 
				ParseFailureKind.NativeException => _innerException, 
				_ => new FormatException("Unrecognized Guid format."), 
			};
		}
	}

	/// <summary>A read-only instance of the <see cref="T:System.Guid" /> structure whose value is all zeros.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly Guid Empty;

	private int _a;

	private short _b;

	private short _c;

	private byte _d;

	private byte _e;

	private byte _f;

	private byte _g;

	private byte _h;

	private byte _i;

	private byte _j;

	private byte _k;

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure.</summary>
	/// <returns>A new GUID object.</returns>
	/// <filterpriority>1</filterpriority>
	public unsafe static Guid NewGuid()
	{
		Guid result = default(Guid);
		Interop.GetRandomBytes((byte*)(&result), sizeof(Guid));
		result._c = (short)((result._c & -61441) | 0x4000);
		result._d = (byte)((result._d & -193) | 0x80);
		return result;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified array of bytes.</summary>
	/// <param name="b">A 16-element byte array containing values with which to initialize the GUID. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="b" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="b" /> is not 16 bytes long. </exception>
	public Guid(byte[] b)
		: this(new ReadOnlySpan<byte>(b ?? throw new ArgumentNullException("b")))
	{
	}

	public Guid(ReadOnlySpan<byte> b)
	{
		if (b.Length != 16)
		{
			throw new ArgumentException(SR.Format("Byte array for GUID must be exactly {0} bytes long.", "16"), "b");
		}
		_a = (b[3] << 24) | (b[2] << 16) | (b[1] << 8) | b[0];
		_b = (short)((b[5] << 8) | b[4]);
		_c = (short)((b[7] << 8) | b[6]);
		_d = b[8];
		_e = b[9];
		_f = b[10];
		_g = b[11];
		_h = b[12];
		_i = b[13];
		_j = b[14];
		_k = b[15];
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified unsigned integers and bytes.</summary>
	/// <param name="a">The first 4 bytes of the GUID. </param>
	/// <param name="b">The next 2 bytes of the GUID. </param>
	/// <param name="c">The next 2 bytes of the GUID. </param>
	/// <param name="d">The next byte of the GUID. </param>
	/// <param name="e">The next byte of the GUID. </param>
	/// <param name="f">The next byte of the GUID. </param>
	/// <param name="g">The next byte of the GUID. </param>
	/// <param name="h">The next byte of the GUID. </param>
	/// <param name="i">The next byte of the GUID. </param>
	/// <param name="j">The next byte of the GUID. </param>
	/// <param name="k">The next byte of the GUID. </param>
	[CLSCompliant(false)]
	public Guid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
	{
		_a = (int)a;
		_b = (short)b;
		_c = (short)c;
		_d = d;
		_e = e;
		_f = f;
		_g = g;
		_h = h;
		_i = i;
		_j = j;
		_k = k;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified integers and byte array.</summary>
	/// <param name="a">The first 4 bytes of the GUID. </param>
	/// <param name="b">The next 2 bytes of the GUID. </param>
	/// <param name="c">The next 2 bytes of the GUID. </param>
	/// <param name="d">The remaining 8 bytes of the GUID. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="d" /> is not 8 bytes long. </exception>
	public Guid(int a, short b, short c, byte[] d)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		if (d.Length != 8)
		{
			throw new ArgumentException(SR.Format("Byte array for GUID must be exactly {0} bytes long.", "8"), "d");
		}
		_a = a;
		_b = b;
		_c = c;
		_d = d[0];
		_e = d[1];
		_f = d[2];
		_g = d[3];
		_h = d[4];
		_i = d[5];
		_j = d[6];
		_k = d[7];
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified integers and bytes.</summary>
	/// <param name="a">The first 4 bytes of the GUID. </param>
	/// <param name="b">The next 2 bytes of the GUID. </param>
	/// <param name="c">The next 2 bytes of the GUID. </param>
	/// <param name="d">The next byte of the GUID. </param>
	/// <param name="e">The next byte of the GUID. </param>
	/// <param name="f">The next byte of the GUID. </param>
	/// <param name="g">The next byte of the GUID. </param>
	/// <param name="h">The next byte of the GUID. </param>
	/// <param name="i">The next byte of the GUID. </param>
	/// <param name="j">The next byte of the GUID. </param>
	/// <param name="k">The next byte of the GUID. </param>
	public Guid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
	{
		_a = a;
		_b = b;
		_c = c;
		_d = d;
		_e = e;
		_f = f;
		_g = g;
		_h = h;
		_i = i;
		_j = j;
		_k = k;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the value represented by the specified string.</summary>
	/// <param name="g">A string that contains a GUID in one of the following formats ("d" represents a hexadecimal digit whose case is ignored): 32 contiguous digits: dddddddddddddddddddddddddddddddd -or- Groups of 8, 4, 4, 4, and 12 digits with hyphens between the groups. The entire GUID can optionally be enclosed in matching braces or parentheses: dddddddd-dddd-dddd-dddd-dddddddddddd -or- {dddddddd-dddd-dddd-dddd-dddddddddddd} -or- (dddddddd-dddd-dddd-dddd-dddddddddddd) -or- Groups of 8, 4, and 4 digits, and a subset of eight groups of 2 digits, with each group prefixed by "0x" or "0X", and separated by commas. The entire GUID, as well as the subset, is enclosed in matching braces: {0xdddddddd, 0xdddd, 0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}} All braces, commas, and "0x" prefixes are required. All embedded spaces are ignored. All leading zeros in a group are ignored.The digits shown in a group are the maximum number of meaningful digits that can appear in that group. You can specify from 1 to the number of digits shown for a group. The specified digits are assumed to be the low-order digits of the group. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="g" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format of <paramref name="g" /> is invalid. </exception>
	/// <exception cref="T:System.OverflowException">The format of <paramref name="g" /> is invalid. </exception>
	public Guid(string g)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		GuidResult result = default(GuidResult);
		result.Init(GuidParseThrowStyle.All);
		if (TryParseGuid(g, GuidStyles.Any, ref result))
		{
			this = result._parsedGuid;
			return;
		}
		throw result.GetGuidParseException();
	}

	/// <summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure.</summary>
	/// <returns>A structure that contains the value that was parsed.</returns>
	/// <param name="input">The GUID to convert.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is null.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not in a recognized format.</exception>
	public static Guid Parse(string input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		return Parse((ReadOnlySpan<char>)input);
	}

	public static Guid Parse(ReadOnlySpan<char> input)
	{
		GuidResult result = default(GuidResult);
		result.Init(GuidParseThrowStyle.AllButOverflow);
		if (TryParseGuid(input, GuidStyles.Any, ref result))
		{
			return result._parsedGuid;
		}
		throw result.GetGuidParseException();
	}

	/// <summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure. </summary>
	/// <returns>true if the parse operation was successful; otherwise, false.</returns>
	/// <param name="input">The GUID to convert.</param>
	/// <param name="result">The structure that will contain the parsed value. If the method returns true, <paramref name="result" /> contains a valid <see cref="T:System.Guid" />. If the method returns false, <paramref name="result" /> equals <see cref="F:System.Guid.Empty" />. </param>
	public static bool TryParse(string input, out Guid result)
	{
		if (input == null)
		{
			result = default(Guid);
			return false;
		}
		return TryParse((ReadOnlySpan<char>)input, out result);
	}

	public static bool TryParse(ReadOnlySpan<char> input, out Guid result)
	{
		GuidResult result2 = default(GuidResult);
		result2.Init(GuidParseThrowStyle.None);
		if (TryParseGuid(input, GuidStyles.Any, ref result2))
		{
			result = result2._parsedGuid;
			return true;
		}
		result = default(Guid);
		return false;
	}

	/// <summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure, provided that the string is in the specified format.</summary>
	/// <returns>A structure that contains the value that was parsed.</returns>
	/// <param name="input">The GUID to convert.</param>
	/// <param name="format">One of the following specifiers that indicates the exact format to use when interpreting <paramref name="input" />: "N", "D", "B", "P", or "X".</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> or <paramref name="format" /> is null.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not in a recognized format.</exception>
	public static Guid ParseExact(string input, string format)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		ReadOnlySpan<char> input2 = input;
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		return ParseExact(input2, format);
	}

	public static Guid ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format)
	{
		if (format.Length != 1)
		{
			throw new FormatException("Format String can be only 'D', 'd', 'N', 'n', 'P', 'p', 'B', 'b', 'X' or 'x'.");
		}
		GuidStyles flags;
		switch (format[0])
		{
		case 'D':
		case 'd':
			flags = GuidStyles.RequireDashes;
			break;
		case 'N':
		case 'n':
			flags = GuidStyles.None;
			break;
		case 'B':
		case 'b':
			flags = GuidStyles.BraceFormat;
			break;
		case 'P':
		case 'p':
			flags = GuidStyles.ParenthesisFormat;
			break;
		case 'X':
		case 'x':
			flags = GuidStyles.HexFormat;
			break;
		default:
			throw new FormatException("Format String can be only 'D', 'd', 'N', 'n', 'P', 'p', 'B', 'b', 'X' or 'x'.");
		}
		GuidResult result = default(GuidResult);
		result.Init(GuidParseThrowStyle.AllButOverflow);
		if (TryParseGuid(input, flags, ref result))
		{
			return result._parsedGuid;
		}
		throw result.GetGuidParseException();
	}

	/// <summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure, provided that the string is in the specified format.</summary>
	/// <returns>true if the parse operation was successful; otherwise, false.</returns>
	/// <param name="input">The GUID to convert.</param>
	/// <param name="format">One of the following specifiers that indicates the exact format to use when interpreting <paramref name="input" />: "N", "D", "B", "P", or "X".</param>
	/// <param name="result">The structure that will contain the parsed value. If the method returns true, <paramref name="result" /> contains a valid <see cref="T:System.Guid" />. If the method returns false, <paramref name="result" /> equals <see cref="F:System.Guid.Empty" />.</param>
	public static bool TryParseExact(string input, string format, out Guid result)
	{
		if (input == null)
		{
			result = default(Guid);
			return false;
		}
		return TryParseExact((ReadOnlySpan<char>)input, (ReadOnlySpan<char>)format, out result);
	}

	public static bool TryParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, out Guid result)
	{
		if (format.Length != 1)
		{
			result = default(Guid);
			return false;
		}
		GuidStyles flags;
		switch (format[0])
		{
		case 'D':
		case 'd':
			flags = GuidStyles.RequireDashes;
			break;
		case 'N':
		case 'n':
			flags = GuidStyles.None;
			break;
		case 'B':
		case 'b':
			flags = GuidStyles.BraceFormat;
			break;
		case 'P':
		case 'p':
			flags = GuidStyles.ParenthesisFormat;
			break;
		case 'X':
		case 'x':
			flags = GuidStyles.HexFormat;
			break;
		default:
			result = default(Guid);
			return false;
		}
		GuidResult result2 = default(GuidResult);
		result2.Init(GuidParseThrowStyle.None);
		if (TryParseGuid(input, flags, ref result2))
		{
			result = result2._parsedGuid;
			return true;
		}
		result = default(Guid);
		return false;
	}

	private static bool TryParseGuid(ReadOnlySpan<char> guidString, GuidStyles flags, ref GuidResult result)
	{
		guidString = guidString.Trim();
		if (guidString.Length == 0)
		{
			result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
			return false;
		}
		bool flag = guidString.IndexOf('-') >= 0;
		if (flag)
		{
			if ((flags & (GuidStyles.AllowDashes | GuidStyles.RequireDashes)) == 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
				return false;
			}
		}
		else if ((flags & GuidStyles.RequireDashes) != GuidStyles.None)
		{
			result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
			return false;
		}
		bool flag2 = guidString.IndexOf('{') >= 0;
		if (flag2)
		{
			if ((flags & (GuidStyles.AllowBraces | GuidStyles.RequireBraces)) == 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
				return false;
			}
		}
		else if ((flags & GuidStyles.RequireBraces) != GuidStyles.None)
		{
			result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
			return false;
		}
		if (guidString.IndexOf('(') >= 0)
		{
			if ((flags & (GuidStyles.AllowParenthesis | GuidStyles.RequireParenthesis)) == 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
				return false;
			}
		}
		else if ((flags & GuidStyles.RequireParenthesis) != GuidStyles.None)
		{
			result.SetFailure(ParseFailureKind.Format, "Unrecognized Guid format.");
			return false;
		}
		try
		{
			if (flag)
			{
				return TryParseGuidWithDashes(guidString, ref result);
			}
			if (flag2)
			{
				return TryParseGuidWithHexPrefix(guidString, ref result);
			}
			return TryParseGuidWithNoStyle(guidString, ref result);
		}
		catch (IndexOutOfRangeException innerException)
		{
			result.SetFailure(ParseFailureKind.FormatWithInnerException, "Unrecognized Guid format.", null, null, innerException);
			return false;
		}
		catch (ArgumentException innerException2)
		{
			result.SetFailure(ParseFailureKind.FormatWithInnerException, "Unrecognized Guid format.", null, null, innerException2);
			return false;
		}
	}

	private static bool TryParseGuidWithHexPrefix(ReadOnlySpan<char> guidString, ref GuidResult result)
	{
		int num = 0;
		int num2 = 0;
		guidString = EatAllWhitespace(guidString);
		if (guidString.Length == 0 || guidString[0] != '{')
		{
			result.SetFailure(ParseFailureKind.Format, "Expected {0xdddddddd, etc}.");
			return false;
		}
		if (!IsHexPrefix(guidString, 1))
		{
			result.SetFailure(ParseFailureKind.Format, "Expected hex 0x in '{0}'.", "{0xdddddddd, etc}");
			return false;
		}
		num = 3;
		num2 = guidString.Slice(num).IndexOf(',');
		if (num2 <= 0)
		{
			result.SetFailure(ParseFailureKind.Format, "Could not find a comma, or the length between the previous token and the comma was zero (i.e., '0x,'etc.).");
			return false;
		}
		if (!StringToInt(guidString.Slice(num, num2), -1, 4096, out result._parsedGuid._a, ref result))
		{
			return false;
		}
		if (!IsHexPrefix(guidString, num + num2 + 1))
		{
			result.SetFailure(ParseFailureKind.Format, "Expected hex 0x in '{0}'.", "{0xdddddddd, 0xdddd, etc}");
			return false;
		}
		num = num + num2 + 3;
		num2 = guidString.Slice(num).IndexOf(',');
		if (num2 <= 0)
		{
			result.SetFailure(ParseFailureKind.Format, "Could not find a comma, or the length between the previous token and the comma was zero (i.e., '0x,'etc.).");
			return false;
		}
		if (!StringToShort(guidString.Slice(num, num2), -1, 4096, out result._parsedGuid._b, ref result))
		{
			return false;
		}
		if (!IsHexPrefix(guidString, num + num2 + 1))
		{
			result.SetFailure(ParseFailureKind.Format, "Expected hex 0x in '{0}'.", "{0xdddddddd, 0xdddd, 0xdddd, etc}");
			return false;
		}
		num = num + num2 + 3;
		num2 = guidString.Slice(num).IndexOf(',');
		if (num2 <= 0)
		{
			result.SetFailure(ParseFailureKind.Format, "Could not find a comma, or the length between the previous token and the comma was zero (i.e., '0x,'etc.).");
			return false;
		}
		if (!StringToShort(guidString.Slice(num, num2), -1, 4096, out result._parsedGuid._c, ref result))
		{
			return false;
		}
		if (guidString.Length <= num + num2 + 1 || guidString[num + num2 + 1] != '{')
		{
			result.SetFailure(ParseFailureKind.Format, "Expected {0xdddddddd, etc}.");
			return false;
		}
		num2++;
		Span<byte> span = stackalloc byte[8];
		for (int i = 0; i < span.Length; i++)
		{
			if (!IsHexPrefix(guidString, num + num2 + 1))
			{
				result.SetFailure(ParseFailureKind.Format, "Expected hex 0x in '{0}'.", "{... { ... 0xdd, ...}}");
				return false;
			}
			num = num + num2 + 3;
			if (i < 7)
			{
				num2 = guidString.Slice(num).IndexOf(',');
				if (num2 <= 0)
				{
					result.SetFailure(ParseFailureKind.Format, "Could not find a comma, or the length between the previous token and the comma was zero (i.e., '0x,'etc.).");
					return false;
				}
			}
			else
			{
				num2 = guidString.Slice(num).IndexOf('}');
				if (num2 <= 0)
				{
					result.SetFailure(ParseFailureKind.Format, "Could not find a brace, or the length between the previous token and the brace was zero (i.e., '0x,'etc.).");
					return false;
				}
			}
			if (!StringToInt(guidString.Slice(num, num2), -1, 4096, out var result2, ref result))
			{
				return false;
			}
			uint num3 = (uint)result2;
			if (num3 > 255)
			{
				result.SetFailure(ParseFailureKind.Format, "Value was either too large or too small for an unsigned byte.");
				return false;
			}
			span[i] = (byte)num3;
		}
		result._parsedGuid._d = span[0];
		result._parsedGuid._e = span[1];
		result._parsedGuid._f = span[2];
		result._parsedGuid._g = span[3];
		result._parsedGuid._h = span[4];
		result._parsedGuid._i = span[5];
		result._parsedGuid._j = span[6];
		result._parsedGuid._k = span[7];
		if (num + num2 + 1 >= guidString.Length || guidString[num + num2 + 1] != '}')
		{
			result.SetFailure(ParseFailureKind.Format, "Could not find the ending brace.");
			return false;
		}
		if (num + num2 + 1 != guidString.Length - 1)
		{
			result.SetFailure(ParseFailureKind.Format, "Additional non-parsable characters are at the end of the string.");
			return false;
		}
		return true;
	}

	private static bool TryParseGuidWithNoStyle(ReadOnlySpan<char> guidString, ref GuidResult result)
	{
		int num = 0;
		int num2 = 0;
		if (guidString.Length != 32)
		{
			result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
			return false;
		}
		for (int i = 0; i < guidString.Length; i++)
		{
			char c = guidString[i];
			if (c < '0' || c > '9')
			{
				char c2 = char.ToUpperInvariant(c);
				if (c2 < 'A' || c2 > 'F')
				{
					result.SetFailure(ParseFailureKind.Format, "Guid string should only contain hexadecimal characters.");
					return false;
				}
			}
		}
		if (!StringToInt(guidString.Slice(num, 8), -1, 4096, out result._parsedGuid._a, ref result))
		{
			return false;
		}
		num += 8;
		if (!StringToShort(guidString.Slice(num, 4), -1, 4096, out result._parsedGuid._b, ref result))
		{
			return false;
		}
		num += 4;
		if (!StringToShort(guidString.Slice(num, 4), -1, 4096, out result._parsedGuid._c, ref result))
		{
			return false;
		}
		num += 4;
		if (!StringToInt(guidString.Slice(num, 4), -1, 4096, out var result2, ref result))
		{
			return false;
		}
		num += 4;
		num2 = num;
		if (!StringToLong(guidString, ref num2, 8192, out var result3, ref result))
		{
			return false;
		}
		if (num2 - num != 12)
		{
			result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
			return false;
		}
		result._parsedGuid._d = (byte)(result2 >> 8);
		result._parsedGuid._e = (byte)result2;
		result2 = (int)(result3 >> 32);
		result._parsedGuid._f = (byte)(result2 >> 8);
		result._parsedGuid._g = (byte)result2;
		result2 = (int)result3;
		result._parsedGuid._h = (byte)(result2 >> 24);
		result._parsedGuid._i = (byte)(result2 >> 16);
		result._parsedGuid._j = (byte)(result2 >> 8);
		result._parsedGuid._k = (byte)result2;
		return true;
	}

	private static bool TryParseGuidWithDashes(ReadOnlySpan<char> guidString, ref GuidResult result)
	{
		int num = 0;
		int num2 = 0;
		if (guidString[0] == '{')
		{
			if (guidString.Length != 38 || guidString[37] != '}')
			{
				result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
				return false;
			}
			num = 1;
		}
		else if (guidString[0] == '(')
		{
			if (guidString.Length != 38 || guidString[37] != ')')
			{
				result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
				return false;
			}
			num = 1;
		}
		else if (guidString.Length != 36)
		{
			result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
			return false;
		}
		if (guidString[8 + num] != '-' || guidString[13 + num] != '-' || guidString[18 + num] != '-' || guidString[23 + num] != '-')
		{
			result.SetFailure(ParseFailureKind.Format, "Dashes are in the wrong position for GUID parsing.");
			return false;
		}
		num2 = num;
		if (!StringToInt(guidString, ref num2, 8, 8192, out var result2, ref result))
		{
			return false;
		}
		result._parsedGuid._a = result2;
		num2++;
		if (!StringToInt(guidString, ref num2, 4, 8192, out result2, ref result))
		{
			return false;
		}
		result._parsedGuid._b = (short)result2;
		num2++;
		if (!StringToInt(guidString, ref num2, 4, 8192, out result2, ref result))
		{
			return false;
		}
		result._parsedGuid._c = (short)result2;
		num2++;
		if (!StringToInt(guidString, ref num2, 4, 8192, out result2, ref result))
		{
			return false;
		}
		num2++;
		num = num2;
		if (!StringToLong(guidString, ref num2, 8192, out var result3, ref result))
		{
			return false;
		}
		if (num2 - num != 12)
		{
			result.SetFailure(ParseFailureKind.Format, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
			return false;
		}
		result._parsedGuid._d = (byte)(result2 >> 8);
		result._parsedGuid._e = (byte)result2;
		result2 = (int)(result3 >> 32);
		result._parsedGuid._f = (byte)(result2 >> 8);
		result._parsedGuid._g = (byte)result2;
		result2 = (int)result3;
		result._parsedGuid._h = (byte)(result2 >> 24);
		result._parsedGuid._i = (byte)(result2 >> 16);
		result._parsedGuid._j = (byte)(result2 >> 8);
		result._parsedGuid._k = (byte)result2;
		return true;
	}

	private static bool StringToShort(ReadOnlySpan<char> str, int requiredLength, int flags, out short result, ref GuidResult parseResult)
	{
		int parsePos = 0;
		return StringToShort(str, ref parsePos, requiredLength, flags, out result, ref parseResult);
	}

	private static bool StringToShort(ReadOnlySpan<char> str, ref int parsePos, int requiredLength, int flags, out short result, ref GuidResult parseResult)
	{
		result = 0;
		int result3;
		bool result2 = StringToInt(str, ref parsePos, requiredLength, flags, out result3, ref parseResult);
		result = (short)result3;
		return result2;
	}

	private static bool StringToInt(ReadOnlySpan<char> str, int requiredLength, int flags, out int result, ref GuidResult parseResult)
	{
		int parsePos = 0;
		return StringToInt(str, ref parsePos, requiredLength, flags, out result, ref parseResult);
	}

	private static bool StringToInt(ReadOnlySpan<char> str, ref int parsePos, int requiredLength, int flags, out int result, ref GuidResult parseResult)
	{
		result = 0;
		int num = parsePos;
		try
		{
			result = ParseNumbers.StringToInt(str, 16, flags, ref parsePos);
		}
		catch (OverflowException ex)
		{
			if (parseResult._throwStyle == GuidParseThrowStyle.All)
			{
				throw;
			}
			if (parseResult._throwStyle == GuidParseThrowStyle.AllButOverflow)
			{
				throw new FormatException("Unrecognized Guid format.", ex);
			}
			parseResult.SetFailure(ex);
			return false;
		}
		catch (Exception failure)
		{
			if (parseResult._throwStyle == GuidParseThrowStyle.None)
			{
				parseResult.SetFailure(failure);
				return false;
			}
			throw;
		}
		if (requiredLength != -1 && parsePos - num != requiredLength)
		{
			parseResult.SetFailure(ParseFailureKind.Format, "Guid string should only contain hexadecimal characters.");
			return false;
		}
		return true;
	}

	private static bool StringToLong(ReadOnlySpan<char> str, ref int parsePos, int flags, out long result, ref GuidResult parseResult)
	{
		result = 0L;
		try
		{
			result = ParseNumbers.StringToLong(str, 16, flags, ref parsePos);
		}
		catch (OverflowException ex)
		{
			if (parseResult._throwStyle == GuidParseThrowStyle.All)
			{
				throw;
			}
			if (parseResult._throwStyle == GuidParseThrowStyle.AllButOverflow)
			{
				throw new FormatException("Unrecognized Guid format.", ex);
			}
			parseResult.SetFailure(ex);
			return false;
		}
		catch (Exception failure)
		{
			if (parseResult._throwStyle == GuidParseThrowStyle.None)
			{
				parseResult.SetFailure(failure);
				return false;
			}
			throw;
		}
		return true;
	}

	private static ReadOnlySpan<char> EatAllWhitespace(ReadOnlySpan<char> str)
	{
		int i;
		for (i = 0; i < str.Length && !char.IsWhiteSpace(str[i]); i++)
		{
		}
		if (i == str.Length)
		{
			return str;
		}
		char[] array = new char[str.Length];
		int length = 0;
		if (i > 0)
		{
			length = i;
			str.Slice(0, i).CopyTo(array);
		}
		for (; i < str.Length; i++)
		{
			char c = str[i];
			if (!char.IsWhiteSpace(c))
			{
				array[length++] = c;
			}
		}
		return new ReadOnlySpan<char>(array, 0, length);
	}

	private static bool IsHexPrefix(ReadOnlySpan<char> str, int i)
	{
		if (i + 1 < str.Length && str[i] == '0')
		{
			if (str[i + 1] != 'x')
			{
				return char.ToLowerInvariant(str[i + 1]) == 'x';
			}
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WriteByteHelper(Span<byte> destination)
	{
		destination[0] = (byte)_a;
		destination[1] = (byte)(_a >> 8);
		destination[2] = (byte)(_a >> 16);
		destination[3] = (byte)(_a >> 24);
		destination[4] = (byte)_b;
		destination[5] = (byte)(_b >> 8);
		destination[6] = (byte)_c;
		destination[7] = (byte)(_c >> 8);
		destination[8] = _d;
		destination[9] = _e;
		destination[10] = _f;
		destination[11] = _g;
		destination[12] = _h;
		destination[13] = _i;
		destination[14] = _j;
		destination[15] = _k;
	}

	/// <summary>Returns a 16-element byte array that contains the value of this instance.</summary>
	/// <returns>A 16-element byte array.</returns>
	/// <filterpriority>2</filterpriority>
	public byte[] ToByteArray()
	{
		byte[] array = new byte[16];
		WriteByteHelper(array);
		return array;
	}

	public bool TryWriteBytes(Span<byte> destination)
	{
		if (destination.Length < 16)
		{
			return false;
		}
		WriteByteHelper(destination);
		return true;
	}

	/// <summary>Returns a string representation of the value of this instance in registry format.</summary>
	/// <returns>The value of this <see cref="T:System.Guid" />, formatted by using the "D" format specifier as follows: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx where the value of the GUID is represented as a series of lowercase hexadecimal digits in groups of 8, 4, 4, 4, and 12 digits and separated by hyphens. An example of a return value is "382c74c3-721d-4f34-80e5-57657b6cbc27". To convert the hexadecimal digits from a through f to uppercase, call the <see cref="M:System.String.ToString" /> method on the returned string.</returns>
	/// <filterpriority>1</filterpriority>
	public override string ToString()
	{
		return ToString("D", null);
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>The hash code for this instance.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return _a ^ Unsafe.Add(ref _a, 1) ^ Unsafe.Add(ref _a, 2) ^ Unsafe.Add(ref _a, 3);
	}

	/// <summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="o" /> is a <see cref="T:System.Guid" /> that has the same value as this instance; otherwise, false.</returns>
	/// <param name="o">The object to compare with this instance. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Guid guid))
		{
			return false;
		}
		if (guid._a == _a && Unsafe.Add(ref guid._a, 1) == Unsafe.Add(ref _a, 1) && Unsafe.Add(ref guid._a, 2) == Unsafe.Add(ref _a, 2))
		{
			return Unsafe.Add(ref guid._a, 3) == Unsafe.Add(ref _a, 3);
		}
		return false;
	}

	/// <summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Guid" /> object represent the same value.</summary>
	/// <returns>true if <paramref name="g" /> is equal to this instance; otherwise, false.</returns>
	/// <param name="g">An object to compare to this instance.</param>
	/// <filterpriority>2</filterpriority>
	public bool Equals(Guid g)
	{
		if (g._a == _a && Unsafe.Add(ref g._a, 1) == Unsafe.Add(ref _a, 1) && Unsafe.Add(ref g._a, 2) == Unsafe.Add(ref _a, 2))
		{
			return Unsafe.Add(ref g._a, 3) == Unsafe.Add(ref _a, 3);
		}
		return false;
	}

	private int GetResult(uint me, uint them)
	{
		if (me < them)
		{
			return -1;
		}
		return 1;
	}

	/// <summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Return value Description A negative integer This instance is less than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. A positive integer This instance is greater than <paramref name="value" />, or <paramref name="value" /> is null. </returns>
	/// <param name="value">An object to compare, or null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a <see cref="T:System.Guid" />. </exception>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(object value)
	{
		if (value == null)
		{
			return 1;
		}
		if (!(value is Guid guid))
		{
			throw new ArgumentException("Object must be of type GUID.", "value");
		}
		if (guid._a != _a)
		{
			return GetResult((uint)_a, (uint)guid._a);
		}
		if (guid._b != _b)
		{
			return GetResult((uint)_b, (uint)guid._b);
		}
		if (guid._c != _c)
		{
			return GetResult((uint)_c, (uint)guid._c);
		}
		if (guid._d != _d)
		{
			return GetResult(_d, guid._d);
		}
		if (guid._e != _e)
		{
			return GetResult(_e, guid._e);
		}
		if (guid._f != _f)
		{
			return GetResult(_f, guid._f);
		}
		if (guid._g != _g)
		{
			return GetResult(_g, guid._g);
		}
		if (guid._h != _h)
		{
			return GetResult(_h, guid._h);
		}
		if (guid._i != _i)
		{
			return GetResult(_i, guid._i);
		}
		if (guid._j != _j)
		{
			return GetResult(_j, guid._j);
		}
		if (guid._k != _k)
		{
			return GetResult(_k, guid._k);
		}
		return 0;
	}

	/// <summary>Compares this instance to a specified <see cref="T:System.Guid" /> object and returns an indication of their relative values.</summary>
	/// <returns>A signed number indicating the relative values of this instance and <paramref name="value" />.Return value Description A negative integer This instance is less than <paramref name="value" />. Zero This instance is equal to <paramref name="value" />. A positive integer This instance is greater than <paramref name="value" />. </returns>
	/// <param name="value">An object to compare to this instance.</param>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(Guid value)
	{
		if (value._a != _a)
		{
			return GetResult((uint)_a, (uint)value._a);
		}
		if (value._b != _b)
		{
			return GetResult((uint)_b, (uint)value._b);
		}
		if (value._c != _c)
		{
			return GetResult((uint)_c, (uint)value._c);
		}
		if (value._d != _d)
		{
			return GetResult(_d, value._d);
		}
		if (value._e != _e)
		{
			return GetResult(_e, value._e);
		}
		if (value._f != _f)
		{
			return GetResult(_f, value._f);
		}
		if (value._g != _g)
		{
			return GetResult(_g, value._g);
		}
		if (value._h != _h)
		{
			return GetResult(_h, value._h);
		}
		if (value._i != _i)
		{
			return GetResult(_i, value._i);
		}
		if (value._j != _j)
		{
			return GetResult(_j, value._j);
		}
		if (value._k != _k)
		{
			return GetResult(_k, value._k);
		}
		return 0;
	}

	/// <summary>Indicates whether the values of two specified <see cref="T:System.Guid" /> objects are equal.</summary>
	/// <returns>true if <paramref name="a" /> and <paramref name="b" /> are equal; otherwise, false.</returns>
	/// <param name="a">The first object to compare. </param>
	/// <param name="b">The second object to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(Guid a, Guid b)
	{
		if (a._a == b._a && Unsafe.Add(ref a._a, 1) == Unsafe.Add(ref b._a, 1) && Unsafe.Add(ref a._a, 2) == Unsafe.Add(ref b._a, 2))
		{
			return Unsafe.Add(ref a._a, 3) == Unsafe.Add(ref b._a, 3);
		}
		return false;
	}

	/// <summary>Indicates whether the values of two specified <see cref="T:System.Guid" /> objects are not equal.</summary>
	/// <returns>true if <paramref name="a" /> and <paramref name="b" /> are not equal; otherwise, false.</returns>
	/// <param name="a">The first object to compare. </param>
	/// <param name="b">The second object to compare. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(Guid a, Guid b)
	{
		if (a._a == b._a && Unsafe.Add(ref a._a, 1) == Unsafe.Add(ref b._a, 1) && Unsafe.Add(ref a._a, 2) == Unsafe.Add(ref b._a, 2))
		{
			return Unsafe.Add(ref a._a, 3) != Unsafe.Add(ref b._a, 3);
		}
		return true;
	}

	/// <summary>Returns a string representation of the value of this <see cref="T:System.Guid" /> instance, according to the provided format specifier.</summary>
	/// <returns>The value of this <see cref="T:System.Guid" />, represented as a series of lowercase hexadecimal digits in the specified format. </returns>
	/// <param name="format">A single format specifier that indicates how to format the value of this <see cref="T:System.Guid" />. The <paramref name="format" /> parameter can be "N", "D", "B", "P", or "X". If <paramref name="format" /> is null or an empty string (""), "D" is used. </param>
	/// <exception cref="T:System.FormatException">The value of <paramref name="format" /> is not null, an empty string (""), "N", "D", "B", "P", or "X". </exception>
	/// <filterpriority>1</filterpriority>
	public string ToString(string format)
	{
		return ToString(format, null);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static char HexToChar(int a)
	{
		a &= 0xF;
		return (char)((a > 9) ? (a - 10 + 97) : (a + 48));
	}

	private unsafe static int HexsToChars(char* guidChars, int a, int b)
	{
		*guidChars = HexToChar(a >> 4);
		guidChars[1] = HexToChar(a);
		guidChars[2] = HexToChar(b >> 4);
		guidChars[3] = HexToChar(b);
		return 4;
	}

	private unsafe static int HexsToCharsHexOutput(char* guidChars, int a, int b)
	{
		*guidChars = '0';
		guidChars[1] = 'x';
		guidChars[2] = HexToChar(a >> 4);
		guidChars[3] = HexToChar(a);
		guidChars[4] = ',';
		guidChars[5] = '0';
		guidChars[6] = 'x';
		guidChars[7] = HexToChar(b >> 4);
		guidChars[8] = HexToChar(b);
		return 9;
	}

	/// <summary>Returns a string representation of the value of this instance of the <see cref="T:System.Guid" /> class, according to the provided format specifier and culture-specific format information.</summary>
	/// <returns>The value of this <see cref="T:System.Guid" />, represented as a series of lowercase hexadecimal digits in the specified format.</returns>
	/// <param name="format">A single format specifier that indicates how to format the value of this <see cref="T:System.Guid" />. The <paramref name="format" /> parameter can be "N", "D", "B", "P", or "X". If <paramref name="format" /> is null or an empty string (""), "D" is used. </param>
	/// <param name="provider">(Reserved) An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of <paramref name="format" /> is not null, an empty string (""), "N", "D", "B", "P", or "X". </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe string ToString(string format, IFormatProvider provider)
	{
		if (format == null || format.Length == 0)
		{
			format = "D";
		}
		if (format.Length != 1)
		{
			throw new FormatException("Format String can be only 'D', 'd', 'N', 'n', 'P', 'p', 'B', 'b', 'X' or 'x'.");
		}
		int length;
		switch (format[0])
		{
		case 'D':
		case 'd':
			length = 36;
			break;
		case 'N':
		case 'n':
			length = 32;
			break;
		case 'B':
		case 'P':
		case 'b':
		case 'p':
			length = 38;
			break;
		case 'X':
		case 'x':
			length = 68;
			break;
		default:
			throw new FormatException("Format String can be only 'D', 'd', 'N', 'n', 'P', 'p', 'B', 'b', 'X' or 'x'.");
		}
		string text = string.FastAllocateString(length);
		fixed (char* pointer = text)
		{
			TryFormat(new Span<char>(pointer, text.Length), out var _, format);
		}
		return text;
	}

	public unsafe bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default(ReadOnlySpan<char>))
	{
		if (format.Length == 0)
		{
			format = "D";
		}
		if (format.Length != 1)
		{
			throw new FormatException("Format String can be only 'D', 'd', 'N', 'n', 'P', 'p', 'B', 'b', 'X' or 'x'.");
		}
		bool flag = true;
		bool flag2 = false;
		int num = 0;
		int num2;
		switch (format[0])
		{
		case 'D':
		case 'd':
			num2 = 36;
			break;
		case 'N':
		case 'n':
			flag = false;
			num2 = 32;
			break;
		case 'B':
		case 'b':
			num = 8192123;
			num2 = 38;
			break;
		case 'P':
		case 'p':
			num = 2687016;
			num2 = 38;
			break;
		case 'X':
		case 'x':
			num = 8192123;
			flag = false;
			flag2 = true;
			num2 = 68;
			break;
		default:
			throw new FormatException("Format String can be only 'D', 'd', 'N', 'n', 'P', 'p', 'B', 'b', 'X' or 'x'.");
		}
		if (destination.Length < num2)
		{
			charsWritten = 0;
			return false;
		}
		fixed (char* reference = &MemoryMarshal.GetReference(destination))
		{
			char* ptr = reference;
			if (num != 0)
			{
				*(ptr++) = (char)num;
			}
			if (flag2)
			{
				*(ptr++) = '0';
				*(ptr++) = 'x';
				ptr += HexsToChars(ptr, _a >> 24, _a >> 16);
				ptr += HexsToChars(ptr, _a >> 8, _a);
				*(ptr++) = ',';
				*(ptr++) = '0';
				*(ptr++) = 'x';
				ptr += HexsToChars(ptr, _b >> 8, _b);
				*(ptr++) = ',';
				*(ptr++) = '0';
				*(ptr++) = 'x';
				ptr += HexsToChars(ptr, _c >> 8, _c);
				*(ptr++) = ',';
				*(ptr++) = '{';
				ptr += HexsToCharsHexOutput(ptr, _d, _e);
				*(ptr++) = ',';
				ptr += HexsToCharsHexOutput(ptr, _f, _g);
				*(ptr++) = ',';
				ptr += HexsToCharsHexOutput(ptr, _h, _i);
				*(ptr++) = ',';
				ptr += HexsToCharsHexOutput(ptr, _j, _k);
				*(ptr++) = '}';
			}
			else
			{
				ptr += HexsToChars(ptr, _a >> 24, _a >> 16);
				ptr += HexsToChars(ptr, _a >> 8, _a);
				if (flag)
				{
					*(ptr++) = '-';
				}
				ptr += HexsToChars(ptr, _b >> 8, _b);
				if (flag)
				{
					*(ptr++) = '-';
				}
				ptr += HexsToChars(ptr, _c >> 8, _c);
				if (flag)
				{
					*(ptr++) = '-';
				}
				ptr += HexsToChars(ptr, _d, _e);
				if (flag)
				{
					*(ptr++) = '-';
				}
				ptr += HexsToChars(ptr, _f, _g);
				ptr += HexsToChars(ptr, _h, _i);
				ptr += HexsToChars(ptr, _j, _k);
			}
			if (num != 0)
			{
				*(ptr++) = (char)(num >> 16);
			}
		}
		charsWritten = num2;
		return true;
	}

	bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
	{
		return TryFormat(destination, out charsWritten, format);
	}

	internal unsafe static byte[] FastNewGuidArray()
	{
		byte[] array = new byte[16];
		fixed (byte* buffer = array)
		{
			Interop.GetRandomBytes(buffer, 16);
		}
		array[8] = (byte)((array[8] & 0x3F) | 0x80);
		array[7] = (byte)((array[7] & 0xF) | 0x40);
		return array;
	}
}
