namespace Sony.PS4.SaveData;

public struct SaveDataInfo
{
	internal ulong blocks;

	internal ulong freeBlocks;

	public ulong Blocks => blocks;

	public ulong FreeBlocks => freeBlocks;

	internal void Read(MemoryBuffer buffer)
	{
		blocks = buffer.ReadUInt64();
		freeBlocks = buffer.ReadUInt64();
	}
}
