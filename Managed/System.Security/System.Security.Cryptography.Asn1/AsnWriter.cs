using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Cryptography.Asn1;

internal sealed class AsnWriter : IDisposable
{
	private class ArrayIndexSetOfValueComparer : IComparer<(int, int)>
	{
		private readonly byte[] _data;

		public ArrayIndexSetOfValueComparer(byte[] data)
		{
			_data = data;
		}

		public int Compare((int, int) x, (int, int) y)
		{
			int item = x.Item1;
			int item2 = x.Item2;
			int item3 = y.Item1;
			int item4 = y.Item2;
			int num = SetOfValueComparer.Instance.Compare(new ReadOnlyMemory<byte>(_data, item, item2), new ReadOnlyMemory<byte>(_data, item3, item4));
			if (num == 0)
			{
				return item - item3;
			}
			return num;
		}
	}

	private byte[] _buffer;

	private int _offset;

	private Stack<(Asn1Tag, int)> _nestingStack;

	public AsnEncodingRules RuleSet { get; }

	public AsnWriter(AsnEncodingRules ruleSet)
	{
		if (ruleSet != AsnEncodingRules.BER && ruleSet != AsnEncodingRules.CER && ruleSet != AsnEncodingRules.DER)
		{
			throw new ArgumentOutOfRangeException("ruleSet");
		}
		RuleSet = ruleSet;
	}

	public void Dispose()
	{
		_nestingStack = null;
		if (_buffer != null)
		{
			Array.Clear(_buffer, 0, _offset);
			ArrayPool<byte>.Shared.Return(_buffer);
			_buffer = null;
			_offset = 0;
		}
	}

	private void EnsureWriteCapacity(int pendingCount)
	{
		if (pendingCount < 0)
		{
			throw new OverflowException();
		}
		if (_buffer == null || _buffer.Length - _offset < pendingCount)
		{
			int num = checked(_offset + pendingCount + 1023) / 1024;
			byte[] array = ArrayPool<byte>.Shared.Rent(1024 * num);
			if (_buffer != null)
			{
				Buffer.BlockCopy(_buffer, 0, array, 0, _offset);
				Array.Clear(_buffer, 0, _offset);
				ArrayPool<byte>.Shared.Return(_buffer);
			}
			_buffer = array;
		}
	}

	private void WriteTag(Asn1Tag tag)
	{
		int num = tag.CalculateEncodedSize();
		EnsureWriteCapacity(num);
		if (!tag.TryWrite(_buffer.AsSpan(_offset, num), out var bytesWritten) || bytesWritten != num)
		{
			throw new CryptographicException();
		}
		_offset += num;
	}

	private void WriteLength(int length)
	{
		if (length == -1)
		{
			EnsureWriteCapacity(1);
			_buffer[_offset] = 128;
			_offset++;
			return;
		}
		if (length < 128)
		{
			EnsureWriteCapacity(1 + length);
			_buffer[_offset] = (byte)length;
			_offset++;
			return;
		}
		int encodedLengthSubsequentByteCount = GetEncodedLengthSubsequentByteCount(length);
		EnsureWriteCapacity(encodedLengthSubsequentByteCount + 1 + length);
		_buffer[_offset] = (byte)(0x80 | encodedLengthSubsequentByteCount);
		int num = _offset + encodedLengthSubsequentByteCount;
		int num2 = length;
		do
		{
			_buffer[num] = (byte)num2;
			num2 >>= 8;
			num--;
		}
		while (num2 > 0);
		_offset += encodedLengthSubsequentByteCount + 1;
	}

	private static int GetEncodedLengthSubsequentByteCount(int length)
	{
		if (length <= 127)
		{
			return 0;
		}
		if (length <= 255)
		{
			return 1;
		}
		if (length <= 65535)
		{
			return 2;
		}
		if (length <= 16777215)
		{
			return 3;
		}
		return 4;
	}

	public void WriteEncodedValue(ReadOnlyMemory<byte> preEncodedValue)
	{
		AsnReader asnReader = new AsnReader(preEncodedValue, RuleSet);
		asnReader.GetEncodedValue();
		if (asnReader.HasData)
		{
			throw new ArgumentException("The input to WriteEncodedValue must represent a single encoded value with no trailing data.", "preEncodedValue");
		}
		EnsureWriteCapacity(preEncodedValue.Length);
		preEncodedValue.Span.CopyTo(_buffer.AsSpan(_offset));
		_offset += preEncodedValue.Length;
	}

	private void WriteEndOfContents()
	{
		EnsureWriteCapacity(2);
		_buffer[_offset++] = 0;
		_buffer[_offset++] = 0;
	}

