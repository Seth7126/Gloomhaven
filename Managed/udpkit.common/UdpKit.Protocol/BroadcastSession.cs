namespace UdpKit.Protocol;

internal class BroadcastSession : Message
{
	public UdpSession Host;

	public int Port;

	protected override void OnSerialize()
	{
		Serialize(ref Host);
		Serialize(ref Port);
	}
}
