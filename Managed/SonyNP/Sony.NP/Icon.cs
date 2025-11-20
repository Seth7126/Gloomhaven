namespace Sony.NP;

public class Icon
{
	internal byte[] rawBytes;

	internal int width;

	internal int height;

	public byte[] RawBytes => rawBytes;

	public int Width => width;

	public int Height => height;

	internal static Icon ReadAndCreate(MemoryBuffer buffer)
	{
		Icon icon = null;
		buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PNGBegin);
		if (buffer.ReadBool())
		{
			icon = new Icon();
			int num = buffer.ReadInt32();
			icon.width = buffer.ReadInt32();
			icon.height = buffer.ReadInt32();
			buffer.ReadData(ref icon.rawBytes);
		}
		buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PNGEnd);
		return icon;
	}
}