	public void WriteBoolean(bool value)
	{
		WriteBooleanCore(Asn1Tag.Boolean, value);
	}

	public void WriteBoolean(Asn1Tag tag, bool value)
	{
		CheckUniversalTag(tag, UniversalTagNumber.Boolean);
		WriteBooleanCore(tag.AsPrimitive(), value);
	}

	private void WriteBooleanCore(Asn1Tag tag, bool value)
	{
		WriteTag(tag);
		WriteLength(1);
		_buffer[_offset] = (byte)(value ? 255u : 0u);
		_offset++;
	}

	public void WriteInteger(long value)
	{
		WriteIntegerCore(Asn1Tag.Integer, value);
	}

	public void WriteInteger(ulong value)
	{
		WriteNonNegativeIntegerCore(Asn1Tag.Integer, value);
	}

	public void WriteInteger(BigInteger value)
	{
		WriteIntegerCore(Asn1Tag.Integer, value);
	}

	public void WriteInteger(ReadOnlySpan<byte> value)
	{
		WriteIntegerCore(Asn1Tag.Integer, value);
	}

	public void WriteInteger(Asn1Tag tag, long value)
	{
		CheckUniversalTag(tag, UniversalTagNumber.Integer);
		WriteIntegerCore(tag.AsPrimitive(), value);
	}

	private void WriteIntegerCore(Asn1Tag tag, long value)
	{
		if (value >= 0)
		{
			WriteNonNegativeIntegerCore(tag, (ulong)value);
			return;
		}
		int num = ((value >= -128) ? 1 : ((value >= -32768) ? 2 : ((value >= -8388608) ? 3 : ((value >= int.MinValue) ? 4 : ((value >= -549755813888L) ? 5 : ((value >= -140737488355328L) ? 6 : ((value < -36028797018963968L) ? 8 : 7)))))));
		WriteTag(tag);
		WriteLength(num);
		long num2 = value;
		int num3 = _offset + num - 1;
		do
		{
			_buffer[num3] = (byte)num2;
			num2 >>= 8;
			num3--;
		}
		while (num3 >= _offset);
		_offset += num;
	}

	public void WriteInteger(Asn1Tag tag, ulong value)
	{
		CheckUniversalTag(tag, UniversalTagNumber.Integer);
		WriteNonNegativeIntegerCore(tag.AsPrimitive(), value);
	}

	private void WriteNonNegativeIntegerCore(Asn1Tag tag, ulong value)
	{
		int num = ((value < 128) ? 1 : ((value < 32768) ? 2 : ((value < 8388608) ? 3 : ((value < 2147483648u) ? 4 : ((value < 549755813888L) ? 5 : ((value < 140737488355328L) ? 6 : ((value < 36028797018963968L) ? 7 : ((value >= 9223372036854775808uL) ? 9 : 8))))))));
		WriteTag(tag);
		WriteLength(num);
		ulong num2 = value;
		int num3 = _offset + num - 1;
		do
		{
			_buffer[num3] = (byte)num2;
			num2 >>= 8;
			num3--;
		}
		while (num3 >= _offset);
		_offset += num;
	}

	public void WriteInteger(Asn1Tag tag, BigInteger value)
	{
		CheckUniversalTag(tag, UniversalTagNumber.Integer);
		WriteIntegerCore(tag.AsPrimitive(), value);
	}

	public void WriteInteger(Asn1Tag tag, ReadOnlySpan<byte> value)
	{
		CheckUniversalTag(tag, UniversalTagNumber.Integer);
		WriteIntegerCore(tag.AsPrimitive(), value);
	}

	private void WriteIntegerCore(Asn1Tag tag, ReadOnlySpan<byte> value)
	{
		if (value.IsEmpty)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		if (value.Length > 1)
		{
			ushort num = (ushort)((ushort)((value[0] << 8) | value[1]) & 0xFF80);
			if (num == 0 || num == 65408)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
		}
		WriteTag(tag);
		WriteLength(value.Length);
		value.CopyTo(_buffer.AsSpan(_offset));
		_offset += value.Length;
	}

	private void WriteIntegerCore(Asn1Tag tag, BigInteger value)
	{
		byte[] array = value.ToByteArray();
		Array.Reverse(array);
		WriteTag(tag);
		WriteLength(array.Length);
		Buffer.BlockCopy(array, 0, _buffer, _offset, array.Length);
		_offset += array.Length;
	}

	public void WriteBitString(ReadOnlySpan<byte> bitString, int unusedBitCount = 0)
	{
		WriteBitStringCore(Asn1Tag.PrimitiveBitString, bitString, unusedBitCount);
	}

