using System.Buffers;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Cryptography.Asn1;

internal class AsnReader
{
	private delegate void BitStringCopyAction(ReadOnlyMemory<byte> value, byte normalizedLastByte, Span<byte> destination);

	internal const int MaxCERSegmentSize = 1000;

	private const int EndOfContentsEncodedLength = 2;

	private ReadOnlyMemory<byte> _data;

	private readonly AsnEncodingRules _ruleSet;

	private const byte HmsState = 0;

	private const byte FracState = 1;

	private const byte SuffixState = 2;

	public bool HasData => !_data.IsEmpty;

	public AsnReader(ReadOnlyMemory<byte> data, AsnEncodingRules ruleSet)
	{
		CheckEncodingRules(ruleSet);
		_data = data;
		_ruleSet = ruleSet;
	}

	public void ThrowIfNotEmpty()
	{
		if (HasData)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
	}

	public static bool TryPeekTag(ReadOnlySpan<byte> source, out Asn1Tag tag, out int bytesRead)
	{
		return Asn1Tag.TryParse(source, out tag, out bytesRead);
	}

	public Asn1Tag PeekTag()
	{
		if (TryPeekTag(_data.Span, out var tag, out var _))
		{
			return tag;
		}
		throw new CryptographicException("ASN1 corrupted data.");
	}

	private static bool TryReadLength(ReadOnlySpan<byte> source, AsnEncodingRules ruleSet, out int? length, out int bytesRead)
	{
		length = null;
		bytesRead = 0;
		CheckEncodingRules(ruleSet);
		if (source.IsEmpty)
		{
			return false;
		}
		byte b = source[bytesRead];
		bytesRead++;
		if (b == 128)
		{
			if (ruleSet == AsnEncodingRules.DER)
			{
				bytesRead = 0;
				return false;
			}
			return true;
		}
		if (b < 128)
		{
			length = b;
			return true;
		}
		if (b == byte.MaxValue)
		{
			bytesRead = 0;
			return false;
		}
		byte b2 = (byte)(b & -129);
		if (b2 + 1 > source.Length)
		{
			bytesRead = 0;
			return false;
		}
		bool flag = ruleSet == AsnEncodingRules.DER || ruleSet == AsnEncodingRules.CER;
		if (flag && b2 > 4)
		{
			bytesRead = 0;
			return false;
		}
		uint num = 0u;
		for (int i = 0; i < b2; i++)
		{
			byte b3 = source[bytesRead];
			bytesRead++;
			if (num == 0)
			{
				if (flag && b3 == 0)
				{
					bytesRead = 0;
					return false;
				}
				if (!flag && b3 != 0 && b2 - i > 4)
				{
					bytesRead = 0;
					return false;
				}
			}
			num <<= 8;
			num |= b3;
		}
		if (num > int.MaxValue)
		{
			bytesRead = 0;
			return false;
		}
		if (flag && num < 128)
		{
			bytesRead = 0;
			return false;
		}
		length = (int)num;
		return true;
	}

	internal Asn1Tag ReadTagAndLength(out int? contentsLength, out int bytesRead)
	{
		if (TryPeekTag(_data.Span, out var tag, out var bytesRead2) && TryReadLength(_data.Slice(bytesRead2).Span, _ruleSet, out var length, out var bytesRead3))
		{
			int num = bytesRead2 + bytesRead3;
			if (tag.IsConstructed)
			{
				if (_ruleSet == AsnEncodingRules.CER && length.HasValue)
				{
					throw new CryptographicException("ASN1 corrupted data.");
				}
			}
			else if (!length.HasValue)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			bytesRead = num;
			contentsLength = length;
			return tag;
		}
		throw new CryptographicException("ASN1 corrupted data.");
	}

	private static void ValidateEndOfContents(Asn1Tag tag, int? length, int headerLength)
	{
		if (tag.IsConstructed || length != 0 || headerLength != 2)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
	}

	private int SeekEndOfContents(ReadOnlyMemory<byte> source)
	{
		int num = 0;
		AsnReader asnReader = new AsnReader(source, _ruleSet);
		int num2 = 1;
		while (asnReader.HasData)
		{
			int? contentsLength;
			int bytesRead;
			Asn1Tag asn1Tag = asnReader.ReadTagAndLength(out contentsLength, out bytesRead);
			if (asn1Tag == Asn1Tag.EndOfContents)
			{
				ValidateEndOfContents(asn1Tag, contentsLength, bytesRead);
				num2--;
				if (num2 == 0)
				{
					return num;
				}
			}
			if (!contentsLength.HasValue)
			{
				num2++;
				asnReader._data = asnReader._data.Slice(bytesRead);
				num += bytesRead;
			}
			else
			{
				ReadOnlyMemory<byte> readOnlyMemory = Slice(asnReader._data, 0, bytesRead + contentsLength.Value);
				asnReader._data = asnReader._data.Slice(readOnlyMemory.Length);
				num += readOnlyMemory.Length;
			}
		}
		throw new CryptographicException("ASN1 corrupted data.");
	}

	public ReadOnlyMemory<byte> PeekEncodedValue()
	{
		ReadTagAndLength(out var contentsLength, out var bytesRead);
		if (!contentsLength.HasValue)
		{
			int num = SeekEndOfContents(_data.Slice(bytesRead));
			return Slice(_data, 0, bytesRead + num + 2);
		}
		return Slice(_data, 0, bytesRead + contentsLength.Value);
	}

	public ReadOnlyMemory<byte> PeekContentBytes()
	{
		ReadTagAndLength(out var contentsLength, out var bytesRead);
		if (!contentsLength.HasValue)
		{
			return Slice(_data, bytesRead, SeekEndOfContents(_data.Slice(bytesRead)));
		}
		return Slice(_data, bytesRead, contentsLength.Value);
	}

	public ReadOnlyMemory<byte> GetEncodedValue()
	{
		ReadOnlyMemory<byte> result = PeekEncodedValue();
		_data = _data.Slice(result.Length);
		return result;
	}

	private static bool ReadBooleanValue(ReadOnlySpan<byte> source, AsnEncodingRules ruleSet)
	{
		if (source.Length != 1)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		switch (source[0])
		{
		case 0:
			return false;
		default:
			if (ruleSet == AsnEncodingRules.DER || ruleSet == AsnEncodingRules.CER)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			break;
		case byte.MaxValue:
			break;
		}
		return true;
	}

	public bool ReadBoolean()
	{
		return ReadBoolean(Asn1Tag.Boolean);
	}

	public bool ReadBoolean(Asn1Tag expectedTag)
	{
		int? contentsLength;
		int bytesRead;
		Asn1Tag tag = ReadTagAndLength(out contentsLength, out bytesRead);
		CheckExpectedTag(tag, expectedTag, UniversalTagNumber.Boolean);
		if (tag.IsConstructed)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		bool result = ReadBooleanValue(Slice(_data, bytesRead, contentsLength.Value).Span, _ruleSet);
		_data = _data.Slice(bytesRead + contentsLength.Value);
		return result;
	}

	private ReadOnlyMemory<byte> GetIntegerContents(Asn1Tag expectedTag, UniversalTagNumber tagNumber, out int headerLength)
	{
		int? contentsLength;
		Asn1Tag tag = ReadTagAndLength(out contentsLength, out headerLength);
		CheckExpectedTag(tag, expectedTag, tagNumber);
		if (tag.IsConstructed || contentsLength < 1)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		ReadOnlyMemory<byte> result = Slice(_data, headerLength, contentsLength.Value);
		ReadOnlySpan<byte> span = result.Span;
		if (result.Length > 1)
		{
			ushort num = (ushort)((ushort)((span[0] << 8) | span[1]) & 0xFF80);
			if (num == 0 || num == 65408)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
		}
		return result;
	}

	public ReadOnlyMemory<byte> GetIntegerBytes()
	{
		return GetIntegerBytes(Asn1Tag.Integer);
	}

