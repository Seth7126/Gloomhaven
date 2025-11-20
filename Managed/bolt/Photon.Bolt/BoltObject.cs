namespace Photon.Bolt;

public class BoltObject
{
	internal bool _pooled = true;

	[Documentation(Ignore = true)]
	public static implicit operator bool(BoltObject obj)
	{
		return obj != null;
	}
}