	public void WriteBitString(Asn1Tag tag, ReadOnlySpan<byte> bitString, int unusedBitCount = 0)
	{
		CheckUniversalTag(tag, UniversalTagNumber.BitString);
		WriteBitStringCore(tag, bitString, unusedBitCount);
	}

	private void WriteBitStringCore(Asn1Tag tag, ReadOnlySpan<byte> bitString, int unusedBitCount)
	{
		if (unusedBitCount < 0 || unusedBitCount > 7)
		{
			throw new ArgumentOutOfRangeException("unusedBitCount", unusedBitCount, "Unused bit count must be between 0 and 7, inclusive.");
		}
		if (bitString.Length == 0 && unusedBitCount != 0)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		int num = (1 << unusedBitCount) - 1;
		if ((((!bitString.IsEmpty) ? bitString[bitString.Length - 1] : 0) & num) != 0)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		if (RuleSet == AsnEncodingRules.CER && bitString.Length >= 1000)
		{
			WriteConstructedCerBitString(tag, bitString, unusedBitCount);
			return;
		}
		WriteTag(tag.AsPrimitive());
		WriteLength(bitString.Length + 1);
		_buffer[_offset] = (byte)unusedBitCount;
		_offset++;
		bitString.CopyTo(_buffer.AsSpan(_offset));
		_offset += bitString.Length;
	}

	private void WriteConstructedCerBitString(Asn1Tag tag, ReadOnlySpan<byte> payload, int unusedBitCount)
	{
		WriteTag(tag.AsConstructed());
		WriteLength(-1);
		int result;
		int num = Math.DivRem(payload.Length, 999, out result);
		int num2 = ((result != 0) ? (3 + result + GetEncodedLengthSubsequentByteCount(result)) : 0);
		int pendingCount = num * 1004 + num2 + 2;
		EnsureWriteCapacity(pendingCount);
		_ = _buffer;
		_ = _offset;
		ReadOnlySpan<byte> readOnlySpan = payload;
		Asn1Tag primitiveBitString = Asn1Tag.PrimitiveBitString;
		Span<byte> destination;
		while (readOnlySpan.Length > 999)
		{
			WriteTag(primitiveBitString);
			WriteLength(1000);
			_buffer[_offset] = 0;
			_offset++;
			destination = _buffer.AsSpan(_offset);
			readOnlySpan.Slice(0, 999).CopyTo(destination);
			readOnlySpan = readOnlySpan.Slice(999);
			_offset += 999;
		}
		WriteTag(primitiveBitString);
		WriteLength(readOnlySpan.Length + 1);
		_buffer[_offset] = (byte)unusedBitCount;
		_offset++;
		destination = _buffer.AsSpan(_offset);
		readOnlySpan.CopyTo(destination);
		_offset += readOnlySpan.Length;
		WriteEndOfContents();
	}

	public void WriteNamedBitList(object enumValue)
	{
		if (enumValue == null)
		{
			throw new ArgumentNullException("enumValue");
		}
		WriteNamedBitList(Asn1Tag.PrimitiveBitString, enumValue);
	}

	public void WriteNamedBitList<TEnum>(TEnum enumValue) where TEnum : struct
	{
		WriteNamedBitList(Asn1Tag.PrimitiveBitString, enumValue);
	}

	public void WriteNamedBitList(Asn1Tag tag, object enumValue)
	{
		if (enumValue == null)
		{
			throw new ArgumentNullException("enumValue");
		}
		WriteNamedBitList(tag, enumValue.GetType(), enumValue);
	}

	public void WriteNamedBitList<TEnum>(Asn1Tag tag, TEnum enumValue) where TEnum : struct
	{
		WriteNamedBitList(tag, typeof(TEnum), enumValue);
	}

	private void WriteNamedBitList(Asn1Tag tag, Type tEnum, object enumValue)
	{
		Type enumUnderlyingType = tEnum.GetEnumUnderlyingType();
		if (!tEnum.IsDefined(typeof(FlagsAttribute), inherit: false))
		{
			throw new ArgumentException("Named bit list operations require an enum with the [Flags] attribute.", "tEnum");
		}
		ulong integralValue = ((!(enumUnderlyingType == typeof(ulong))) ? ((ulong)Convert.ToInt64(enumValue)) : Convert.ToUInt64(enumValue));
		WriteNamedBitList(tag, integralValue);
	}

	private void WriteNamedBitList(Asn1Tag tag, ulong integralValue)
	{
		Span<byte> span = stackalloc byte[8];
		span.Clear();
		int num = -1;
		int num2 = 0;
		while (integralValue != 0L)
		{
			if ((integralValue & 1) != 0L)
			{
				span[num2 / 8] |= (byte)(128 >> num2 % 8);
				num = num2;
			}
			integralValue >>= 1;
			num2++;
		}
		if (num < 0)
		{
			WriteBitString(tag, ReadOnlySpan<byte>.Empty);
			return;
		}
		int length = num / 8 + 1;
		int unusedBitCount = 7 - num % 8;
		WriteBitString(tag, span.Slice(0, length), unusedBitCount);
	}