	public ReadOnlyMemory<byte> GetIntegerBytes(Asn1Tag expectedTag)
	{
		int headerLength;
		ReadOnlyMemory<byte> integerContents = GetIntegerContents(expectedTag, UniversalTagNumber.Integer, out headerLength);
		_data = _data.Slice(headerLength + integerContents.Length);
		return integerContents;
	}

	public BigInteger GetInteger()
	{
		return GetInteger(Asn1Tag.Integer);
	}

	public BigInteger GetInteger(Asn1Tag expectedTag)
	{
		int headerLength;
		ReadOnlyMemory<byte> integerContents = GetIntegerContents(expectedTag, UniversalTagNumber.Integer, out headerLength);
		byte[] array = ArrayPool<byte>.Shared.Rent(integerContents.Length);
		BigInteger result;
		try
		{
			byte value = (byte)(((integerContents.Span[0] & 0x80) != 0) ? byte.MaxValue : 0);
			new Span<byte>(array, integerContents.Length, array.Length - integerContents.Length).Fill(value);
			integerContents.CopyTo(array);
			AsnWriter.Reverse(new Span<byte>(array, 0, integerContents.Length));
			result = new BigInteger(array);
		}
		finally
		{
			Array.Clear(array, 0, array.Length);
			ArrayPool<byte>.Shared.Return(array);
		}
		_data = _data.Slice(headerLength + integerContents.Length);
		return result;
	}

	private bool TryReadSignedInteger(int sizeLimit, Asn1Tag expectedTag, UniversalTagNumber tagNumber, out long value)
	{
		int headerLength;
		ReadOnlyMemory<byte> integerContents = GetIntegerContents(expectedTag, tagNumber, out headerLength);
		if (integerContents.Length > sizeLimit)
		{
			value = 0L;
			return false;
		}
		ReadOnlySpan<byte> span = integerContents.Span;
		long num = (((span[0] & 0x80) != 0) ? (-1) : 0);
		for (int i = 0; i < integerContents.Length; i++)
		{
			num <<= 8;
			num |= span[i];
		}
		_data = _data.Slice(headerLength + integerContents.Length);
		value = num;
		return true;
	}

