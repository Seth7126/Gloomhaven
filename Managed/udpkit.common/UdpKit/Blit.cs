using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UdpKit;

internal static class Blit
{
	[StructLayout(LayoutKind.Explicit)]
	private struct BitUnion
	{
		[FieldOffset(0)]
		public ushort UInt16;

		[FieldOffset(0)]
		public short Int16;

		[FieldOffset(0)]
		public uint UInt32;

		[FieldOffset(0)]
		public int Int32;

		[FieldOffset(0)]
		public float Float32;

		[FieldOffset(0)]
		public byte Byte0;

		[FieldOffset(1)]
		public byte Byte1;

		[FieldOffset(2)]
		public byte Byte2;

		[FieldOffset(3)]
		public byte Byte3;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct BitUnion64
	{
		[FieldOffset(0)]
		public ulong UInt64;

		[FieldOffset(0)]
		public byte Byte0;

		[FieldOffset(1)]
		public byte Byte1;

		[FieldOffset(2)]
		public byte Byte2;

		[FieldOffset(3)]
		public byte Byte3;

		[FieldOffset(4)]
		public byte Byte4;

		[FieldOffset(5)]
		public byte Byte5;

		[FieldOffset(6)]
		public byte Byte6;

		[FieldOffset(7)]
		public byte Byte7;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct BitUnion128
	{
		[FieldOffset(0)]
		public Guid Guid;

		[FieldOffset(0)]
		public byte Byte0;

		[FieldOffset(1)]
		public byte Byte1;

		[FieldOffset(2)]
		public byte Byte2;

		[FieldOffset(3)]
		public byte Byte3;

		[FieldOffset(4)]
		public byte Byte4;

		[FieldOffset(5)]
		public byte Byte5;

		[FieldOffset(6)]
		public byte Byte6;

		[FieldOffset(7)]
		public byte Byte7;

		[FieldOffset(8)]
		public byte Byte8;

		[FieldOffset(9)]
		public byte Byte9;

		[FieldOffset(10)]
		public byte Byte10;

		[FieldOffset(11)]
		public byte Byte11;

		[FieldOffset(12)]
		public byte Byte12;

		[FieldOffset(13)]
		public byte Byte13;

		[FieldOffset(14)]
		public byte Byte14;

		[FieldOffset(15)]
		public byte Byte15;
	}

	public static void Clear(byte[] bytes)
	{
		Array.Clear(bytes, 0, bytes.Length);
	}

	public static bool PackBool(byte[] bytes, ref int offset, bool value)
	{
		PackByte(bytes, ref offset, (byte)(value ? 1 : 0));
		return value;
	}

	public static bool ReadBool(byte[] bytes, ref int offset)
	{
		return ReadByte(bytes, ref offset) == 1;
	}

	public static void PackByte(byte[] bytes, ref int offset, byte value)
	{
		bytes[offset] = value;
		offset++;
	}

	public static byte ReadByte(byte[] bytes, ref int offset)
	{
		byte result = bytes[offset];
		offset++;
		return result;
	}

	public static int GetBytesPrefixSize(byte[] bytes)
	{
		if (bytes == null)
		{
			return 1;
		}
		return 5 + bytes.Length;
	}

	public static void PackBytesPrefix(byte[] bytes, ref int offset, byte[] from)
	{
		if (PackBool(bytes, ref offset, from != null))
		{
			PackI32(bytes, ref offset, from.Length);
			Array.Copy(from, 0, bytes, offset, from.Length);
			offset += from.Length;
		}
	}

	public static byte[] ReadBytesPrefix(byte[] bytes, ref int offset)
	{
		if (ReadBool(bytes, ref offset))
		{
			byte[] array = new byte[ReadI32(bytes, ref offset)];
			Array.Copy(bytes, offset, array, 0, array.Length);
			offset += array.Length;
			return array;
		}
		return null;
	}

	public static void PackBytes(byte[] bytes, int offset, byte[] from, int length)
	{
		Array.Copy(from, 0, bytes, offset, length);
	}

	public static void ReadBytes(byte[] bytes, int offset, byte[] into, int length)
	{
		Array.Copy(bytes, offset, into, 0, length);
	}

	public static void PackBytes(byte[] bytes, ref int bytesOffset, byte[] from, int fromOffset, int length)
	{
		Array.Copy(from, fromOffset, bytes, bytesOffset, length);
		bytesOffset += length;
	}

	public static void ReadBytes(byte[] bytes, ref int bytesOffset, byte[] into, int intoOffset, int length)
	{
		Array.Copy(bytes, bytesOffset, into, intoOffset, length);
		bytesOffset += length;
	}

	public static void PackU16(byte[] bytes, ref int offset, ushort value)
	{
		BitUnion bitUnion = new BitUnion
		{
			UInt16 = value
		};
		bytes[offset] = bitUnion.Byte0;
		bytes[offset + 1] = bitUnion.Byte1;
		offset += 2;
	}

	public static ushort ReadU16(byte[] bytes, ref int offset)
	{
		BitUnion bitUnion = new BitUnion
		{
			Byte0 = bytes[offset],
			Byte1 = bytes[offset + 1]
		};
		offset += 2;
		return bitUnion.UInt16;
	}

	public static void PackI32(byte[] bytes, ref int offset, int value)
	{
		BitUnion bitUnion = new BitUnion
		{
			Int32 = value
		};
		bytes[offset] = bitUnion.Byte0;
		bytes[offset + 1] = bitUnion.Byte1;
		bytes[offset + 2] = bitUnion.Byte2;
		bytes[offset + 3] = bitUnion.Byte3;
		offset += 4;
	}

	public static int ReadI32(byte[] bytes, ref int offset)
	{
		BitUnion bitUnion = new BitUnion
		{
			Byte0 = bytes[offset],
			Byte1 = bytes[offset + 1],
			Byte2 = bytes[offset + 2],
			Byte3 = bytes[offset + 3]
		};
		offset += 4;
		return bitUnion.Int32;
	}

	public static void PackU32(byte[] bytes, ref int offset, uint value)
	{
		PackU32(bytes, ref offset, value, 4);
	}

	public static void PackU32(byte[] bytes, ref int offset, uint value, int byteCount)
	{
		BitUnion bitUnion = new BitUnion
		{
			UInt32 = value
		};
		switch (byteCount)
		{
		case 1:
			bytes[offset] = bitUnion.Byte0;
			offset++;
			break;
		case 2:
			bytes[offset] = bitUnion.Byte0;
			bytes[offset + 1] = bitUnion.Byte1;
			offset += 2;
			break;
		case 3:
			bytes[offset] = bitUnion.Byte0;
			bytes[offset + 1] = bitUnion.Byte1;
			bytes[offset + 2] = bitUnion.Byte2;
			offset += 3;
			break;
		case 4:
			bytes[offset] = bitUnion.Byte0;
			bytes[offset + 1] = bitUnion.Byte1;
			bytes[offset + 2] = bitUnion.Byte2;
			bytes[offset + 3] = bitUnion.Byte3;
			offset += 4;
			break;
		}
	}

	public static uint ReadU32(byte[] bytes, ref int offset)
	{
		return ReadU32(bytes, ref offset, 4);
	}

	public static uint ReadU32(byte[] bytes, ref int offset, int byteCount)
	{
		BitUnion bitUnion = default(BitUnion);
		switch (byteCount)
		{
		case 1:
			bitUnion.Byte0 = bytes[offset];
			offset++;
			break;
		case 2:
			bitUnion.Byte0 = bytes[offset];
			bitUnion.Byte1 = bytes[offset + 1];
			offset += 2;
			break;
		case 3:
			bitUnion.Byte0 = bytes[offset];
			bitUnion.Byte1 = bytes[offset + 1];
			bitUnion.Byte2 = bytes[offset + 2];
			offset += 3;
			break;
		case 4:
			bitUnion.Byte0 = bytes[offset];
			bitUnion.Byte1 = bytes[offset + 1];
			bitUnion.Byte2 = bytes[offset + 2];
			bitUnion.Byte3 = bytes[offset + 3];
			offset += 4;
			break;
		}
		return bitUnion.UInt32;
	}

	public static void PackF32(byte[] bytes, int offset, float value)
	{
		BitUnion bitUnion = new BitUnion
		{
			Float32 = value
		};
		bytes[offset] = bitUnion.Byte0;
		bytes[offset + 1] = bitUnion.Byte1;
		bytes[offset + 2] = bitUnion.Byte2;
		bytes[offset + 3] = bitUnion.Byte3;
	}

	public static float ReadF32(byte[] bytes, int offset)
	{
		BitUnion bitUnion = new BitUnion
		{
			Byte0 = bytes[offset],
			Byte1 = bytes[offset + 1],
			Byte2 = bytes[offset + 2],
			Byte3 = bytes[offset + 3]
		};
		return bitUnion.Float32;
	}

	public static void PackU64(byte[] bytes, ref int offset, ulong value)
	{
		BitUnion64 bitUnion = new BitUnion64
		{
			UInt64 = value
		};
		bytes[offset] = bitUnion.Byte0;
		bytes[offset + 1] = bitUnion.Byte1;
		bytes[offset + 2] = bitUnion.Byte2;
		bytes[offset + 3] = bitUnion.Byte3;
		bytes[offset + 4] = bitUnion.Byte4;
		bytes[offset + 5] = bitUnion.Byte5;
		bytes[offset + 6] = bitUnion.Byte6;
		bytes[offset + 7] = bitUnion.Byte7;
		offset += 8;
	}

	public static ulong ReadU64(byte[] bytes, ref int offset)
	{
		BitUnion64 bitUnion = new BitUnion64
		{
			Byte0 = bytes[offset],
			Byte1 = bytes[offset + 1],
			Byte2 = bytes[offset + 2],
			Byte3 = bytes[offset + 3],
			Byte4 = bytes[offset + 4],
			Byte5 = bytes[offset + 5],
			Byte6 = bytes[offset + 6],
			Byte7 = bytes[offset + 7]
		};
		offset += 8;
		return bitUnion.UInt64;
	}

	public static int GetStringSize(string value)
	{
		if (value == null)
		{
			return 1;
		}
		return 1 + value.Length + Encoding.UTF8.GetByteCount(value);
	}

	public static void PackString(byte[] bytes, ref int offset, string value)
	{
		if (PackBool(bytes, ref offset, value != null))
		{
			int offset2 = offset;
			offset += 4;
			int bytes2 = Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, offset);
			PackI32(bytes, ref offset2, bytes2);
			offset += bytes2;
		}
	}

