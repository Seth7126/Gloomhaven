using System;

namespace UdpKit.Protocol;

internal abstract class Result : Message
{
	public Guid Query;

	protected override void OnSerialize()
	{
		base.OnSerialize();
		Serialize(ref Query);
	}
}
