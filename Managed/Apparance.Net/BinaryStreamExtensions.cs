using System.IO;

public static class BinaryStreamExtensions
{
	public unsafe static void WriteNoAlloc(this BinaryWriter stream, float value)
	{
		uint value2 = *(uint*)(&value);
		stream.Write(value2);
	}

	public unsafe static float ReadSingleNoAlloc(this BinaryReader stream)
	{
		uint num = stream.ReadUInt32();
		return *(float*)(&num);
	}
}
