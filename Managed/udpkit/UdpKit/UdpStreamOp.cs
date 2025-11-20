namespace UdpKit;

internal class UdpStreamOp
{
	public readonly ulong Key;

	public readonly byte[] Data;

	public readonly UdpChannelName Channel;

	public int BlockCurrent;

	public int BlockSize;

	public int BlockCount;

	public uint CRC;

	public uint DoneTime = 0u;

	public ulong Pending;

	public ulong Delivered;

	public bool IsDone => DoneTime != 0;

	public int BlocksRemaining => BlockCount - BlockCurrent;

	public UdpStreamOp(ulong key, UdpChannelName channel, byte[] data)
	{
		Key = key;
		Data = data;
		Channel = channel;
	}
}
