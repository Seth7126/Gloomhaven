using System;
using System.Text;

namespace UdpKit;

public class UdpPacket : IDisposable
{
	public bool Write;

	internal bool IsPooled = true;

	internal byte[] Data;

	private readonly UdpPacketPool Pool;

	internal int Ptr;

	private int Length;

	public object UserToken { get; set; }

	public int Size
	{
		get
		{
			return Length;
		}
		set
		{
			Length = UdpMath.Clamp(value, 0, Data.Length << 3);
		}
	}

	public int Position
	{
		get
		{
			return Ptr;
		}
		set
		{
			Ptr = UdpMath.Clamp(value, 0, Length);
		}
	}

	public bool Done => Ptr == Length;

	public bool Overflowing => Ptr > Length;

	public byte[] ByteBuffer => Data;

	public UdpPacket(UdpPacketPool pool)
	{
		Pool = pool;
	}

	public UdpPacket(byte[] arr, UdpPacketPool pool)
		: this(arr, arr.Length, pool)
	{
	}

	public UdpPacket(byte[] arr, int size, UdpPacketPool pool)
	{
		Ptr = 0;
		Data = arr;
		Length = size << 3;
		Pool = pool;
	}

	public bool CanWrite()
	{
		return CanWrite(1);
	}

	public bool CanRead()
	{
		return CanRead(1);
	}

	public bool CanWrite(int bits)
	{
		return Ptr + bits <= Length;
	}

	public bool CanRead(int bits)
	{
		return Ptr + bits <= Length;
	}

	public byte[] DuplicateData()
	{
		byte[] array = new byte[UdpMath.BytesRequired(Ptr)];
		Array.Copy(Data, 0, array, 0, array.Length);
		return array;
	}

	public bool WriteBool(bool value)
	{
		if (value)
		{
			Data[Ptr >> 3] |= (byte)(1 << Ptr % 8);
		}
		else
		{
			Data[Ptr >> 3] &= (byte)(~(1 << Ptr % 8));
		}
		Ptr++;
		return value;
	}

	public bool ReadBool()
	{
		bool result = (Data[Ptr >> 3] & (byte)(1 << Ptr % 8)) > 0;
		Ptr++;
		return result;
	}

	public void WriteByte(byte value, int bits)
	{
		InternalWriteByte(value, bits);
	}

	public byte ReadByte(int bits)
	{
		return InternalReadByte(bits);
	}

	public void WriteByte(byte value)
	{
		WriteByte(value, 8);
	}

	public byte ReadByte()
	{
		return ReadByte(8);
	}

	public void WriteUShort(ushort value, int bits)
	{
		if (bits <= 8)
		{
			InternalWriteByte((byte)(value & 0xFF), bits);
			return;
		}
		InternalWriteByte((byte)(value & 0xFF), 8);
		InternalWriteByte((byte)(value >> 8), bits - 8);
	}

	public ushort ReadUShort(int bits)
	{
		if (bits <= 8)
		{
			return InternalReadByte(bits);
		}
		return (ushort)(InternalReadByte(8) | (InternalReadByte(bits - 8) << 8));
	}

	public void WriteUShort(ushort value)
	{
		WriteUShort(value, 16);
	}

	public ushort ReadUShort()
	{
		return ReadUShort(16);
	}

	public void WriteShort(short value, int bits)
	{
		WriteUShort((ushort)value, bits);
	}

	public short ReadShort(int bits)
	{
		return (short)ReadUShort(bits);
	}

	public void WriteShort(short value)
	{
		WriteShort(value, 16);
	}

	public short ReadShort()
	{
		return ReadShort(16);
	}

	public void Serialize(ref uint value, int bits)
	{
		if (Write)
		{
			WriteUInt(value, bits);
		}
		else
		{
			value = ReadUInt(bits);
		}
	}

