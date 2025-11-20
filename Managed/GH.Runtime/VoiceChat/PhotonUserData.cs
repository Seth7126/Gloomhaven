using System;

namespace VoiceChat;

[Serializable]
internal class PhotonUserData
{
	private readonly string _name;

	private readonly string _accountID;

	private readonly string _platformName;

	public string Name => _name;

	public string AccountID => _accountID;

	public string PlatformName => _platformName;

	public bool IsHost { get; private set; }

	public PhotonUserData(string name, string accountID, string platformName, bool isHost)
	{
		_name = name;
		_accountID = accountID;
		_platformName = platformName;
		IsHost = isHost;
	}
}
