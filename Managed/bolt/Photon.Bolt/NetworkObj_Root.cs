namespace Photon.Bolt;

public abstract class NetworkObj_Root : NetworkObj
{
	internal NetworkObj_Root(NetworkObj_Meta meta)
		: base(meta)
	{
		InitRoot();
	}
}
