namespace System.Security.Cryptography.Asn1;

internal struct Asn1Tag : IEquatable<Asn1Tag>
{
	private const byte ClassMask = 192;

	private const byte ConstructedMask = 32;

	private const byte ControlMask = 224;

	private const byte TagNumberMask = 31;

	internal static readonly Asn1Tag EndOfContents = new Asn1Tag((byte)0, 0);

	internal static readonly Asn1Tag Boolean = new Asn1Tag((byte)0, 1);

	internal static readonly Asn1Tag Integer = new Asn1Tag((byte)0, 2);

	internal static readonly Asn1Tag PrimitiveBitString = new Asn1Tag((byte)0, 3);

	internal static readonly Asn1Tag ConstructedBitString = new Asn1Tag(32, 3);

	internal static readonly Asn1Tag PrimitiveOctetString = new Asn1Tag((byte)0, 4);

	internal static readonly Asn1Tag ConstructedOctetString = new Asn1Tag(32, 4);

	internal static readonly Asn1Tag Null = new Asn1Tag((byte)0, 5);

	internal static readonly Asn1Tag ObjectIdentifier = new Asn1Tag((byte)0, 6);

	internal static readonly Asn1Tag Enumerated = new Asn1Tag((byte)0, 10);

	internal static readonly Asn1Tag Sequence = new Asn1Tag(32, 16);

	internal static readonly Asn1Tag SetOf = new Asn1Tag(32, 17);

	internal static readonly Asn1Tag UtcTime = new Asn1Tag((byte)0, 23);

	internal static readonly Asn1Tag GeneralizedTime = new Asn1Tag((byte)0, 24);

	private readonly byte _controlFlags;

	private readonly int _tagValue;

	public TagClass TagClass => (TagClass)(_controlFlags & 0xC0);

	public bool IsConstructed => (_controlFlags & 0x20) != 0;

	public int TagValue => _tagValue;

	private Asn1Tag(byte controlFlags, int tagValue)
	{
		_controlFlags = (byte)(controlFlags & 0xE0);
		_tagValue = tagValue;
	}

	public Asn1Tag(UniversalTagNumber universalTagNumber, bool isConstructed = false)
		: this((byte)(isConstructed ? 32 : 0), (int)universalTagNumber)
	{
		if (universalTagNumber < UniversalTagNumber.EndOfContents || universalTagNumber > UniversalTagNumber.RelativeObjectIdentifierIRI || universalTagNumber == (UniversalTagNumber)15)
		{
			throw new ArgumentOutOfRangeException("universalTagNumber");
		}
	}

	public Asn1Tag(TagClass tagClass, int tagValue, bool isConstructed = false)
		: this((byte)((uint)tagClass | (uint)(isConstructed ? 32 : 0)), tagValue)
	{
		if ((int)tagClass < 0 || (int)tagClass > 192)
		{
			throw new ArgumentOutOfRangeException("tagClass");
		}
		if (tagValue < 0)
		{
			throw new ArgumentOutOfRangeException("tagValue");
		}
	}

	public Asn1Tag AsConstructed()
	{
		return new Asn1Tag((byte)(_controlFlags | 0x20), _tagValue);
	}

	public Asn1Tag AsPrimitive()
	{
		return new Asn1Tag((byte)(_controlFlags & -33), _tagValue);
	}

	public static bool TryParse(ReadOnlySpan<byte> source, out Asn1Tag tag, out int bytesRead)
	{
		tag = default(Asn1Tag);
		bytesRead = 0;
		if (source.IsEmpty)
		{
			return false;
		}
		byte b = source[bytesRead];
		bytesRead++;
		uint num = (uint)(b & 0x1F);
		if (num == 31)
		{
			num = 0u;
			byte b2;
			do
			{
				if (source.Length <= bytesRead)
				{
					bytesRead = 0;
					return false;
				}
				b2 = source[bytesRead];
				byte b3 = (byte)(b2 & 0x7F);
				bytesRead++;
				if (num >= 33554432)
				{
					bytesRead = 0;
					return false;
				}
				num <<= 7;
				num |= b3;
				if (num == 0)
				{
					bytesRead = 0;
					return false;
				}
			}
			while ((b2 & 0x80) == 128);
			if (num <= 30)
			{
				bytesRead = 0;
				return false;
			}
			if (num > int.MaxValue)
			{
				bytesRead = 0;
				return false;
			}
		}
		tag = new Asn1Tag(b, (int)num);
		return true;
	}

	public int CalculateEncodedSize()
	{
		if (TagValue < 31)
		{
			return 1;
		}
		if (TagValue <= 127)
		{
			return 2;
		}
		if (TagValue <= 16383)
		{
			return 3;
		}
		if (TagValue <= 2097151)
		{
			return 4;
		}
		if (TagValue <= 268435455)
		{
			return 5;
		}
		return 6;
	}

	public bool TryWrite(Span<byte> destination, out int bytesWritten)
	{
		int num = CalculateEncodedSize();
		if (destination.Length < num)
		{
			bytesWritten = 0;
			return false;
		}
		if (num == 1)
		{
			byte b = (byte)(_controlFlags | TagValue);
			destination[0] = b;
			bytesWritten = 1;
			return true;
		}
		byte b2 = (byte)(_controlFlags | 0x1F);
		destination[0] = b2;
		int num2 = TagValue;
		int num3 = num - 1;
		while (num2 > 0)
		{
			int num4 = num2 & 0x7F;
			if (num2 != TagValue)
			{
				num4 |= 0x80;
			}
			destination[num3] = (byte)num4;
			num2 >>= 7;
			num3--;
		}
		bytesWritten = num;
		return true;
	}

	public bool Equals(Asn1Tag other)
	{
		if (_controlFlags == other._controlFlags)
		{
			return _tagValue == other._tagValue;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is Asn1Tag)
		{
			return Equals((Asn1Tag)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (_controlFlags << 24) ^ _tagValue;
	}

	public static bool operator ==(Asn1Tag left, Asn1Tag right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Asn1Tag left, Asn1Tag right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		string text = ((TagClass != TagClass.Universal) ? (TagClass.ToString() + "-" + TagValue) : ((UniversalTagNumber)TagValue/*cast due to .constrained prefix*/).ToString());
		if (IsConstructed)
		{
			return "Constructed " + text;
		}
		return text;
	}
}
