using System;

namespace UdpKit.Platform.Photon.Puncher;

internal class StunMsgHeader
{
	public uint Type;

	public uint Length;

	public uint Magic;

	public byte[] TS_ID;

	public byte[] buffer;

	public StunMsgHeader(uint type = 0u, uint length = 0u, uint magic = 0u, byte[] ts_id = null)
	{
		Type = type;
		Length = length;
		Magic = magic;
		TS_ID = ts_id;
		buffer = null;
	}

	public byte[] Encode()
	{
		uint num = 0u;
		Type <<= 16;
		num |= Type;
		num |= Length;
		byte[] bytes = BitConverter.GetBytes(num);
		byte[] bytes2 = BitConverter.GetBytes(Magic);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse((Array)bytes);
			Array.Reverse((Array)bytes2);
		}
		byte[] array = new byte[20];
		Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
		Buffer.BlockCopy(bytes2, 0, array, 4, bytes2.Length);
		if (TS_ID != null)
		{
			Buffer.BlockCopy(TS_ID, 0, array, 8, TS_ID.Length);
		}
		return array;
	}

	public StunMsgHeader Decode(byte[] data)
	{
		if (data.Length < 20)
		{
			throw new Exception("Stun message incomplete");
		}
		byte[] array = new byte[4];
		byte[] array2 = new byte[4];
		Array.ConstrainedCopy(data, 0, array, 0, 4);
		Array.ConstrainedCopy(data, 4, array2, 0, 4);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse((Array)array);
			Array.Reverse((Array)array2);
		}
		Length = (uint)BitConverter.ToInt16(array, 0);
		Type = (uint)BitConverter.ToInt16(array, 2);
		Magic = (uint)BitConverter.ToInt32(array2, 0);
		if (TS_ID == null)
		{
			TS_ID = new byte[12];
		}
		Buffer.BlockCopy(data, 8, TS_ID, 0, 12);
		buffer = data;
		return this;
	}
}
