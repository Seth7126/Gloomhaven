using Photon.Bolt;

namespace FFSNet;

public interface IControllable
{
	bool IsParticipant { get; }

	bool IsAlive { get; }

	void OnControlAssigned(NetworkPlayer controller);

	void OnControlReleased();

	PrefabId GetNetworkEntityPrefabID();

	string GetName();
}