	public static string ReadString(byte[] bytes, ref int offset)
	{
		if (ReadBool(bytes, ref offset))
		{
			int num = ReadI32(bytes, ref offset);
			string result = Encoding.UTF8.GetString(bytes, offset, num);
			offset += num;
			return result;
		}
		return null;
	}

	public static UdpEndPoint ReadEndPoint(byte[] bytes, ref int offset)
	{
		BitUnion bitUnion = new BitUnion
		{
			Byte0 = ReadByte(bytes, ref offset),
			Byte1 = ReadByte(bytes, ref offset),
			Byte2 = ReadByte(bytes, ref offset),
			Byte3 = ReadByte(bytes, ref offset)
		};
		uint uInt = bitUnion.UInt32;
		bitUnion = new BitUnion
		{
			Byte0 = ReadByte(bytes, ref offset),
			Byte1 = ReadByte(bytes, ref offset)
		};
		ushort uInt2 = bitUnion.UInt16;
		return new UdpEndPoint(new UdpIPv4Address(uInt), uInt2);
	}

	public static void PackEndPoint(byte[] bytes, ref int offset, UdpEndPoint endpoint)
	{
		BitUnion bitUnion = new BitUnion
		{
			UInt32 = endpoint.Address.Packed
		};
		PackByte(bytes, ref offset, bitUnion.Byte0);
		PackByte(bytes, ref offset, bitUnion.Byte1);
		PackByte(bytes, ref offset, bitUnion.Byte2);
		PackByte(bytes, ref offset, bitUnion.Byte3);
		bitUnion = new BitUnion
		{
			UInt16 = endpoint.Port
		};
		PackByte(bytes, ref offset, bitUnion.Byte0);
		PackByte(bytes, ref offset, bitUnion.Byte1);
	}

