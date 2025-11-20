namespace UdpKit.Protocol;

internal class Error : Message
{
	public string Text;

	protected override void OnSerialize()
	{
		base.OnSerialize();
		Serialize(ref Text);
	}
}