	private bool TryReadUnsignedInteger(int sizeLimit, Asn1Tag expectedTag, UniversalTagNumber tagNumber, out ulong value)
	{
		int headerLength;
		ReadOnlyMemory<byte> integerContents = GetIntegerContents(expectedTag, tagNumber, out headerLength);
		ReadOnlySpan<byte> readOnlySpan = integerContents.Span;
		int length = integerContents.Length;
		if ((readOnlySpan[0] & 0x80) != 0)
		{
			value = 0uL;
			return false;
		}
		if (readOnlySpan.Length > 1 && readOnlySpan[0] == 0)
		{
			readOnlySpan = readOnlySpan.Slice(1);
		}
		if (readOnlySpan.Length > sizeLimit)
		{
			value = 0uL;
			return false;
		}
		ulong num = 0uL;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			num <<= 8;
			num |= readOnlySpan[i];
		}
		_data = _data.Slice(headerLength + length);
		value = num;
		return true;
	}

	public bool TryReadInt32(out int value)
	{
		return TryReadInt32(Asn1Tag.Integer, out value);
	}

	public bool TryReadInt32(Asn1Tag expectedTag, out int value)
	{
		if (TryReadSignedInteger(4, expectedTag, UniversalTagNumber.Integer, out var value2))
		{
			value = (int)value2;
			return true;
		}
		value = 0;
		return false;
	}

	public bool TryReadUInt32(out uint value)
	{
		return TryReadUInt32(Asn1Tag.Integer, out value);
	}

	public bool TryReadUInt32(Asn1Tag expectedTag, out uint value)
	{
		if (TryReadUnsignedInteger(4, expectedTag, UniversalTagNumber.Integer, out var value2))
		{
			value = (uint)value2;
			return true;
		}
		value = 0u;
		return false;
	}

	public bool TryReadInt64(out long value)
	{
		return TryReadInt64(Asn1Tag.Integer, out value);
	}

	public bool TryReadInt64(Asn1Tag expectedTag, out long value)
	{
		return TryReadSignedInteger(8, expectedTag, UniversalTagNumber.Integer, out value);
	}

	public bool TryReadUInt64(out ulong value)
	{
		return TryReadUInt64(Asn1Tag.Integer, out value);
	}

	public bool TryReadUInt64(Asn1Tag expectedTag, out ulong value)
	{
		return TryReadUnsignedInteger(8, expectedTag, UniversalTagNumber.Integer, out value);
	}

	public bool TryReadInt16(out short value)
	{
		return TryReadInt16(Asn1Tag.Integer, out value);
	}

	public bool TryReadInt16(Asn1Tag expectedTag, out short value)
	{
		if (TryReadSignedInteger(2, expectedTag, UniversalTagNumber.Integer, out var value2))
		{
			value = (short)value2;
			return true;
		}
		value = 0;
		return false;
	}

	public bool TryReadUInt16(out ushort value)
	{
		return TryReadUInt16(Asn1Tag.Integer, out value);
	}

	public bool TryReadUInt16(Asn1Tag expectedTag, out ushort value)
	{
		if (TryReadUnsignedInteger(2, expectedTag, UniversalTagNumber.Integer, out var value2))
		{
			value = (ushort)value2;
			return true;
		}
		value = 0;
		return false;
	}

	public bool TryReadInt8(out sbyte value)
	{
		return TryReadInt8(Asn1Tag.Integer, out value);
	}

	public bool TryReadInt8(Asn1Tag expectedTag, out sbyte value)
	{
		if (TryReadSignedInteger(1, expectedTag, UniversalTagNumber.Integer, out var value2))
		{
			value = (sbyte)value2;
			return true;
		}
		value = 0;
		return false;
	}

	public bool TryReadUInt8(out byte value)
	{
		return TryReadUInt8(Asn1Tag.Integer, out value);
	}

	public bool TryReadUInt8(Asn1Tag expectedTag, out byte value)
	{
		if (TryReadUnsignedInteger(1, expectedTag, UniversalTagNumber.Integer, out var value2))
		{
			value = (byte)value2;
			return true;
		}
		value = 0;
		return false;
	}

	private void ParsePrimitiveBitStringContents(ReadOnlyMemory<byte> source, out int unusedBitCount, out ReadOnlyMemory<byte> value, out byte normalizedLastByte)
	{
		if (_ruleSet == AsnEncodingRules.CER && source.Length > 1000)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		if (source.Length == 0)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		ReadOnlySpan<byte> span = source.Span;
		unusedBitCount = span[0];
		if (unusedBitCount > 7)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		if (source.Length == 1)
		{
			if (unusedBitCount > 0)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			value = ReadOnlyMemory<byte>.Empty;
			normalizedLastByte = 0;
			return;
		}
		int num = -1 << unusedBitCount;
		byte b = span[span.Length - 1];
		byte b2 = (byte)(b & num);
		if (b2 != b && (_ruleSet == AsnEncodingRules.DER || _ruleSet == AsnEncodingRules.CER))
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		normalizedLastByte = b2;
		value = source.Slice(1);
	}

	private static void CopyBitStringValue(ReadOnlyMemory<byte> value, byte normalizedLastByte, Span<byte> destination)
	{
		if (value.Length != 0)
		{
			value.Span.CopyTo(destination);
			destination[value.Length - 1] = normalizedLastByte;
		}
	}

	private int CountConstructedBitString(ReadOnlyMemory<byte> source, bool isIndefinite)
	{
		Span<byte> empty = Span<byte>.Empty;
		int lastUnusedBitCount;
		int bytesRead;
		return ProcessConstructedBitString(source, empty, null, isIndefinite, out lastUnusedBitCount, out bytesRead);
	}

	private void CopyConstructedBitString(ReadOnlyMemory<byte> source, Span<byte> destination, bool isIndefinite, out int unusedBitCount, out int bytesRead, out int bytesWritten)
	{
		bytesWritten = ProcessConstructedBitString(source, destination, delegate(ReadOnlyMemory<byte> value, byte lastByte, Span<byte> dest)
		{
			CopyBitStringValue(value, lastByte, dest);
		}, isIndefinite, out unusedBitCount, out bytesRead);
	}

	private int ProcessConstructedBitString(ReadOnlyMemory<byte> source, Span<byte> destination, BitStringCopyAction copyAction, bool isIndefinite, out int lastUnusedBitCount, out int bytesRead)
	{
		lastUnusedBitCount = 0;
		bytesRead = 0;
		int num = 1000;
		AsnReader asnReader = new AsnReader(source, _ruleSet);
		Stack<(AsnReader, bool, int)> stack = null;
		int num2 = 0;
		Asn1Tag asn1Tag = Asn1Tag.ConstructedBitString;
		Span<byte> destination2 = destination;
		do
		{
			IL_01f2:
			if (asnReader.HasData)
			{
				asn1Tag = asnReader.ReadTagAndLength(out var contentsLength, out var bytesRead2);
				if (asn1Tag == Asn1Tag.PrimitiveBitString)
				{
					if (lastUnusedBitCount != 0)
					{
						throw new CryptographicException("ASN1 corrupted data.");
					}
					if (_ruleSet == AsnEncodingRules.CER && num != 1000)
					{
						throw new CryptographicException("ASN1 corrupted data.");
					}
					ReadOnlyMemory<byte> source2 = Slice(asnReader._data, bytesRead2, contentsLength.Value);
					ParsePrimitiveBitStringContents(source2, out lastUnusedBitCount, out var value, out var normalizedLastByte);
					int num3 = bytesRead2 + source2.Length;
					asnReader._data = asnReader._data.Slice(num3);
					bytesRead += num3;
					num2 += value.Length;
					num = source2.Length;
					if (copyAction != null)
					{
						copyAction(value, normalizedLastByte, destination2);
						destination2 = destination2.Slice(value.Length);
					}
					goto IL_01f2;
				}
				if (!(asn1Tag == Asn1Tag.EndOfContents && isIndefinite))
				{
					if (asn1Tag == Asn1Tag.ConstructedBitString)
					{
						if (_ruleSet == AsnEncodingRules.CER)
						{
							throw new CryptographicException("ASN1 corrupted data.");
						}
						if (stack == null)
						{
							stack = new Stack<(AsnReader, bool, int)>();
						}
						stack.Push((asnReader, isIndefinite, bytesRead));
						asnReader = new AsnReader(Slice(asnReader._data, bytesRead2, contentsLength), _ruleSet);
						bytesRead = bytesRead2;
						isIndefinite = !contentsLength.HasValue;
						goto IL_01f2;
					}
					throw new CryptographicException("ASN1 corrupted data.");
				}
				ValidateEndOfContents(asn1Tag, contentsLength, bytesRead2);
				bytesRead += bytesRead2;
				if (stack != null && stack.Count > 0)
				{
					(AsnReader, bool, int) tuple = stack.Pop();
					AsnReader item = tuple.Item1;
					bool item2 = tuple.Item2;
					int item3 = tuple.Item3;
					item._data = item._data.Slice(bytesRead);
					bytesRead += item3;
					isIndefinite = item2;
					asnReader = item;
					goto IL_01f2;
				}
			}
			if (isIndefinite && asn1Tag != Asn1Tag.EndOfContents)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			if (stack != null && stack.Count > 0)
			{
				(AsnReader, bool, int) tuple2 = stack.Pop();
				AsnReader item4 = tuple2.Item1;
				bool item5 = tuple2.Item2;
				int item6 = tuple2.Item3;
				asnReader = item4;
				asnReader._data = asnReader._data.Slice(bytesRead);
				isIndefinite = item5;
				bytesRead += item6;
			}
			else
			{
				asnReader = null;
			}
		}
		while (asnReader != null);
		return num2;
	}

	private bool TryCopyConstructedBitStringValue(ReadOnlyMemory<byte> source, Span<byte> dest, bool isIndefinite, out int unusedBitCount, out int bytesRead, out int bytesWritten)
	{
		int num = CountConstructedBitString(source, isIndefinite);
		if (_ruleSet == AsnEncodingRules.CER && num < 1000)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		if (dest.Length < num)
		{
			unusedBitCount = 0;
			bytesRead = 0;
			bytesWritten = 0;
			return false;
		}
		CopyConstructedBitString(source, dest, isIndefinite, out unusedBitCount, out bytesRead, out bytesWritten);
		return true;
	}

	private bool TryGetPrimitiveBitStringValue(Asn1Tag expectedTag, out Asn1Tag actualTag, out int? contentsLength, out int headerLength, out int unusedBitCount, out ReadOnlyMemory<byte> value, out byte normalizedLastByte)
	{
		actualTag = ReadTagAndLength(out contentsLength, out headerLength);
		CheckExpectedTag(actualTag, expectedTag, UniversalTagNumber.BitString);
		if (actualTag.IsConstructed)
		{
			if (_ruleSet == AsnEncodingRules.DER)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			unusedBitCount = 0;
			value = default(ReadOnlyMemory<byte>);
			normalizedLastByte = 0;
			return false;
		}
		ReadOnlyMemory<byte> source = Slice(_data, headerLength, contentsLength.Value);
		ParsePrimitiveBitStringContents(source, out unusedBitCount, out value, out normalizedLastByte);
		return true;
	}

	public bool TryGetPrimitiveBitStringValue(out int unusedBitCount, out ReadOnlyMemory<byte> contents)
	{
		return TryGetPrimitiveBitStringValue(Asn1Tag.PrimitiveBitString, out unusedBitCount, out contents);
	}

	public bool TryGetPrimitiveBitStringValue(Asn1Tag expectedTag, out int unusedBitCount, out ReadOnlyMemory<byte> value)
	{
		Asn1Tag actualTag;
		int? contentsLength;
		int headerLength;
		byte normalizedLastByte;
		bool flag = TryGetPrimitiveBitStringValue(expectedTag, out actualTag, out contentsLength, out headerLength, out unusedBitCount, out value, out normalizedLastByte);
		if (flag)
		{
			if (value.Length != 0 && normalizedLastByte != value.Span[value.Length - 1])
			{
				unusedBitCount = 0;
				value = default(ReadOnlyMemory<byte>);
				return false;
			}
			_data = _data.Slice(headerLength + value.Length + 1);
		}
		return flag;
	}

	public bool TryCopyBitStringBytes(Span<byte> destination, out int unusedBitCount, out int bytesWritten)
	{
		return TryCopyBitStringBytes(Asn1Tag.PrimitiveBitString, destination, out unusedBitCount, out bytesWritten);
	}

	public bool TryCopyBitStringBytes(Asn1Tag expectedTag, Span<byte> destination, out int unusedBitCount, out int bytesWritten)
	{
		if (TryGetPrimitiveBitStringValue(expectedTag, out var _, out var contentsLength, out var headerLength, out unusedBitCount, out var value, out var normalizedLastByte))
		{
			if (value.Length > destination.Length)
			{
				bytesWritten = 0;
				unusedBitCount = 0;
				return false;
			}
			CopyBitStringValue(value, normalizedLastByte, destination);
			bytesWritten = value.Length;
			_data = _data.Slice(headerLength + value.Length + 1);
			return true;
		}
		int bytesRead;
		bool num = TryCopyConstructedBitStringValue(Slice(_data, headerLength, contentsLength), destination, !contentsLength.HasValue, out unusedBitCount, out bytesRead, out bytesWritten);
		if (num)
		{
			_data = _data.Slice(headerLength + bytesRead);
		}
		return num;
	}

	public TFlagsEnum GetNamedBitListValue<TFlagsEnum>() where TFlagsEnum : struct
	{
		return GetNamedBitListValue<TFlagsEnum>(Asn1Tag.PrimitiveBitString);
	}

	public TFlagsEnum GetNamedBitListValue<TFlagsEnum>(Asn1Tag expectedTag) where TFlagsEnum : struct
	{
		Type typeFromHandle = typeof(TFlagsEnum);
		return (TFlagsEnum)Enum.ToObject(typeFromHandle, GetNamedBitListValue(expectedTag, typeFromHandle));
	}

	public Enum GetNamedBitListValue(Type tFlagsEnum)
	{
		return GetNamedBitListValue(Asn1Tag.PrimitiveBitString, tFlagsEnum);
	}

	public Enum GetNamedBitListValue(Asn1Tag expectedTag, Type tFlagsEnum)
	{
		Type enumUnderlyingType = tFlagsEnum.GetEnumUnderlyingType();
		if (!tFlagsEnum.IsDefined(typeof(FlagsAttribute), inherit: false))
		{
			throw new ArgumentException("Named bit list operations require an enum with the [Flags] attribute.", "tFlagsEnum");
		}
		Span<byte> destination = stackalloc byte[Marshal.SizeOf(enumUnderlyingType)];
		ReadOnlyMemory<byte> data = _data;
		try
		{
			if (!TryCopyBitStringBytes(expectedTag, destination, out var unusedBitCount, out var bytesWritten))
			{
				throw new CryptographicException(global::SR.Format("The encoded named bit list value is larger than the value size of the '{0}' enum.", tFlagsEnum.Name));
			}
			if (bytesWritten == 0)
			{
				return (Enum)Enum.ToObject(tFlagsEnum, 0);
			}
			ReadOnlySpan<byte> valueSpan = destination.Slice(0, bytesWritten);
			if (_ruleSet == AsnEncodingRules.DER || _ruleSet == AsnEncodingRules.CER)
			{
				byte num = valueSpan[bytesWritten - 1];
				byte b = (byte)(1 << unusedBitCount);
				if ((num & b) == 0)
				{
					throw new CryptographicException("ASN1 corrupted data.");
				}
			}
			return (Enum)Enum.ToObject(tFlagsEnum, InterpretNamedBitListReversed(valueSpan));
		}
		catch
		{
			_data = data;
			throw;
		}
	}

	private static long InterpretNamedBitListReversed(ReadOnlySpan<byte> valueSpan)
	{
		long num = 0L;
		long num2 = 1L;
		for (int i = 0; i < valueSpan.Length; i++)
		{
			byte b = valueSpan[i];
			for (int num3 = 7; num3 >= 0; num3--)
			{
				int num4 = 1 << num3;
				if ((b & num4) != 0)
				{
					num |= num2;
				}
				num2 <<= 1;
			}
		}
		return num;
	}

	public ReadOnlyMemory<byte> GetEnumeratedBytes()
	{
		return GetEnumeratedBytes(Asn1Tag.Enumerated);
	}

	public ReadOnlyMemory<byte> GetEnumeratedBytes(Asn1Tag expectedTag)
	{
		int headerLength;
		ReadOnlyMemory<byte> integerContents = GetIntegerContents(expectedTag, UniversalTagNumber.Enumerated, out headerLength);
		_data = _data.Slice(headerLength + integerContents.Length);
		return integerContents;
	}

	public TEnum GetEnumeratedValue<TEnum>() where TEnum : struct
	{
		Type typeFromHandle = typeof(TEnum);
		return (TEnum)Enum.ToObject(typeFromHandle, GetEnumeratedValue(typeFromHandle));
	}

	public TEnum GetEnumeratedValue<TEnum>(Asn1Tag expectedTag) where TEnum : struct
	{
		Type typeFromHandle = typeof(TEnum);
		return (TEnum)Enum.ToObject(typeFromHandle, GetEnumeratedValue(expectedTag, typeFromHandle));
	}

	public Enum GetEnumeratedValue(Type tEnum)
	{
		return GetEnumeratedValue(Asn1Tag.Enumerated, tEnum);
	}

	public Enum GetEnumeratedValue(Asn1Tag expectedTag, Type tEnum)
	{
		Type enumUnderlyingType = tEnum.GetEnumUnderlyingType();
		if (tEnum.IsDefined(typeof(FlagsAttribute), inherit: false))
		{
			throw new ArgumentException("ASN.1 Enumerated values only apply to enum types without the [Flags] attribute.", "tEnum");
		}
		int sizeLimit = Marshal.SizeOf(enumUnderlyingType);
		if (enumUnderlyingType == typeof(int) || enumUnderlyingType == typeof(long) || enumUnderlyingType == typeof(short) || enumUnderlyingType == typeof(sbyte))
		{
			if (!TryReadSignedInteger(sizeLimit, expectedTag, UniversalTagNumber.Enumerated, out var value))
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			return (Enum)Enum.ToObject(tEnum, value);
		}
		if (enumUnderlyingType == typeof(uint) || enumUnderlyingType == typeof(ulong) || enumUnderlyingType == typeof(ushort) || enumUnderlyingType == typeof(byte))
		{
			if (!TryReadUnsignedInteger(sizeLimit, expectedTag, UniversalTagNumber.Enumerated, out var value2))
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			return (Enum)Enum.ToObject(tEnum, value2);
		}
		throw new CryptographicException();
	}

	private bool TryGetPrimitiveOctetStringBytes(Asn1Tag expectedTag, out Asn1Tag actualTag, out int? contentLength, out int headerLength, out ReadOnlyMemory<byte> contents, UniversalTagNumber universalTagNumber = UniversalTagNumber.OctetString)
	{
		actualTag = ReadTagAndLength(out contentLength, out headerLength);
		CheckExpectedTag(actualTag, expectedTag, universalTagNumber);
		if (actualTag.IsConstructed)
		{
			if (_ruleSet == AsnEncodingRules.DER)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			contents = default(ReadOnlyMemory<byte>);
			return false;
		}
		ReadOnlyMemory<byte> readOnlyMemory = Slice(_data, headerLength, contentLength.Value);
		if (_ruleSet == AsnEncodingRules.CER && readOnlyMemory.Length > 1000)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		contents = readOnlyMemory;
		return true;
	}

	private bool TryGetPrimitiveOctetStringBytes(Asn1Tag expectedTag, UniversalTagNumber universalTagNumber, out ReadOnlyMemory<byte> contents)
	{
		if (TryGetPrimitiveOctetStringBytes(expectedTag, out var _, out var _, out var headerLength, out contents, universalTagNumber))
		{
			_data = _data.Slice(headerLength + contents.Length);
			return true;
		}
		return false;
	}

	public bool TryGetPrimitiveOctetStringBytes(out ReadOnlyMemory<byte> contents)
	{
		return TryGetPrimitiveOctetStringBytes(Asn1Tag.PrimitiveOctetString, out contents);
	}

	public bool TryGetPrimitiveOctetStringBytes(Asn1Tag expectedTag, out ReadOnlyMemory<byte> contents)
	{
		return TryGetPrimitiveOctetStringBytes(expectedTag, UniversalTagNumber.OctetString, out contents);
	}

	private int CountConstructedOctetString(ReadOnlyMemory<byte> source, bool isIndefinite)
	{
		int bytesRead;
		int num = CopyConstructedOctetString(source, Span<byte>.Empty, write: false, isIndefinite, out bytesRead);
		if (_ruleSet == AsnEncodingRules.CER && num <= 1000)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		return num;
	}

	private void CopyConstructedOctetString(ReadOnlyMemory<byte> source, Span<byte> destination, bool isIndefinite, out int bytesRead, out int bytesWritten)
	{
		bytesWritten = CopyConstructedOctetString(source, destination, write: true, isIndefinite, out bytesRead);
	}

	private int CopyConstructedOctetString(ReadOnlyMemory<byte> source, Span<byte> destination, bool write, bool isIndefinite, out int bytesRead)
	{
		bytesRead = 0;
		int num = 1000;
		AsnReader asnReader = new AsnReader(source, _ruleSet);
		Stack<(AsnReader, bool, int)> stack = null;
		int num2 = 0;
		Asn1Tag asn1Tag = Asn1Tag.ConstructedBitString;
		Span<byte> destination2 = destination;
		do
		{
			IL_01f2:
			if (asnReader.HasData)
			{
				asn1Tag = asnReader.ReadTagAndLength(out var contentsLength, out var bytesRead2);
				if (asn1Tag == Asn1Tag.PrimitiveOctetString)
				{
					if (_ruleSet == AsnEncodingRules.CER && num != 1000)
					{
						throw new CryptographicException("ASN1 corrupted data.");
					}
					ReadOnlyMemory<byte> readOnlyMemory = Slice(asnReader._data, bytesRead2, contentsLength.Value);
					int num3 = bytesRead2 + readOnlyMemory.Length;
					asnReader._data = asnReader._data.Slice(num3);
					bytesRead += num3;
					num2 += readOnlyMemory.Length;
					num = readOnlyMemory.Length;
					if (_ruleSet == AsnEncodingRules.CER && num > 1000)
					{
						throw new CryptographicException("ASN1 corrupted data.");
					}
					if (write)
					{
						readOnlyMemory.Span.CopyTo(destination2);
						destination2 = destination2.Slice(readOnlyMemory.Length);
					}
					goto IL_01f2;
				}
				if (!(asn1Tag == Asn1Tag.EndOfContents && isIndefinite))
				{
					if (asn1Tag == Asn1Tag.ConstructedOctetString)
					{
						if (_ruleSet == AsnEncodingRules.CER)
						{
							throw new CryptographicException("ASN1 corrupted data.");
						}
						if (stack == null)
						{
							stack = new Stack<(AsnReader, bool, int)>();
						}
						stack.Push((asnReader, isIndefinite, bytesRead));
						asnReader = new AsnReader(Slice(asnReader._data, bytesRead2, contentsLength), _ruleSet);
						bytesRead = bytesRead2;
						isIndefinite = !contentsLength.HasValue;
						goto IL_01f2;
					}
					throw new CryptographicException("ASN1 corrupted data.");
				}
				ValidateEndOfContents(asn1Tag, contentsLength, bytesRead2);
				bytesRead += bytesRead2;
				if (stack != null && stack.Count > 0)
				{
					(AsnReader, bool, int) tuple = stack.Pop();
					AsnReader item = tuple.Item1;
					bool item2 = tuple.Item2;
					int item3 = tuple.Item3;
					item._data = item._data.Slice(bytesRead);
					bytesRead += item3;
					isIndefinite = item2;
					asnReader = item;
					goto IL_01f2;
				}
			}
			if (isIndefinite && asn1Tag != Asn1Tag.EndOfContents)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			if (stack != null && stack.Count > 0)
			{
				(AsnReader, bool, int) tuple2 = stack.Pop();
				AsnReader item4 = tuple2.Item1;
				bool item5 = tuple2.Item2;
				int item6 = tuple2.Item3;
				asnReader = item4;
				asnReader._data = asnReader._data.Slice(bytesRead);
				isIndefinite = item5;
				bytesRead += item6;
			}
			else
			{
				asnReader = null;
			}
		}
		while (asnReader != null);
		return num2;
	}

	private bool TryCopyConstructedOctetStringContents(ReadOnlyMemory<byte> source, Span<byte> dest, bool isIndefinite, out int bytesRead, out int bytesWritten)
	{
		bytesRead = 0;
		int num = CountConstructedOctetString(source, isIndefinite);
		if (dest.Length < num)
		{
			bytesWritten = 0;
			return false;
		}
		CopyConstructedOctetString(source, dest, isIndefinite, out bytesRead, out bytesWritten);
		return true;
	}

	public bool TryCopyOctetStringBytes(Span<byte> destination, out int bytesWritten)
	{
		return TryCopyOctetStringBytes(Asn1Tag.PrimitiveOctetString, destination, out bytesWritten);
	}

	public bool TryCopyOctetStringBytes(Asn1Tag expectedTag, Span<byte> destination, out int bytesWritten)
	{
		if (TryGetPrimitiveOctetStringBytes(expectedTag, out var _, out var contentLength, out var headerLength, out var contents))
		{
			if (contents.Length > destination.Length)
			{
				bytesWritten = 0;
				return false;
			}
			contents.Span.CopyTo(destination);
			bytesWritten = contents.Length;
			_data = _data.Slice(headerLength + contents.Length);
			return true;
		}
		int bytesRead;
		bool num = TryCopyConstructedOctetStringContents(Slice(_data, headerLength, contentLength), destination, !contentLength.HasValue, out bytesRead, out bytesWritten);
		if (num)
		{
			_data = _data.Slice(headerLength + bytesRead);
		}
		return num;
	}

	public void ReadNull()
	{
		ReadNull(Asn1Tag.Null);
	}

	public void ReadNull(Asn1Tag expectedTag)
	{
		int? contentsLength;
		int bytesRead;
		Asn1Tag tag = ReadTagAndLength(out contentsLength, out bytesRead);
		CheckExpectedTag(tag, expectedTag, UniversalTagNumber.Null);
		if (tag.IsConstructed || contentsLength != 0)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		_data = _data.Slice(bytesRead);
	}

	private static void ReadSubIdentifier(ReadOnlySpan<byte> source, out int bytesRead, out long? smallValue, out BigInteger? largeValue)
	{
		if (source[0] == 128)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		int num = -1;
		int i;
		for (i = 0; i < source.Length; i++)
		{
			if ((source[i] & 0x80) == 0)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		bytesRead = num + 1;
		long num2 = 0L;
		if (bytesRead <= 9)
		{
			for (i = 0; i < bytesRead; i++)
			{
				byte b = source[i];
				num2 <<= 7;
				num2 |= (byte)(b & 0x7F);
			}
			largeValue = null;
			smallValue = num2;
			return;
		}
		int minimumLength = (bytesRead / 8 + 1) * 7;
		byte[] array = ArrayPool<byte>.Shared.Rent(minimumLength);
		Array.Clear(array, 0, array.Length);
		Span<byte> destination = array;
		Span<byte> destination2 = stackalloc byte[8];
		int num3 = bytesRead;
		i = bytesRead - 8;
		while (num3 > 0)
		{
			byte b2 = source[i];
			num2 <<= 7;
			num2 |= (byte)(b2 & 0x7F);
			i++;
			if (i >= num3)
			{
				BinaryPrimitives.WriteInt64LittleEndian(destination2, num2);
				destination2.Slice(0, 7).CopyTo(destination);
				destination = destination.Slice(7);
				num2 = 0L;
				num3 -= 8;
				i = Math.Max(0, num3 - 8);
			}
		}
		int length = array.Length - destination.Length;
		largeValue = new BigInteger(array);
		smallValue = null;
		Array.Clear(array, 0, length);
		ArrayPool<byte>.Shared.Return(array);
	}

	private string ReadObjectIdentifierAsString(Asn1Tag expectedTag, out int totalBytesRead)
	{
		int? contentsLength;
		int bytesRead;
		Asn1Tag tag = ReadTagAndLength(out contentsLength, out bytesRead);
		CheckExpectedTag(tag, expectedTag, UniversalTagNumber.ObjectIdentifier);
		if (tag.IsConstructed || contentsLength < 1)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		ReadOnlySpan<byte> source = Slice(_data, bytesRead, contentsLength.Value).Span;
		StringBuilder stringBuilder = new StringBuilder((byte)source.Length * 4);
		ReadSubIdentifier(source, out var bytesRead2, out var smallValue, out var largeValue);
		if (smallValue.HasValue)
		{
			long num = smallValue.Value;
			byte value;
			if (num < 40)
			{
				value = 0;
			}
			else if (num < 80)
			{
				value = 1;
				num -= 40;
			}
			else
			{
				value = 2;
				num -= 80;
			}
			stringBuilder.Append(value);
			stringBuilder.Append('.');
			stringBuilder.Append(num);
		}
		else
		{
			BigInteger value2 = largeValue.Value;
			byte value = 2;
			value2 -= (BigInteger)80;
			stringBuilder.Append(value);
			stringBuilder.Append('.');
			stringBuilder.Append(value2.ToString());
		}
		source = source.Slice(bytesRead2);
		while (!source.IsEmpty)
		{
			ReadSubIdentifier(source, out bytesRead2, out smallValue, out largeValue);
			stringBuilder.Append('.');
			if (smallValue.HasValue)
			{
				stringBuilder.Append(smallValue.Value);
			}
			else
			{
				stringBuilder.Append(largeValue.Value.ToString());
			}
			source = source.Slice(bytesRead2);
		}
		totalBytesRead = bytesRead + contentsLength.Value;
		return stringBuilder.ToString();
	}

	public string ReadObjectIdentifierAsString()
	{
		return ReadObjectIdentifierAsString(Asn1Tag.ObjectIdentifier);
	}

	public string ReadObjectIdentifierAsString(Asn1Tag expectedTag)
	{
		int totalBytesRead;
		string result = ReadObjectIdentifierAsString(expectedTag, out totalBytesRead);
		_data = _data.Slice(totalBytesRead);
		return result;
	}

	public Oid ReadObjectIdentifier(bool skipFriendlyName = false)
	{
		return ReadObjectIdentifier(Asn1Tag.ObjectIdentifier, skipFriendlyName);
	}

	public Oid ReadObjectIdentifier(Asn1Tag expectedTag, bool skipFriendlyName = false)
	{
		int totalBytesRead;
		string text = ReadObjectIdentifierAsString(expectedTag, out totalBytesRead);
		Oid result = (skipFriendlyName ? new Oid(text, text) : new Oid(text));
		_data = _data.Slice(totalBytesRead);
		return result;
	}

	private bool TryCopyCharacterStringBytes(Asn1Tag expectedTag, UniversalTagNumber universalTagNumber, Span<byte> destination, out int bytesRead, out int bytesWritten)
	{
		if (TryGetPrimitiveOctetStringBytes(expectedTag, out var _, out var contentLength, out var headerLength, out var contents, universalTagNumber))
		{
			bytesWritten = contents.Length;
			if (destination.Length < bytesWritten)
			{
				bytesWritten = 0;
				bytesRead = 0;
				return false;
			}
			contents.Span.CopyTo(destination);
			bytesRead = headerLength + bytesWritten;
			return true;
		}
		int bytesRead2;
		bool num = TryCopyConstructedOctetStringContents(Slice(_data, headerLength, contentLength), destination, !contentLength.HasValue, out bytesRead2, out bytesWritten);
		if (num)
		{
			bytesRead = headerLength + bytesRead2;
			return num;
		}
		bytesRead = 0;
		return num;
	}

	private unsafe static bool TryCopyCharacterString(ReadOnlySpan<byte> source, Span<char> destination, Encoding encoding, out int charsWritten)
	{
		if (source.Length == 0)
		{
			charsWritten = 0;
			return true;
		}
		fixed (byte* reference = &MemoryMarshal.GetReference(source))
		{
			fixed (char* reference2 = &MemoryMarshal.GetReference(destination))
			{
				try
				{
					if (encoding.GetCharCount(reference, source.Length) > destination.Length)
					{
						charsWritten = 0;
						return false;
					}
					charsWritten = encoding.GetChars(reference, source.Length, reference2, destination.Length);
				}
				catch (DecoderFallbackException inner)
				{
					throw new CryptographicException("ASN1 corrupted data.", inner);
				}
				return true;
			}
		}
	}

	private unsafe string GetCharacterString(Asn1Tag expectedTag, UniversalTagNumber universalTagNumber, Encoding encoding)
	{
		byte[] rented = null;
		int bytesRead;
		ReadOnlySpan<byte> octetStringContents = GetOctetStringContents(expectedTag, universalTagNumber, out bytesRead, ref rented);
		try
		{
			string result;
			if (octetStringContents.Length == 0)
			{
				result = string.Empty;
			}
			else
			{
				fixed (byte* reference = &MemoryMarshal.GetReference(octetStringContents))
				{
					try
					{
						result = encoding.GetString(reference, octetStringContents.Length);
					}
					catch (DecoderFallbackException inner)
					{
						throw new CryptographicException("ASN1 corrupted data.", inner);
					}
				}
			}
			_data = _data.Slice(bytesRead);
			return result;
		}
		finally
		{
			if (rented != null)
			{
				Array.Clear(rented, 0, octetStringContents.Length);
				ArrayPool<byte>.Shared.Return(rented);
			}
		}
	}

	private bool TryCopyCharacterString(Asn1Tag expectedTag, UniversalTagNumber universalTagNumber, Encoding encoding, Span<char> destination, out int charsWritten)
	{
		byte[] rented = null;
		int bytesRead;
		ReadOnlySpan<byte> octetStringContents = GetOctetStringContents(expectedTag, universalTagNumber, out bytesRead, ref rented);
		try
		{
			bool num = TryCopyCharacterString(octetStringContents, destination, encoding, out charsWritten);
			if (num)
			{
				_data = _data.Slice(bytesRead);
			}
			return num;
		}
		finally
		{
			if (rented != null)
			{
				Array.Clear(rented, 0, octetStringContents.Length);
				ArrayPool<byte>.Shared.Return(rented);
			}
		}
	}

	public bool TryGetPrimitiveCharacterStringBytes(UniversalTagNumber encodingType, out ReadOnlyMemory<byte> contents)
	{
		return TryGetPrimitiveCharacterStringBytes(new Asn1Tag(encodingType), encodingType, out contents);
	}

	public bool TryGetPrimitiveCharacterStringBytes(Asn1Tag expectedTag, UniversalTagNumber encodingType, out ReadOnlyMemory<byte> contents)
	{
		CheckCharacterStringEncodingType(encodingType);
		return TryGetPrimitiveOctetStringBytes(expectedTag, encodingType, out contents);
	}

	public bool TryCopyCharacterStringBytes(UniversalTagNumber encodingType, Span<byte> destination, out int bytesWritten)
	{
		return TryCopyCharacterStringBytes(new Asn1Tag(encodingType), encodingType, destination, out bytesWritten);
	}

	public bool TryCopyCharacterStringBytes(Asn1Tag expectedTag, UniversalTagNumber encodingType, Span<byte> destination, out int bytesWritten)
	{
		CheckCharacterStringEncodingType(encodingType);
		int bytesRead;
		bool num = TryCopyCharacterStringBytes(expectedTag, encodingType, destination, out bytesRead, out bytesWritten);
		if (num)
		{
			_data = _data.Slice(bytesRead);
		}
		return num;
	}

	public bool TryCopyCharacterString(UniversalTagNumber encodingType, Span<char> destination, out int charsWritten)
	{
		return TryCopyCharacterString(new Asn1Tag(encodingType), encodingType, destination, out charsWritten);
	}

	public bool TryCopyCharacterString(Asn1Tag expectedTag, UniversalTagNumber encodingType, Span<char> destination, out int charsWritten)
	{
		Encoding encoding = AsnCharacterStringEncodings.GetEncoding(encodingType);
		return TryCopyCharacterString(expectedTag, encodingType, encoding, destination, out charsWritten);
	}

	public string GetCharacterString(UniversalTagNumber encodingType)
	{
		return GetCharacterString(new Asn1Tag(encodingType), encodingType);
	}

	public string GetCharacterString(Asn1Tag expectedTag, UniversalTagNumber encodingType)
	{
		Encoding encoding = AsnCharacterStringEncodings.GetEncoding(encodingType);
		return GetCharacterString(expectedTag, encodingType, encoding);
	}

	public AsnReader ReadSequence()
	{
		return ReadSequence(Asn1Tag.Sequence);
	}

	public AsnReader ReadSequence(Asn1Tag expectedTag)
	{
		int? contentsLength;
		int bytesRead;
		Asn1Tag tag = ReadTagAndLength(out contentsLength, out bytesRead);
		CheckExpectedTag(tag, expectedTag, UniversalTagNumber.Sequence);
		if (!tag.IsConstructed)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		int num = 0;
		if (!contentsLength.HasValue)
		{
			contentsLength = SeekEndOfContents(_data.Slice(bytesRead));
			num = 2;
		}
		ReadOnlyMemory<byte> data = Slice(_data, bytesRead, contentsLength.Value);
		_data = _data.Slice(bytesRead + data.Length + num);
		return new AsnReader(data, _ruleSet);
	}

	public AsnReader ReadSetOf(bool skipSortOrderValidation = false)
	{
		return ReadSetOf(Asn1Tag.SetOf, skipSortOrderValidation);
	}

	public AsnReader ReadSetOf(Asn1Tag expectedTag, bool skipSortOrderValidation = false)
	{
		int? contentsLength;
		int bytesRead;
		Asn1Tag tag = ReadTagAndLength(out contentsLength, out bytesRead);
		CheckExpectedTag(tag, expectedTag, UniversalTagNumber.Set);
		if (!tag.IsConstructed)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		int num = 0;
		if (!contentsLength.HasValue)
		{
			contentsLength = SeekEndOfContents(_data.Slice(bytesRead));
			num = 2;
		}
		ReadOnlyMemory<byte> data = Slice(_data, bytesRead, contentsLength.Value);
		if (!skipSortOrderValidation && (_ruleSet == AsnEncodingRules.DER || _ruleSet == AsnEncodingRules.CER))
		{
			AsnReader asnReader = new AsnReader(data, _ruleSet);
			ReadOnlyMemory<byte> readOnlyMemory = ReadOnlyMemory<byte>.Empty;
			SetOfValueComparer instance = SetOfValueComparer.Instance;
			while (asnReader.HasData)
			{
				ReadOnlyMemory<byte> y = readOnlyMemory;
				readOnlyMemory = asnReader.GetEncodedValue();
				if (instance.Compare(readOnlyMemory, y) < 0)
				{
					throw new CryptographicException("ASN1 corrupted data.");
				}
			}
		}
		_data = _data.Slice(bytesRead + data.Length + num);
		return new AsnReader(data, _ruleSet);
	}

	private static int ParseNonNegativeIntAndSlice(ref ReadOnlySpan<byte> data, int bytesToRead)
	{
		int result = ParseNonNegativeInt(Slice(data, 0, bytesToRead));
		data = data.Slice(bytesToRead);
		return result;
	}

	private static int ParseNonNegativeInt(ReadOnlySpan<byte> data)
	{
		if (Utf8Parser.TryParse(data, out uint value, out int bytesConsumed, '\0') && value <= int.MaxValue && bytesConsumed == data.Length)
		{
			return (int)value;
		}
		throw new CryptographicException("ASN1 corrupted data.");
	}

	private DateTimeOffset ParseUtcTime(ReadOnlySpan<byte> contentOctets, int twoDigitYearMax)
	{
		if ((_ruleSet == AsnEncodingRules.DER || _ruleSet == AsnEncodingRules.CER) && contentOctets.Length != 13)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		if (contentOctets.Length < 11 || contentOctets.Length > 17 || (contentOctets.Length & 1) != 1)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		ReadOnlySpan<byte> data = contentOctets;
		int num = ParseNonNegativeIntAndSlice(ref data, 2);
		int month = ParseNonNegativeIntAndSlice(ref data, 2);
		int day = ParseNonNegativeIntAndSlice(ref data, 2);
		int hour = ParseNonNegativeIntAndSlice(ref data, 2);
		int minute = ParseNonNegativeIntAndSlice(ref data, 2);
		int second = 0;
		int hours = 0;
		int num2 = 0;
		bool flag = false;
		if (contentOctets.Length == 17 || contentOctets.Length == 13)
		{
			second = ParseNonNegativeIntAndSlice(ref data, 2);
		}
		if (contentOctets.Length == 11 || contentOctets.Length == 13)
		{
			if (data[0] != 90)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
		}
		else
		{
			if (data[0] == 45)
			{
				flag = true;
			}
			else if (data[0] != 43)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			data = data.Slice(1);
			hours = ParseNonNegativeIntAndSlice(ref data, 2);
			num2 = ParseNonNegativeIntAndSlice(ref data, 2);
		}
		if (num2 > 59)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		TimeSpan timeSpan = new TimeSpan(hours, num2, 0);
		if (flag)
		{
			timeSpan = -timeSpan;
		}
		int num3 = twoDigitYearMax / 100;
		if (num > twoDigitYearMax % 100)
		{
			num3--;
		}
		int year = num3 * 100 + num;
		try
		{
			return new DateTimeOffset(year, month, day, hour, minute, second, timeSpan);
		}
		catch (Exception inner)
		{
			throw new CryptographicException("ASN1 corrupted data.", inner);
		}
	}

	public DateTimeOffset GetUtcTime(int twoDigitYearMax = 2049)
	{
		return GetUtcTime(Asn1Tag.UtcTime, twoDigitYearMax);
	}

	public DateTimeOffset GetUtcTime(Asn1Tag expectedTag, int twoDigitYearMax = 2049)
	{
		byte[] rented = null;
		Span<byte> tmpSpace = stackalloc byte[17];
		int bytesRead;
		ReadOnlySpan<byte> octetStringContents = GetOctetStringContents(expectedTag, UniversalTagNumber.UtcTime, out bytesRead, ref rented, tmpSpace);
		DateTimeOffset result = ParseUtcTime(octetStringContents, twoDigitYearMax);
		if (rented != null)
		{
			Array.Clear(rented, 0, octetStringContents.Length);
			ArrayPool<byte>.Shared.Return(rented);
		}
		_data = _data.Slice(bytesRead);
		return result;
	}

	private static byte? ParseGeneralizedTime_GetNextState(byte octet)
	{
		switch (octet)
		{
		case 43:
		case 45:
		case 90:
			return 2;
		case 44:
		case 46:
			return 1;
		default:
			return null;
		}
	}

	private static DateTimeOffset ParseGeneralizedTime(AsnEncodingRules ruleSet, ReadOnlySpan<byte> contentOctets, bool disallowFractions)
	{
		bool flag = ruleSet == AsnEncodingRules.DER || ruleSet == AsnEncodingRules.CER;
		if (flag && contentOctets.Length < 15)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		if (contentOctets.Length < 10)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		ReadOnlySpan<byte> data = contentOctets;
		int year = ParseNonNegativeIntAndSlice(ref data, 4);
		int month = ParseNonNegativeIntAndSlice(ref data, 2);
		int day = ParseNonNegativeIntAndSlice(ref data, 2);
		int hour = ParseNonNegativeIntAndSlice(ref data, 2);
		int? num = null;
		int? num2 = null;
		ulong value = 0uL;
		ulong num3 = 1uL;
		byte b = byte.MaxValue;
		TimeSpan? timeSpan = null;
		bool flag2 = false;
		byte b2 = 0;
		while (b2 == 0 && data.Length != 0)
		{
			byte? b3 = ParseGeneralizedTime_GetNextState(data[0]);
			if (!b3.HasValue)
			{
				if (!num.HasValue)
				{
					num = ParseNonNegativeIntAndSlice(ref data, 2);
					continue;
				}
				if (num2.HasValue)
				{
					throw new CryptographicException("ASN1 corrupted data.");
				}
				num2 = ParseNonNegativeIntAndSlice(ref data, 2);
			}
			else
			{
				b2 = b3.Value;
			}
		}
		if (b2 == 1)
		{
			if (disallowFractions)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			switch (data[0])
			{
			case 44:
				if (flag)
				{
					throw new CryptographicException("ASN1 corrupted data.");
				}
				break;
			default:
				throw new CryptographicException();
			case 46:
				break;
			}
			data = data.Slice(1);
			if (data.IsEmpty)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			if (!Utf8Parser.TryParse(SliceAtMost(data, 12), out value, out int bytesConsumed, '\0') || bytesConsumed == 0)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			b = (byte)(value % 10);
			for (int i = 0; i < bytesConsumed; i++)
			{
				num3 *= 10;
			}
			data = data.Slice(bytesConsumed);
			uint value2;
			while (Utf8Parser.TryParse(SliceAtMost(data, 9), out value2, out bytesConsumed, '\0'))
			{
				data = data.Slice(bytesConsumed);
				b = (byte)(value2 % 10);
			}
			if (data.Length != 0)
			{
				byte? b4 = ParseGeneralizedTime_GetNextState(data[0]);
				if (!b4.HasValue)
				{
					throw new CryptographicException("ASN1 corrupted data.");
				}
				b2 = b4.Value;
			}
		}
		if (b2 == 2)
		{
			byte b5 = data[0];
			data = data.Slice(1);
			if (b5 == 90)
			{
				timeSpan = TimeSpan.Zero;
				flag2 = true;
			}
			else
			{
				bool flag3 = b5 switch
				{
					43 => false, 
					45 => true, 
					_ => throw new CryptographicException("ASN1 corrupted data."), 
				};
				if (data.IsEmpty)
				{
					throw new CryptographicException("ASN1 corrupted data.");
				}
				int hours = ParseNonNegativeIntAndSlice(ref data, 2);
				int num4 = 0;
				if (data.Length != 0)
				{
					num4 = ParseNonNegativeIntAndSlice(ref data, 2);
				}
				if (num4 > 59)
				{
					throw new CryptographicException("ASN1 corrupted data.");
				}
				TimeSpan timeSpan2 = new TimeSpan(hours, num4, 0);
				if (flag3)
				{
					timeSpan2 = -timeSpan2;
				}
				timeSpan = timeSpan2;
			}
		}
		if (!data.IsEmpty)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		if (flag)
		{
			if (!flag2 || !num2.HasValue)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			if (b == 0)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
		}
		double num5 = (double)value / (double)num3;
		TimeSpan timeSpan3 = TimeSpan.Zero;
		if (!num.HasValue)
		{
			num = 0;
			num2 = 0;
			if (value != 0L)
			{
				timeSpan3 = new TimeSpan((long)(num5 * 36000000000.0));
			}
		}
		else if (!num2.HasValue)
		{
			num2 = 0;
			if (value != 0L)
			{
				timeSpan3 = new TimeSpan((long)(num5 * 600000000.0));
			}
		}
		else if (value != 0L)
		{
			timeSpan3 = new TimeSpan((long)(num5 * 10000000.0));
		}
		try
		{
			DateTimeOffset dateTimeOffset = (timeSpan.HasValue ? new DateTimeOffset(year, month, day, hour, num.Value, num2.Value, timeSpan.Value) : new DateTimeOffset(new DateTime(year, month, day, hour, num.Value, num2.Value)));
			return dateTimeOffset + timeSpan3;
		}
		catch (Exception inner)
		{
			throw new CryptographicException("ASN1 corrupted data.", inner);
		}
	}

	public DateTimeOffset GetGeneralizedTime(bool disallowFractions = false)
	{
		return GetGeneralizedTime(Asn1Tag.GeneralizedTime, disallowFractions);
	}

	public DateTimeOffset GetGeneralizedTime(Asn1Tag expectedTag, bool disallowFractions = false)
	{
		byte[] rented = null;
		int bytesRead;
		ReadOnlySpan<byte> octetStringContents = GetOctetStringContents(expectedTag, UniversalTagNumber.GeneralizedTime, out bytesRead, ref rented);
		DateTimeOffset result = ParseGeneralizedTime(_ruleSet, octetStringContents, disallowFractions);
		if (rented != null)
		{
			Array.Clear(rented, 0, octetStringContents.Length);
			ArrayPool<byte>.Shared.Return(rented);
		}
		_data = _data.Slice(bytesRead);
		return result;
	}

	private ReadOnlySpan<byte> GetOctetStringContents(Asn1Tag expectedTag, UniversalTagNumber universalTagNumber, out int bytesRead, ref byte[] rented, Span<byte> tmpSpace = default(Span<byte>))
	{
		if (TryGetPrimitiveOctetStringBytes(expectedTag, out var _, out var contentLength, out var headerLength, out var contents, universalTagNumber))
		{
			bytesRead = headerLength + contents.Length;
			return contents.Span;
		}
		ReadOnlyMemory<byte> source = Slice(_data, headerLength, contentLength);
		bool isIndefinite = !contentLength.HasValue;
		int num = CountConstructedOctetString(source, isIndefinite);
		if (tmpSpace.Length < num)
		{
			rented = ArrayPool<byte>.Shared.Rent(num);
			tmpSpace = rented;
		}
		CopyConstructedOctetString(source, tmpSpace, isIndefinite, out var bytesRead2, out var bytesWritten);
		bytesRead = headerLength + bytesRead2;
		return tmpSpace.Slice(0, bytesWritten);
	}

	private static ReadOnlySpan<byte> SliceAtMost(ReadOnlySpan<byte> source, int longestPermitted)
	{
		return source[..Math.Min(longestPermitted, source.Length)];
	}

	private static ReadOnlySpan<byte> Slice(ReadOnlySpan<byte> source, int offset, int length)
	{
		if (length < 0 || source.Length - offset < length)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		return source.Slice(offset, length);
	}

	private static ReadOnlyMemory<byte> Slice(ReadOnlyMemory<byte> source, int offset, int? length)
	{
		if (!length.HasValue)
		{
			return source.Slice(offset);
		}
		int value = length.Value;
		if (value < 0 || source.Length - offset < value)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		return source.Slice(offset, value);
	}

	private static void CheckEncodingRules(AsnEncodingRules ruleSet)
	{
		if (ruleSet != AsnEncodingRules.BER && ruleSet != AsnEncodingRules.CER && ruleSet != AsnEncodingRules.DER)
		{
			throw new ArgumentOutOfRangeException("ruleSet");
		}
	}

	private static void CheckExpectedTag(Asn1Tag tag, Asn1Tag expectedTag, UniversalTagNumber tagNumber)
	{
		if (expectedTag.TagClass == TagClass.Universal && expectedTag.TagValue != (int)tagNumber)
		{
			throw new ArgumentException("Tags with TagClass Universal must have the appropriate TagValue value for the data type being read or written.", "expectedTag");
		}
		if (expectedTag.TagClass != tag.TagClass || expectedTag.TagValue != tag.TagValue)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
	}

	private static void CheckCharacterStringEncodingType(UniversalTagNumber encodingType)
	{
		switch (encodingType)
		{
		case UniversalTagNumber.UTF8String:
		case UniversalTagNumber.NumericString:
		case UniversalTagNumber.PrintableString:
		case UniversalTagNumber.TeletexString:
		case UniversalTagNumber.VideotexString:
		case UniversalTagNumber.IA5String:
		case UniversalTagNumber.GraphicString:
		case UniversalTagNumber.VisibleString:
		case UniversalTagNumber.GeneralString:
		case UniversalTagNumber.UniversalString:
		case UniversalTagNumber.BMPString:
			return;
		}
		throw new ArgumentOutOfRangeException("encodingType");
	}
}