	public void WriteOctetString(ReadOnlySpan<byte> octetString)
	{
		WriteOctetString(Asn1Tag.PrimitiveOctetString, octetString);
	}

	public void WriteOctetString(Asn1Tag tag, ReadOnlySpan<byte> octetString)
	{
		CheckUniversalTag(tag, UniversalTagNumber.OctetString);
		WriteOctetStringCore(tag, octetString);
	}

	private void WriteOctetStringCore(Asn1Tag tag, ReadOnlySpan<byte> octetString)
	{
		if (RuleSet == AsnEncodingRules.CER && octetString.Length > 1000)
		{
			WriteConstructedCerOctetString(tag, octetString);
			return;
		}
		WriteTag(tag.AsPrimitive());
		WriteLength(octetString.Length);
		octetString.CopyTo(_buffer.AsSpan(_offset));
		_offset += octetString.Length;
	}

	private void WriteConstructedCerOctetString(Asn1Tag tag, ReadOnlySpan<byte> payload)
	{
		WriteTag(tag.AsConstructed());
		WriteLength(-1);
		int result;
		int num = Math.DivRem(payload.Length, 1000, out result);
		int num2 = ((result != 0) ? (2 + result + GetEncodedLengthSubsequentByteCount(result)) : 0);
		int pendingCount = num * 1004 + num2 + 2;
		EnsureWriteCapacity(pendingCount);
		_ = _buffer;
		_ = _offset;
		ReadOnlySpan<byte> readOnlySpan = payload;
		Asn1Tag primitiveOctetString = Asn1Tag.PrimitiveOctetString;
		Span<byte> destination;
		while (readOnlySpan.Length > 1000)
		{
			WriteTag(primitiveOctetString);
			WriteLength(1000);
			destination = _buffer.AsSpan(_offset);
			readOnlySpan.Slice(0, 1000).CopyTo(destination);
			_offset += 1000;
			readOnlySpan = readOnlySpan.Slice(1000);
		}
		WriteTag(primitiveOctetString);
		WriteLength(readOnlySpan.Length);
		destination = _buffer.AsSpan(_offset);
		readOnlySpan.CopyTo(destination);
		_offset += readOnlySpan.Length;
		WriteEndOfContents();
	}

	public void WriteNull()
	{
		WriteNullCore(Asn1Tag.Null);
	}

	public void WriteNull(Asn1Tag tag)
	{
		CheckUniversalTag(tag, UniversalTagNumber.Null);
		WriteNullCore(tag.AsPrimitive());
	}

	private void WriteNullCore(Asn1Tag tag)
	{
		WriteTag(tag);
		WriteLength(0);
	}

	public void WriteObjectIdentifier(Oid oid)
	{
		if (oid == null)
		{
			throw new ArgumentNullException("oid");
		}
		WriteObjectIdentifier(oid.Value);
	}

	public void WriteObjectIdentifier(string oidValue)
	{
		if (oidValue == null)
		{
			throw new ArgumentNullException("oidValue");
		}
		WriteObjectIdentifier(oidValue.AsSpan());
	}

	public void WriteObjectIdentifier(ReadOnlySpan<char> oidValue)
	{
		WriteObjectIdentifierCore(Asn1Tag.ObjectIdentifier, oidValue);
	}

	public void WriteObjectIdentifier(Asn1Tag tag, Oid oid)
	{
		if (oid == null)
		{
			throw new ArgumentNullException("oid");
		}
		WriteObjectIdentifier(tag, oid.Value);
	}

	public void WriteObjectIdentifier(Asn1Tag tag, string oidValue)
	{
		if (oidValue == null)
		{
			throw new ArgumentNullException("oidValue");
		}
		WriteObjectIdentifier(tag, oidValue.AsSpan());
	}

	public void WriteObjectIdentifier(Asn1Tag tag, ReadOnlySpan<char> oidValue)
	{
		CheckUniversalTag(tag, UniversalTagNumber.ObjectIdentifier);
		WriteObjectIdentifierCore(tag.AsPrimitive(), oidValue);
	}