	public static void PackGuid(byte[] bytes, ref int offset, Guid value)
	{
		BitUnion128 bitUnion = new BitUnion128
		{
			Guid = value
		};
		bytes[offset] = bitUnion.Byte0;
		bytes[offset + 1] = bitUnion.Byte1;
		bytes[offset + 2] = bitUnion.Byte2;
		bytes[offset + 3] = bitUnion.Byte3;
		bytes[offset + 4] = bitUnion.Byte4;
		bytes[offset + 5] = bitUnion.Byte5;
		bytes[offset + 6] = bitUnion.Byte6;
		bytes[offset + 7] = bitUnion.Byte7;
		bytes[offset + 8] = bitUnion.Byte8;
		bytes[offset + 9] = bitUnion.Byte9;
		bytes[offset + 10] = bitUnion.Byte10;
		bytes[offset + 11] = bitUnion.Byte11;
		bytes[offset + 12] = bitUnion.Byte12;
		bytes[offset + 13] = bitUnion.Byte13;
		bytes[offset + 14] = bitUnion.Byte14;
		bytes[offset + 15] = bitUnion.Byte15;
		offset += 16;
	}

	public static Guid ReadGuid(byte[] bytes, ref int offset)
	{
		BitUnion128 bitUnion = new BitUnion128
		{
			Byte0 = bytes[offset],
			Byte1 = bytes[offset + 1],
			Byte2 = bytes[offset + 2],
			Byte3 = bytes[offset + 3],
			Byte4 = bytes[offset + 4],
			Byte5 = bytes[offset + 5],
			Byte6 = bytes[offset + 6],
			Byte7 = bytes[offset + 7],
			Byte8 = bytes[offset + 8],
			Byte9 = bytes[offset + 9],
			Byte10 = bytes[offset + 10],
			Byte11 = bytes[offset + 11],
			Byte12 = bytes[offset + 12],
			Byte13 = bytes[offset + 13],
			Byte14 = bytes[offset + 14],
			Byte15 = bytes[offset + 15]
		};
		offset += 16;
		return bitUnion.Guid;
	}
}
