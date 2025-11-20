namespace Photon.Bolt.Internal;

public interface IExtraConnectionStats
{
	void OnPacketSend(BoltConnection connection, Packet packet);
}
