using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

public class Icon
{
	internal struct IHDR
	{
		internal uint png;

		internal uint crlfczlf;

		internal uint ihdr;

		internal int ihdrlen;

		internal int width;

		internal int height;

		internal byte bitDepth;

		internal byte colorType;

		internal byte compressionMethod;

		internal byte filterMethod;

		internal byte interlaceMethod;
	}

	internal byte[] rawBytes;

	internal int width;

	internal int height;

	public byte[] RawBytes => rawBytes;

	public int Width => width;

	public int Height => height;

	internal static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
	{
		GCHandle gCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		try
		{
			return (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
		}
		finally
		{
			gCHandle.Free();
		}
	}

	internal static IHDR GetPNGHeader(byte[] bytes)
	{
		return ByteArrayToStructure<IHDR>(bytes);
	}

	internal static Icon ReadAndCreate(MemoryBuffer buffer)
	{
		Icon icon = null;
		buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PNGBegin);
		if (buffer.ReadBool())
		{
			icon = new Icon();
			buffer.ReadInt32();
			icon.width = buffer.ReadInt32();
			icon.height = buffer.ReadInt32();
			buffer.ReadData(ref icon.rawBytes);
		}
		buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PNGEnd);
		return icon;
	}
}
