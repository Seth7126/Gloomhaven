namespace UdpKit;

internal class UdpStreamChannel
{
	public UdpChannelConfig Config;

	public UdpChannelName Name => Config.ChannelName;

	public bool IsUnreliable => Config.Mode == UdpChannelMode.Unreliable;

	public bool IsReliable => Config.Mode == UdpChannelMode.Reliable;
}
