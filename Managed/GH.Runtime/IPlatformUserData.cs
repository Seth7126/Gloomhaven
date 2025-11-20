using FFSNet;
using Photon.Bolt;
using Platforms;

public interface IPlatformUserData
{
	string PlatformPlayerID { get; }

	string PlatformAccountID { get; }

	string PlatformNetworkAccountPlayerID { get; }

	string UserName { get; }

	bool IsSignedIn { get; }

	void Initialise(IPlatform platform);

	void GetAvatarForSaveOwner(SaveOwner saveOwner);

	void GetAvatarForNetworkPlayer(NetworkPlayer networkPlayer, string platformPlayerID);

	string GetUserNameForConnection(BoltConnection connection);

	string GetPlatformIDForConnection(BoltConnection connection);

	string GetDefaultPlatformID();

	bool CanLogOutEpicStore();
}
