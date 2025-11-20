using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Sony.PS4.SaveData;

internal class MemoryBuffer
{
	public enum BufferIntegrityChecks
	{
		BufferBegin,
		BufferEnd,
		PNGBegin,
		PNGEnd
	}

	private MemoryBufferNative rawBuffer;

	private IntPtr pos;

	public MemoryBuffer(MemoryBufferNative pointer)
	{
		rawBuffer.data = pointer.data;
		rawBuffer.size = pointer.size;
		pos = rawBuffer.data;
	}

	public void CheckStartMarker()
	{
		CheckMarker(BufferIntegrityChecks.BufferBegin);
	}

	public void CheckEndMarker()
	{
		CheckMarker(BufferIntegrityChecks.BufferEnd);
	}

	public void CheckMarker(BufferIntegrityChecks value)
	{
		byte b = Marshal.ReadByte(pos);
		pos = new IntPtr(pos.ToInt64() + 1);
		byte b2 = Marshal.ReadByte(pos);
		pos = new IntPtr(pos.ToInt64() + 1);
		byte b3 = Marshal.ReadByte(pos);
		pos = new IntPtr(pos.ToInt64() + 1);
		byte b4 = Marshal.ReadByte(pos);
		pos = new IntPtr(pos.ToInt64() + 1);
		if (b == byte.MaxValue && b2 == 254 && b3 == 253 && (BufferIntegrityChecks)b4 == value)
		{
			return;
		}
		throw new SaveDataException("MemoryBuffer - CheckMarker error - Expecting " + value);
	}

	public void CheckBufferOverflow(string method)
	{
		long num = pos.ToInt64() - rawBuffer.data.ToInt64();
		if ((uint)num > rawBuffer.size)
		{
			throw new SaveDataException("MemoryBuffer - Overflow error detected. (" + method + ") (" + num + "," + rawBuffer.size + ")");
		}
	}

	public bool ReadBool()
	{
		CheckBufferOverflow("ReadBool");
		byte b = Marshal.ReadByte(pos);
		pos = new IntPtr(pos.ToInt64() + 1);
		if (b == 0)
		{
			return false;
		}
		return true;
	}

	public sbyte ReadInt8()
	{
		CheckBufferOverflow("ReadInt8");
		sbyte result = (sbyte)Marshal.ReadByte(pos);
		pos = new IntPtr(pos.ToInt64() + 1);
		return result;
	}

	public byte ReadUInt8()
	{
		CheckBufferOverflow("ReadUInt8");
		byte result = Marshal.ReadByte(pos);
		pos = new IntPtr(pos.ToInt64() + 1);
		return result;
	}

	public short ReadInt16()
	{
		CheckBufferOverflow("ReadInt16");
		short result = Marshal.ReadInt16(pos);
		pos = new IntPtr(pos.ToInt64() + 2);
		return result;
	}

	public ushort ReadUInt16()
	{
		CheckBufferOverflow("ReadUInt16");
		ushort result = (ushort)Marshal.ReadInt16(pos);
		pos = new IntPtr(pos.ToInt64() + 2);
		return result;
	}

	public int ReadInt32()
	{
		CheckBufferOverflow("ReadInt32");
		int result = Marshal.ReadInt32(pos);
		pos = new IntPtr(pos.ToInt64() + 4);
		return result;
	}

	public uint ReadUInt32()
	{
		CheckBufferOverflow("ReadUInt32");
		uint result = (uint)Marshal.ReadInt32(pos);
		pos = new IntPtr(pos.ToInt64() + 4);
		return result;
	}

	public long ReadInt64()
	{
		CheckBufferOverflow("ReadInt64");
		long result = Marshal.ReadInt64(pos);
		pos = new IntPtr(pos.ToInt64() + 8);
		return result;
	}

	public ulong ReadUInt64()
	{
		CheckBufferOverflow("ReadUInt64");
		ulong result = (ulong)Marshal.ReadInt64(pos);
		pos = new IntPtr(pos.ToInt64() + 8);
		return result;
	}

	public IntPtr ReadPtr()
	{
		CheckBufferOverflow("ReadPtr");
		long value = Marshal.ReadInt64(pos);
		pos = new IntPtr(pos.ToInt64() + 8);
		return new IntPtr(value);
	}

	public double ReadDouble()
	{
		CheckBufferOverflow("ReadDouble");
		double[] array = new double[1];
		Marshal.Copy(pos, array, 0, 1);
		pos = new IntPtr(pos.ToInt64() + 8);
		return array[0];
	}

	public uint ReadData(ref byte[] data)
	{
		CheckBufferOverflow("ReadData");
		uint num = ReadUInt32();
		if (num == 0)
		{
			return 0u;
		}
		if (data == null || data.Length != num)
		{
			data = new byte[num];
		}
		Marshal.Copy(pos, data, 0, (int)num);
		pos = new IntPtr(pos.ToInt64() + num);
		return num;
	}

	public uint ReadData(ref byte[] data, uint startIndex)
	{
		CheckBufferOverflow("ReadData");
		uint num = ReadUInt32();
		if (num == 0)
		{
			return 0u;
		}
		if (data == null || startIndex + num > data.Length)
		{
			byte[] array = new byte[num];
			if (data != null)
			{
				Array.Copy(data, array, startIndex);
			}
			data = array;
		}
		Marshal.Copy(pos, data, (int)startIndex, (int)num);
		pos = new IntPtr(pos.ToInt64() + num);
		return num;
	}

	public void ReadString(ref string str)
	{
		CheckBufferOverflow("ReadString");
		byte[] data = null;
		if (ReadData(ref data) == 0)
		{
			str = "";
		}
		else
		{
			str = Encoding.UTF8.GetString(data, 0, data.Length);
		}
	}

	public override string ToString()
	{
		long num = pos.ToInt64() - rawBuffer.data.ToInt64();
		long num2 = rawBuffer.data.ToInt64();
		return "Memorry buffer : Data = (" + num2.ToString("X") + ") Size = (" + rawBuffer.size + ") Read = (" + num + ")";
	}
}
