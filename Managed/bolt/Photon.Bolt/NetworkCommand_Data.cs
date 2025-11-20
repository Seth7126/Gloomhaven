namespace Photon.Bolt;

internal abstract class NetworkCommand_Data : NetworkObj, INetworkCommandData
{
	public IProtocolToken Token { get; set; }

	internal Command RootCommand => (Command)Root;

	IProtocolToken INetworkCommandData.Token
	{
		get
		{
			return Token;
		}
		set
		{
			Token.Release();
			Token = value;
		}
	}

	internal NetworkCommand_Data(NetworkObj_Meta meta)
		: base(meta)
	{
	}
}
