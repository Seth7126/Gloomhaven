namespace Photon.Bolt.Internal;

[Documentation(Ignore = true)]
public struct PacketStats
{
	public int StateBits;

	public int EventBits;

	public int CommandBits;

	public void Clear()
	{
		StateBits = 0;
		EventBits = 0;
		CommandBits = 0;
	}
}