	public void Serialize(ref int value, int bits)
	{
		if (Write)
		{
			WriteInt(value, bits);
		}
		else
		{
			value = ReadInt(bits);
		}
	}

	public void WriteUInt(uint value, int bits)
	{
		byte value2 = (byte)value;
		byte value3 = (byte)(value >> 8);
		byte value4 = (byte)(value >> 16);
		byte value5 = (byte)(value >> 24);
		switch ((bits + 7) / 8)
		{
		case 1:
			InternalWriteByte(value2, bits);
			break;
		case 2:
			InternalWriteByte(value2, 8);
			InternalWriteByte(value3, bits - 8);
			break;
		case 3:
			InternalWriteByte(value2, 8);
			InternalWriteByte(value3, 8);
			InternalWriteByte(value4, bits - 16);
			break;
		case 4:
			InternalWriteByte(value2, 8);
			InternalWriteByte(value3, 8);
			InternalWriteByte(value4, 8);
			InternalWriteByte(value5, bits - 24);
			break;
		}
	}

	public uint ReadUInt(int bits)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		switch ((bits + 7) / 8)
		{
		case 1:
			num = InternalReadByte(bits);
			break;
		case 2:
			num = InternalReadByte(8);
			num2 = InternalReadByte(bits - 8);
			break;
		case 3:
			num = InternalReadByte(8);
			num2 = InternalReadByte(8);
			num3 = InternalReadByte(bits - 16);
			break;
		case 4:
			num = InternalReadByte(8);
			num2 = InternalReadByte(8);
			num3 = InternalReadByte(8);
			num4 = InternalReadByte(bits - 24);
			break;
		}
		return (uint)(num | (num2 << 8) | (num3 << 16) | (num4 << 24));
	}

	public void WriteUInt(uint value)
	{
		WriteUInt(value, 32);
	}

	public uint ReadUInt()
	{
		return ReadUInt(32);
	}

	public void WriteInt_Shifted(int value, int bits, int shift)
	{
		WriteInt(value, 32);
	}

	public int ReadInt_Shifted(int bits, int shift)
	{
		return ReadInt(32);
	}

	public void WriteInt(int value, int bits)
	{
		WriteUInt((uint)value, bits);
	}

	public int ReadInt(int bits)
	{
		return (int)ReadUInt(bits);
	}

	public void WriteInt(int value)
	{
		WriteInt(value, 32);
	}

	public int ReadInt()
	{
		return ReadInt(32);
	}

	public void WriteULong(ulong value, int bits)
	{
		if (bits <= 32)
		{
			WriteUInt((uint)(value & 0xFFFFFFFFu), bits);
			return;
		}
		WriteUInt((uint)value, 32);
		WriteUInt((uint)(value >> 32), bits - 32);
	}

	public ulong ReadULong(int bits)
	{
		if (bits <= 32)
		{
			return ReadUInt(bits);
		}
		ulong num = ReadUInt(32);
		ulong num2 = ReadUInt(bits - 32);
		return num | (num2 << 32);
	}

	public void WriteULong(ulong value)
	{
		WriteULong(value, 64);
	}

	public ulong ReadULong()
	{
		return ReadULong(64);
	}

	public void WriteLong(long value, int bits)
	{
		WriteULong((ulong)value, bits);
	}

	public long ReadLong(int bits)
	{
		return (long)ReadULong(bits);
	}

	public void WriteLong(long value)
	{
		WriteLong(value, 64);
	}

	public long ReadLong()
	{
		return ReadLong(64);
	}

	public void WriteFloat(float value)
	{
		UdpByteConverter udpByteConverter = value;
		InternalWriteByte(udpByteConverter.Byte0, 8);
		InternalWriteByte(udpByteConverter.Byte1, 8);
		InternalWriteByte(udpByteConverter.Byte2, 8);
		InternalWriteByte(udpByteConverter.Byte3, 8);
	}

	public float ReadFloat()
	{
		UdpByteConverter udpByteConverter = new UdpByteConverter
		{
			Byte0 = InternalReadByte(8),
			Byte1 = InternalReadByte(8),
			Byte2 = InternalReadByte(8),
			Byte3 = InternalReadByte(8)
		};
		return udpByteConverter.Float32;
	}

	public void WriteDouble(double value)
	{
		UdpByteConverter udpByteConverter = value;
		InternalWriteByte(udpByteConverter.Byte0, 8);
		InternalWriteByte(udpByteConverter.Byte1, 8);
		InternalWriteByte(udpByteConverter.Byte2, 8);
		InternalWriteByte(udpByteConverter.Byte3, 8);
		InternalWriteByte(udpByteConverter.Byte4, 8);
		InternalWriteByte(udpByteConverter.Byte5, 8);
		InternalWriteByte(udpByteConverter.Byte6, 8);
		InternalWriteByte(udpByteConverter.Byte7, 8);
	}

	public double ReadDouble()
	{
		UdpByteConverter udpByteConverter = new UdpByteConverter
		{
			Byte0 = InternalReadByte(8),
			Byte1 = InternalReadByte(8),
			Byte2 = InternalReadByte(8),
			Byte3 = InternalReadByte(8),
			Byte4 = InternalReadByte(8),
			Byte5 = InternalReadByte(8),
			Byte6 = InternalReadByte(8),
			Byte7 = InternalReadByte(8)
		};
		return udpByteConverter.Float64;
	}

	public void WriteByteArray(byte[] from)
	{
		WriteByteArray(from, 0, from.Length);
	}

	public void WriteByteArray(byte[] from, int count)
	{
		WriteByteArray(from, 0, count);
	}

	public void WriteByteArray(byte[] from, int offset, int count)
	{
		int num = Ptr >> 3;
		int num2 = Ptr % 8;
		int num3 = 8 - num2;
		if (num2 == 0)
		{
			Buffer.BlockCopy(from, offset, Data, num, count);
		}
		else
		{
			for (int i = 0; i < count; i++)
			{
				byte b = from[offset + i];
				Data[num] &= (byte)(255 >> num3);
				Data[num] |= (byte)(b << num2);
				num++;
				Data[num] &= (byte)(255 << num2);
				Data[num] |= (byte)(b >> num3);
			}
		}
		Ptr += count * 8;
	}

	public byte[] ReadByteArray(int size)
	{
		byte[] array = new byte[size];
		ReadByteArray(array);
		return array;
	}

	public void ReadByteArray(byte[] to)
	{
		ReadByteArray(to, 0, to.Length);
	}

	public void ReadByteArray(byte[] to, int count)
	{
		ReadByteArray(to, 0, count);
	}

	public void ReadByteArray(byte[] to, int offset, int count)
	{
		int num = Ptr >> 3;
		int num2 = Ptr % 8;
		if (num2 == 0)
		{
			Buffer.BlockCopy(Data, num, to, offset, count);
		}
		else
		{
			int num3 = 8 - num2;
			for (int i = 0; i < count; i++)
			{
				int num4 = Data[num] >> num2;
				num++;
				int num5 = Data[num] & (255 >> num3);
				to[offset + i] = (byte)(num4 | (num5 << num3));
			}
		}
		Ptr += count * 8;
	}

	public void WriteByteArrayWithPrefix(byte[] array)
	{
		WriteByteArrayLengthPrefixed(array, 65536);
	}

	public void WriteByteArrayLengthPrefixed(byte[] array, int maxLength)
	{
		if (WriteBool(array != null))
		{
			int num = Math.Min(array.Length, maxLength);
			if (num < array.Length)
			{
				UdpLog.Warn("Only sendig {0}/{1} bytes from byte array", num, array.Length);
			}
			WriteUShort((ushort)num);
			WriteByteArray(array, 0, num);
		}
	}

	public byte[] ReadByteArrayWithPrefix()
	{
		if (ReadBool())
		{
			int num = ReadUShort();
			byte[] array = new byte[num];
			ReadByteArray(array, 0, array.Length);
			return array;
		}
		return null;
	}

	public void WriteString(string value, Encoding encoding)
	{
		WriteString(value, encoding, int.MaxValue);
	}

	public void WriteString(string value, Encoding encoding, int length)
	{
		if (string.IsNullOrEmpty(value))
		{
			WriteUShort(0);
			return;
		}
		if (length < value.Length)
		{
			value = value.Substring(0, length);
		}
		byte[] bytes = encoding.GetBytes(value);
		WriteUShort((ushort)bytes.Length);
		WriteByteArray(bytes);
	}

	public void WriteString(string value)
	{
		WriteString(value, Encoding.UTF8);
	}

	public string ReadString(Encoding encoding)
	{
		int num = ReadUShort();
		if (num == 0)
		{
			return "";
		}
		byte[] array = new byte[num];
		ReadByteArray(array);
		return encoding.GetString(array, 0, array.Length);
	}

	public string ReadString()
	{
		return ReadString(Encoding.UTF8);
	}

	public void WriteGuid(Guid guid)
	{
		WriteByteArray(guid.ToByteArray());
	}

	public Guid ReadGuid()
	{
		byte[] array = new byte[16];
		ReadByteArray(array);
		return new Guid(array);
	}

	public void WriteEndPoint(UdpEndPoint endpoint)
	{
		WriteByte(endpoint.Address.Byte3);
		WriteByte(endpoint.Address.Byte2);
		WriteByte(endpoint.Address.Byte1);
		WriteByte(endpoint.Address.Byte0);
		WriteUShort(endpoint.Port);
	}

	public UdpEndPoint ReadEndPoint()
	{
		byte a = ReadByte();
		byte b = ReadByte();
		byte c = ReadByte();
		byte d = ReadByte();
		ushort port = ReadUShort();
		UdpIPv4Address address = new UdpIPv4Address(a, b, c, d);
		return new UdpEndPoint(address, port);
	}

	internal static void WriteByteAt(byte[] data, int ptr, int bits, byte value)
	{
		if (bits > 0)
		{
			value = (byte)(value & (255 >> 8 - bits));
			int num = ptr >> 3;
			int num2 = ptr & 7;
			int num3 = 8 - num2;
			int num4 = num3 - bits;
			if (num4 >= 0)
			{
				int num5 = (255 >> num3) | (255 << 8 - num4);
				data[num] = (byte)((data[num] & num5) | (value << num2));
			}
			else
			{
				data[num] = (byte)((data[num] & (255 >> num3)) | (value << num2));
				data[num + 1] = (byte)((data[num + 1] & (255 << bits - num3)) | (value >> num3));
			}
		}
	}

	public void Dispose()
	{
		if (Pool != null)
		{
			Pool.Release(this);
		}
	}

	public override string ToString()
	{
		return $"UdpPacket: size: {Size}, overflow: {Overflowing}";
	}

	private void InternalWriteByte(byte value, int bits)
	{
		WriteByteAt(Data, Ptr, bits, value);
		Ptr += bits;
	}

	private byte InternalReadByte(int bits)
	{
		if (bits <= 0)
		{
			return 0;
		}
		int num = Ptr >> 3;
		int num2 = Ptr % 8;
		byte result;
		if (num2 == 0 && bits == 8)
		{
			result = Data[num];
		}
		else
		{
			int num3 = Data[num] >> num2;
			int num4 = bits - (8 - num2);
			if (num4 < 1)
			{
				result = (byte)(num3 & (255 >> 8 - bits));
			}
			else
			{
				int num5 = Data[num + 1] & (255 >> 8 - num4);
				result = (byte)(num3 | (num5 << bits - num4));
			}
		}
		Ptr += bits;
		return result;
	}
}
