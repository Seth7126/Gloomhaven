using Photon.Bolt.Collections;

namespace Photon.Bolt;

internal class NetworkStorage : BitSet, IBoltListNode<NetworkStorage>
{
	public int Frame;

	public NetworkObj Root;

	public NetworkValue[] Values;

	NetworkStorage IBoltListNode<NetworkStorage>.prev { get; set; }

	NetworkStorage IBoltListNode<NetworkStorage>.next { get; set; }

	object IBoltListNode<NetworkStorage>.list { get; set; }

	public NetworkStorage(int size)
	{
		Values = new NetworkValue[size];
	}

	public void PropertyChanged(int property)
	{
		Set(property);
	}
}
