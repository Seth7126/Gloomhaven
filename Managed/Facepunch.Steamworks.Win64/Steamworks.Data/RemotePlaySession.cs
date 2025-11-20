namespace Steamworks.Data;

public struct RemotePlaySession
{
	public uint Id { get; set; }

	public bool IsValid => Id != 0;

	public SteamId SteamId => SteamRemotePlay.Internal.GetSessionSteamID(Id);

	public string ClientName => SteamRemotePlay.Internal.GetSessionClientName(Id);

	public SteamDeviceFormFactor FormFactor => SteamRemotePlay.Internal.GetSessionClientFormFactor(Id);

	public override string ToString()
	{
		return Id.ToString();
	}

	public static implicit operator RemotePlaySession(uint value)
	{
		return new RemotePlaySession
		{
			Id = value
		};
	}

	public static implicit operator uint(RemotePlaySession value)
	{
		return value.Id;
	}
}
