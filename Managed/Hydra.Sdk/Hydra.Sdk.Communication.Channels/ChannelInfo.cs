namespace Hydra.Sdk.Communication.Channels;

public class ChannelInfo
{
	public string Address { get; }

	public bool IsSecure { get; }

	public ChannelInfo(string address, bool isSecure)
	{
		Address = address;
		IsSecure = isSecure;
	}
}