	private void WriteObjectIdentifierCore(Asn1Tag tag, ReadOnlySpan<char> oidValue)
	{
		if (oidValue.Length < 3)
		{
			throw new CryptographicException("The OID value was invalid.");
		}
		if (oidValue[1] != '.')
		{
			throw new CryptographicException("The OID value was invalid.");
		}
		byte[] array = ArrayPool<byte>.Shared.Rent(oidValue.Length / 2);
		int num = 0;
		try
		{
			int num2 = oidValue[0] switch
			{
				'0' => 0, 
				'1' => 1, 
				'2' => 2, 
				_ => throw new CryptographicException("The OID value was invalid."), 
			};
			ReadOnlySpan<char> oidValue2 = oidValue.Slice(2);
			BigInteger subIdentifier = ParseSubIdentifier(ref oidValue2);
			subIdentifier += (BigInteger)(40 * num2);
			int num3 = EncodeSubIdentifier(array.AsSpan(num), ref subIdentifier);
			num += num3;
			while (!oidValue2.IsEmpty)
			{
				subIdentifier = ParseSubIdentifier(ref oidValue2);
				num3 = EncodeSubIdentifier(array.AsSpan(num), ref subIdentifier);
				num += num3;
			}
			WriteTag(tag);
			WriteLength(num);
			Buffer.BlockCopy(array, 0, _buffer, _offset, num);
			_offset += num;
		}
		finally
		{
			Array.Clear(array, 0, num);
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	private static BigInteger ParseSubIdentifier(ref ReadOnlySpan<char> oidValue)
	{
		int num = oidValue.IndexOf('.');
		if (num == -1)
		{
			num = oidValue.Length;
		}
		else if (num == oidValue.Length - 1)
		{
			throw new CryptographicException("The OID value was invalid.");
		}
		BigInteger zero = BigInteger.Zero;
		for (int i = 0; i < num; i++)
		{
			if (i > 0 && zero == 0L)
			{
				throw new CryptographicException("The OID value was invalid.");
			}
			zero *= (BigInteger)10;
			zero += (BigInteger)AtoI(oidValue[i]);
		}
		oidValue = oidValue.Slice(Math.Min(oidValue.Length, num + 1));
		return zero;
	}

	private static int AtoI(char c)
	{
		if (c >= '0' && c <= '9')
		{
			return c - 48;
		}
		throw new CryptographicException("The OID value was invalid.");
	}

	private static int EncodeSubIdentifier(Span<byte> dest, ref BigInteger subIdentifier)
	{
		if (subIdentifier.IsZero)
		{
			dest[0] = 0;
			return 1;
		}
		BigInteger bigInteger = subIdentifier;
		int num = 0;
		do
		{
			byte b = (byte)(bigInteger & 127);
			if (subIdentifier != bigInteger)
			{
				b |= 0x80;
			}
			bigInteger >>= 7;
			dest[num] = b;
			num++;
		}
		while (bigInteger != BigInteger.Zero);
		Reverse(dest.Slice(0, num));
		return num;
	}

	public void WriteEnumeratedValue(object enumValue)
	{
		if (enumValue == null)
		{
			throw new ArgumentNullException("enumValue");
		}
		WriteEnumeratedValue(Asn1Tag.Enumerated, enumValue);
	}

	public void WriteEnumeratedValue<TEnum>(TEnum value) where TEnum : struct
	{
		WriteEnumeratedValue(Asn1Tag.Enumerated, value);
	}

	public void WriteEnumeratedValue(Asn1Tag tag, object enumValue)
	{
		if (enumValue == null)
		{
			throw new ArgumentNullException("enumValue");
		}
		WriteEnumeratedValue(tag.AsPrimitive(), enumValue.GetType(), enumValue);
	}

	public void WriteEnumeratedValue<TEnum>(Asn1Tag tag, TEnum value) where TEnum : struct
	{
		WriteEnumeratedValue(tag.AsPrimitive(), typeof(TEnum), value);
	}

	private void WriteEnumeratedValue(Asn1Tag tag, Type tEnum, object enumValue)
	{
		CheckUniversalTag(tag, UniversalTagNumber.Enumerated);
		Type enumUnderlyingType = tEnum.GetEnumUnderlyingType();
		if (tEnum.IsDefined(typeof(FlagsAttribute), inherit: false))
		{
			throw new ArgumentException("ASN.1 Enumerated values only apply to enum types without the [Flags] attribute.", "tEnum");
		}
		if (enumUnderlyingType == typeof(ulong))
		{
			ulong value = Convert.ToUInt64(enumValue);
			WriteNonNegativeIntegerCore(tag, value);
		}
		else
		{
			long value2 = Convert.ToInt64(enumValue);
			WriteIntegerCore(tag, value2);
		}
	}

	public void PushSequence()
	{
		PushSequenceCore(Asn1Tag.Sequence);
	}

	public void PushSequence(Asn1Tag tag)
	{
		CheckUniversalTag(tag, UniversalTagNumber.Sequence);
		PushSequenceCore(tag.AsConstructed());
	}

	private void PushSequenceCore(Asn1Tag tag)
	{
		PushTag(tag.AsConstructed());
	}

	public void PopSequence()
	{
		PopSequence(Asn1Tag.Sequence);
	}

	public void PopSequence(Asn1Tag tag)
	{
		CheckUniversalTag(tag, UniversalTagNumber.Sequence);
		PopSequenceCore(tag.AsConstructed());
	}

	private void PopSequenceCore(Asn1Tag tag)
	{
		PopTag(tag);
	}

	public void PushSetOf()
	{
		PushSetOf(Asn1Tag.SetOf);
	}

	public void PushSetOf(Asn1Tag tag)
	{
		CheckUniversalTag(tag, UniversalTagNumber.Set);
		PushSetOfCore(tag.AsConstructed());
	}

	private void PushSetOfCore(Asn1Tag tag)
	{
		PushTag(tag);
	}

	public void PopSetOf()
	{
		PopSetOfCore(Asn1Tag.SetOf);
	}

	public void PopSetOf(Asn1Tag tag)
	{
		CheckUniversalTag(tag, UniversalTagNumber.Set);
		PopSetOfCore(tag.AsConstructed());
	}

	private void PopSetOfCore(Asn1Tag tag)
	{
		bool sortContents = RuleSet == AsnEncodingRules.CER || RuleSet == AsnEncodingRules.DER;
		PopTag(tag, sortContents);
	}

	public void WriteUtcTime(DateTimeOffset value)
	{
		WriteUtcTimeCore(Asn1Tag.UtcTime, value);
	}

	public void WriteUtcTime(Asn1Tag tag, DateTimeOffset value)
	{
		CheckUniversalTag(tag, UniversalTagNumber.UtcTime);
		WriteUtcTimeCore(tag.AsPrimitive(), value);
	}

	public void WriteUtcTime(DateTimeOffset value, int minLegalYear)
	{
		if (minLegalYear <= value.Year && value.Year < minLegalYear + 100)
		{
			WriteUtcTime(value);
			return;
		}
		throw new ArgumentOutOfRangeException("value");
	}

	private void WriteUtcTimeCore(Asn1Tag tag, DateTimeOffset value)
	{
		WriteTag(tag);
		WriteLength(13);
		DateTimeOffset dateTimeOffset = value.ToUniversalTime();
		int year = dateTimeOffset.Year;
		int month = dateTimeOffset.Month;
		int day = dateTimeOffset.Day;
		int hour = dateTimeOffset.Hour;
		int minute = dateTimeOffset.Minute;
		int second = dateTimeOffset.Second;
		Span<byte> span = _buffer.AsSpan(_offset);
		StandardFormat format = new StandardFormat('D', 2);
		if (!Utf8Formatter.TryFormat(year % 100, span.Slice(0, 2), out var bytesWritten, format) || !Utf8Formatter.TryFormat(month, span.Slice(2, 2), out bytesWritten, format) || !Utf8Formatter.TryFormat(day, span.Slice(4, 2), out bytesWritten, format) || !Utf8Formatter.TryFormat(hour, span.Slice(6, 2), out bytesWritten, format) || !Utf8Formatter.TryFormat(minute, span.Slice(8, 2), out bytesWritten, format) || !Utf8Formatter.TryFormat(second, span.Slice(10, 2), out bytesWritten, format))
		{
			throw new CryptographicException();
		}
		_buffer[_offset + 12] = 90;
		_offset += 13;
	}

	public void WriteGeneralizedTime(DateTimeOffset value, bool omitFractionalSeconds = false)
	{
		WriteGeneralizedTimeCore(Asn1Tag.GeneralizedTime, value, omitFractionalSeconds);
	}

	public void WriteGeneralizedTime(Asn1Tag tag, DateTimeOffset value, bool omitFractionalSeconds = false)
	{
		CheckUniversalTag(tag, UniversalTagNumber.GeneralizedTime);
		WriteGeneralizedTimeCore(tag.AsPrimitive(), value, omitFractionalSeconds);
	}

	private void WriteGeneralizedTimeCore(Asn1Tag tag, DateTimeOffset value, bool omitFractionalSeconds)
	{
		DateTimeOffset dateTimeOffset = value.ToUniversalTime();
		if (dateTimeOffset.Year > 9999)
		{
			throw new ArgumentOutOfRangeException("value");
		}
		Span<byte> destination = default(Span<byte>);
		if (!omitFractionalSeconds)
		{
			long num = dateTimeOffset.Ticks % 10000000;
			if (num != 0L)
			{
				destination = stackalloc byte[9];
				if (!Utf8Formatter.TryFormat((decimal)num / 10000000m, destination, out var bytesWritten, new StandardFormat('G')))
				{
					throw new CryptographicException();
				}
				destination = destination.Slice(1, bytesWritten - 1);
			}
		}
		int length = 15 + destination.Length;
		WriteTag(tag);
		WriteLength(length);
		int year = dateTimeOffset.Year;
		int month = dateTimeOffset.Month;
		int day = dateTimeOffset.Day;
		int hour = dateTimeOffset.Hour;
		int minute = dateTimeOffset.Minute;
		int second = dateTimeOffset.Second;
		Span<byte> span = _buffer.AsSpan(_offset);
		StandardFormat format = new StandardFormat('D', 4);
		StandardFormat format2 = new StandardFormat('D', 2);
		if (!Utf8Formatter.TryFormat(year, span.Slice(0, 4), out var bytesWritten2, format) || !Utf8Formatter.TryFormat(month, span.Slice(4, 2), out bytesWritten2, format2) || !Utf8Formatter.TryFormat(day, span.Slice(6, 2), out bytesWritten2, format2) || !Utf8Formatter.TryFormat(hour, span.Slice(8, 2), out bytesWritten2, format2) || !Utf8Formatter.TryFormat(minute, span.Slice(10, 2), out bytesWritten2, format2) || !Utf8Formatter.TryFormat(second, span.Slice(12, 2), out bytesWritten2, format2))
		{
			throw new CryptographicException();
		}
		_offset += 14;
		destination.CopyTo(span.Slice(14));
		_offset += destination.Length;
		_buffer[_offset] = 90;
		_offset++;
	}

	public bool TryEncode(Span<byte> dest, out int bytesWritten)
	{
		Stack<(Asn1Tag, int)> nestingStack = _nestingStack;
		if (nestingStack != null && nestingStack.Count != 0)
		{
			throw new InvalidOperationException("Encode cannot be called while a Sequence or SetOf is still open.");
		}
		if (dest.Length < _offset)
		{
			bytesWritten = 0;
			return false;
		}
		if (_offset == 0)
		{
			bytesWritten = 0;
			return true;
		}
		bytesWritten = _offset;
		_buffer.AsSpan(0, _offset).CopyTo(dest);
		return true;
	}

	public byte[] Encode()
	{
		Stack<(Asn1Tag, int)> nestingStack = _nestingStack;
		if (nestingStack != null && nestingStack.Count != 0)
		{
			throw new InvalidOperationException("Encode cannot be called while a Sequence or SetOf is still open.");
		}
		if (_offset == 0)
		{
			return Array.Empty<byte>();
		}
		return _buffer.AsSpan(0, _offset).ToArray();
	}

	public ReadOnlySpan<byte> EncodeAsSpan()
	{
		Stack<(Asn1Tag, int)> nestingStack = _nestingStack;
		if (nestingStack != null && nestingStack.Count != 0)
		{
			throw new InvalidOperationException("Encode cannot be called while a Sequence or SetOf is still open.");
		}
		if (_offset == 0)
		{
			return ReadOnlySpan<byte>.Empty;
		}
		return new ReadOnlySpan<byte>(_buffer, 0, _offset);
	}

	private void PushTag(Asn1Tag tag)
	{
		if (_nestingStack == null)
		{
			_nestingStack = new Stack<(Asn1Tag, int)>();
		}
		WriteTag(tag);
		_nestingStack.Push((tag, _offset));
		WriteLength(-1);
	}

	private void PopTag(Asn1Tag tag, bool sortContents = false)
	{
		if (_nestingStack == null || _nestingStack.Count == 0)
		{
			throw new ArgumentException("Cannot pop the requested tag as it is not currently in progress.", "tag");
		}
		var (asn1Tag, num) = _nestingStack.Peek();
		if (asn1Tag != tag)
		{
			throw new ArgumentException("Cannot pop the requested tag as it is not currently in progress.", "tag");
		}
		_nestingStack.Pop();
		if (sortContents)
		{
			SortContents(_buffer, num + 1, _offset);
		}
		if (RuleSet == AsnEncodingRules.CER)
		{
			WriteEndOfContents();
			return;
		}
		int num2 = _offset - 1 - num;
		int encodedLengthSubsequentByteCount = GetEncodedLengthSubsequentByteCount(num2);
		if (encodedLengthSubsequentByteCount == 0)
		{
			_buffer[num] = (byte)num2;
			return;
		}
		EnsureWriteCapacity(encodedLengthSubsequentByteCount);
		int num3 = num + 1;
		Buffer.BlockCopy(_buffer, num3, _buffer, num3 + encodedLengthSubsequentByteCount, num2);
		int offset = _offset;
		_offset = num;
		WriteLength(num2);
		_offset = offset + encodedLengthSubsequentByteCount;
	}

	public void WriteCharacterString(UniversalTagNumber encodingType, string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		WriteCharacterString(encodingType, str.AsSpan());
	}

	public void WriteCharacterString(UniversalTagNumber encodingType, ReadOnlySpan<char> str)
	{
		Encoding encoding = AsnCharacterStringEncodings.GetEncoding(encodingType);
		WriteCharacterStringCore(new Asn1Tag(encodingType), encoding, str);
	}

	public void WriteCharacterString(Asn1Tag tag, UniversalTagNumber encodingType, string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		WriteCharacterString(tag, encodingType, str.AsSpan());
	}

	public void WriteCharacterString(Asn1Tag tag, UniversalTagNumber encodingType, ReadOnlySpan<char> str)
	{
		CheckUniversalTag(tag, encodingType);
		Encoding encoding = AsnCharacterStringEncodings.GetEncoding(encodingType);
		WriteCharacterStringCore(tag, encoding, str);
	}

	private unsafe void WriteCharacterStringCore(Asn1Tag tag, Encoding encoding, ReadOnlySpan<char> str)
	{
		int num = -1;
		if (RuleSet == AsnEncodingRules.CER)
		{
			fixed (char* reference = &MemoryMarshal.GetReference(str))
			{
				num = encoding.GetByteCount(reference, str.Length);
				if (num > 1000)
				{
					WriteConstructedCerCharacterString(tag, encoding, str, num);
					return;
				}
			}
		}
		fixed (char* reference2 = &MemoryMarshal.GetReference(str))
		{
			if (num < 0)
			{
				num = encoding.GetByteCount(reference2, str.Length);
			}
			WriteTag(tag.AsPrimitive());
			WriteLength(num);
			Span<byte> span = _buffer.AsSpan(_offset, num);
			fixed (byte* reference3 = &MemoryMarshal.GetReference(span))
			{
				if (encoding.GetBytes(reference2, str.Length, reference3, span.Length) != num)
				{
					throw new InvalidOperationException();
				}
			}
			_offset += num;
		}
	}

	private unsafe void WriteConstructedCerCharacterString(Asn1Tag tag, Encoding encoding, ReadOnlySpan<char> str, int size)
	{
		byte[] array;
		fixed (char* reference = &MemoryMarshal.GetReference(str))
		{
			array = ArrayPool<byte>.Shared.Rent(size);
			fixed (byte* bytes = array)
			{
				if (encoding.GetBytes(reference, str.Length, bytes, array.Length) != size)
				{
					throw new InvalidOperationException();
				}
			}
		}
		WriteConstructedCerOctetString(tag, array.AsSpan(0, size));
		Array.Clear(array, 0, size);
		ArrayPool<byte>.Shared.Return(array);
	}

	private static void SortContents(byte[] buffer, int start, int end)
	{
		int num = end - start;
		if (num == 0)
		{
			return;
		}
		AsnReader asnReader = new AsnReader(new ReadOnlyMemory<byte>(buffer, start, num), AsnEncodingRules.BER);
		List<(int, int)> list = new List<(int, int)>();
		int num2 = start;
		while (asnReader.HasData)
		{
			ReadOnlyMemory<byte> encodedValue = asnReader.GetEncodedValue();
			list.Add((num2, encodedValue.Length));
			num2 += encodedValue.Length;
		}
		ArrayIndexSetOfValueComparer comparer = new ArrayIndexSetOfValueComparer(buffer);
		list.Sort(comparer);
		byte[] array = ArrayPool<byte>.Shared.Rent(num);
		num2 = 0;
		foreach (var (srcOffset, num3) in list)
		{
			Buffer.BlockCopy(buffer, srcOffset, array, num2, num3);
			num2 += num3;
		}
		Buffer.BlockCopy(array, 0, buffer, start, num);
		Array.Clear(array, 0, num);
		ArrayPool<byte>.Shared.Return(array);
	}

	internal static void Reverse(Span<byte> span)
	{
		int num = 0;
		int num2 = span.Length - 1;
		while (num < num2)
		{
			byte b = span[num];
			span[num] = span[num2];
			span[num2] = b;
			num++;
			num2--;
		}
	}

	private static void CheckUniversalTag(Asn1Tag tag, UniversalTagNumber universalTagNumber)
	{
		if (tag.TagClass == TagClass.Universal && tag.TagValue != (int)universalTagNumber)
		{
			throw new ArgumentException("Tags with TagClass Universal must have the appropriate TagValue value for the data type being read or written.", "tag");
		}
	}
}
